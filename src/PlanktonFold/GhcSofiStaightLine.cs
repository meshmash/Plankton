using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using PlanktonGh;
using Plankton;
using PlanktonFold;
using Grasshopper;
using Grasshopper.Kernel.Data;

namespace PlanktonFold
{
    public class GhcSofiStaightLine : GH_Component
    {

        public GhcSofiStaightLine()
          : base("GhcSofiStaightLine", "Sofi_StreightLineFold",
              "generate the geometry for straight line folding sofistik simulation, including KNOT, QUAD, Stab(beam), SEIL(cable), constraint",
              "MT", "Sofistik")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0
            pManager.AddGenericParameter("Mesh", "Mesh", "crease pattern", GH_ParamAccess.item);

            // 1
            pManager.AddNumberParameter("Hinge Width", "Hinge Width", "Hinge Width", GH_ParamAccess.item);

            // 2
            // if the input mesh is non flat, MV assignment is fixed, so this input is redundant.
            pManager.AddGenericParameter("MV Assignment", "MV Assignment", "MV Assignment", GH_ParamAccess.list);
            pManager[2].Optional = true;

            // 3
            pManager.AddNumberParameter("Actuation Assignment", "Actuation Assignment", "Actuation Assignment", GH_ParamAccess.list); // 1 as actuation, 0 as no actuation
            pManager[3].Optional = true;

            // 4
            pManager.AddNumberParameter("Subdivide Count", "Subdivide Count", "Subdivide Count", GH_ParamAccess.item);

            // 5 

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0
            pManager.AddPointParameter("Cable Start Nr", "Cable Start Nr", "Cable Start Nr", GH_ParamAccess.list);
             
            // 1
            pManager.AddPointParameter("Cable End Nr", "Cable End Nr", "Cable End Nr", GH_ParamAccess.list); 

            // 2
            pManager.AddPointParameter("Beam Start Nr", "Beam Start Nr", "Beam Start Nr", GH_ParamAccess.list);

            // 3
            pManager.AddPointParameter("Beam End Nr", "Beam End Nr", "Beam End Nr", GH_ParamAccess.list);

            // 4
            pManager.AddGenericParameter("FoldLines", "FoldLines", "FoldLines", GH_ParamAccess.tree);

            // 5
            pManager.AddGenericParameter("Points Tree", "Points Tree", "Points Tree", GH_ParamAccess.tree);

            // 6
            pManager.AddMeshParameter("Subdivided Mesh", "Subdivided Mesh", "Subdivided Mesh", GH_ParamAccess.item);

            // 7 
            pManager.AddPointParameter("Moved Points", "Moved Points", "Moved Points", GH_ParamAccess.tree);

            // 8 
            pManager.AddLineParameter("Cable Tree", "Cable Tree", "Cable Tree", GH_ParamAccess.tree);

            // 9
            pManager.AddLineParameter("Short Beam Vertical", "Short Beam Vertical", "Short Beam Vertical", GH_ParamAccess.tree);

            // 10
            pManager.AddLineParameter("Short Beam Level", "Short Beam Level", "Short Beam Level", GH_ParamAccess.tree);

            // 11
            pManager.AddPointParameter("Fix Points on Mesh", "Fix Points on Mesh", "Fix Points on Mesh", GH_ParamAccess.tree);

            // 12 
           

        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = new Mesh();
            DA.GetData<Mesh>("Mesh", ref M);
            PlanktonMesh P = RhinoSupport.ToPlanktonMesh(M);

            double hingeWidth = 0.1;
            DA.GetData<double>("Hinge Width", ref hingeWidth);

            List<string> mvAssignment = new List<string>();
            DA.GetDataList<string>("MV Assignment", mvAssignment);
            
            List<double> actuationAssignment = new List<double>();
            DA.GetDataList<double>("Actuation Assignment", actuationAssignment);

            // ======================fix points on the mesh plane======================

            Vector3d planeNormal = RhinoSupport.GetFaceNormal(P, 0).First().UnitTangent; // a unit vector of the mesh surface vector
            List<Line> foldLines = RhinoSupport.GetInnerEdges(P);
            List<Vector3d> moveVectors = foldLines.Select(o => Vector3d.CrossProduct(o.UnitTangent, planeNormal)).ToList();
            int foldLineCount = foldLines.Count / 2;
            int foldlineDivide = 5; // how many points on one fold line

            //each foldline becomes 3 lines 
            List<List<Line>> linesToSubdivide = new List<List<Line>>();
            List<foldLinePoints> allFoldLinePts = new List<foldLinePoints>();
            DataTree<Line> linesTree = new DataTree<Line>();
            DataTree<Point3d> pointsTree = new DataTree<Point3d>();

            // loop for each foldline
            for (int i = 0; i < foldLineCount ; i++)
            {
                // add first line
                List<Line> threeLines = new List<Line>();

                // construct translation movements
                Line l1 = foldLines[2 * i];
                Line l2 = foldLines[2 * i + 1];

                l1.Transform(Transform.Translation(moveVectors[2 * i] * (hingeWidth/2.0)));
                l2.Transform(Transform.Translation(moveVectors[2 * i  + 1] * (hingeWidth / 2.0)));

                // add three lines to the list
                threeLines.Add( new Line(l1.PointAt(0.1), l1.PointAt(0.9) ));
                threeLines.Add(new Line(foldLines[2 * i].PointAt(0.1), foldLines[2 * i].PointAt(0.9))); // duplicate the line 
                threeLines.Add(new Line(l2.PointAt(0.9), l2.PointAt(0.1)));

                linesToSubdivide.Add(threeLines);

                // put three lines in a node in the data tree
                GH_Path iPath = new GH_Path(i);
                linesTree.AddRange(threeLines, iPath);
                foldLinePoints iFoldLinePts = new foldLinePoints();
                iFoldLinePts.foldLinePts = new List<List<Point3d>>();

                for (int j = 0; j <= foldlineDivide; j++)
                {
                    // where the points are located on the line?
                    double p = j / (double)foldlineDivide;
                    List<Point3d> threePts = new List<Point3d>();

                    // append 3 points on position p/foldlineDivide
                    //threeLines = threeLines.Select(o => o.ToNurbsCurve()).ToList();

                    threePts.Add(threeLines[0].PointAt(p));
                    threePts.Add(threeLines[1].PointAt(p));
                    threePts.Add(threeLines[2].PointAt(p));

                    GH_Path ijPath = new GH_Path(i, j);

                    pointsTree.AddRange(threePts, ijPath);
                    iFoldLinePts.foldLinePts.Add(threePts);
                    allFoldLinePts.Add(iFoldLinePts);
                }
            }

            // flatten into a list
            DA.SetDataTree(4, linesTree);
            DA.SetDataTree(5, pointsTree);

            // Mesh Subdivision
            // trim points tree in a list for the mesh adjustment

            List<Point3d> fixPoints = pointsTree.AllData();

            double maxSubdivision = 0.0;
            DA.GetData<double>("Subdivide Count", ref maxSubdivision);

            // divide
            int count = 0;
            do
            {
                P = RhinoSupport.QuadSubdivide(P);
                count += 1;
            } while (count < maxSubdivision);

            // move
            RhinoSupport.MoveVertices(P, fixPoints);

            Mesh subdividedMesh = RhinoSupport.ToRhinoMesh(P);
            DA.SetData("Subdivided Mesh", subdividedMesh);


            // =============================move points up or down according to MV assignment=============================
            //List<foldLinePoints> hingedPOintsMoved = new List<foldLinePoints>();
            DataTree<Point3d> movedPointsTree = new DataTree<Point3d>();

            DataTree<Line> cablesTree = new DataTree<Line>();
            DataTree<int> cableStartNr = new DataTree<int>();
            DataTree<int> cableEndNr = new DataTree<int>();

            DataTree<Line> shortbeamTreeNormal = new DataTree<Line>();
            DataTree<Line> shortbeamTreeLevel = new DataTree<Line>();

            int fineMehsVertixCount = subdividedMesh.Vertices.Count; // when outputing the cable 

            for (int i = 0; i < mvAssignment.Count; i++)
            {
                for (int j = 0; j <= foldlineDivide; j++)
                {
                    GH_Path ijPath = new GH_Path(i, j);
                    List<Point3d> pts = pointsTree.Branch(ijPath);
                    
                    // if -1, valley, move upwards
                    if (mvAssignment[i] == "-1")
                    {
                        List<Point3d> movedPts = new List<Point3d>();

                        foreach (Point3d pt in pts)
                        {
                            Point3d movedPt = pt;
                            movedPt.Transform(Transform.Translation(planeNormal*0.2));
                            movedPts.Add(movedPt);
                        }
                        movedPointsTree.AddRange(movedPts, ijPath);
                        
                        Line cable1 = new Line(movedPts[0], movedPts[1]);
                        Line cable2 = new Line(movedPts[1], movedPts[2]);
                        cablesTree.AddRange(new List<Line> { cable1, cable2 }, ijPath);
                    }
                        
                    // if +1, mountain, move downwards
                    else if (mvAssignment[i] == "1")
                    {
                        List<Point3d> movedPts = new List<Point3d>();

                        foreach (Point3d pt in pts)
                        {
                            Point3d movedPt = pt;
                            movedPt.Transform(Transform.Translation(planeNormal * 0.2 * (-1)));
                            movedPts.Add(movedPt);
                        }
                        movedPointsTree.AddRange(movedPts, ijPath);
                        Line cable1 = new Line(movedPts[0], movedPts[1]);
                        Line cable2 = new Line(movedPts[1], movedPts[2]);

                        cablesTree.AddRange(new List<Line> { cable1, cable2 }, ijPath);
                    }
                }
            }

            // loop to create vertical beam connection
            for (int i = 0; i < foldLineCount; i++)
            {
                for (int j = 0; j <= foldlineDivide; j++)
                {
                    GH_Path ijPath = new GH_Path(i, j);
                    List<Point3d> ptsOnMesh = pointsTree.Branch(ijPath);
                    List<Point3d> ptsMoved = movedPointsTree.Branch(ijPath);
                    List<Line> shortbeamN = new List<Line>();
                    for (int k = 0; k < ptsOnMesh.Count; k++)
                        shortbeamN.Add(new Line(ptsOnMesh[k], ptsMoved[k]));
                    shortbeamTreeNormal.AddRange(shortbeamN, ijPath);

                } 
            }

            // loop to create level beam connection
            for (int i = 0; i < foldLineCount; i++)
            {
                for (int j = 0; j < foldlineDivide; j++)
                {
                    GH_Path ijPath = new GH_Path(i, j);
                    GH_Path ijjPath = new GH_Path(i, j+1);

                    List<Point3d> ptsMovedij = movedPointsTree.Branch(ijPath);
                    List<Point3d> ptsMovedijj = movedPointsTree.Branch(ijjPath);

                    for (int k = 0; k < ptsMovedij.Count; k++)
                    {
                        List<Line> shortbeamL = new List<Line>();
                        shortbeamL.Add(new Line(ptsMovedij[k], ptsMovedijj[k]));
                        shortbeamTreeLevel.AddRange(shortbeamL, ijPath);
                    }


                }
            }

            DA.SetDataTree(7, movedPointsTree);
            DA.SetDataTree(8, cablesTree);
            DA.SetDataTree(9, shortbeamTreeNormal);
            DA.SetDataTree(10, shortbeamTreeLevel);
            DA.SetDataTree(11, pointsTree);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{4170ec9e-4508-43c7-802c-0b00b4d3969d}"); }
        }

        public struct foldLinePoints
        {
            public List<List<Point3d>> foldLinePts;

        }
    }
}