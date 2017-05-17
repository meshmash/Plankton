using System;
using Grasshopper.Kernel;

namespace PlanktonGh
{
  public class PlanktonAssemblyInfo : GH_AssemblyInfo
  {
    public override string Name
    {
      get
      {
        return "plankton";
      }
    }

    public override string Version
    {
      get
      {
        return "0.4.2";
      }
    }

    public override string AuthorContact
    {
      get
      {
        return "http://www.grasshopper3d.com/group/plankton";
      }
    }

    public override string AuthorName
    {
      get
      {
        return "Daniel Piker and Will Pearson";
      }
    }
  }
}
