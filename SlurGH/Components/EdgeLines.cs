﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using SpatialSlur.SlurCore;
using SpatialSlur.SlurRhino;
using SpatialSlur.SlurMesh;

using SlurSketchGH.Grasshopper.Types;
using SlurSketchGH.Grasshopper.Params;

/*
 * Notes
 */

namespace SlurSketchGH.Grasshopper.Components
{
    public class EdgeLines : GH_Component
    {
        /// <summary>
        /// 
        /// </summary>
        public EdgeLines()
          : base("Edge Lines", "EdgeLns",
              "Returns a line for each edge in a given mesh",
              "SpatialSlur", "Mesh")
        {
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new HeMeshParam(), "heMesh", "heMesh", "Mesh to extract from", GH_ParamAccess.item);
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("result", "result", "Edge lines", GH_ParamAccess.list);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            HeMesh3d mesh = null;
            if (!DA.GetData(0, ref mesh)) return;

            var hedges = mesh.Topology.Halfedges;
            var lines = new Line[hedges.Count >> 1];
            hedges.GetEdgeLines(mesh.VertexPositions, lines);

            DA.SetDataList(0, lines.Select(ln => new GH_Line(ln)));
        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return null; }
        }


        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{D3EBB8A8-08D5-47FB-AD7A-275C9A82C4BD}"); }
        }
    }
}

