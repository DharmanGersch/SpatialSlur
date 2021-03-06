﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpatialSlur.SlurCore;

/*
 * Notes
 */

namespace SpatialSlur.SlurField
{
    /// <summary>
    /// Utility class for related constants and static methods.
    /// </summary>
    public static class GridUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wrapMode"></param>
        /// <returns></returns>
        public static Func<int, int, int> SelectWrapFunction(WrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case WrapMode.Clamp:
                    return Clamp;
                case WrapMode.Repeat:
                    return Repeat;
                case WrapMode.MirrorRepeat:
                    return MirrorRepeat;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static int Wrap(int i, int n, WrapMode mode)
        {
            switch (mode)
            {
                case (WrapMode.Clamp):
                    return Clamp(i, n);
                case (WrapMode.Repeat):
                    return Repeat(i, n);
                case (WrapMode.MirrorRepeat):
                    return MirrorRepeat(i, n);
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static int Clamp(int i, int n)
        {
            return (i < 0) ? 0 : (i < n) ? i : n - 1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static int Repeat(int i, int n)
        {
            i %= n;
            return (i < 0) ? i + n : i;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static int MirrorRepeat(int i, int n)
        {
            i = Repeat(i, n + n);
            return (i < n) ? i : n + n - i - 1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="nx"></param>
        /// <returns></returns>
        public static int FlattenIndices(int x, int y, int nx)
        {
            return x + y * nx;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="nx"></param>
        /// <param name="nxy"></param>
        /// <returns></returns>
        public static int FlattenIndices(int x, int y, int z, int nx, int nxy)
        {
            return x + y * nx + z * nxy;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nx"></param>
        /// <returns></returns>
        public static (int, int) ExpandIndex(int index, int nx)
        {
            int y = index / nx;
            return (index - y * nx, y);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nx"></param>
        /// <param name="nxy"></param>
        /// <returns></returns>
        public static (int, int, int) ExpandIndex(int index, int nx, int nxy)
        {
            int z = index / nxy;
            index -= z * nxy;
            int y = index / nx;
            return (index - y * nx, y, z);
        }
    }
}
