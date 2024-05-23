using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Color = System.Drawing.Color;
using SBSDKNet3;

namespace PolygaVision
{


public class PolygaVision : GH_Component
{



private SBScanner scanner = null;
List<GH_Point> pointCloud = new List<GH_Point>();
List<GH_Colour> colors = new List<GH_Colour>();
double maxExposure = 1000.0;
double minExposure = 0.0;

/// <summary>
/// Each implementation of GH_Component must provide a public 
/// constructor without any arguments.
/// Category represents the Tab in which the component will appear, 
/// Subcategory the panel. If you use non-existing tab or panel names, 
/// new tabs/panels will automatically be created.
/// </summary>
     public PolygaVision()
  : base("PolygaVision", "PV",
    "Description",
    "Params", "Input")
{
}

public override Guid ComponentGuid => new Guid("dab050ad-dd47-4429-933a-801a647036ba");


//protected override System.Drawing.Bitmap Icon => Properties.Resources.AzureKinectIcon;


/// <summary>
/// Registers all the input parameters for this component.
/// </summary>
protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
{
    pManager.AddBooleanParameter("Enabled", "E", "Enable the camera", GH_ParamAccess.item, false);
    pManager.AddBooleanParameter("Trigger", "T", "Takes new capture if true", GH_ParamAccess.item, false);
    pManager.AddBooleanParameter("Color", "C", "Captures color if true", GH_ParamAccess.item, false);
    pManager.AddNumberParameter("Color Exposure", "CE", "Exposure of color capture", GH_ParamAccess.item, 2);
    pManager.AddNumberParameter("Scanner Exposure", "SE", "Exposure of scanner", GH_ParamAccess.item, 2);

}

/// <summary>
/// Registers all the output parameters for this component.
/// </summary>
protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
{
    //pManager.AddGenericParameter("Point Cloud", "CC", "The point cloud", GH_ParamAccess.item);
    pManager.AddPointParameter("Points", "P", "The points", GH_ParamAccess.list);
    pManager.AddColourParameter("Colors", "C", "The colors", GH_ParamAccess.list);
}




/// <summary>
/// This is the method that actually does the work.
/// </summary>
/// <param name="DA">The DA object can be used to retrieve data from input parameters and 
/// to store data in output parameters.</param>
protected override void SolveInstance(IGH_DataAccess DA)
{
    bool enabled = false;
    bool trigger = false;
    bool captureColor = false;
    double colorExposure = 2000;
    double scannerExposure = 10;


    if (!DA.GetData(0, ref enabled)) return;
    if (!DA.GetData(1, ref trigger)) return;
    if (!DA.GetData(2, ref captureColor)) return;
    if (!DA.GetData(3, ref colorExposure)) return;
    if (!DA.GetData(3, ref colorExposure)) return;
    if (!DA.GetData(4, ref scannerExposure)) return;

    if (!enabled)
    {
        if (scanner != null)
            scanner.disconnect();
        return;
    }


    if (scanner == null)
    {
        List<SBDeviceInfo> devList = SBFactory.getDevices();
        if (devList.Count == 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No camera found.");
            return;
        }
        scanner = SBFactory.createDevice(devList[0]);
    }
    if (!scanner.isConnected()) { 
        SBStatus status = scanner.connect();
        if (status != SBStatus.OK)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Status not ok: " + status);
            return;
        }
    }

    if (scannerExposure< minExposure || scannerExposure > maxExposure)
    {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Camera exposure needs to be between: " + minExposure + "-"+maxExposure);
        return;
    }

    if (trigger)
    {
        SBMesh mesh = new SBMesh();

        SBProcessParams processParams = new SBProcessParams();
        SBCaptureParams captureParams = new SBCaptureParams();

        captureParams.textureEnable = captureColor;
        captureParams.textureExposure = colorExposure;

        scanner.setCameraExposure(scannerExposure);

        scanner.scan(out mesh, processParams, captureParams);
        if (mesh == null)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to  get mesh");
        }
        else
        {
            var vertices = mesh.getVertices();
            var colorsReturned = mesh.getVertexColors();

            pointCloud = new List<GH_Point>();
            colors = new List<GH_Colour>();

            for (var i =0; i<vertices.Count; i++)
            {
                pointCloud.Add(new GH_Point(new Point3d(vertices[i].x, vertices[i].y, vertices[i].z)));
                colors.Add(new GH_Colour(Color.FromArgb(colorsReturned[i].r, colorsReturned[i].g, colorsReturned[i].b)));
            }
        }




    }

    DA.SetDataList(0, pointCloud);
    DA.SetDataList(1, colors);

}

public override void RemovedFromDocument(GH_Document document)
{
    base.RemovedFromDocument(document);
    if (scanner != null)
    {
        scanner.disconnect();
        scanner.Dispose();
        scanner = null;
    }
}
}
}
