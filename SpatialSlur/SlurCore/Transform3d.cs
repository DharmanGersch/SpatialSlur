﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Notes
 */ 

namespace SpatialSlur.SlurCore
{
    /// <summary>
    /// Represents an angle preserving affine transformation in 3 dimensions
    /// </summary>
    public struct Transform3d
    {
        #region Static

        /// <summary></summary>
        public static readonly Transform3d Identity = new Transform3d(new Vec3d(1.0), Rotation3d.Identity, Vec3d.Zero);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        public static implicit operator Transform3d(Transform2d transform)
        {
            return new Transform3d(transform.Scale, transform.Rotation, transform.Translation);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotation"></param>
        public static implicit operator Transform3d(Rotation3d rotation)
        {
            return new Transform3d(new Vec3d(1.0), rotation, Vec3d.Zero);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="orient"></param>
        public static implicit operator Transform3d(Orient3d orient)
        {
            return new Transform3d(new Vec3d(1.0), orient.Rotation, orient.Translation);
        }


        /// <summary>
        /// Applies the given transformation to the given point.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vec3d operator *(Transform3d transform, Vec3d point)
        {
            return transform.Apply(point);
        }


        /// <summary>
        /// Combines the two given transformations.
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static Transform3d operator *(Transform3d t0, Transform3d t1)
        {
            return t0.Apply(t1);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vec3d Multiply(ref Transform3d transform, Vec3d point)
        {
            return transform.Apply(point);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        public static Transform3d Multiply(ref Transform3d t0, ref Transform3d t1)
        {
            return t0.Apply(t1);
        }


        /// <summary>
        /// Creates relative transformation from t0 to t1.
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static Transform3d CreateRelative(Transform3d t0, Transform3d t1)
        {
            return CreateRelative(ref t0, ref t1);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static Transform3d CreateRelative(ref Transform3d t0, ref Transform3d t1)
        {
            return t1.Apply(t0.Inverse);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static Transform3d CreateReflection(Vec3d point, Vec3d normal)
        {
            throw new NotImplementedException();
        }

        #endregion


        /// <summary></summary>
        public Rotation3d Rotation;
        /// <summary></summary>
        public Vec3d Translation;
        /// <summary></summary>
        public Vec3d Scale;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="translation"></param>
        public Transform3d(Vec3d scale, Rotation3d rotation, Vec3d translation)
        {
            Scale = scale;
            Rotation = rotation;
            Translation = translation;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="orientation"></param>
        public Transform3d(Vec3d scale, Orient3d orientation)
        {
            Scale = scale;
            Rotation = orientation.Rotation;
            Translation = orientation.Translation;
        }


        /// <summary>
        /// 
        /// </summary>
        public Transform3d Inverse
        {
            get
            {
                var result = this;
                result.Invert();
                return result;
            }
        }


        /// <summary>
        /// Return false if the rotation is undefined.
        /// </summary>
        bool IsValid
        {
            get { return Rotation.IsValid; }
        }


        /// <summary>
        /// Inverts this transformation in place.
        /// </summary>
        public void Invert()
        {
            Scale = 1.0 / Scale;
            Rotation.Invert();
            Translation = Rotation.Apply(-Translation) * Scale;
        }


        /// <summary>
        /// Applies this transformation to the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vec3d Apply(Vec3d point)
        {
            return Rotation.Apply(point * Scale) + Translation;
        }


        /// <summary>
        /// Applies this transformation to the given transformation.
        /// </summary>
        /// <param name="other"></param>
        public Transform3d Apply(Transform3d other)
        {
            other.Rotation = Rotation.Apply(other.Rotation);
            other.Translation = Apply(other.Translation);
            other.Scale *= Scale;
            return other;
        }


        /// <summary>
        /// Applies the inverse of this transformation to the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vec3d ApplyInverse(Vec3d point)
        {
            return Rotation.ApplyInverse(point - Translation) / Scale;
        }


        /// <summary>
        /// Applies the inverse of this transformation to the given transformation.
        /// </summary>
        /// <param name="other"></param>
        public Transform3d ApplyInverse(Transform3d other)
        {
            other.Rotation = Rotation.ApplyInverse(other.Rotation);
            other.Translation = ApplyInverse(other.Translation);
            other.Scale /= Scale;
            return other;
        }
    }
}
