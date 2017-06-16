using AIO.ACES.Models;
using AIO.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyTestDesignAutomation
{
    class VariousInputs
    {

        static string myAppPackName = "MyTest-Activity-GEO-AppPackName";
        static string myAct_with_Pack_Name = "MyTest-Activity-GEO-Map";
        static string myAppPackBundleName = "MyTest-Activity-GEO-Bundle.zip";

        static Container container = new Container(new Uri("https://developer.api.autodesk.com/autocad.io/us-east/v2/"));

        static public void VariousInputsMain()
        {
            //get Token
           
            var token = GetToken();

            //set token for all calls afterwards 
            container.SendingRequest2 += (sender, e) => e.RequestMessage.SetHeader(
              "Authorization",
              token);
  

            ini();
            //post a WorkItem 
            CreateWorkItem(container, myAct_with_Pack_Name);
        }

        static void ini()
        {
            //create an AppPackage of Design Automation
            //just for demoing the step. Not neccessary to delete every time.
            CreateZip(myAppPackBundleName);

            //delete one AppPackage
            //just for demoing the step. Not neccessary to delete every time.
            DeletePackage(container, myAppPackName);
            //create AppPackage
            //just for demoing the step. Not neccessary to delete every time.
            AppPackage oAppPack = null;
            CreatePackage(container,
               myAppPackBundleName,
               myAppPackName, oAppPack);

            //delete Activity
            //just for demoing the step. Not neccessary to delete every time.
            DeleteActivity(container, myAct_with_Pack_Name);

            //ceate Activity
            CreateOneActivity(container, myAct_with_Pack_Name, myAppPackName);
        }
         static string GetToken()
        {
                using (var client = new HttpClient())
                {
                    // API key and secret
                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>(
                        "client_id",
                        Credentials.ConsumerKey));

                    values.Add(new KeyValuePair<string, string>(
                        "client_secret",
                        Credentials.ConsumerSecret));

                    values.Add(new KeyValuePair<string, string>(
                        "grant_type",
                        "client_credentials"));

                 values.Add(new KeyValuePair<string, string>(
                       "scope",
                       "code:all"));

                var requestContent = new FormUrlEncodedContent(values);

                    //post request for authentication
                    // https://developer.api.autodesk.com/authentication/v1/authenticate

                var response = client.PostAsync(
                        "https://developer.api.autodesk.com/authentication/v1/authenticate",
                        requestContent).Result;

                    //check the response
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    //parse json string
                    var resValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        responseContent);
                    //get out token
                    return resValues["token_type"] + " " + resValues["access_token"];
                }
            }

        // delete one Activity        
        private static void DeleteActivity(Container container, string actId)
        {
            Console.WriteLine("deleting one Activity...");

            Activity activity = null;
            try
            {
                activity = container.Activities.ByKey(actId).GetValue();
            }
            catch { }

            if (activity != null)
            {
                container.DeleteObject(activity);
                container.SaveChanges();
                activity = null;
            }
        }

        //create an AutoCAD bundle 
        static void CreateZip(string zipname)
        {
            Console.WriteLine("packaging AutoCAD plugin as bundle...");

            if (System.IO.File.Exists(zipname))
                System.IO.File.Delete(zipname);
            using (var archive = ZipFile.Open(zipname, ZipArchiveMode.Create))
            {
                string bundle = zipname + ".bundle";
                string name = "PackageContents.xml";
                archive.CreateEntryFromFile(name, System.IO.Path.Combine(bundle, name));
                name = "PackageNetPlugin.dll";
                archive.CreateEntryFromFile(name, System.IO.Path.Combine(bundle, "Contents", name));
                name = "Newtonsoft.Json.dll";
                archive.CreateEntryFromFile(name, System.IO.Path.Combine(bundle, "Contents", name));
                name = "RestSharp.dll";
                archive.CreateEntryFromFile(name, System.IO.Path.Combine(bundle, "Contents", name));
            }
        }

        //upload AppPackage to AWS
        static void UploadObject(string url, string filePath)
        {
            Console.WriteLine("uploading AppPackage package to AWS...");

            using (var client = new HttpClient())
            {
                client.PutAsync(
                  url,
                  new StreamContent(File.OpenRead(filePath))
                ).Result.EnsureSuccessStatusCode();
            }
        }

        //delete one AppPackage
        static void DeletePackage(Container container, string appPackName)
        {
            Console.WriteLine("deleting one AppPackage...");

            AppPackage package = null;
            try
            {
                package =
                  container.AppPackages.Where(
                    a => a.Id == appPackName
                  ).FirstOrDefault();


            }
            catch { }

            if (package != null)
            {
                container.DeleteObject(package);
                container.SaveChanges();
                package = null;
            }

            Console.WriteLine("AppPackage is deleted!");

        }

        //Create AppPackage
        static AppPackage CreatePackage(Container container,
                                                string zip,
                                                string appPackName,
                                                AppPackage package)
        {
            Console.WriteLine("creating AppPackage...");

            // step1: ：get AppPackage URL

            var url = container.AppPackages.GetUploadUrl().GetValue();

            // step2：upload AppPackage to cloud

            Console.WriteLine("uploading AppPackage zip...");
            UploadObject(url, zip);

            if (package == null)
            {
                // step3: create AppPackage object

                package = new AppPackage()
                {
                    Id = appPackName,
                    Version = 1,
                    RequiredEngineVersion = "21.0",
                    Resource = url
                };
                container.AddToAppPackages(package);
            }

            container.SaveChanges();

            Console.WriteLine("ceate AppPackage succeeded!");

            return package;
        }

        /// create one Activity
        static Activity CreateOneActivity(Container container,
                                        string actId,
                                        string appPackName)
        {
            Console.WriteLine("creating one Activity...");

            var activity = new Activity()
            {
                Id = actId,
                Version = 1,
                Instruction = new Instruction()
                {                   
                    Script = "_tilemode 1 MyGEOTest _save result.dwg\n"

                },
                Parameters = new Parameters()
                {
                    InputParameters =
                    {
                        new Parameter()
                        {
                          Name = "HostDwg", LocalFileName = "$(HostDwg)"
                        } 
                     },
                    OutputParameters = {
                    new Parameter()
                    {
                      Name = "Result", LocalFileName = "Result.dwg"
                    }
                  }
                },
                RequiredEngineVersion = "21.0"
            };

            //link with one AppPackage
            activity.AppPackages.Add(appPackName);

            container.AddToActivities(activity);
            container.SaveChanges();

            Console.WriteLine("Activity is Created！");


            return activity;
        }

        //post Work Item
        static void CreateWorkItem(Container container, string actId)
        {
            Console.WriteLine("creating Work Item...");


            //creare WorkItem
            var wi = new WorkItem()
            {
                Id = "",  
                Arguments = new Arguments(),
                //which Activity
                ActivityId = actId
            };
            //input param
            wi.Arguments.InputArguments.Add(new Argument()
            {
               
                Name = "HostDwg", 
                Resource = "https://s3-us-west-2.amazonaws.com/xiaodongforgetestio/simpleDWG.dwg",

                StorageProvider = StorageProvider.Generic
            }); 

            //output param
            wi.Arguments.OutputArguments.Add(new Argument()
            { 
                Name = "Result",
                 
                StorageProvider = StorageProvider.Generic, 
                HttpVerb = HttpVerbType.POST, 
                Resource = null, // Use storage provided by AutoCAD.IO
             });

            // 
            container.AddToWorkItems(wi);
            container.SaveChanges();

            // 
            Console.WriteLine("Id= {0}", wi.Id);

            container.MergeOption = Microsoft.OData.Client.MergeOption.OverwriteChanges;

            
            do
            {
                System.Threading.Thread.Sleep(10000);
                wi = container.WorkItems.Where(p => p.Id == wi.Id).SingleOrDefault();
                Console.WriteLine(wi.Status);
            }
            while (wi.Status == ExecutionStatus.Pending || wi.Status == ExecutionStatus.InProgress);


            //download report, no matter succeeded or failed.

            Console.WriteLine("The report is downloadable at {0}", wi.StatusDetails.Report);
            Until.DownloadToDocs(wi.StatusDetails.Report);

            //download result if any
            if (wi.Status == ExecutionStatus.Succeeded)
            {
               
                Console.WriteLine("The result is downloadable at {0}", wi.Arguments.OutputArguments.First().Resource);
               
                Until.DownloadToDocs(wi.Arguments.OutputArguments.First().Resource);
            }

        }



    }
}
