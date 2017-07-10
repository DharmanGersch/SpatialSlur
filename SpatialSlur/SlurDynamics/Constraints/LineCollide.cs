﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SpatialSlur.SlurCore;
using SpatialSlur.SlurData;

/*
 * Notes
 */

namespace SpatialSlur.SlurDynamics.Constraints
{
    using H = ParticleHandle;

    /// <summary>
    /// 
    /// </summary>
    public class LineCollide<P> : DynamicConstraint<P, H>
        where P : IParticle
    {
        private SpatialGrid3d<H> _grid;
        private double _radius;


        /// <summary>
        /// 
        /// </summary>
        public double Radius
        {
            get { return _radius; }
            set
            {
                if (value < 0.0)
                    throw new ArgumentOutOfRangeException("The value can not be negative");

                _radius = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="weight"></param>
        public LineCollide(int capacity, double weight = 1.0)
            : base(capacity, weight)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="handles"></param>
        /// <param name="weight"></param>
        public LineCollide(IEnumerable<H> handles, double weight = 1.0)
            : base(handles, weight)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="particles"></param>
        public override void Calculate(IReadOnlyList<P> particles)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}