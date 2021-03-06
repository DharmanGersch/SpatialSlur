﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpatialSlur.SlurData;

/*
 * Notes
 */ 

namespace SpatialSlur.SlurCore
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public struct Vec3d
    {
        #region Static

        /// <summary></summary>
        public static readonly Vec3d Zero = new Vec3d();
        /// <summary></summary>
        public static readonly Vec3d UnitX = new Vec3d(1.0, 0.0, 0.0);
        /// <summary></summary>
        public static readonly Vec3d UnitY = new Vec3d(0.0, 1.0, 0.0);
        /// <summary></summary>
        public static readonly Vec3d UnitZ = new Vec3d(0.0, 0.0, 1.0);


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuple"></param>
        public static implicit operator Vec3d((double, double, double) tuple)
        {
            return new Vec3d(tuple.Item1, tuple.Item2, tuple.Item3);
        }
        */


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Vec3d(Vec2d vector)
        {
            return new Vec3d(vector.X, vector.Y, 0.0);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Vec3d(Vec4d vector)
        {
            return new Vec3d(vector.X, vector.Y, vector.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d operator +(Vec3d v0, Vec3d v1)
        {
            v0.X += v1.X;
            v0.Y += v1.Y;
            v0.Z += v1.Z;
            return v0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d operator -(Vec3d v0, Vec3d v1)
        {
            v0.X -= v1.X;
            v0.Y -= v1.Y;
            v0.Z -= v1.Z;
            return v0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vec3d operator -(Vec3d vector)
        {
            vector.X = -vector.X;
            vector.Y = -vector.Y;
            vector.Z = -vector.Z;
            return vector;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Vec3d operator *(Vec3d vector, double scalar)
        {
            vector.X *= scalar;
            vector.Y *= scalar;
            vector.Z *= scalar;
            return vector;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Vec3d operator *(double scalar, Vec3d vector)
        {
            vector.X *= scalar;
            vector.Y *= scalar;
            vector.Z *= scalar;
            return vector;
        }


        /// <summary>
        /// Component-wise multiplication.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d operator *(Vec3d v0, Vec3d v1)
        {
            v0.X *= v1.X;
            v0.Y *= v1.Y;
            v0.Z *= v1.Z;
            return v0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Vec3d operator /(Vec3d vector, double scalar)
        {
            scalar = 1.0 / scalar;
            vector.X *= scalar;
            vector.Y *= scalar;
            vector.Z *= scalar;
            return vector;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Vec3d operator /(double scalar, Vec3d vector)
        {
            vector.X = scalar / vector.X;
            vector.Y = scalar / vector.Y;
            vector.Z = scalar / vector.Z;
            return vector;
        }


        /// <summary>
        /// Component-wise division.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d operator /(Vec3d v0, Vec3d v1)
        {
            v0.X /= v1.X;
            v0.Y /= v1.Y;
            v0.Z /= v1.Z;
            return v0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d Max(Vec3d v0, Vec3d v1)
        {
            return new Vec3d(Math.Max(v0.X, v1.X), Math.Max(v0.Y, v1.Y), Math.Max(v0.Z, v1.Z));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d Min(Vec3d v0, Vec3d v1)
        {
            return new Vec3d(Math.Min(v0.X, v1.X), Math.Min(v0.Y, v1.Y), Math.Min(v0.Z, v1.Z));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vec3d Abs(Vec3d vector)
        {
            return new Vec3d(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static double Dot(Vec3d v0, Vec3d v1)
        {
            return v0.X * v1.X + v0.Y * v1.Y + v0.Z * v1.Z;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static double AbsDot(Vec3d v0, Vec3d v1)
        {
            return Math.Abs(v0.X * v1.X) + Math.Abs(v0.Y * v1.Y) + Math.Abs(v0.Z * v1.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d Cross(Vec3d v0, Vec3d v1)
        {
            return new Vec3d(
                v0.Y * v1.Z - v0.Z * v1.Y,
                v0.Z * v1.X - v0.X * v1.Z,
                v0.X * v1.Y - v0.Y * v1.X);
        }


        /// <summary>
        /// Returns the angle between two vectors.
        /// If either vector is zero length, Double.NaN is returned.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static double Angle(Vec3d v0, Vec3d v1)
        {
            double d = v0.Length * v1.Length;

            if (d > 0.0)
                return Math.Acos(SlurMath.Clamp(Dot(v0, v1) / d, -1.0, 1.0)); // clamp dot product to remove noise

            return double.NaN;
        }


        /// <summary>
        /// Returns the cotangent of the angle between 2 vectors as per http://www.cs.columbia.edu/~keenan/Projects/Other/TriangleAreasCheatSheet.pdf.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static double Cotangent(Vec3d v0, Vec3d v1)
        {
            return Dot(v0, v1) / Cross(v0, v1).Length;
        }


        /// <summary>
        /// Returns the projection of v0 onto v1.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d Project(Vec3d v0, Vec3d v1)
        {
            return Dot(v0, v1) / v1.SquareLength * v1;
        }


        /// <summary>
        /// Returns the rejection of v0 onto v1.
        /// This is the perpendicular component of v0 with respect to v1.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d Reject(Vec3d v0, Vec3d v1)
        {
            return v0 - Project(v0, v1);
        }


        /// <summary>
        /// Returns the reflection of v0 about v1.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d Reflect(Vec3d v0, Vec3d v1)
        {
            //return Project(v0, v1) * 2.0 - v0;
            return v1 * (Dot(v0, v1) / v1.SquareLength * 2.0) - v0;
        }


        /// <summary>
        /// Returns a vector parallel to v0 whos projection onto v1 equals v1
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vec3d MatchProjection(Vec3d v0, Vec3d v1)
        {
            return v1.SquareLength / Dot(v0, v1) * v0;
        }


        /// <summary>
        /// Returns a vector parallel to v0 whose projection onto v2 equals the projection of v1 onto v2
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vec3d MatchProjection(Vec3d v0, Vec3d v1, Vec3d v2)
        {
            return Dot(v1, v2) / Dot(v0, v2) * v0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vec3d Lerp(Vec3d v0, Vec3d v1, double t)
        {
            v0.X += (v1.X - v0.X) * t;
            v0.Y += (v1.Y - v0.Y) * t;
            v0.Z += (v1.Z - v0.Z) * t;
            return v0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vec3d Slerp(Vec3d v0, Vec3d v1, double t)
        {
            return Slerp(v0, v1, Angle(v0, v1), t);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="theta"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vec3d Slerp(Vec3d v0, Vec3d v1, double theta, double t)
        {
            double st = 1.0 / Math.Sin(theta);
            return v0 * (Math.Sin((1.0 - t) * theta) * st) + v1 * (Math.Sin(t * theta) * st);
        }

        #endregion


        /// <summary></summary>
        public double X;
        /// <summary></summary>
        public double Y;
        /// <summary></summary>
        public double Z;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyz"></param>
        public Vec3d(double xyz)
        {
            X = Y = Z = xyz;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vec3d(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="z"></param>
        public Vec3d(Vec2d other, double z)
        {
            X = other.X;
            Y = other.Y;
            this.Z = z;
        }


        /// <summary>
        /// Returns a unit length copy of this vector.
        /// Returns the zero vector if this vector is zero length.
        /// </summary>
        /// <returns></returns>
        public Vec3d Direction
        {
            get
            {
                double d = SquareLength;
                return (d > 0.0) ? this / Math.Sqrt(d) : Zero;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Length
        {
            get { return Math.Sqrt(SquareLength); }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double SquareLength
        {
            get { return X * X + Y * Y + Z * Z; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double ManhattanLength
        {
            get { return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z); }
        }


        /// <summary>
        /// 
        /// </summary>
        public (double, double, double) Components
        {
            get { return (X, Y, Z); }
        }


        /// <summary>
        /// Returns the sum of components.
        /// </summary>
        public double ComponentSum
        {
            get { return X + Y + Z; }
        }


        /// <summary>
        /// Returns the mean of components.
        /// </summary>
        public double ComponentMean
        {
            get
            {
                const double inv3 = 1.0 / 3.0;
                return (X + Y + Z) * inv3;
            }
        }


        /// <summary>
        /// Returns the largest component in the vector.
        /// </summary>
        /// <returns></returns>
        public double ComponentMax
        {
            get { return Math.Max(X, Math.Max(Y, Z)); }
        }


        /// <summary>
        /// Returns the smallest component in the vector.
        /// </summary>
        /// <returns></returns>
        public double ComponentMin
        {
            get { return Math.Min(X, Math.Min(Y, Z)); }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool IsZero(double tolerance)
        {
            return (Math.Abs(X) < tolerance) && (Math.Abs(Y) < tolerance) && (Math.Abs(Z) < tolerance);
        }


        /// <summary>
        /// 
        /// </summary>
        public bool IsUnit(double tolerance)
        {
            return Math.Abs(SquareLength - 1.0) < tolerance;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0},{1},{2})", X, Y, Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyz"></param>
        public void Set(double xyz)
        {
            X = Y = Z = xyz;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }


        /// <summary>
        /// Converts from euclidean to spherical coordiantes.
        /// (x,y,z) = (radius, azimuth, polar)
        /// </summary>
        /// <returns></returns>
        public Vec3d ToSpherical()
        {
            double r = this.Length;
            return new Vec3d(r, Math.Atan(Y / X), Math.Acos(Z / r));
        }


        /// <summary>
        /// Converts from spherical to euclidean coordiantes.
        /// (x,y,z) = (radius, azimuth, polar)
        /// </summary>
        /// <returns></returns>
        public Vec3d ToEuclidean()
        {
            double rxy = Math.Sin(Z) * X * X;
            return new Vec3d(Math.Cos(Y) * rxy, Math.Sin(Y) * rxy, Math.Cos(Z) * X);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public bool ApproxEquals(Vec3d other, double tolerance)
        {
            return (Math.Abs(other.X - X) < tolerance) && (Math.Abs(other.Y - Y) < tolerance) && (Math.Abs(other.Z - Z) < tolerance);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public bool ApproxEquals(Vec3d other, Vec3d tolerance)
        {
            return (Math.Abs(other.X - X) < tolerance.X) && (Math.Abs(other.Y - Y) < tolerance.Y) && (Math.Abs(other.Z - Z) < tolerance.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double DistanceTo(Vec3d other)
        {
            other.X -= X;
            other.Y -= Y;
            other.Z -= Z;
            return other.Length;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double SquareDistanceTo(Vec3d other)
        {
            other.X -= X;
            other.Y -= Y;
            other.Z -= Z;
            return other.SquareLength;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double ManhattanDistanceTo(Vec3d other)
        {
            other.X -= X;
            other.Y -= Y;
            other.Z -= Z;
            return other.ManhattanLength;
        }


        /// <summary>
        /// Unitizes the vector in place.
        /// Returns false if the vector is zero length.
        /// </summary>
        public bool Unitize()
        {
            double d = SquareLength;

            if (d > 0.0)
            {
                d = 1.0 / Math.Sqrt(d);
                X *= d;
                Y *= d;
                Z *= d;
                return true;
            }

            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public Vec3d LerpTo(Vec3d other, double factor)
        {
            return new Vec3d(
                X + (other.X - X) * factor,
                Y + (other.Y - Y) * factor,
                Z + (other.Z - Z) * factor);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double[] ToArray()
        {
            var result = new double[3];
            ToArray(result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public void ToArray(double[] result)
        {
            result[0] = X;
            result[1] = Y;
            result[2] = Z;
        }
    }
}
