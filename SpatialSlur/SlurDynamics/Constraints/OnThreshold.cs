﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpatialSlur.SlurCore;
using SpatialSlur.SlurField;

/*
 * Notes
 */ 

namespace SpatialSlur.SlurDynamics
{
    using H = ParticleHandle;

    /// <summary>
    /// 
    /// </summary>
    public class OnThreshold : MultiParticleConstraint<H>
    {
        /// <summary></summary>
        public double Threshold;

        private GridScalarField3d _field;
        private GridVectorField3d _gradient;


        /// <summary>
        /// 
        /// </summary>
        public GridScalarField3d Field
        {
            get { return _field; }
            set { _field = value ?? throw new ArgumentNullException(); }
        }


        /// <summary>
        /// This must be called after any changes to the field.
        /// </summary>
        public void UpdateGradient()
        {
            if (_gradient == null || _gradient.Count != _field.Count)
                _gradient = new GridVectorField3d(_field);

            _field.GetGradient(_gradient);
        }

   
        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="threshold"></param>
        /// <param name="weight"></param>
        public OnThreshold(GridScalarField3d field, double threshold, double weight = 1.0)
            :base(weight)
        {
            Field = field;
            Threshold = threshold;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="threshold"></param>
        /// <param name="capacity"></param>
        /// <param name="weight"></param>
        public OnThreshold(GridScalarField3d field, double threshold, int capacity, double weight = 1.0)
            : base(capacity, weight)
        {
            Field = field;
            Threshold = threshold;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="field"></param>
        /// <param name="threshold"></param>
        /// <param name="weight"></param>
        public OnThreshold(IEnumerable<int> indices, GridScalarField3d field, double threshold, double weight = 1.0)
            : base(weight)
        {
            Handles.AddRange(indices.Select(i => new H(i)));
            Field = field;
            Threshold = threshold;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="particles"></param>
        public override sealed void Calculate(IReadOnlyList<IBody> particles)
        {
            var gp = new GridPoint3d();

            for (int i = 0; i < Handles.Count; i++)
            {
                var h = Handles[i];
                var p = particles[h].Position;

                _field.GridPointAt(p, gp);
                var t = _field.ValueAt(gp);
                var g = _gradient.ValueAt(gp).Direction;

                h.Delta = g * (Threshold - t);
                h.Weight = Weight;
            }
        }
    }
}
