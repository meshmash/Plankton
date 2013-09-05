using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Input.Custom;
using PlanktonGh.Properties;

namespace PlanktonGh
{
    public class GH_PlanktonMeshParam : GH_PersistentGeometryParam<GH_PlanktonMesh>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public GH_PlanktonMeshParam()
            : base(new GH_InstanceDescription("PlanktonMesh", "PMesh", "Represents a list of 3D ngonal halfedge meshes", "Params", "Geometry"))
        { }

        protected override GH_PlanktonMesh InstantiateT()
        {
            return new GH_PlanktonMesh(null);
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_PlanktonMesh> values)
        {
            GetObject go = new SpecialPolygonsGetObject();
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Mesh;
            //go.GeometryAttributeFilter = GeometryAttributeFilter.ClosedMesh;

            if (go.GetMultiple(1, 0) != Rhino.Input.GetResult.Object)
                return GH_GetterResult.cancel;

            if (values == null) values = new List<GH_PlanktonMesh>();
            
            for (int i=0; i<go.ObjectCount; i++)
              values.Add(HandleOne(go, i));

            return GH_GetterResult.success;
        }

        class SpecialPolygonsGetObject : GetObject
        {
            public override bool CustomGeometryFilter(
                Rhino.DocObjects.RhinoObject rhObject,
                Rhino.Geometry.GeometryBase geometry,
                Rhino.Geometry.ComponentIndex componentIndex)
            {
                var m = geometry as Mesh;
                if (m == null) return false;

                return m.IsValid;
            }
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_PlanktonMesh value)
        {
            GetObject go = new SpecialPolygonsGetObject();
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Mesh;
            //go.GeometryAttributeFilter = GeometryAttributeFilter.ClosedMesh;
            
            if (go.Get() != Rhino.Input.GetResult.Object)
                return GH_GetterResult.cancel;

            var m = HandleOne(go, 0);
            
            value = m;

            return GH_GetterResult.success;
        }

        private static GH_PlanktonMesh HandleOne(GetObject go, int index)
        {
            var o = go.Object(index);
            var m = o.Mesh();

            var p = RhinoSupport.ToPlanktonMesh(m);

            return new GH_PlanktonMesh(p) { ReferenceID = o.ObjectId };
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a484c1c6-bb88-4507-b650-58aadebda4c1"); }
        }

        public BoundingBox ClippingBox
        {
            get { return Preview_ComputeClippingBox(); }
        }

        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (args.Document.PreviewMode == GH_PreviewMode.Shaded &&
                args.Display.SupportsShading)
            {
                Preview_DrawMeshes(args);
            }
        }

        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            switch (args.Document.PreviewMode)
            {
                case GH_PreviewMode.Wireframe:
                    Preview_DrawWires(args);
                    break;
                case GH_PreviewMode.Shaded:
                    if (CentralSettings.PreviewMeshEdges)
                    {
                        Preview_DrawWires(args);
                    }
                    break;
            }
        }

        bool _hidden;
        public bool Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;
            }
        }

        public bool IsPreviewCapable
        {
            get { return true; }
        }

        public void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            if (att == null)
            {
                att = doc.CreateDefaultAttributes();
            }
            foreach (IGH_BakeAwareData item in m_data)
            {
                if (item != null)
                {
                    Guid id;
                    if (item.BakeGeometry(doc, att, out id))
                    {
                        obj_ids.Add(id);
                    }
                }
            }
        }

        public void BakeGeometry(Rhino.RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, null, obj_ids);
        }

        public bool IsBakeCapable
        {
            get { return !m_data.IsEmpty; }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.tertiary;
            }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.plankton;
            }
        }
    }
}