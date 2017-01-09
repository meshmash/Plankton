using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace PlanktonFold
{
    public class PlanktonFoldInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "PlanktonFold";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("582fff2e-e6b5-44d3-a4b0-7ee164fa8087");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
