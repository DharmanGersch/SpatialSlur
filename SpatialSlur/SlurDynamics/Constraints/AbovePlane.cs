﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpatialSlur.SlurCore;

/*
 * Notes
 */ 

namespace SpatialSlur.SlurDynamics
{
    using H = ParticleHandle;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class AbovePlane : MultiParticleConstraint<H>
    {
        /// <summary></summary>
        public Vec3d Origin;
        /// <summary></summary>
        public Vec3d Normal;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="normal"></param>
        /// <param name="weight"></param>
        public AbovePlane(Vec3d origin, Vec3d normal, double weight = 1.0)
            : base(weight)
        {
            Origin = origin;
            Normal = normal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="normal"></param>
        /// <param name="capacity"></param>
        /// <param name="weight"></param>
        public AbovePlane(Vec3d origin, Vec3d normal, int capacity, double weight = 1.0)
            : base(capacity, weight)
        {
            Origin = origin;
            Normal = normal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="origin"></param>
        /// <param name="normal"></param>
        /// <param name="weight"></param>
        public AbovePlane(IEnumerable<int> indices, Vec3d origin, Vec3d normal, double weight = 1.0)
            : base(weight)
        {
            Handles.AddRange(indices.Select(i => new H(i)));
            Origin = origin;
            Normal = normal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="particles"></param>
        public override sealed void Calculate(IReadOnlyList<IBody> particles)
        {
            foreach (var h in Handles)
            {
                double d = Vec3d.Dot(Origin - particles[h].Position, Normal);

                if (d <= 0.0)
                {
                    h.Delta = Vec3d.Zero;
                    continue;
                }

                h.Delta = (d / Normal.SquareLength * Normal);
                h.Weight = Weight;
            }
        }


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="particles"></param>
        public override sealed void Calculate(IReadOnlyList<IBody> particles)
        {
            foreach(var h in Handles)
            {
                double d = (Origin - particles[h].Position) * Normal;

                if (d > 0.0)
                {
                    h.Delta = (d / Normal.SquareLength * Normal);
                    h.Weight = Weight;
                }
                else
                {
                    h.Weight = 0.0;
                }
            }
        }
        */
    }
}
