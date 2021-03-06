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
    /// <summary>
    /// Base class for position-only constraints.
    /// </summary>
    [Serializable]
    public abstract class ParticleConstraint<H> : IConstraint
        where H : ParticleHandle
    {
        private double _weight;


        /// <summary>
        /// All handles used by this constraint.
        /// </summary>
        public abstract IEnumerable<H> Handles { get; }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public double Weight
        {
            get { return _weight; }
            set
            {
                if (value < 0.0)
                    throw new ArgumentOutOfRangeException("Weight cannot be negative.");

                _weight = value;
            }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public bool AppliesRotation
        {
            get { return false; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="particles"></param>
        public abstract void Calculate(IReadOnlyList<IBody> particles);


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="particles"></param>
        public void Apply(IReadOnlyList<IBody> particles)
        {
            foreach (var h in Handles)
                particles[h].ApplyMove(h.Delta, h.Weight);
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indices"></param>
        public void SetHandles(IEnumerable<int> indices)
        {
            var itr = indices.GetEnumerator();

            foreach (var h in Handles)
            {
                itr.MoveNext();
                h.Index = itr.Current;
            }
        }
    }
}
