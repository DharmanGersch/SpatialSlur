﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Notes
 */ 

namespace SpatialSlur.SlurDynamics
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConstraint
    {
        /// <summary>
        /// 
        /// </summary>
        double Weight { get; set; }
        

        /// <summary>
        /// Returns true if this constraint acts on the orientation of particles.
        /// </summary>
        /// <returns></returns>
        bool AppliesRotation { get; }


        /// <summary>
        /// This method is responsible for calculating all deltas and weights associated with the constraint.
        /// </summary>
        /// <param name="bodies"></param>
        void Calculate(IReadOnlyList<IBody> bodies);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bodies"></param>
        void Apply(IReadOnlyList<IBody> bodies);


        /// <summary>
        /// 
        /// </summary>
        void SetHandles(IEnumerable<int> indices);
    }
}
