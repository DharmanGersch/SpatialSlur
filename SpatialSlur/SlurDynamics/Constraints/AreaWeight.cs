﻿using System;
using System.Collections.Generic;
using SpatialSlur.SlurCore;

/*
 * Notes
 */

namespace SpatialSlur.SlurDynamics
{
    using H = ParticleHandle;

    /// <summary>
    /// Applies a force proportional to the area of the triangle defined by 3 particles.
    /// </summary>
    [Serializable]
    public class AreaWeight : ParticleConstraint<H>
    {
        private H _h0 = new H();
        private H _h1 = new H();
        private H _h2 = new H();

        /// <summary>Describes the direction and magnitude of the applied weight</summary>
        public Vec3d Vector;
        private double _massPerArea = 1.0;


        /// <summary>
        /// 
        /// </summary>
        public H Vertex0
        {
            get { return _h0; }
        }


        /// <summary>
        /// 
        /// </summary>
        public H Vertex1
        {
            get { return _h1; }
        }


        /// <summary>
        /// 
        /// </summary>
        public H Vertex2
        {
            get { return _h2; }
        }


        /// <summary>
        /// 
        /// </summary>
        public override sealed IEnumerable<H> Handles
        {
            get
            {
                yield return _h0;
                yield return _h1;
                yield return _h2;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public double MassPerArea
        {
            get { return _massPerArea; }
            set
            {
                if (value <= 0.0)
                    throw new ArgumentException("Mass must be greater than zero.");

                _massPerArea = value;
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertex0"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vector"></param>
        /// <param name="massPerArea"></param>
        /// <param name="weight"></param>
        public AreaWeight(int vertex0, int vertex1, int vertex2, Vec3d vector, double massPerArea = 1.0, double weight = 1.0)
        {
            SetHandles(vertex0, vertex1, vertex2);
            Vector = vector;
            MassPerArea = massPerArea;
            Weight = weight;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="particles"></param>
        public override sealed void Calculate(IReadOnlyList<IBody> particles)
        {
            Vec3d p0 = particles[_h0].Position;
            Vec3d p1 = particles[_h1].Position;
            Vec3d p2 = particles[_h2].Position;

            const double inv6 = 1.0 / 6.0;
            _h0.Delta = _h1.Delta = _h2.Delta = Vector * (Vec3d.Cross(p1 - p0, p2 - p1).Length * _massPerArea * inv6);
            _h0.Weight = _h1.Weight = _h2.Weight = Weight;
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertex0"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        public void SetHandles(int vertex0, int vertex1, int vertex2)
        {
            _h0.Index = vertex0;
            _h1.Index = vertex1;
            _h2.Index = vertex2;
        }
    }
}
