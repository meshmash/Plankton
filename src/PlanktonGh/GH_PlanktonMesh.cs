using System;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using Rhino.Geometry;
using Plankton;

namespace PlanktonGh
{
    public class GH_PlanktonMesh : GH_GeometricGoo<PlanktonMesh>,
        IGH_BakeAwareData, IGH_PreviewData, IGH_PreviewMeshData
    {
        Guid reference;
        BoundingBox _b = BoundingBox.Unset;
        Polyline[] _polylines;
        Mesh _mesh;

        public GH_PlanktonMesh() : this(null)
        {
        }

        public GH_PlanktonMesh(PlanktonMesh mesh)
        {
            m_value = mesh;

            ClearCaches();
        }

        public override PlanktonMesh Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
                ClearCaches();
            }
        }

        public override Guid ReferenceID
        {
            get
            {
                return reference;
            }
            set
            {
                reference = value;
            }
        }

        public override Rhino.Geometry.BoundingBox Boundingbox
        {
            get
            {
                if (m_value != null && !_b.IsValid)
                {
                    _b = new BoundingBox(m_value.Vertices.Select(v => v.ToPoint3d()));
                }
                return _b;
            }
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            if(m_value == null) return null;

            return new GH_PlanktonMesh(m_value == null ? null : new PlanktonMesh(m_value)) { ReferenceID = ReferenceID };
        }

        public override Rhino.Geometry.BoundingBox GetBoundingBox(Rhino.Geometry.Transform xform)
        {
            var b = Boundingbox;
            b.Transform(xform);
            return b;
        }

        public override IGH_GeometricGoo Morph(Rhino.Geometry.SpaceMorph xmorph)
        {
            if (m_value != null)
            {
                var m = new PlanktonMesh(m_value);

                foreach (var v in m.Vertices)
                {
                    Point3d p = new Point3d(v.X, v.Y, v.Z);
                    p = xmorph.MorphPoint(p);

                    v.X = (float)p.X;
                    v.Y = (float)p.Y;
                    v.Z = (float)p.Z;
                }

                return new GH_PlanktonMesh(m);
            }
            else
                return new GH_PlanktonMesh(null);
        }

        public override IGH_GeometricGoo Transform(Rhino.Geometry.Transform xform)
        {
            if (m_value != null)
            {
                var m = new PlanktonMesh(m_value);

                foreach (var v in m.Vertices)
                {
                    Point3f p = new Point3f(v.X, v.Y, v.Z);
                    p.Transform(xform);

                    v.X = p.X;
                    v.Y = p.Y;
                    v.Z = p.Z;
                }

                return new GH_PlanktonMesh(m);
            }
            else
                return new GH_PlanktonMesh(null);
        }

        public override string ToString()
        {
            if (m_value == null)
                return "<Null mesh>";
            else return m_value.ToString();
        }

        public override string TypeDescription
        {
            get { return "N-gonal Halfedge Mesh provided by Plankton"; }
        }

        public override string TypeName
        {
            get { return "PlanktonMesh"; }
        }

        public bool BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, out Guid obj_guid)
        {
            if (_polylines == null)
                ClearCaches();

            obj_guid = Guid.Empty;

            if (_polylines == null) return false;

            for (int i = 0; i < _polylines.Length; i++)
                doc.Objects.AddPolyline(_polylines[i]);

            return true;
        }



        #region IGH_PreviewData Members

        public BoundingBox ClippingBox
        {
            get { return Boundingbox; }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (this.m_value == null || _polylines == null)
                return;

            if (args.Pipeline.SupportsShading)
            {
                var c = args.Material.Diffuse;
                c = System.Drawing.Color.FromArgb((int)(args.Material.Transparency * 255),
                    c);

                args.Pipeline.DrawMeshShaded(_mesh, args.Material);
            }
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (this.m_value == null || _polylines == null)
                return;

            for (int i = 0; i < _polylines.Length; i++)
                args.Pipeline.DrawPolygon(_polylines[i], args.Color, false);
        }

        #endregion

        #region IGH_PreviewMeshData Members

        public void DestroyPreviewMeshes()
        {
            m_value = null;
        }

        public Mesh[] GetPreviewMeshes()
        {
            if (m_value == null)
            {
                _mesh = null;
                return null;
            }

            if (_mesh == null) _mesh = RhinoSupport.ToRhinoMesh(m_value);

            return new Mesh[]
            {
                _mesh,
            };
        }

        #endregion

        public override bool LoadGeometry(Rhino.RhinoDoc doc)
        {
            RhinoObject obj = doc.Objects.Find(ReferenceID);
            if (obj == null)
            {
                return false;
            }
            //if (obj.Geometry.ObjectType == ObjectType.Curve)
            //{
            //    var c = (Curve)obj.Geometry;
            //
            //    m_value = RhinoMeshSupport.ExtractTMesh(c);
            //    ClearCaches();
            //    return true;
            //}
            if (obj.Geometry.ObjectType == ObjectType.Mesh)
            {
                var m = (Mesh)obj.Geometry;

                m_value = RhinoSupport.ToPlanktonMesh(m);
                ClearCaches();
                return true;
            }
            return false;
        }

        public override void ClearCaches()
        {
            //base.ClearCaches();

            if (m_value == null)
            {
                _polylines = null;
                _b = BoundingBox.Empty;
                _mesh = null;
            }
            else
            {
                _polylines = RhinoSupport.ToPolylines(m_value);

                _mesh = RhinoSupport.ToRhinoMesh(m_value);
            }
        }

        //public override IGH_GooProxy EmitProxy()
        //{
        //    return new GH_PlanktonMeshProxy(this);
        //}

        public override bool CastFrom(object source)
        {
            if(source == null)
            {
                m_value = null;
                ClearCaches();
                return true;
            }

            if (source is GH_GeometricGoo<Mesh>)
            {
                source = ((GH_GeometricGoo<Mesh>)source).Value;
            }
            else if (source is GH_GeometricGoo<Curve>)
            {
                source = ((GH_GeometricGoo<Curve>)source).Value;
            }

            if (source is PlanktonMesh)
            {
                m_value = source as PlanktonMesh;
                ClearCaches();
                return true;
            }
            else if (source is Mesh)
            {
                m_value = RhinoSupport.ToPlanktonMesh((Mesh)source);
                ClearCaches();
                return true;
            }
            //else if (source is Curve)
            //{
            //    m_value = RhinoMeshSupport.ExtractTMesh((Curve)source);
            //    ClearCaches();
            //    return true;
            //}
            //else if (source is Grasshopper.Kernel.Types.GH_Curve)
            //{
            //    m_value = RhinoMeshSupport.ExtractTMesh((Curve)source);
            //    ClearCaches();
            //    return true;
            //}

            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(out Q target)
        {
            if (typeof(Q) == typeof(Mesh) || typeof(Q) == typeof(GeometryBase))
            {
                target = (Q)(object)RhinoSupport.ToRhinoMesh(m_value);
                return true;
            }
            if (typeof(Q) == (typeof(GH_Mesh)))
            {
                target = (Q)(object)new GH_Mesh(RhinoSupport.ToRhinoMesh(m_value));
                return true;
            }
            if (typeof(Q) == typeof(PlanktonMesh))
            {
                target = (Q)(object)m_value;
                return true;
            }

            return base.CastTo<Q>(out target);
        }

        //public override bool Read(GH_IO.Serialization.GH_IReader reader)
        //{
        //    var b = base.Read(reader);
        //
        //    var t = reader.GetString("PlanktonMesh");
        //    m_value = Turtle.Serialization.Persistance.Read(new StringReader(t));
        //
        //    return b;
        //}

        //public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        //{
        //    if(m_value != null)
        //    {
        //        StringWriter sw = new StringWriter();
        //        Turtle.Serialization.Persistance.Write(m_value, sw);
        //        sw.Flush();
        //        var t = sw.ToString();
        //
        //        writer.SetString("PlanktonMesh", t);
        //    }
        //
        //    return base.Write(writer);
        //}

        public override object ScriptVariable()
        {
            return Value;
        }
    }
}
