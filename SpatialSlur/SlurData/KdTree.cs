﻿using System;
using System.Collections.Generic;
using System.Linq;

using SpatialSlur.SlurCore;

using static SpatialSlur.SlurCore.ArrayMath;

/*
 * Notes
 * 
 * Left subtree is strictly less than
 * Right subtree is equal or greater than
 * 
 * KdTree search performance degrades at higher dimensions.
 * In general the number of nodes should be larger than 2^k for decent search performance.
 * 
 * References
 * https://www.cs.umd.edu/class/spring2008/cmsc420/L19.kd-trees.pdf
 * http://www.cs.umd.edu/~meesh/420/Notes/MountNotes/lecture18-kd2.pdf
 */

namespace SpatialSlur.SlurData
{
    /// <summary>
    /// Generic implementation of a k-dimensional binary search tree.
    /// </summary>
    [Serializable]
    public class KdTree<T>
    {
        #region Static

        /// <summary>
        /// Inserts point value pairs in a way that produces a balanced tree.
        /// </summary>
        public static KdTree<T> CreateBalanced(IEnumerable<double[]> points, IEnumerable<T> values)
        {
            KdTree<T> result = new KdTree<T>(points.First().Length);
            var nodes = points.Zip(values, (p, v) => new Node(p, v)).ToArray();

            result._root = result.InsertBalanced(nodes, 0, nodes.Length - 1, 0);
            result._count = nodes.Length;
            return result;
        }


        /// <summary>
        /// Inserts point value pairs in a way that produces a balanced tree.
        /// </summary>
        public static KdTree<T> CreateBalanced(double[][] points, T[] values)
        {
            KdTree<T> result = new KdTree<T>(points[0].Length);
            var nodes = points.Convert((p, i) => new Node(p, values[i]));

            result._root = result.InsertBalanced(nodes, 0, nodes.Length - 1, 0);
            result._count = nodes.Length;
            return result;
        }

        
        /// <summary>
        /// Returns the node with the smallest value in the given dimension.
        /// If equal, n0 is returned.
        /// </summary>
        /// <param name="n0"></param>
        /// <param name="n1"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static Node Min(Node n0, Node n1, int i)
        {
            return (n1.Point[i] < n0.Point[i]) ? n1 : n0;
        }

        #endregion


        private Node _root;
        private double _tolerance = 1.0e-8;
        private readonly int _k;
        private int _count;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="k"></param>
        public KdTree(int k)
        {
            if (k < 2)
                throw new System.ArgumentException("The tree must have at least 2 dimensions.");

            _k = k;
        }


        /// <summary>
        /// Returns the number of dimensions used by the tree.
        /// </summary>
        public int K
        {
            get{return _k;}
        }


        /// <summary>
        /// Returns the number of dimensions used by the tree (i.e. K)
        /// </summary>
        public int Dimension
        {
            get { return _k; }
        }


        /// <summary>
        /// Returns the number of nodes in the tree.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }


        /// <summary>
        /// Sets the tolerance used for finding equal points in the tree.
        /// By default, this is set to 1.0e-8.
        /// </summary>
        public double Tolerance
        {
            get { return _tolerance; }
            set
            {
                if (value <= 0.0)
                    throw new ArgumentException("The value must be greater than zero.");

                _tolerance = value;
            }
        }


        /// <summary>
        ///
        /// </summary>
        public bool IsEmpty
        {
            get { return _root == null; }
        }


        /// <summary>
        /// Creates a shallow copy of the tree
        /// </summary>
        /// <returns></returns>
        public KdTree<T> Duplicate()
        {
            var copy = new KdTree<T>(_k);
            copy._root = Duplicate(_root);
            return copy;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private Node Duplicate(Node other)
        {
            var copy = new Node(other.Point, other.Value);

            if (other.Left != null)
                copy.Left = Duplicate(other.Left);

            if (other.Right != null)
                copy.Right = Duplicate(other.Right);

            return copy;
        }
        

        /// <summary>
        /// 
        /// </summary>
        public int GetMaxDepth()
        {
            return GetMaxDepth(_root, 0);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private int GetMaxDepth(Node node, int i)
        {
            if (node == null) return i;
            return Math.Max(GetMaxDepth(node.Left, i + 1), GetMaxDepth(node.Right, i + 1));
        }


        /// <summary>
        /// 
        /// </summary>
        public int GetMinDepth()
        {
            var q0 = new Queue<Node>();
            var q1 = new Queue<Node>();
            int depth = 0;

            q0.Enqueue(_root);

            while(true)
            {
                // iterate through nodes at the current depth and return if leaf is found
                while (q0.Count > 0)
                {
                    var node = q0.Dequeue();
                    if (node.IsLeaf) return depth;
                    q1.Enqueue(node.Left);
                    q1.Enqueue(node.Right);
                }

                // swap queues and continue on the next level
                CoreUtil.Swap(ref q0, ref q1);
                depth++;
            }
        }


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        private void DimCheck(double[] point)
        {
            if (_k != point.Length)
                throw new System.ArgumentException("The given point must have the same number of dimensions as this tree.");
        }
        */


        /// <summary>
        /// Returns true if the tree contains the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(double[] point)
        {
            return Find(_root, point, 0) != null;
        }


        /// <summary>
        /// Returns true if the tree contains the given point.
        /// The value associated with the given point is also returned in the out parameter on success.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(double[] point, out T value)
        {
            Node n = Find(_root, point, 0);

            if (n == null)
            {
                value = default(T);
                return false;
            }
            else
            {
                value = n.Value;
                return true;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private Node Find(Node node, double[] point, int i)
        {
            if (node == null) 
                return null;

            if (ApproxEquals(point, node.Point, _tolerance))
                return node;

            // wrap dimension
            if (i == _k) i = 0;

            if (point[i] < node.Point[i])
                return Find(node.Left, point, i + 1);
            else
                return Find(node.Right, point, i + 1);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="value"></param>
        public void Insert(double[] point, T value)
        {
            _root = Insert(_root, point, value, 0);
            _count++;
        }
      

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="value"></param>
        /// <param name="i"></param>
        private Node Insert(Node node, double[] point, T value, int i)
        {
            if (node == null)
                return new Node(point, value);
   
            // wrap dimension
            if (i == _k) i = 0;

            // left or right of node?
            if (point[i] < node.Point[i])
                node.Left = Insert(node.Left, point, value, i + 1);
            else
                node.Right = Insert(node.Right, point, value, i + 1);

            return node;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private Node InsertBalanced(Node[] nodes, int from, int to, int i)
        {
            // stopping conditions
            if (from > to)
                return null;
            else if (from == to)
                return nodes[from];

            // wrap dimension
            if (i == _k) i = 0;

            // sort the median element
            int mid = ((to - from) >> 1) + from;
            var midNode = nodes.QuickSelect(mid, from, to, (n0, n1) => n0.Point[i].CompareTo(n1.Point[i]));

            // ensure no duplicate elements to the left of the median
            int j = from;
            var t = midNode.Point[i];

            while (j < mid)
            {
                if (nodes[j].Point[i] == t)
                    nodes.Swap(j, --mid);
                else
                    j++;
            }

            // create node and recall on left and right children
            midNode.Left = InsertBalanced(nodes, from, mid - 1, i + 1);
            midNode.Right = InsertBalanced(nodes, mid + 1, to, i + 1);
            return nodes[mid];
        }


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="values"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private Node InsertBalanced(double[][] points, T[] values, int from, int to, int i)
        {
            // stopping conditions
            if (from > to)
                return null;
            else if (from == to)
                return new Node(points[from], values[from]);

            // wrap dimension
            if (i == _k) i = 0;

            // sort the median element
            int mid = ((to - from) >> 1) + from;
            var midPoint = points.QuickSelect(values, mid, from, to, (p0, p1) => p0[i].CompareTo(p1[i]));

            // ensure no duplicate elements to the left of the median
            int j = from;
            double t = midPoint[i];

            while (j < mid)
            {
                if (points[j][i] == t)
                {
                    points.Swap(j, --mid);
                    values.Swap(j, mid);
                }
                else
                {
                    j++;
                }
            }

            // create node and recall on left and right children
            Node node = new Node(points[mid], values[mid]);
            node.Left = InsertBalanced(points, values, from, mid - 1, i + 1);
            node.Right = InsertBalanced(points, values, mid + 1, to, i + 1);
            return node;
        }
        */
      

        /// <summary> 
        /// Removes the first point in the tree which is equal to the given point.
        /// Note that repeated node removal can result in unbalanced trees which degrades search performance.
        /// </summary>
        /// <param name="point"></param>
        public bool Remove(double[] point)
        {
            int n = _count;
            _root = Remove(_root, point, 0);
            return n != _count;
        }


        /// <summary>
        /// http://www.cs.umd.edu/~meesh/420/Notes/MountNotes/lecture18-kd2.pdf
        /// http://www.cs.umd.edu/class/spring2002/cmsc420-0401/pbasic.pdf
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private Node Remove(Node node, double[] point, int i)
        {
            if (node == null)
                return null;

            // wrap dimension
            if (i == _k) i = 0;

            if (ApproxEquals(point, node.Point, _tolerance))
            {
                // found the node to remove, find replacement if it has children
                if (node.Right != null)
                {
                    Node min = FindMin(node.Right, i, i + 1); // search the right sub-tree for a replacement node (min in the current dimension)
                    min.Right = Remove(node.Right, min.Point, i + 1); // remove the replacement node from right subtree
                    min.Left = node.Left;
                    node = min;
                }
                else if (node.Left != null)
                {
                    Node min = FindMin(node.Left, i, i + 1); // search the left sub-tree for a replacement node (min in the current dimension)
                    min.Right = Remove(node.Left, min.Point, i + 1); // remove the replacement node from the left sub tree
                    min.Left = null;
                    node = min;
                }
                else
                {
                    node = null; // node is a leaf, can safely remove
                    _count--; // decrement node count
                }
            }
            else if (point[i] < node.Point[i])
            {
                // node not found, continue search to the left
                node.Left = Remove(node.Left, point, i + 1);
            }
            else
            {
                // node not found, continue search to the right
                node.Right = Remove(node.Right, point, i + 1);
            }

            return node;
        }


        /// <summary>
        /// Returns the node in the subtree with the smallest value in the specified dimension.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dim"></param>
        /// <param name="i"></param>
        private Node FindMin(Node node, int dim, int i)
        {
            // wrap dimension
            if (i == _k) i = 0;

            if (dim == i)
            {
                // recall on left subtree tree only
                if (node.Left == null)
                    return node;
                else
                    return FindMin(node.Left, dim, i + 1);
            }
            else
            {
                // recall on both subtrees if they exist
                if (node.IsLeaf)
                    return node;
                else if (node.Right == null)
                    return Min(node, FindMin(node.Left, dim, i + 1), dim);
                else if (node.Left == null)
                    return Min(node, FindMin(node.Right, dim, i + 1), dim);
                else
                    return Min(node, Min(FindMin(node.Left, dim, i + 1), FindMin(node.Right, dim, i + 1), dim), dim);
            }
        }


        /// <summary>
        /// Calls the given delegate on each found value.
        /// Returns false if the search was aborted which occurs if the delegate return false.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool BoxSearch(double[] point, double[] range, Func<T, bool> callback)
        {
            return BoxSearchImpl(_root, point, range, callback, 0);
        }


        /// <summary>
        /// Returns all found values by appending them to the given list.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="result"></param>
        public void BoxSearch(double[] point, double[] range, List<T> result)
        {
            BoxSearch(point, range, t =>
            {
                result.Add(t);
                return true;
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="callback"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private bool BoxSearchImpl(Node node, double[] point, double[] range, Func<T, bool> callback, int i)
        {
            if (node == null) return true;

            // wrap dimension
            if (i == _k) i = 0;

            // callback on node if within range
            if (ApproxEquals(point, node.Point, range))
                if (!callback(node.Value)) return false;

            // if left side of tree
            double d = point[i] - node.Point[i]; // signed distance to cut plane
            if (d < 0.0)
            {
                // point is to the left, recall in left subtree first
                BoxSearchImpl(node.Left, point, range, callback, i + 1);

                // recall in right subtree only if necessary
                if (Math.Abs(d) < range[i])
                    BoxSearchImpl(node.Right, point, range, callback, i + 1);
            }
            else
            {
                // point is to the right, recall in right subtree first
                BoxSearchImpl(node.Right, point, range, callback, i + 1);

                // recall in left subtree only if necessary
                if (Math.Abs(d) < range[i])
                    BoxSearchImpl(node.Left, point, range, callback, i + 1);
            }

            return true;
        }


        /// <summary>
        /// Calls the given delegate on each found value.
        /// Returns false if the search was aborted which occurs if the delegate return false.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="callback"></param>
        public bool RangeSearchL2(double[] point, double range, Func<T, bool> callback)
        {
            return RangeSearchL2Impl(_root, point, range, callback, 0);
        }


        /// <summary>
        /// Returns all found values by appending them to the given list.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="result"></param>
        public void RangeSearchL2(double[] point, double range, List<T> result)
        {
            RangeSearchL2(point, range, t =>
             {
                 result.Add(t);
                 return true;
             });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="callback"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private bool RangeSearchL2Impl(Node node, double[] point, double range, Func<T,bool> callback, int i)
        {
            if (node == null) return true;

            // wrap dimension
            if (i == _k) i = 0;

            // enqueue value if node is within range
            if (SquareDistanceL2(point, node.Point) < range)
                if(!callback(node.Value)) return false;

            double d = point[i] - node.Point[i]; // signed distance to cut plane
            if (d < 0.0)
            {
                // point is to the left, recall in left subtree first
                RangeSearchL2Impl(node.Left, point, range, callback, i + 1);

                // recall in right subtree only if necessary
                if (d * d < range)
                    RangeSearchL2Impl(node.Right, point, range, callback, i + 1);
            }
            else
            {
                // point is to the right, recall in right subtree first
                RangeSearchL2Impl(node.Right, point, range, callback, i + 1);

                // recall in left subtree only if necessary
                if (d * d < range)
                    RangeSearchL2Impl(node.Left, point, range, callback, i + 1);
            }

            return true;
        }


        /// <summary>
        /// Calls the given delegate on each found value.
        /// Returns false if the search was aborted which occurs if the delegate return false.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="callback"></param>
        public bool RangeSearchL1(double[] point, double range, Func<T, bool> callback)
        {
            return RangeSearchL1Impl(_root, point, range, callback, 0);
        }


        /// <summary>
        /// Returns all found values by appending them to the given list.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="result"></param>
        public void RangeSearchL1(double[] point, double range, List<T> result)
        {
            RangeSearchL1(point, range, t =>
            {
                result.Add(t);
                return true;
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <param name="callback"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private bool RangeSearchL1Impl(Node node, double[] point, double range, Func<T, bool> callback, int i)
        {
            if (node == null) return true;

            // wrap dimension
            if (i == _k) i = 0;

            // add node if within range
            if (DistanceL1(point, node.Point) < range)
                if (!callback(node.Value)) return false;

            double d = point[i] - node.Point[i]; // signed distance to cut plane
            if (d < 0.0)
            {
                // point is to the left, recall in left subtree first
                RangeSearchL1Impl(node.Left, point, range, callback, i + 1);

                // recall in right subtree only if necessary
                if (Math.Abs(d) < range)
                    RangeSearchL1Impl(node.Right, point, range, callback, i + 1);
            }
            else
            {
                // point is to the right, recall in right subtree first
                RangeSearchL1Impl(node.Right, point, range, callback, i + 1);

                // recall in left subtree only if necessary
                if (Math.Abs(d) < range)
                    RangeSearchL1Impl(node.Left, point, range, callback, i + 1);
            }

            return true;
        }


        /// <summary>
        /// Returns the nearest value in the tree using a Euclidean distance metric.
        /// If the tree is empty the default value of T is returned.
        /// Also returns the square distance to the nearest value as an out parameter.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public T NearestL2(double[] point)
        {
            return NearestL2(point, out double d);
        }


        /// <summary>
        /// Returns the nearest value in the tree using a Euclidean distance metric.
        /// If the tree is empty the default value of T is returned.
        /// Also returns the square distance to the nearest value as an out parameter.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public T NearestL2(double[] point, out double distance)
        {
            T nearest = default(T);
            distance = double.MaxValue;
            NearestL2(_root, point, 0, ref nearest, ref distance);

            return nearest;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="i"></param>
        /// <param name="nearest"></param>
        /// <param name="distance"></param>
        private void NearestL2(Node node, double[] point, int i, ref T nearest, ref double distance)
        {
            if (node == null) return;

            // wrap dimension
            if (i == _k) i = 0;

            // update nearest if necessary
            double d = SquareDistanceL2(point, node.Point);
            if (d < distance)
            {
                nearest = node.Value;
                distance = d;
            }

            d = point[i] - node.Point[i]; // signed distance to cut plane
            if (d < 0.0)
            {
                // point is to the left, recall in left subtree first
                NearestL2(node.Left, point, i + 1, ref nearest, ref distance);

                // recall in right subtree only if necessary
                if (d * d < distance)
                    NearestL2(node.Right, point, i + 1, ref nearest, ref distance);
            }
            else
            {
                // point is to the right, recall in right subtree first
                NearestL2(node.Right, point, i + 1, ref nearest, ref distance);

                // recall in left subtree only if necessary
                if (d * d < distance)
                    NearestL2(node.Left, point, i + 1, ref nearest, ref distance);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public T NearestL1(double[] point)
        {
            return NearestL1(point, out double d);
        }


        /// <summary>
        /// Returns the nearest value in the tree using a Manhattan distance metric.
        /// If the tree is empty the default value of T is returned.
        /// Also returns the distance to the nearest value as an out parameter.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public T NearestL1(double[] point, out double distance)
        {
            T nearest = default(T);
            distance = Double.MaxValue;
            NearestL1(_root, point, 0, ref nearest, ref distance);

            return nearest;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="i"></param>
        /// <param name="nearest"></param>
        /// <param name="distance"></param>
        private void NearestL1(Node node, double[] point, int i, ref T nearest, ref double distance)
        {
            if (node == null) return;

            // wrap dimension
            if (i == _k) i = 0;

            // update nearest if necessary
            double d = DistanceL1(point, node.Point);
            if (d < distance)
            {
                nearest = node.Value;
                distance = d;
            }

            d = point[i] - node.Point[i]; // signed distance to cut plane
            if (d < 0.0)
            {
                // point is to the left, recall in left subtree first
                NearestL1(node.Left, point, i + 1, ref nearest, ref distance);

                // recall in right subtree only if necessary
                if (Math.Abs(d) < distance)
                    NearestL1(node.Right, point, i + 1, ref nearest, ref distance);
            }
            else
            {
                // point is to the right, recall in right subtree first
                NearestL1(node.Right, point, i + 1, ref nearest, ref distance);

                // recall in left subtree only if necessary
                if (Math.Abs(d) < distance)
                    NearestL1(node.Left, point, i + 1, ref nearest, ref distance); 
            }
        }


        /// <summary>
        /// Returns the nearest k values and their Euclidean distances.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public PriorityQueue<(T, double)> KNearestL2(double[] point, int k)
        {
            var result = new PriorityQueue<(T, double)>((t0, t1) => t1.Item2.CompareTo(t0.Item2), k);
            KNearestL2(_root, point, k, 0, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="k"></param>
        /// <param name="i"></param>
        /// <param name="result"></param>
        private void KNearestL2(Node node, double[] point, int k, int i, PriorityQueue<(T, double)> result)
        {
            if (node == null) return;

            // wrap dimension
            if (i == _k) i = 0;

            // add node if closer than nth element
            double d = SquareDistanceL2(point, node.Point);

            if (result.Count < k)
                result.Insert((node.Value, d));
            else if (d < result.Min.Item2)
                result.ReplaceMin((node.Value, d));

            d = point[i] - node.Point[i]; // signed distance to cut plane
            if (d < 0.0)
            {
                // point is to the left, recall in left subtree first
                KNearestL2(node.Left, point, k, i + 1, result);

                // recall in right subtree only if necessary
                if (result.Count < k || d * d < result.Min.Item2)
                    KNearestL2(node.Right, point, k, i + 1, result);
            }
            else
            {
                // point is to the right, recall in right subtree first
                KNearestL2(node.Right, point, k, i + 1, result);

                // recall in left subtree only if necessary
                if (result.Count < k || d * d < result.Min.Item2)
                    KNearestL2(node.Left, point, k, i + 1, result);
            }
        }


        /// <summary>
        /// Returns the nearest k values and their Manhattan distances.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public PriorityQueue<(T, double)> KNearestL1(double[] point, int k)
        {
            var result = new PriorityQueue<(T, double)>((t0, t1) => t1.Item2.CompareTo(t0.Item2), k);
            KNearestL1(_root, point, k, 0, result);
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <param name="k"></param>
        /// <param name="i"></param>
        /// <param name="result"></param>
        private void KNearestL1(Node node, double[] point, int k, int i, PriorityQueue<(T, double)> result)
        {
            if (node == null) return;

            // wrap dimension
            if (i == _k) i = 0;

            // add node if closer than nth element
            double d = DistanceL1(point, node.Point);

            if (result.Count < k)
                result.Insert((node.Value, d));
            else if (d < result.Min.Item2)
                result.ReplaceMin((node.Value, d));

            d = point[i] - node.Point[i]; // signed distance to cut plane
            if (d < 0.0)
            {
                // point is to the left, recall in left subtree first
                KNearestL1(node.Left, point, k, i + 1, result);

                // recall in right subtree only if necessary
                if (result.Count < k || Math.Abs(d) < result.Min.Item2)
                    KNearestL1(node.Right, point, k, i + 1, result); 
            }
            else
            {
                // point is to the right, recall in right subtree first
                KNearestL1(node.Right, point, k, i + 1, result);

                // recall in left subtree only if necessary
                if (result.Count < k || Math.Abs(d) < result.Min.Item2)
                    KNearestL1(node.Left, point, k, i + 1, result); 
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class Node
        {
            /// <summary></summary>
            public Node Left;
            /// <summary></summary>
            public Node Right;
            /// <summary></summary>
            public readonly double[] Point;
            /// <summary></summary>
            public readonly T Value;


            /// <summary>
            /// 
            /// </summary>
            public bool IsLeaf
            {
                get { return Left == null && Right == null; }
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="point"></param>
            /// <param name="value"></param>
            public Node(double[] point, T value)
            {
                Point = point;
                Value = value;
            }
        }
    }
}
