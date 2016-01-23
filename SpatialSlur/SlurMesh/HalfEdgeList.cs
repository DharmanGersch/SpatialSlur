﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpatialSlur.SlurCore;

namespace SpatialSlur.SlurMesh
{
    public class HalfEdgeList:HeElementList<HalfEdge>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        internal HalfEdgeList(HeMesh mesh)
            : base(mesh)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="capacity"></param>
        internal HalfEdgeList(HeMesh mesh, int capacity)
            : base(mesh, capacity)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributes"></param>
        internal void HalfSizeCheck<U>(IList<U> attributes)
        {
            if (attributes.Count != Count >> 1)
                throw new ArgumentException("The number of attributes provided does not match the number of edges in the mesh.");
        }


        /// <summary>
        /// Adds an edge and its twin to the list.
        /// </summary>
        /// <param name="edge"></param>
        internal void AddPair(HalfEdge edge)
        {
            Add(edge);
            Add(edge.Twin);
        }


        /// <summary>
        /// Creates a pair of halfedges between the given vertices and add them to the list.
        /// Note that the face references of the new halfedges are left unassigned.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        internal HalfEdge AddPair(HeVertex start, HeVertex end)
        {
            HalfEdge e0 = new HalfEdge();
            HalfEdge e1 = new HalfEdge();

            e0.Start = start;
            e1.Start = end;
            HalfEdge.MakeTwins(e0, e1);

            AddPair(e0);
            return e0;
        }


        #region Element Attributes

        /// <summary>
        /// Returns the length of each half-edge in the mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public double[] GetHalfEdgeLengths()
        {
            double[] result = new double[Count];
            UpdateHalfEdgeLengths(result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public void UpdateHalfEdgeLengths(IList<double> result)
        {
            SizeCheck(result);

            Parallel.ForEach(Partitioner.Create(0, Count >> 1), range =>
            {
                int i0 = range.Item1 << 1;
                int i1 = range.Item2 << 1;

                for (int i = i0; i < i1; i += 2)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;

                    double d = e.Span.Length;
                    result[i] = d;
                    result[i + 1] = d;
                }
            });
        }



        /// <summary>
        /// Returns the length of each edge (pair of half-edges) in the mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public double[] GetEdgeLengths()
        {
            double[] result = new double[Count >> 1];
            UpdateEdgeLengths(result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public void UpdateEdgeLengths(IList<double> result)
        {
            HalfSizeCheck(result);

            Parallel.ForEach(Partitioner.Create(0, Count >> 1), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i << 1];
                    if (e.IsUnused) continue;
                    result[i] = e.Span.Length;
                }
            });
        }

        
        /// <summary>
        /// Returns the angle between each half-edge and its previous.
        /// </summary>
        /// <returns></returns>
        public double[] GetHalfEdgeAngles()
        {
            double[] result = new double[Count];
            UpdateHalfEdgeAngles(result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double[] GetHalfEdgeAngles(IList<double> edgeLengths)
        {
            double[] result = new double[Count];
            UpdateHalfEdgeAngles(edgeLengths, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void UpdateHalfEdgeAngles(IList<double> result)
        {
            SizeCheck(result);

            Parallel.ForEach(Partitioner.Create(0, Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;
                    result[i] = Vec3d.Angle(e.Span, e.Prev.Twin.Span);
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void UpdateHalfEdgeAngles(IList<double> edgeLengths, IList<double> result)
        {
            SizeCheck(edgeLengths);
            SizeCheck(result);

            Parallel.ForEach(Partitioner.Create(0, Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e0 = this[i];
                    if (e0.IsUnused) continue;

                    HalfEdge e1 = e0.Prev.Twin;
                    result[i] = Math.Acos((e0.Span / edgeLengths[i]) * (e1.Span / edgeLengths[e1.Index]));
                }
            });
        }


        /// <summary>
        /// Returns a dihedral angle at each edge (pair of half-edges) in the mesh.
        /// </summary>
        /// <returns></returns>
        public double[] GetDihedralAngles(IList<Vec3d> faceNormals)
        {
            double[] result = new double[Count >> 1];
            UpdateDihedralAngles(faceNormals, result);
            return result;
        }


        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public void UpdateDihedralAngles(IList<Vec3d> faceNormals, IList<double> result)
        {
            SizeCheck(result);
            Mesh.Faces.SizeCheck(faceNormals);
       
            Parallel.ForEach(Partitioner.Create(0, Count >> 1), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i << 1];
                    if (e.IsUnused || e.IsBoundary) continue;

                    double angle = Vec3d.Angle(faceNormals[e.Face.Index], faceNormals[e.Twin.Face.Index]);
                    result[i] = angle;
                }
            });
        }


        /// <summary>
        /// Returns the area associated with each halfedge.
        /// TODO Look into alternate formulations.
        /// http://www.cs.columbia.edu/~keenan/Projects/Other/TriangleAreasCheatSheet.pdf
        /// </summary>
        /// <param name="faceCenters"></param>
        /// <returns></returns>
        public double[] GetHalfEdgeAreas(IList<Vec3d> faceCenters)
        {
            double[] result = new double[Count];
            UpdateHalfEdgeAreas(faceCenters, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="faceCenters"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public void UpdateHalfEdgeAreas(IList<Vec3d> faceCenters, IList<double> result)
        {
            SizeCheck(result);
            Mesh.Faces.SizeCheck(faceCenters);

            Parallel.ForEach(Partitioner.Create(0, Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused || e.Face == null) continue;

                    Vec3d v0 = e.Span * 0.5;
                    Vec3d v1 = faceCenters[e.Face.Index] - e.Start.Position;
                    Vec3d v2 = e.Prev.Span * -0.5;

                    result[i] = (Vec3d.Cross(v0, v1).Length + Vec3d.Cross(v1, v2).Length) * 0.5;
                }
            });
        }


        /// <summary>
        /// Returns the cotangent of each half-edge.
        /// Assumes triangle mesh.
        /// http://www.cs.columbia.edu/~keenan/Projects/Other/TriangleAreasCheatSheet.pdf
        /// </summary>
        /// <returns></returns>
        public double[] GetCotangents()
        {
            double[] result = new double[Count];
            UpdateCotangents(result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public void UpdateCotangents(IList<double> result)
        {
            SizeCheck(result);

            Parallel.ForEach(Partitioner.Create(0, Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;

                    Vec3d v0 = e.Prev.Span;
                    Vec3d v1 = e.Next.Twin.Span;
                    result[i] = v0 * v1 / Vec3d.Cross(v0, v1).Length;
                }
            });
        }


        /// <summary>
        /// Returns the cotangent weight for each half-edge.
        /// Assumes triangle mesh.
        /// http://www.multires.caltech.edu/pubs/diffGeoOps.pdf
        /// http://courses.cms.caltech.edu/cs177/hmw/Hmw2.pdf
        /// </summary>
        /// <param name="edgeAngles"></param>
        /// <returns></returns>
        public double[] GetCotanWeights()
        {
            double[] result = new double[Count];
            UpdateCotanWeights(result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public void UpdateCotanWeights(IList<double> result)
        {
            SizeCheck(result);

            Parallel.ForEach(Partitioner.Create(0, Count >> 1), range =>
            {
                int i0 = range.Item1 << 1;
                int i1 = range.Item2 << 1;

                for (int i = i0; i < i1; i += 2)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;
                    double w = 0.0;

                    if (e.Face != null)
                    {
                        Vec3d v0 = e.Prev.Span;
                        Vec3d v1 = e.Next.Twin.Span;
                        w += v0 * v1 / Vec3d.Cross(v0, v1).Length;
                    }

                    e = e.Twin;

                    if (e.Face != null)
                    {
                        Vec3d v0 = e.Prev.Span;
                        Vec3d v1 = e.Next.Twin.Span;
                        w += v0 * v1 / Vec3d.Cross(v0, v1).Length;
                    }

                    w *= 0.5;
                    result[i] = w;
                    result[i + 1] = w;
                }
            });
        }


        /// <summary>
        /// Returns the area normalized cotangent weight for each half-edge.
        /// Assumes triangle mesh.
        /// http://www.multires.caltech.edu/pubs/diffGeoOps.pdf
        /// http://courses.cms.caltech.edu/cs177/hmw/Hmw2.pdf
        /// </summary>
        /// <param name="edgeAngles"></param>
        /// <returns></returns>
        public double[] GetAreaCotanWeights()
        {
            double[] result = new double[Count];
            UpdateAreaCotanWeights(result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public void UpdateAreaCotanWeights(IList<double> result)
        {
            SizeCheck(result);

            double[] areas = new double[Mesh.Vertices.Count];
            HeFaceList faces = Mesh.Faces;
            double t = 1.0 / 3.0;

            // Can't parallelize due to multiple threads potentially writing to the same array address
            // TODO consider parallel alternatives
            for (int i = 0; i < faces.Count; i++)
            {
                HeFace f = faces[i];
                if (f.IsUnused) continue;

                foreach (HalfEdge e in f.Edges)
                {
                    Vec3d v0 = e.Prev.Span;
                    Vec3d v1 = e.Next.Twin.Span;

                    // add to vertex areas
                    double a = Vec3d.Cross(v0, v1).Length;
                    areas[e.Start.Index] += 0.5 * a * t; // 1/3rd the triangular area

                    // add to edge weights
                    double w = 0.5 * v0 * v1 / a;
                    result[e.Index] += w;
                    result[e.Twin.Index] += w;
                }
            }

            // normalize weights with vertex areas
            Parallel.ForEach(Partitioner.Create(0, Count / 2), range =>
            {
                int i0 = range.Item1 * 2;
                int i1 = range.Item2 * 2;

                for (int i = i0; i < i1; i += 2)
                {
                    HeVertex v0 = this[i].Start;
                    HeVertex v1 = this[i + 1].Start;

                    double w = result[i] / Math.Sqrt(areas[v0.Index] * areas[v1.Index]);
                    result[i] = w;
                    result[i + 1] = w;
                }
            });
        }


        /*
        /// <summary>
        /// returns the cotangent weight for each halfedge pair
        /// assumes triangle mesh
        /// 
        /// http://www.multires.caltech.edu/pubs/diffGeoOps.pdf
        /// http://courses.cms.caltech.edu/cs177/hmw/Hmw2.pdf
        /// </summary>
        /// <param name="edgeAngles"></param>
        /// <returns></returns>
        public double[] GetCotanWeights()
        {
            double[] result = new double[Count];
            GetCotanWeights(result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public void GetCotanWeights(IList<double> result)
        {
            Parallel.ForEach(Partitioner.Create(0, Count / 2), range =>
            {
                int i0 = range.Item1 * 2;
                int i1 = range.Item2 * 2;

                for (int i = i0; i < i1; i += 2)
                {
                    HeEdge e = List[i];
                    if (e.IsUnused) continue;

                    double w = 0.0;
                    if (e.Face != null) w += SlurMath.Cotan(Vec3d.Angle(e.Prev.Span, e.Next.Twin.Span));

                    e = e.Twin;
                    if (e.Face != null) w += SlurMath.Cotan(Vec3d.Angle(e.Prev.Span, e.Next.Twin.Span));

                    w *= 0.5;
                    result[i] = w;
                    result[i + 1] = w;
                }
            });
        }
        */


        /// <summary>
        /// Normalizes half-edge weights such that weights of outgoing edges around each vertex sum to 1.
        /// Note that this breaks weight symmetry between half-edge pairs.
        /// </summary>
        /// <param name="halfEdgeWeights"></param>
        public void NormalizeHalfEdgeWeights(IList<double> halfEdgeWeights)
        {
            SizeCheck(halfEdgeWeights);

            HeVertexList verts = Mesh.Vertices;
            Parallel.ForEach(Partitioner.Create(0, verts.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HeVertex v = verts[i];
                    if (v.IsUnused) continue;

                    double sum = 0.0;

                    foreach (HalfEdge e in v.OutgoingEdges)
                        sum += halfEdgeWeights[e.Index];

                    if (sum > 0.0)
                    {
                        sum = 1.0 / sum;
                        foreach (HalfEdge e in v.OutgoingEdges)
                            halfEdgeWeights[e.Index] *= sum;
                    }
                }
            });
        }


        /// <summary>
        /// Returns the span vector for each half-edge in the mesh.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public Vec3d[] GetHalfEdgeVectors(bool unitize)
        {
            Vec3d[] result = new Vec3d[Count];
            UpdateHalfEdgeVectors(unitize, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public void UpdateHalfEdgeVectors(bool unitize, IList<Vec3d> result)
        {
            SizeCheck(result);

            Parallel.ForEach(Partitioner.Create(0, Count >> 1), range =>
            {
                int i0 = range.Item1 << 1;
                int i1 = range.Item2 << 1;

                for (int i = i0; i < i1; i += 2)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;

                    Vec3d v = e.Span;
                    if (unitize) v.Unitize();
                    result[i] = v;
                    result[i + 1] = -v;
                }
            });
        }


        /// <summary>
        /// Returns a normal for each half-edge in the mesh.
        /// These are calculated as the cross product of each edge and its previous.
        /// Note that these are used in the calculation of both vertex and face normals.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public Vec3d[] GetHalfEdgeNormals(bool unitize)
        {
            Vec3d[] result = new Vec3d[Count];
            UpdateHalfEdgeNormals(unitize, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public void UpdateHalfEdgeNormals(bool unitize, IList<Vec3d> result)
        {
            SizeCheck(result);

            if (unitize)
                UpdateHalfEdgeUnitNormals(result);
            else
                UpdateHalfEdgeNormals(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private void UpdateHalfEdgeNormals(IList<Vec3d> result)
        {
            Parallel.ForEach(Partitioner.Create(0, Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;

                    result[i] = Vec3d.Cross(e.Prev.Span, e.Span);
                }
            });
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private void UpdateHalfEdgeUnitNormals(IList<Vec3d> result)
        {
            Parallel.ForEach(Partitioner.Create(0, Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;

                    Vec3d v = Vec3d.Cross(e.Prev.Span, e.Span);
                    v.Unitize();
                    result[i] = v;
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="edgeLengths"></param>
        /// <param name="unitize"></param>
        /// <returns></returns>
        public Vec3d[] GetHalfEdgeBisectors(bool unitize)
        {
            Vec3d[] result = new Vec3d[Count];
            UpdateHalfEdgeBisectors(unitize, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="edgeLengths"></param>
        /// <param name="unitize"></param>
        /// <returns></returns>
        public void UpdateHalfEdgeBisectors(bool unitize, IList<Vec3d> result)
        {
            SizeCheck(result);

            if (unitize)
                UpdateHalfEdgeUnitBisectors(result);
            else
                UpdateHalfEdgeBisectors(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        private void UpdateHalfEdgeBisectors(IList<Vec3d> result)
        {

            Parallel.ForEach(Partitioner.Create(0, Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;

                    Vec3d v0 = e.Span;
                    Vec3d v1 = e.Prev.Span;

                    v0 = (v0 / v0.Length - v1 / v1.Length) * 0.5;
                    result[i] = v0;
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        private void UpdateHalfEdgeUnitBisectors(IList<Vec3d> result)
        {

            Parallel.ForEach(Partitioner.Create(0, Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    HalfEdge e = this[i];
                    if (e.IsUnused) continue;

                    Vec3d v0 = e.Span;
                    Vec3d v1 = e.Prev.Span;

                    v0 = (v0 / v0.Length - v1 / v1.Length) * 0.5;
                    v0.Unitize();
                    result[i] = v0;
                }
            });
        }

        #endregion


        #region Euler Operators

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        public void Remove(HalfEdge edge)
        {
            Mesh.Faces.MergeFaces(edge);
        }


        /// <summary>
        /// Removes a pair of edges from the mesh.
        /// Note that this method does not update any face-edge references.
        /// </summary>
        /// <param name="edge"></param>
        internal void RemovePair(HalfEdge edge)
        {
            HalfEdge e0 = edge;
            HalfEdge e1 = e0.Twin;

            if (e0.IsFromDeg1)
                e0.Start.MakeUnused(); // flag degree 1 vertex as unused
            else
            {
                HalfEdge.MakeConsecutive(e0.Prev, e1.Next); // update edge-edge refs
                if (e0.IsOutgoing) e1.Next.MakeOutgoing(); // update vertex-edge ref if necessary
            }

            if (e1.IsFromDeg1)
                e1.Start.MakeUnused(); // flag degree 1 vertex as unused
            else
            {
                HalfEdge.MakeConsecutive(e1.Prev, e0.Next); // update edge-edge refs
                if (e1.IsOutgoing) e0.Next.MakeOutgoing(); // update vertex-edge ref if necessary
            }

            // flag elements for removal
            e0.MakeUnused();
            e1.MakeUnused();
        }


        /// <summary>
        /// Splits the given edge by adding a new vertex in the middle.
        /// Returns the new edge outgoing from the new vertex.
        /// </summary>
        public HalfEdge SplitEdge(HalfEdge edge)
        {
            Validate(edge);

            HalfEdge e0 = edge;
            HalfEdge e1 = e0.Twin;
        
            HeVertex v0 = e0.Start;
            HeVertex v1 = e1.Start;
            //HeVertex v2 = Mesh.Vertices.Add(v0.Position);
            HeVertex v2 = Mesh.Vertices.Add((v0.Position + v1.Position) * 0.5);

            HalfEdge e2 = Mesh.HalfEdges.AddPair(v2, v1);
            HalfEdge e3 = e2.Twin;

            // update edge-vertex references
            e1.Start = v2;
    
            // update edge-face references
            e2.Face = e0.Face;
            e3.Face = e1.Face;

            // update vertex-edge references if necessary
            if (v1.Outgoing == e1)
            {
                v1.Outgoing = e3;
                v2.Outgoing = e1;
            }
            else
            {
                v2.Outgoing = e2;
            }

            // update edge-edge references
            HalfEdge.MakeConsecutive(e2, e0.Next);
            HalfEdge.MakeConsecutive(e1.Prev, e3);
            HalfEdge.MakeConsecutive(e0, e2);
            HalfEdge.MakeConsecutive(e3, e1);

            return e2;
        }


        /// <summary>
        /// Splits an edge by adding a new vertex in the middle. 
        /// Faces adjacent to the given edge are also split at the new vertex.
        /// Returns the new edge outgoing from the new vertex or null on failure.
        /// Assumes triangle mesh.
        /// </summary>
        public HalfEdge SplitEdgeFace(HalfEdge edge)
        {
            HalfEdge e0 = SplitEdge(edge);
            HalfEdge e1 = e0.Twin.Next;

            HeFaceList faces = Mesh.Faces;
            faces.SplitFace(e0, e0.Next.Next);
            faces.SplitFace(e1, e1.Next.Next);

            return e0;
        }


        /// <summary>
        /// Collapses the given half edge by merging the vertices at either end.
        /// The start vertex is retained and the end vertex is flagged as unused.
        /// Return true on success.
        /// </summary>
        /// <param name="edge"></param>
        public bool CollapseEdge(HalfEdge edge)
        {
            Validate(edge);

            HalfEdge e0 = edge;
            HalfEdge e1 = e0.Twin;

            HeVertex v0 = e0.Start; // to be removed
            HeVertex v1 = e1.Start;

            HeFace f0 = e0.Face;
            HeFace f1 = e1.Face;

            /*
            // avoids creation of non-manifold vertices
            if (!e0.IsBoundary && (v0.IsBoundary && v1.IsBoundary))
                return false;
            */

            // avoids creation of non-manifold edges
            int allow = 0; // the number of common neighbours allowed between v0 and v1
            if (f0 != null && f0.IsTri) allow++;
            if (f1 != null && f1.IsTri) allow++;
            if (Mesh.Vertices.CountCommonNeighbours(v0, v1) > allow)
                return false;

            // update edge-vertex refs of all edges emanating from the vertex which is being removed
            foreach (HalfEdge e in v0.OutgoingEdges) e.Start = v1;

            // update vertex-edge ref for the remaining vertex if necessary
            if (e1.IsOutgoing) e1.Next.MakeOutgoing();

            // update edge-edge refs
            HalfEdge.MakeConsecutive(e0.Prev, e0.Next);
            HalfEdge.MakeConsecutive(e1.Prev, e1.Next);

            // update face-edge refs if necessary and deal with potential collapse by merging
            if (f0 != null)
            {
                if (e0.IsFirst) e0.Next.MakeFirst();
                if (!f0.IsValid) Mesh.Faces.MergeFaces(f0.First);
            }

            if (f1 != null)
            {
                if (e1.IsFirst) e1.Next.MakeFirst();
                if (!f1.IsValid) Mesh.Faces.MergeFaces(f1.First);
            }

            // flag elements for removal
            e0.MakeUnused();
            e1.MakeUnused();
            v0.MakeUnused();
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool SpinEdge(HalfEdge edge)
        {
            Validate(edge);

            // edge must be adjacent to 2 faces
            if (edge.IsBoundary) return false;

            HalfEdge e0 = edge;
            HalfEdge e1 = e0.Twin;
            HalfEdge e2 = e0.Next;
            HalfEdge e3 = e1.Next;

            // don't allow for the creation of valence 1 vertices
            if (e0.IsFromDeg2 || e1.IsFromDeg2) return false;

            // update vertex-edge refs if necessary
            if (e0.IsOutgoing) e0.Start.Outgoing = e3;
            if (e1.IsOutgoing) e1.Start.Outgoing = e2;

            // update edge-vertex refs
            e0.Start = e3.End;
            e1.Start = e2.End;

            HeFace f0 = e0.Face;
            HeFace f1 = e1.Face;

            // update face-edge refs if necessary
            if (e2.IsFirst) f0.First = e2.Next;
            if (e3.IsFirst) f1.First = e3.Next;

            // update edge-face refs
            e2.Face = f1;
            e3.Face = f0;

            // update edge-edge refs
            HalfEdge.MakeConsecutive(e0, e2.Next);
            HalfEdge.MakeConsecutive(e1, e3.Next);
            HalfEdge.MakeConsecutive(e1.Prev, e2);
            HalfEdge.MakeConsecutive(e0.Prev, e3);
            HalfEdge.MakeConsecutive(e2, e1);
            HalfEdge.MakeConsecutive(e3, e0);
            return true;
        }


        /*
        /// <summary>
        /// OBSOLETE degree 1 verts are now taken care of in HeEdgeList.RemovePair
        /// 
        /// removes an open chain of degree 1 vertices
        /// note that this method does not update face-edge references
        /// </summary>
        /// <param name="edge"></param>
        internal HeEdge Prune(HeEdge edge)
        {
            HeEdge e0 = edge;
            HeEdge e1 = e0.Twin;

            // advance until edge pair are no longer twins
            do
            {
                // stop if found an isolated segment
                if (e0.Next == e1)
                {
                    e0.Start.MakeUnused();
                    e1.Start.MakeUnused();
                    e0.MakeUnused();
                    e1.MakeUnused();
                    return null;
                }

                // flag elements for removal
                e0.Start.MakeUnused();
                e0.MakeUnused();
                e1.MakeUnused();

                // advance to next edge pair
                e0 = e0.Next;
                e1 = e1.Previous;
            } while (e0.Twin == e1);

            // update vertex-edge refs if necessary
            HeVertex v0 = e0.Start;
            if (v0.Outgoing.IsUnused) v0.Outgoing = e0;
    
            //update edge-edge refs
            HeEdge.MakeConsecutive(e1, e0);
            return e0;
        }
        */

        #endregion

    }
}