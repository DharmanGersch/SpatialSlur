﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

using SpatialSlur.SlurData;

using static SpatialSlur.SlurData.DataUtil;

/*
 * Notes
 * All IList extension methods are redirected to equivalent array extension methods where possible for better performance.
 */

namespace SpatialSlur.SlurCore
{
    /// <summary>
    /// 
    /// </summary>
    public static class IReadOnlyListExtensions
    {
        #region IReadOnlyList<T>

        /// <summary>
        /// 
        /// </summary>
        public static T[] GetRange<T>(this IReadOnlyList<T> list, int index, int count)
        {
            if (list is T[])
                return ArrayExtensions.GetRange((T[])list, index, count);

            T[] result = new T[count];
            GetRangeImpl(list, index, count, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void GetRange<T>(this IReadOnlyList<T> list, int index, int count, IList<T> result)
        {
            if (list is T[] && result is T[])
                ArrayExtensions.GetRange((T[])list, index, count, (T[])result);
            else
                GetRangeImpl(list, index, count, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void GetRangeImpl<T>(IReadOnlyList<T> list, int index, int count, IList<T> result)
        {
            for (int i = 0; i < count; i++)
                result[i] = list[i + index];
        }


        /// <summary>
        /// 
        /// </summary>
        public static T[] GetSelection<T>(this IReadOnlyList<T> list, IReadOnlyList<int> indices)
        {
            if (list is T[] && indices is int[])
                return ArrayExtensions.GetSelection((T[])list, (int[])indices);

            T[] result = new T[indices.Count];
            GetSelectionImpl(list, indices, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void GetSelection<T>(this IReadOnlyList<T> list, IReadOnlyList<int> indices, IList<T> result)
        {
            if (list is T[] && indices is int[] && result is T[])
                ArrayExtensions.GetSelection((T[])list, (int[])indices, (T[])result);
            else
                GetSelectionImpl(list, indices, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void GetSelectionImpl<T>(IReadOnlyList<T> list, IReadOnlyList<int> indices, IList<T> result)
        {
            for (int i = 0; i < indices.Count; i++)
                result[i] = list[indices[i]];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void Action<T>(this IReadOnlyList<T> source, Action<T> action)
        {
            ActionRange(source, 0, source.Count, action);
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] Convert<T, U>(this IReadOnlyList<T> source, Func<T, U> converter)
        {
            return ConvertRange(source, 0, source.Count, converter);
        }


        /// <summary>
        /// 
        /// </summary>
        public static void Convert<T, U>(this IReadOnlyList<T> source, Func<T, U> converter, IList<U> result)
        {
            ConvertRange(source, 0, source.Count, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] Convert<T, U>(this IReadOnlyList<T> source, Func<T, int, U> converter)
        {
            return ConvertRange(source, 0, source.Count, converter);
        }


        /// <summary>
        /// 
        /// </summary>
        public static void Convert<T, U>(this IReadOnlyList<T> source, Func<T, int, U> converter, IList<U> result)
        {
            ConvertRange(source, 0, source.Count, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ActionParallel<T>(this IReadOnlyList<T> source, Action<T> action)
        {
            ActionRangeParallel(source, 0, source.Count, action);
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertParallel<T, U>(this IReadOnlyList<T> source, Func<T, U> converter)
        {
            return ConvertRangeParallel(source, 0, source.Count, converter);
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertParallel<T, U>(this IReadOnlyList<T> source, Func<T, U> converter, IList<U> result)
        {
            ConvertRangeParallel(source, 0, source.Count, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertParallel<T, U>(this IReadOnlyList<T> source, Func<T, int, U> converter)
        {
            return ConvertRangeParallel(source, 0, source.Count, converter);
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertParallel<T, U>(this IReadOnlyList<T> source, Func<T, int, U> converter, IList<U> result)
        {
            ConvertRangeParallel(source, 0, source.Count, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="action"></param>
        public static void ActionRange<T>(this IReadOnlyList<T> source, int index, int count, Action<T> action)
        {
            if (source is T[])
            {
                ArrayExtensions.ActionRange((T[])source, index, count, action);
                return;
            }

            for(int i = 0; i < count; i++)
                action(source[i + index]);
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertRange<T, U>(this IReadOnlyList<T> source, int index, int count, Func<T, U> converter)
        {
            if (source is T[])
                return ArrayExtensions.ConvertRange((T[])source, index, count, converter);

            U[] result = new U[count];
            ConvertRangeImpl(source, index, count, converter, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertRange<T, U>(this IReadOnlyList<T> source, int index, int count, Func<T, U> converter, IList<U> result)
        {
            if (source is T[] && result is U[])
                ArrayExtensions.ConvertRange((T[])source, index, count, converter, (U[])result);
            else
                ConvertRangeImpl(source, index, count, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertRangeImpl<T,U>(IReadOnlyList<T> source, int index, int count, Func<T, U> converter, IList<U> result)
        {
            for (int i = 0; i < count; i++)
                result[i] = converter(source[i + index]);
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertRange<T, U>(this IReadOnlyList<T> source, int index, int count, Func<T, int, U> converter)
        {
            if (source is T[])
                return ArrayExtensions.ConvertRange((T[])source, index, count, converter);

            U[] result = new U[count];
            ConvertRangeImpl(source, index, count, converter, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertRange<T, U>(this IReadOnlyList<T> source, int index, int count, Func<T, int, U> converter, IList<U> result)
        {
            if (source is T[] && result is U[])
                ArrayExtensions.ConvertRange((T[])source, index, count, converter, (U[])result);
            else
                ConvertRangeImpl(source, index, count, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertRangeImpl<T, U>(IReadOnlyList<T> source, int index, int count, Func<T, int, U> converter, IList<U> result)
        {
            for (int i = 0; i < count; i++)
            {
                int j = i + index;
                result[i] = converter(source[j], j);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="action"></param>
        public static void ActionRangeParallel<T>(this IReadOnlyList<T> source, int index, int count, Action<T> action)
        {
            if (source is T[])
            {
                ArrayExtensions.ActionRangeParallel((T[])source, index, count, action);
                return;
            }

            Parallel.ForEach(Partitioner.Create(0, count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                    action(source[i + index]);
            });
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertRangeParallel<T, U>(this IReadOnlyList<T> source, int index, int count, Func<T, U> converter)
        {
            if (source is T[])
                return ArrayExtensions.ConvertRangeParallel((T[])source, index, count, converter);

            U[] result = new U[count];
            ConvertRangeParallelImpl(source, index, count, converter, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertRangeParallel<T, U>(this IReadOnlyList<T> source, int index, int count, Func<T, U> converter, IList<U> result)
        {
            if (source is T[] && result is U[])
                ArrayExtensions.ConvertRangeParallel((T[])source, index, count, converter, (U[])result);
            else
                ConvertRangeParallelImpl(source, index, count, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertRangeParallelImpl<T, U>(IReadOnlyList<T> source, int index, int count, Func<T, U> converter, IList<U> result)
        {
            Parallel.ForEach(Partitioner.Create(0, count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                    result[i] = converter(source[i + index]);
            });
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertRangeParallel<T, U>(this IReadOnlyList<T> source, int index, int count, Func<T, int, U> converter)
        {
            if (source is T[])
                return ArrayExtensions.ConvertRangeParallel((T[])source, index, count, converter);

            U[] result = new U[count];
            ConvertRangeParallelImpl(source, index, count, converter, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertRangeParallel<T, U>(this IReadOnlyList<T> source, int index, int count, Func<T, int, U> converter, IList<U> result)
        {
            if (source is T[] && result is U[])
                ArrayExtensions.ConvertRangeParallel((T[])source, index, count, converter, (U[])result);
            else
                ConvertRangeParallelImpl(source, index, count, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertRangeParallelImpl<T, U>(IReadOnlyList<T> source, int index, int count, Func<T, int, U> converter, IList<U> result)
        {
            Parallel.ForEach(Partitioner.Create(0, count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    int j = i + index;
                    result[i] = converter(source[j], j);
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ActionSelection<T>(IReadOnlyList<T> source, IReadOnlyList<int> indices, Action<T> action)
        {
            if (source is T[] && indices is int[])
            {
                ArrayExtensions.ActionSelection<T>((T[])source, (int[])indices, action);
                return;
            }

            for (int i = 0; i < indices.Count; i++)
                action(source[indices[i]]);
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertSelection<T, U>(this IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, U> converter)
        {
            if (source is T[] && indices is int[])
                return ArrayExtensions.ConvertSelection((T[])source, (int[])indices, converter);

            U[] result = new U[indices.Count];
            ConvertSelectionImpl(source, indices, converter, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertSelection<T, U>(this IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, U> converter, IList<U> result)
        {
            if (source is T[] && indices is int[] && result is U[])
                ArrayExtensions.ConvertSelection((T[])source, (int[])indices, converter, (U[])result);
            else
                ConvertSelectionImpl(source, indices, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertSelectionImpl<T, U>(IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, U> converter, IList<U> result)
        {
            for (int i = 0; i < indices.Count; i++)
                result[i] = converter(source[indices[i]]);
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertSelection<T, U>(this IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, int, U> converter)
        {
            if (source is T[] && indices is int[])
                return ArrayExtensions.ConvertSelection((T[])source, (int[])indices, converter);

            U[] result = new U[indices.Count];
            ConvertSelectionImpl(source, indices, converter, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertSelection<T, U>(this IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, int, U> converter, IList<U> result)
        {
            if (source is T[] && indices is int[] && result is U[])
                ArrayExtensions.ConvertSelection((T[])source, (int[])indices, converter, (U[])result);
            else
                ConvertSelectionImpl(source, indices, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertSelectionImpl<T, U>(IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, int, U> converter, IList<U> result)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                int j = indices[i];
                result[i] = converter(source[j], j);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ActionSelectionParallel<T>(IReadOnlyList<T> source, IReadOnlyList<int> indices, Action<T> action)
        {
            if (source is T[] && indices is int[])
            {
                ArrayExtensions.ActionSelectionParallel<T>((T[])source, (int[])indices, action);
                return;
            }

            Parallel.ForEach(Partitioner.Create(0, indices.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                    action(source[indices[i]]);
            });
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertSelectionParallel<T, U>(this IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, U> converter)
        {
            if (source is T[] && indices is int[])
                return ArrayExtensions.ConvertSelectionParallel((T[])source, (int[])indices, converter);

            U[] result = new U[indices.Count];
            ConvertSelectionParallelImpl(source, indices, converter, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertSelectionParallel<T, U>(this IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, U> converter, IList<U> result)
        {
            if (source is T[] && indices is int[] && result is U[])
                ArrayExtensions.ConvertSelectionParallel((T[])source, (int[])indices, converter, (U[])result);
            else
                ConvertSelectionParallelImpl(source, indices, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertSelectionParallelImpl<T, U>(IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, U> converter, IList<U> result)
        {
            Parallel.ForEach(Partitioner.Create(0, indices.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                    result[i] = converter(source[indices[i]]);
            });
        }


        /// <summary>
        /// 
        /// </summary>
        public static U[] ConvertSelectionParallel<T, U>(this IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, int, U> converter)
        {
            if (source is T[] && indices is int[])
                return ArrayExtensions.ConvertSelectionParallel((T[])source, (int[])indices, converter);

            U[] result = new U[indices.Count];
            ConvertSelectionParallelImpl(source, indices, converter, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void ConvertSelectionParallel<T, U>(this IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, int, U> converter, IList<U> result)
        {
            if (source is T[] && indices is int[] && result is U[])
                ArrayExtensions.ConvertSelectionParallel((T[])source, (int[])indices, converter, (U[])result);
            else
                ConvertSelectionParallelImpl(source, indices, converter, result);
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ConvertSelectionParallelImpl<T, U>(IReadOnlyList<T> source, IReadOnlyList<int> indices, Func<T, int, U> converter, IList<U> result)
        {
            Parallel.ForEach(Partitioner.Create(0, indices.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    int j = indices[i];
                    result[i] = converter(source[j], j);
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<T> TakeRange<T>(this IReadOnlyList<T> source, int index, int count)
        {
            for (int i = 0; i < count; i++)
                yield return source[i + index];
        }


        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<T> TakeSelection<T>(this IReadOnlyList<T> source, IEnumerable<int> indices)
        {
            foreach (int i in indices)
                yield return source[i];
        }


        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<T> TakeEveryNth<T>(this IReadOnlyList<T> source, int n)
        {
            return TakeEveryNth(source, n, 0, source.Count);
        }


        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<T> TakeEveryNth<T>(this IReadOnlyList<T> source, int n, int index, int count)
        {
            for (int i = 0; i < count; i += n)
                yield return source[i + index];
        }


        /// <summary>
        /// Allows enumeration over segments of the given list.
        /// </summary>
        public static IEnumerable<ReadOnlySubList<T>> Segment<T>(this IReadOnlyList<T> source, IEnumerable<int> sizes)
        {
            int marker = 0;
            foreach (int n in sizes)
            {
                yield return new ReadOnlySubList<T>(source, marker, n);
                marker += n;
            }
        }

        #endregion


        #region IReadOnlyList<Color>

        /// <summary>
        /// 
        /// </summary>
        public static Color Lerp(this IReadOnlyList<Color> colors, double t)
        {
            int last = colors.Count - 1;
            t = SlurMath.Fract(t * last, out int i);

            if (i < 0)
                return colors[0];
            else if (i >= last)
                return colors[last];

            return colors[i].LerpTo(colors[i + 1], t);
        }

        #endregion


        #region IReadOnlyList<double>

        /// <summary>
        /// 
        /// </summary>
        public static double Lerp(this IReadOnlyList<double> vector, double t)
        {
            int last = vector.Count - 1;
            t = SlurMath.Fract(t * last, out int i);

            if (i < 0)
                return vector[0];
            else if (i >= last)
                return vector[last];

            return SlurMath.Lerp(vector[i], vector[i + 1], t);
        }

        #endregion


        #region IReadOnlyList<Vec2d>

        /// <summary>
        /// 
        /// </summary>
        public static Vec2d Lerp(this IReadOnlyList<Vec2d> vectors, double t)
        {
            int last = vectors.Count - 1;
            t = SlurMath.Fract(t * last, out int i);

            if (i < 0)
                return vectors[0];
            else if (i >= last)
                return vectors[last];

            return vectors[i].LerpTo(vectors[i + 1], t);
        }


        /// <summary>
        /// Removes duplicate points from the same list
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<Vec2d> RemoveCoincident(this IReadOnlyList<Vec2d> points, double tolerance = 1.0e-8)
        {
            return DataUtil.RemoveCoincident(points, p => p, tolerance);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <param name="indexMap"></param>
        /// <returns></returns>
        public static List<Vec2d> RemoveCoincident(this IReadOnlyList<Vec2d> points, out int[] indexMap, double tolerance = 1.0e-8)
        {
            return DataUtil.RemoveCoincident(points, p => p, out indexMap, tolerance);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <param name="indexMap"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static List<Vec2d> RemoveCoincident(this IReadOnlyList<Vec2d> points, HashGrid2d<int> grid, out int[] indexMap, double tolerance = 1.0e-8)
        {
            return DataUtil.RemoveCoincident(points, p => p, grid, out indexMap, tolerance);
        }


        /// <summary>
        /// For each point, returns the index of the first coincident point within the same list. If no coincident point is found, -1 is returned.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetFirstCoincident(this IReadOnlyList<Vec2d> points, double tolerance = 1.0e-8)
        {
            return DataUtil.GetFirstCoincident(points, p => p, tolerance);
        }


        /// <summary>
        /// For each point, returns the index of the first coincident point within the same list. If no coincident point is found, -1 is returned.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetFirstCoincident(this IReadOnlyList<Vec2d> points, HashGrid2d<int> grid, double tolerance = 1.0e-8)
        {
            return DataUtil.GetFirstCoincident(points, p => p, grid, tolerance);
        }


        /// <summary>
        /// For each point in A, returns the index of the first coincident point in B. If no coincident point is found, -1 is returned.
        /// </summary>
        /// <param name="pointsA"></param>
        /// <param name="pointsB"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetFirstCoincident(this IReadOnlyList<Vec2d> pointsA, IReadOnlyList<Vec2d> pointsB, double tolerance = 1.0e-8)
        {
            return DataUtil.GetFirstCoincident(pointsA, pointsB, p => p, tolerance);
        }


        /// <summary>
        /// For each point in A, returns the index of the first coincident point in B. If no coincident point is found, -1 is returned.
        /// </summary>
        /// <param name="pointsA"></param>
        /// <param name="pointsB"></param>
        /// <param name="tolerance"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetFirstCoincident(this IReadOnlyList<Vec2d> pointsA, IReadOnlyList<Vec2d> pointsB, HashGrid2d<int> grid, double tolerance = 1.0e-8)
        {
            return DataUtil.GetFirstCoincident(pointsA, pointsB, p => p, grid, tolerance);
        }


        /// <summary>
        /// For each point in A, returns the index of all coincident points in B.
        /// </summary>
        /// <param name="pointsA"></param>
        /// <param name="pointsB"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<int>> GetAllCoincident(this IReadOnlyList<Vec2d> pointsA, IReadOnlyList<Vec2d> pointsB, double tolerance = 1.0e-8)
        {
            return DataUtil.GetAllCoincident(pointsA, pointsB, p => p, tolerance);
        }


        /// <summary>
        /// For each point in A, returns the index of all coincident points in B.
        /// </summary>
        /// <param name="pointsA"></param>
        /// <param name="pointsB"></param>
        /// <param name="tolerance"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<int>> GetAllCoincident(this IReadOnlyList<Vec2d> pointsA, IReadOnlyList<Vec2d> pointsB, HashGrid2d<int> grid, double tolerance = 1.0e-8)
        {
            return DataUtil.GetAllCoincident(pointsA, pointsB, p => p, grid, tolerance);
        }

        #endregion


        #region IReadOnlyList<Vec3d>

        /// <summary>
        /// 
        /// </summary>
        public static Vec3d Lerp(this IReadOnlyList<Vec3d> vectors, double t)
        {
            int last = vectors.Count - 1;
            t = SlurMath.Fract(t * last, out int i);

            if (i < 0)
                return vectors[0];
            else if (i >= last)
                return vectors[last];

            return vectors[i].LerpTo(vectors[i + 1], t);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<Vec3d> RemoveCoincident(this IReadOnlyList<Vec3d> points, double tolerance = 1.0e-8)
        {
            return DataUtil.RemoveCoincident(points, p => p, tolerance);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <param name="indexMap"></param>
        /// <returns></returns>
        public static List<Vec3d> RemoveCoincident(this IReadOnlyList<Vec3d> points, out int[] indexMap, double tolerance = 1.0e-8)
        {
            return DataUtil.RemoveCoincident(points, p => p, out indexMap, tolerance);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <param name="indexMap"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static List<Vec3d> RemoveCoincident(this IReadOnlyList<Vec3d> points, HashGrid3d<int> grid, out int[] indexMap, double tolerance = 1.0e-8)
        {
            return DataUtil.RemoveCoincident(points, p => p, grid, out indexMap, tolerance);

        }


        /// <summary>
        /// For each point, returns the index of the first coincident point within the same list. If no coincident point is found, -1 is returned.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetFirstCoincident(this IReadOnlyList<Vec3d> points, double tolerance = 1.0e-8)
        {
            return DataUtil.GetFirstCoincident(points, p => p, tolerance);

        }


        /// <summary>
        /// For each point, returns the index of the first coincident point within the same list. If no coincident point is found, -1 is returned.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetFirstCoincident(this IReadOnlyList<Vec3d> points, HashGrid3d<int> grid, double tolerance = 1.0e-8)
        {
            return DataUtil.GetFirstCoincident(points, p => p, grid, tolerance);
        }


        /// <summary>
        /// For each point in A, returns the index of the first coincident point in B. If no coincident point is found, -1 is returned.
        /// </summary>
        /// <param name="pointsA"></param>
        /// <param name="pointsB"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetFirstCoincident(this IReadOnlyList<Vec3d> pointsA, IReadOnlyList<Vec3d> pointsB, double tolerance = 1.0e-8)
        {
            return DataUtil.GetFirstCoincident(pointsA, pointsB, p => p, tolerance);
        }


        /// <summary>
        /// For each point in A, returns the index of the first coincident point in B. If no coincident point is found, -1 is returned.
        /// </summary>
        /// <param name="pointsA"></param>
        /// <param name="pointsB"></param>
        /// <param name="tolerance"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetFirstCoincident(this IReadOnlyList<Vec3d> pointsA, IReadOnlyList<Vec3d> pointsB, HashGrid3d<int> grid, double tolerance = 1.0e-8)
        {
            return DataUtil.GetFirstCoincident(pointsA, pointsB, p => p, grid, tolerance);
        }


        /// <summary>
        /// For each point in A, returns the index of all coincident points in B.
        /// </summary>
        /// <param name="pointsA"></param>
        /// <param name="pointsB"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<int>> GetAllCoincident(this IReadOnlyList<Vec3d> pointsA, IReadOnlyList<Vec3d> pointsB, double tolerance = 1.0e-8)
        {
            return DataUtil.GetAllCoincident(pointsA, pointsB, p => p, tolerance);
        }


        /// <summary>
        /// For each point in A, returns the index of all coincident points in B.
        /// </summary>
        /// <param name="pointsA"></param>
        /// <param name="pointsB"></param>
        /// <param name="tolerance"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<int>> GetAllCoincident(this IReadOnlyList<Vec3d> pointsA, IReadOnlyList<Vec3d> pointsB, HashGrid3d<int> grid, double tolerance = 1.0e-8)
        {
            return DataUtil.GetAllCoincident(pointsA, pointsB, p => p, grid, tolerance);
        }
        
        #endregion


        #region IReadOnlyList<double[]>

        /// <summary>
        /// 
        /// </summary>
        public static void Lerp(this IReadOnlyList<double[]> vectors, double t, double[] result)
        {
            Lerp(vectors, t, vectors[0].Length, result);
        }


        /// <summary>
        /// 
        /// </summary>
        public static void Lerp(this IReadOnlyList<double[]> vectors, double t, int size, double[] result)
        {
            int last = vectors.Count - 1;
            t = SlurMath.Fract(t * last, out int i);

            if (i < 0)
                result.Set(vectors[0]);
            else if (i >= last)
                result.Set(vectors[last]);
            else
                ArrayMath.Lerp(vectors[i], vectors[i + 1], t, size, result);
        }

        #endregion
    }
}
