﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using SpatialSlur.SlurCore;
using SpatialSlur.SlurData;
using SpatialSlur.SlurField;
using SpatialSlur.SlurMesh;

using SpatialSlur.SlurRhino;
using SpatialSlur.SlurRhino.Remesher;

using static SpatialSlur.SlurCore.SlurMath;
using static SpatialSlur.SlurData.DataUtil;

/*
 * Notes
 */

namespace SpatialSlur.SlurRhino.LoopGrower
{
    using V = HeMeshSim.Vertex;
    using E = HeMeshSim.Halfedge;
    using F = HeMeshSim.Face;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class LoopGrower
    {
        #region Static

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TV"></typeparam>
        /// <typeparam name="TE"></typeparam>
        /// <typeparam name="TF"></typeparam>
        /// <param name="mesh"></param>
        /// <param name="target"></param>
        /// <param name="features"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static LoopGrower Create<TV, TE, TF>(HeMeshBase<TV, TE, TF> mesh, MeshFeature target, IEnumerable<IFeature> features, double tolerance = 1.0e-4)
            where TV : HeVertex<TV, TE, TF>, IVertex3d
            where TE : Halfedge<TV, TE, TF>
            where TF : HeFace<TV, TE, TF>
        {
            var copy = HeMeshSim.Factory.CreateCopy(mesh, (v0, v1) => v0.Position = v1.Position, delegate { }, delegate { });
            return new LoopGrower(copy, target, features, tolerance);
        }

        #endregion


        private const double _targetBinScale = 3.5; // as a factor of radius
        private const double _targetLoadFactor = 3.0;

        //
        // simulation mesh
        //

        private HeMeshSim _mesh;
        private HeElementList<V> _verts;
        private HeElementList<E> _hedges;
        private HashGrid3d<V> _grid;

        //
        // constraint objects
        //

        private MeshFeature _target;
        private List<IFeature> _features;
        private IField3d<double> _lengthField;

        //
        // simulation settings
        //

        private LoopGrowerSettings _settings;
        private int _stepCount = 0;
        private int _vertTag = int.MinValue;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="target"></param>
        /// <param name="features"></param>
        /// <param name="tolerance"></param>
        public LoopGrower(HeMeshSim mesh, MeshFeature target, IEnumerable<IFeature> features, double tolerance = 1.0e-4)
        {
            mesh.Compact();

            _mesh = mesh;
            _verts = _mesh.Vertices;
            _hedges = _mesh.Halfedges;
            _settings = new LoopGrowerSettings();
            _stepCount = 0;

            Target = target;
            InitFeatures(features, tolerance);

            // start on features
            ProjectToFeatures();
        }


        /// <summary>
        /// 
        /// </summary>
        private void InitFeatures(IEnumerable<IFeature> features, double tolerance)
        {
            _features = new List<IFeature>();
            var tolSqr = tolerance * tolerance;

            // create features
            foreach (var f in features)
            {
                int index = _features.Count;
                _features.Add(f);

                // if vertex is close enough, assign feature
                foreach (var v in _verts)
                {
                    if (v.FeatureIndex > -1) continue; // skip if already assigned

                    var p = v.Position;
                    if (p.SquareDistanceTo(f.ClosestPoint(p)) < tolSqr)
                        v.FeatureIndex = index;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public HeMeshBase<V, E, F> Mesh
        {
            get { return _mesh; }
        }


        /// <summary>
        /// 
        /// </summary>
        public MeshFeature Target
        {
            get { return _target; }
            set { _target = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public List<IFeature> Features
        {
            get { return _features; }
            set { _features = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public IField3d<double> EdgeLengthField
        {
            get { return _lengthField; }
            set { _lengthField = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public LoopGrowerSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public int StepCount
        {
            get { return _stepCount; }
        }


        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public void Step()
        {
            for (int i = 0; i < _settings.SubSteps; i++)
            {
                if (++_stepCount % _settings.RefineFrequency == 0)
                    Refine();
           
                CalculateForces();
                UpdateVertices();

                ProjectToFeatures();
            }
        }


        #region Dynamics

        /// <summary>
        /// Calculates all forces in the system
        /// </summary>
        /// <returns></returns>
        private void CalculateForces()
        {
            if (_stepCount % _settings.CollideFrequency == 0)
            {
                SphereCollide(_settings.CollideRadius, 1.0);
                //SphereCollideParallel(_settings.CollideRadius, 1.0);
            }

            LaplacianFair(_settings.SmoothWeight, _settings.SmoothWeight);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="weightInterior"></param>
        /// <param name="weightBoundary"></param>
        private void LaplacianFair(double weightInterior, double weightBoundary)
        {
            for (int i = 0; i < _verts.Count; i++)
            {
                var v0 = _verts[i];
                if (v0.IsRemoved) continue;

                if (v0.IsBoundary)
                {
                    var he = v0.FirstOut;

                    var v1 = he.PrevInFace.Start;
                    var v2 = he.NextInFace.Start;
                    var move = (v1.Position + v2.Position) * 0.5 - v0.Position;

                    // apply to central vertex
                    v0.MoveSum += move * weightInterior;
                    v0.WeightSum += weightInterior;

                    // distribute negated to neighbours
                    move *= -0.5;
                    v1.MoveSum += move * weightBoundary;
                    v1.WeightSum += weightBoundary;
                    v2.MoveSum += move * weightBoundary;
                    v2.WeightSum += weightBoundary;
                }
                else
                {
                    var sum = new Vec3d();
                    var count = 0;

                    foreach (var v1 in v0.ConnectedVertices)
                    {
                        sum += v1.Position;
                        count++;
                    }

                    double t = 1.0 / count;
                    var move = sum * t - v0.Position;

                    // apply to central vertex
                    v0.MoveSum += move * weightInterior;
                    v0.WeightSum += weightInterior;

                    // distribute negated to neighbours
                    move *= -t;
                    foreach (var v1 in v0.ConnectedVertices)
                    {
                        v1.MoveSum += move * weightInterior;
                        v1.WeightSum += weightInterior;
                    }
                }
            }
        }


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="weight"></param>
        private void LaplacianFair(double weight)
        {
            for (int i = 0; i < _verts.Count; i++)
            {
                var v0 = _verts[i];
                if (v0.IsRemoved) continue;

                // calculate graph laplacian
                var sum = new Vec3d();
                var count = 0;

                foreach (var v1 in v0.ConnectedVertices)
                {
                    sum += v1.Position;
                    count++;
                }

                double t = 1.0 / count;
                var move = sum * t - v0.Position;

                // apply to central vertex
                v0.MoveSum += move * weight;
                v0.WeightSum += weight;

                // distribute negated to neighbours
                move *= -t;
                foreach (var v1 in v0.ConnectedVertices)
                {
                    v1.MoveSum += move * weight;
                    v1.WeightSum += weight;
                }
            }
        }
        */


        /// <summary>
        /// Note this method assumes that vertex deltas and weights are clear
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="weight"></param>
        private void SphereCollide(double radius, double weight)
        {
            UpdateGrid(radius);

            var diam = radius * 2.0;
            var diamSqr = diam * diam;
      
            // calculate collisions
            foreach (var v0 in _verts)
            {
                var p0 = v0.Position;

                // search from h0
                foreach (var v1 in _grid.Search(new Interval3d(p0, diam)))
                {
                    var d = v1.Position - p0;
                    var m = d.SquareLength;

                    if (m < diamSqr && m > 0.0)
                    {
                        d *= (1.0 - diam / Math.Sqrt(m)) * 0.5 * weight;
                        v0.MoveSum += d;
                        v1.MoveSum -= d;
                        v0.WeightSum = v1.WeightSum = weight;
                    }
                }

                // insert h0
                _grid.Insert(p0, v0);
            }

            _grid.Clear();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="weight"></param>
        private void SphereCollideParallel(double radius, double weight)
        {
            UpdateGrid(radius);

            var rad2 = radius * 2.0;
            var rad2Sqr = rad2 * rad2;

            // insert vertices
            foreach (var v in _verts)
                _grid.Insert(v.Position, v);

            // search from each vertex and handle collisions
            Parallel.ForEach(Partitioner.Create(0, _verts.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var v0 = _verts[i];
                    var p0 = v0.Position;

                    var moveSum = new Vec3d();
                    int count = 0;

                    _grid.Search(new Interval3d(p0, rad2), v1 =>
                    {
                        var move = v1.Position - p0;
                        var d = move.SquareLength;

                        if (d < rad2Sqr && d > 0.0)
                        {
                            moveSum += move * ((1.0 - rad2 / Math.Sqrt(d)) * 0.5);
                            count++;
                        }

                        return true;
                    });
                    
                    if (count == 0) continue;
              
                    v0.MoveSum += moveSum * weight;
                    v0.WeightSum += weight;
                }
            });

            _grid.Clear();
        }


        /// <summary>
        /// 
        /// </summary>
        private void UpdateGrid(double radius)
        {
            if (_grid == null)
            {
                _grid = new HashGrid3d<V>((int)(_verts.Count * _targetLoadFactor), radius * RadiusToBinScale);
                return;
            }

            _grid.BinScale = radius * RadiusToBinScale;

            int targCount = (int)(_verts.Count * _targetLoadFactor);
            if (_grid.BinCount < (targCount >> 1)) _grid.Resize(targCount);
        }


        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private void UpdateVertices()
        {
            double timeStep = _settings.TimeStep;
            double damping = _settings.Damping;

            Parallel.ForEach(Partitioner.Create(0, _verts.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var v = _verts[i];
                    v.Velocity *= damping;

                    double w = v.WeightSum;
                    if (w > 0.0) v.Velocity += v.MoveSum * (timeStep / w);
                    v.Position += v.Velocity * timeStep;

                    v.MoveSum = new Vec3d();
                    v.WeightSum = 0.0;
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        private void ProjectToFeatures()
        {
            Parallel.ForEach(Partitioner.Create(0, _verts.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var v = _verts[i];

                    var p = v.Position;
                    int fi = v.FeatureIndex;

                    /*
                    // snap to feature or target if one exists
                    if (fi != -1)
                        v.Position = _features[v.FeatureIndex].ClosestPoint(v.Position);
                    else if(_target != null)
                        v.Position = _target.ClosestPoint(v.Position);
                    */

                    // snap to feature
                    if (fi != -1)
                        v.Position = _features[v.FeatureIndex].ClosestPoint(v.Position);

                    // snap to target
                    if (_target != null)
                        v.Position = _target.ClosestPoint(v.Position);
                }
            });
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="weight"></param>
        /// <returns></returns>
        private void PullToFeatures(double weight)
        {
            Parallel.ForEach(Partitioner.Create(0, _verts.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var v = _verts[i];
                    if (v.IsRemoved || v.FeatureIndex == -1) continue;

                    var p = v.Position;
                    int fi = v.FeatureIndex;

                    if (fi != 1)
                        ApplyMove(v, _features[fi]);

                    if(_target != null)
                        ApplyMove(v, _target);
                }
            });


            void ApplyMove(V vertex, IFeature feature)
            {
                var p = vertex.Position;
                vertex.MoveSum += (feature.ClosestPoint(p) - p) * weight;
                vertex.WeightSum += weight;
            }
        }

        #endregion


        #region Topology

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private void Refine()
        {
            UpdateMaxLengths(true);
            SplitEdges(_hedges.Count);
        }


        /// <summary>
        /// Splits long edges
        /// </summary>
        private void SplitEdges(int count)
        {
            _vertTag++;

            for (int i = 0; i < count; i += 2)
            {
                var he = _hedges[i];

                var v0 = he.Start;
                var v1 = he.End;
                
                var fi = GetSplitFeature(v0.FeatureIndex, v1.FeatureIndex);
                if (fi < -1) continue; // don't split between different features
                
                var p0 = v0.Position;
                var p1 = v1.Position;

                // split edge if length exceeds max
                if (p0.SquareDistanceTo(p1) > he.MaxLength * he.MaxLength)
                {
                    var v2 = _mesh.SplitEdge(he).Start;

                    // set attributes of new vertex
                    v2.Position = (v0.Position + v1.Position) * 0.5;
                    v2.FeatureIndex = fi;
                }
            }
        }


        /*
        /// <summary>
        /// Splits long edges
        /// </summary>
        private void SplitEdges(int count)
        {
            _vertTag++;

            for (int i = 0; i < count; i += 2)
            {
                var he = _hedges[i];
                if (he.IsRemoved) continue;

                var v0 = he.Start;
                var v1 = he.End;

                // don't split edge that spans between 2 different features
                var fi0 = v0.FeatureIndex;
                var fi1 = v1.FeatureIndex;
                // if (fi0 > -1 && fi1 > -1 && fi0 != fi1) continue;
                
                var p0 = v0.Position;
                var p1 = v1.Position;

                // split edge if length exceeds max
                if (p0.SquareDistanceTo(p1) > he.MaxLength * he.MaxLength)
                {
                    var v2 = _mesh.SplitEdge(he).Start;

                    // set attributes of new vertex
                    v2.Position = (v0.Position + v1.Position) * 0.5;
                    // v2.FeatureIndex = Math.Min(fi0, fi1);

                    // if same feature
                    v2.FeatureIndex = (fi0 == fi1) ? -1 : Math.Min(fi0, fi1);
                }
            }
        }
        */


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fi0"></param>
        /// <param name="fi1"></param>
        /// <returns></returns>
        private static int GetSplitFeature(int fi0, int fi1)
        {
            if (fi0 == -1 || fi1 == -1) return -1; // only one on feature
            if (fi0 == fi1) return fi0; // both on same feature
            return -2; // on different features
        }

        #endregion


        #region Attributes

        /// <summary>
        /// 
        /// </summary>
        void UpdateMaxLengths(bool parallel = false)
        {
            var lengthRange = _settings.LengthRange;

            // set length targets to default if no field
            if (_lengthField == null)
            {
                var min = lengthRange.Min;

                for (int i = 0; i < _hedges.Count; i += 2)
                    _hedges[i].MaxLength = min;

                return;
            }

            // evaluate field
            Action<Tuple<int, int>> body = range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var he = _hedges[i << 1];
                    if (he.IsRemoved) continue;

                    var p = (he.Start.Position + he.End.Position) * 0.5;
                    he.MaxLength = lengthRange.Evaluate(_lengthField.ValueAt(p));
                }
            };

            if (parallel)
                Parallel.ForEach(Partitioner.Create(0, _hedges.Count >> 1), body);
            else
                body(Tuple.Create(0, _hedges.Count >> 1));
        }

        #endregion
    }
}
