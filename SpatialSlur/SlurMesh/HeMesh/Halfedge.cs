﻿using System;
using System.Collections.Generic;
using SpatialSlur.SlurCore;

/*
 * Notes
 */

namespace SpatialSlur.SlurMesh
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class Halfedge<TV, TE, TF> : HeElement, IHalfedge<TV, TE, TF>
        where TV : HeVertex<TV, TE, TF>
        where TE : Halfedge<TV, TE, TF>
        where TF : HeFace<TV, TE, TF>
    {
        private TE _self; // cached downcasted ref of this instance (TODO test impact)
        private TE _prev;
        private TE _next;
        private TE _twin;
        private TV _start;
        private TF _face;


        /// <summary>
        /// 
        /// </summary>
        public Halfedge()
        {
            _self = (TE)this;
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TE Twin
        {
            get { return _twin; }
            internal set { _twin = value; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TE Older
        {
            get { return _self < _twin ? _self : _twin; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TE PrevInFace
        {
            get { return _prev; }
            internal set { _prev = value; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TE NextInFace
        {
            get { return _next; }
            internal set { _next = value; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TE PrevAtStart
        {
            get { return _prev._twin; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TE NextAtStart
        {
            get { return _twin._next; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TV Start
        {
            get { return _start; }
            internal set { _start = value; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TV End
        {
            get { return _twin._start; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public TF Face
        {
            get { return _face; }
            internal set { _face = value; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public sealed override bool IsRemoved
        {
            get { return _start == null; }
        }


        /// <summary>
        /// Returns true if the halfedge and its twin have different faces.
        /// This should always return true in client side code.
        /// </summary>
        internal bool IsValid
        {
            get { return _face != _twin._face; }
        }


        /// <summary>
        /// Returns true if this halfedge starts at a degree 1 vertex.
        /// Note this is an invalid state.
        /// </summary>
        internal bool IsAtDegree1
        {
            get { return this == NextAtStart; }
        }


        /// <summary>
        /// Returns true if this halfedge starts at a degree 2 vertex.
        /// Note this is an invalid state if the halfedge has a face.
        /// </summary>
        public bool IsAtDegree2
        {
            get { return this == NextAtStart.NextAtStart; }
        }


        /// <summary>
        /// Returns true if this halfedge starts at a degree 3 vertex.
        /// </summary>
        public bool IsAtDegree3
        {
            get { return this == NextAtStart.NextAtStart.NextAtStart; }
        }


        /// <summary>
        /// Returns true if this halfedge starts at a degree 4 vertex.
        /// </summary>
        public bool IsAtDegree4
        {
            get
            {
                var he = NextAtStart.NextAtStart;
                return this != he && this == he.NextAtStart.NextAtStart;
            }
        }


        /// <summary>
        /// Returns true if this halfedge is in a 2 sided face or hole.
        /// </summary>
        internal bool IsInDegree1
        {
            get { return this == _next; }
        }


        /// <summary>
        /// Returns true if this halfedge is in a 2 sided face or hole.
        /// </summary>
        public bool IsInDegree2
        {
            get { return this == _next._next; }
        }


        /// <summary>
        /// Returns true if this halfedge is in a 3 sided face or hole.
        /// </summary>
        public bool IsInDegree3
        {
            get { return this == _next._next._next; }
        }


        /// <summary>
        /// Returns true if this halfedge is in a 4-sided face or hole.
        /// </summary>
        public bool IsInDegree4
        {
            get
            {
                var he = _next._next;
                return this != he && this == he._next._next;
            }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public bool IsBoundary
        {
            get { return _face == null || _twin._face == null; }
        }


        /// <summary>
        /// Returns true this halfedge spans between 2 non-consecutive boundary vertices.
        /// </summary>
        public bool IsBridge
        {
            get { return _start.IsBoundary && _twin._start.IsBoundary && _face != null && _twin._face != null; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public bool IsHole
        {
            get { return _face == null; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public bool IsFirstAtStart
        {
            get { return this == _start.FirstOut; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public bool IsFirstInFace
        {
            get { return this == _face.First; }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<TE> CirculateStart
        {
            get
            {
                var he = _self;

                do
                {
                    yield return he;
                    he = he.NextAtStart;
                } while (he != this);
            }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<TE> CirculateEnd
        {
            get
            {
                var he = _self;

                do
                {
                    yield return he;
                    he = he.NextInFace.Twin;
                } while (he != this);
            }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<TE> CirculateFace
        {
            get
            {
                var he = _self;

                do
                {
                    yield return he;
                    he = he.NextInFace;
                } while (he != this);
            }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<TE> ConnectedPairs
        {
            get
            {
                yield return PrevInFace;
                yield return NextInFace;
                yield return Twin.PrevInFace;
                yield return Twin.NextInFace;
            }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TE NextBoundaryAtStart
        {
            get
            {
                var he = NextAtStart;

                do
                {
                    if (he.Face == null) return he;
                    he = he.NextAtStart;
                } while (he != this);

                return null;
            }
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TE NextBoundaryInFace
        {
            get
            {
                var he1 = NextInFace;

                do
                {
                    if (he1.Twin.Face == null) return he1;
                    he1 = he1.NextInFace;
                } while (he1 != this);

                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        internal void Remove()
        {
            _start = _twin._start = null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        internal void MakeConsecutive(TE other)
        {
            _next = other;
            other._prev = _self;
        }


        /// <summary>
        /// 
        /// </summary>
        public void MakeFirstInFace()
        {
            _face.First = _self;
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int CountEdgesAtStart()
        {
            var he = this;
            int count = 0;

            do
            {
                count++;
                he = he.NextAtStart;
            } while (he != this);

            return count;
        }


        /// <inheritdoc/>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int CountEdgesInFace()
        {
            var he = this;
            int count = 0;

            do
            {
                count++;
                he = he.NextInFace;
            } while (he != this);

            return count;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public TE OffsetAtStart(int count)
        {
            var he = _self;

            if (count < 0)
            {
                for (int i = 0; i < count; i++)
                    he = he.PrevAtStart;
            }
            else
            {
                for (int i = 0; i < count; i++)
                    he = he.NextAtStart;
            }

            return he;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public TE OffsetInFace(int count)
        {
            var he = _self;

            if (count < 0)
            {
                for (int i = 0; i < count; i++)
                    he = he.PrevInFace;
            }
            else
            {
                for (int i = 0; i < count; i++)
                    he = he.NextInFace;
            }

            return he;
        }


        /// <summary>
        /// 
        /// </summary>
        internal void Bypass()
        {
            if (IsAtDegree1)
            {
                Start.Remove();
                return;
            }

            var he = NextAtStart;

            if (Start.FirstOut == this)
                Start.FirstOut = he;

            PrevInFace.MakeConsecutive(he);
        }


        /*
        /// <summary>
        /// Starting from this halfedge, enumerates through face vertices in groups of 3 according to the given triangulation mode.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(TV, TV, TV)> GetFaceTriangles(TriangulationMode mode)
        {
            switch (mode)
            {
                case TriangulationMode.Fan:
                    return GetFaceTrianglesFan();
                case TriangulationMode.Strip:
                    return GetFaceTrianglesStrip();
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<(TV, TV, TV)> GetFaceTrianglesFan()
        {
            var he = NextInFace;

            var v0 = Start;
            var v1 = he.Start;

            do
            {
                he = he.NextInFace;
                var v2 = he.Start;

                if (v2 == v0) break;

                yield return (v0, v1, v2);
                v1 = v2;
            } while (true);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<(TV, TV, TV)> GetFaceTrianglesStrip()
        {
            var he0 = _self;
            var v0 = Start;

            var he1 = NextInFace;
            var v1 = he1.Start;

            do
            {
                he1 = he1.NextInFace;
                var v2 = he1.Start;

                if (v2 == v0) break;
                yield return (v0, v1, v2);

                he0 = he0.PrevInFace;
                var v3 = he0.Start;

                if (v2 == v3) break;
                yield return (v0, v2, v3);
              
                v0 = v3;
                v1 = v2;
            } while (true);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<(TV, TV, TV)> GetFaceTrianglesPoke()
        {
            var he = _self;
            var v0 = he.Start;

            do
            {
                he = he.NextInFace;
                var v1 = he.Start;

                yield return (v0, v1, null);
                v0 = v1;
            } while (he != _self);
        }
   

        /// <summary>
        /// Starting from this halfedge, enumerates through face vertices in groups of 4 according to the given triangulation mode.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(TV, TV, TV, TV)> GetFaceQuads(QuadrangulationMode mode)
        {
            switch (mode)
            {
                case QuadrangulationMode.Fan:
                    return GetFaceQuadsFan();
                case QuadrangulationMode.Strip:
                    return GetFaceQuadsStrip();
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<(TV, TV, TV, TV)> GetFaceQuadsFan()
        {
            var he = NextInFace;

            var v0 = Start;
            var v1 = he.Start;

            do
            {
                he = he.NextInFace;
                var v2 = he.Start;
                
                if (v2 == v0) break;

                he = he.NextInFace;
                var v3 = he.Start;
                
                if(v3 == v0)
                {
                    yield return (v0, v1, v2, null);
                    break;
                }

                yield return (v0, v1, v2, v3);
                v1 = v3;
            } while (true);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<(TV, TV, TV, TV)> GetFaceQuadsStrip()
        {
            var he0 = _self;
            var v0 = Start;

            var he1 = NextInFace;
            var v1 = he1.Start;

            do
            {
                he1 = he1.NextInFace;
                var v2 = he1.Start;

                if (v2 == v0) break;

                he0 = he0.PrevInFace;
                var v3 = he0.Start;

                if(v2 == v3)
                {
                    yield return (v0, v1, v2, null);
                    break;
                }

                yield return (v0, v1, v2, v3);
                v0 = v3;
                v1 = v2;
            } while (true);
        }
        
      
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<(TV, TV, TV, TV)> GetFaceQuadsPoke()
        {
            var he = _self;
            var v0 = he.Start;

            do
            {
                he = he.NextInFace;
                var v1 = he.Start;

                if(he == _self)
                {
                    yield return (v0, v1, null, null);
                    break;
                }
                
                he = he.NextInFace;
                var v2 = he.Start;

                yield return (v0, v1, v2, null);
                v0 = v2;
            } while (he != _self);
        }
        */

        #region Explicit interface implementations

        /// <summary>
        /// 
        /// </summary>
        bool IHalfedge<TV, TE>.IsAtDegree1
        {
            get { return IsAtDegree1; }
        }


        /// <summary>
        /// 
        /// </summary>
        bool IHalfedge<TV, TE, TF>.IsInDegree1
        {
            get { return IsInDegree1; }
        }


        /// <summary>
        /// 
        /// </summary>
        TE IHalfedge<TE>.Next
        {
            get { return _next; }
        }


        /// <summary>
        /// 
        /// </summary>
        TE IHalfedge<TE>.Previous
        {
            get { return _prev; }
        }

        #endregion
    }
}
