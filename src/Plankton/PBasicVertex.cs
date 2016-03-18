using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plankton;

namespace PlanktonGeoTools
{
    public class PBasicVertecList : IEnumerable<PBasicVertex>
    {
        public PBasicVertecList()
        {
            vs = new List<PBasicVertex>();
        }
        public PBasicVertecList(IEnumerable<PBasicVertex> VS)
        {
            vs = new List<PBasicVertex>(VS);
        }
        private List<PBasicVertex> vs;
        public List<string> DisplayPos()
        {
            List<string> output = new List<string>();
            vs.ForEach(delegate (PBasicVertex v) { output.Add(v.pos.ToString()); });
            return output;
        }
        public List<string> Displayenergy()
        {
            List<string> output = new List<string>();
            vs.ForEach(delegate (PBasicVertex v) { output.Add(v.energy.ToString()); });
            return output;
        }
        public List<string> DisplayLife()
        {
            List<string> output = new List<string>();
            vs.ForEach(delegate (PBasicVertex v) { output.Add(v.dead.ToString()); });
            return output;
        }
        public List<PlanktonIndexPair> CreateCollection(List<PlanktonLine> x)
        {
            List<PlanktonIndexPair> id = new List<PlanktonIndexPair>();
            vs = new List<PBasicVertex>();
            id.Add(new PlanktonIndexPair(0, 1));
            vs.Add(new PBasicVertex(x[0].From, 1));
            vs.Add(new PBasicVertex(x[0].To, 0));
            for (int i = 1; i < x.Count; i++)
            {
                bool sign1 = true;
                bool sign2 = true;
                int a = 0, b = 0;
                for (int j = 0; j < vs.Count; j++)
                {
                    if (vs[j].equalTo(x[i].From)) { sign1 = false; a = j; }
                    if (vs[j].equalTo(x[i].To)) { sign2 = false; b = j; }
                    if (!sign1 && !sign2) { break; }
                }
                if (sign1) { vs.Add(new PBasicVertex(x[i].From)); a = vs.Count - 1; }
                if (sign2) { vs.Add(new PBasicVertex(x[i].To)); b = vs.Count - 1; }
                vs[a].Add(b); vs[b].Add(a);
                id.Add(new PlanktonIndexPair(a, b));
            }
            return id;
        }
        public IEnumerator<PBasicVertex> GetEnumerator()
        {
            return this.vs.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
    public class PBasicVertex
    {
        public PlanktonXYZ pos;
        public bool dead = false;
        public List<int> refer = new List<int>();
        public float energy = 0;
        public PBasicVertex()
        {
            this.pos = PlanktonXYZ.Zero;
        }
        public PBasicVertex(PlanktonXYZ p)
        {
            pos = new PlanktonXYZ(p);
        }
        public PBasicVertex(PlanktonXYZ p, int index)
        {
            pos = new PlanktonXYZ(p);
            this.refer.Add(index);
        }
        public PBasicVertex(PlanktonXYZ p, IEnumerable<int> index)
        {
            pos = new PlanktonXYZ(p);
            this.refer.AddRange(index);
        }
        public void Add(int i)
        {
            this.refer.Add(i);
        }
        public void AddRange(IEnumerable<int> i)
        {
            this.refer.AddRange(i);
        }
        public bool equalTo(PlanktonXYZ pt)
        {
            if (pos.DistanceTo(pt) < 0.01) { return true; }
            return false;
        }
        public bool equalTo(PBasicVertex pt)
        {
            if (pos.DistanceTo(pt.pos) < 0.01) { return true; }
            return false;
        }
        /// //////////////////static
    }
}
