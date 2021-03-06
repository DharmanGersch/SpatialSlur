﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpatialSlur.SlurCore;
using SpatialSlur.SlurDynamics;

using Rhino.Geometry;

/*
 * Notes
 */

namespace SpatialSlur.SlurRhino.Constraints
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class OnCurve : OnTarget<Curve>
    {
        #region Static

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static Vec3d ClosestPoint(Curve curve, Vec3d point)
        {
            curve.ClosestPoint(point.ToPoint3d(), out double t);
            return curve.PointAt(t).ToVec3d();
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="parallel"></param>
        /// <param name="capacity"></param>
        /// <param name="weight"></param>
        public OnCurve(Curve curve, bool parallel, int capacity, double weight = 1.0)
            : base(curve, ClosestPoint, parallel, capacity, weight)
        {
        }
    }
}
