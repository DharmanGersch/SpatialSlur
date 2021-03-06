﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class PlanarNgon : MultiParticleConstraint<H>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="weight"></param>
        public PlanarNgon(double weight = 1.0)
         : base(weight)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="weight"></param>
        public PlanarNgon(int capacity, double weight = 1.0)
            :base(capacity,weight)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="weight"></param>
        public PlanarNgon(IEnumerable<int> indices, double weight = 1.0)
            : base(weight)
        {
            Handles.AddRange(indices.Select(i => new H(i)));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="particles"></param>
        public override sealed void Calculate(IReadOnlyList<IBody> particles)
        {
            int n = Handles.Count;

            // tri case
            if (n < 4)
            {
                foreach (var h in Handles) h.Weight = 0.0;
                return;
            }

            // quad case
            if (n == 4)
            {
                var h0 = Handles[0];
                var h1 = Handles[1];
                var h2 = Handles[2];
                var h3 = Handles[3];

                var d = GeometryUtil.LineLineShortestVector(
                    particles[h0].Position,
                    particles[h2].Position,
                    particles[h1].Position,
                    particles[h3].Position) * 0.5;

                h0.Delta = h2.Delta = d;
                h1.Delta = h3.Delta = -d;
                h0.Weight = h1.Weight = h2.Weight = h3.Weight = Weight;
                return;
            }

            // general case
            foreach (var h in Handles)
            {
                h.Delta = new Vec3d();
                h.Weight = Weight;
            }

            for (int i = 0; i < n; i++)
            {
                var h0 = Handles[i];
                var h1 = Handles[(i + 1) % n];
                var h2 = Handles[(i + 2) % n];
                var h3 = Handles[(i + 3) % n];

                var d = GeometryUtil.LineLineShortestVector(
                    particles[h0].Position,
                    particles[h2].Position,
                    particles[h1].Position,
                    particles[h3].Position) * 0.125; // 0.5 / 4 (4 deltas applied per index)

                h0.Delta += d;
                h2.Delta += d;
                h1.Delta -= d;
                h3.Delta -= d;
            }
        }
    }
}
