using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;
using Newtonsoft.Json;
using Autodesk.AutoCAD.ApplicationServices;

using RestSharp;
using Autodesk.AutoCAD.Geometry;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;


[assembly: CommandClass(typeof(PackageNetPlugin.Class1))]

namespace PackageNetPlugin
{ 
    public class Class1
    {   
        private void buildGEOImage()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var db = doc.Database;
            var ed = doc.Editor; 

            // from 
            //http://through-the-interface.typepad.com/through_the_interface/2014/06/attaching-geo-location-data-to-an-autocad-drawing-using-net.html 
            try
            {
                var gdId = db.GeoDataObject;
                //if a map data is available
                //not happen with current drawing template 

            }
            catch
            {
                //no GEO data

                var msId = SymbolUtilityServices.GetBlockModelSpaceId(db);

                var data = new GeoLocationData();
                data.BlockTableRecordId = msId;
                data.PostToDb();

                // We're going to define our geolocation in terms of
                // latitude/longitude using the Mercator projection
                // http://en.wikipedia.org/wiki/Mercator_projection

                data.CoordinateSystem = "WORLD-MERCATOR";
                data.TypeOfCoordinates = TypeOfCoordinates.CoordinateTypeLocal;

                //the two lines will cause GEOMapImage fail! strange!
                //data.HorizontalUnits = UnitsValue.Millimeters;
                //data.VerticalUnits = UnitsValue.Millimeters;
                 
                var geoPt = new Point3d(116,40, 0);

                // Transform from a geographic to a modelspace point
                // and add the information to our geolocation data
                var wcsPt = data.TransformFromLonLatAlt(geoPt);
                data.DesignPoint = new Point3d(0, 0, 0);
                data.ReferencePoint = wcsPt;
                data.ScaleFactor = 7; //? useful? 

                ed.Command("_.GEOMAP", "_ROAD");

                //if we test GEOMap only
                return;

                //to test GEOMapImage
                createMapImage();
            }        
        }

        private void createMapImage()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            var db = doc.Database;
            var ed = doc.Editor;

            ObjectId giId = ObjectId.Null;
            ObjectEventHandler handler =
              (s, e) =>
              {
                  if (e.DBObject is GeomapImage)
                  {
                      giId = e.DBObject.ObjectId;
                   }
              };

            // Simply call the GEOMAPIMAGE command with the two points
            db.ObjectAppended += handler;
            ed.Command("GEOMAPIMAGE", "V");
            db.ObjectAppended -= handler;

            if (giId == ObjectId.Null)
                return;

            // Open the entity and change some values
            try
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    // Get each object and check if it's a GeomapImage
                    var gi =
                      tr.GetObject(giId, OpenMode.ForWrite) as GeomapImage;
                    if (gi != null)
                    {
                        // Let's adjust the brightmess/contrast/fade of the
                        // GeomapImage

                        gi.Brightness = 50;
                        gi.Contrast = 15;
                        gi.Fade = 0;

                        // And make sure it's at the right resolution and
                        // shows both aerial and road information

                        gi.Resolution = GeomapResolution.Optimal;
                        gi.MapType = GeomapType.Road;

                        gi.UpdateMapImage(true); 
                    }

                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception)
            {
                ed.WriteMessage(
                  "\nUnable to update geomap image entity." +
                  "\nPlease check your internet connectivity and call " +
                  "GEOMAPIMAGEUPDATE."
                );
            }
        

    } 

    [CommandMethod("MyGEOTest")]
    public void MyGEOTest()
    { 
        //build GEO Image
        buildGEOImage();
             
    }
         
  }
}
