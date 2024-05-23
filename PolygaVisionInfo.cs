using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace PolygaVision
{
    public class PolygaVisionInfo : GH_AssemblyInfo
    {
        public override string Name => "PolygaVision";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("09547938-77ca-4580-9b38-1c4a4e5f5dfa");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}