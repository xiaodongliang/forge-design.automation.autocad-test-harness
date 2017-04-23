using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyTestDesignAutomation
{
    class Until
    {

        #region "download result and log"
        static public void DownloadToDocs(string url,int index=0)
        {
            var client = new HttpClient();

            var content = (StreamContent)client.GetAsync(url).Result.Content;

            string filename = Path.GetFileName(new Uri(url).AbsolutePath) +index;
            string localFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);


            Console.WriteLine("Downloading to {0}.", localFilePath);

            using (var output = System.IO.File.Create(localFilePath))
            {
                content.ReadAsStreamAsync().Result.CopyTo(output);
                output.Close();
            }
        }
        #endregion

    }
}
