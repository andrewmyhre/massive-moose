using log4net;
using massive_moose.storage.azure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace massive_moose.web.owin
{
    public partial class Startup
    {
        private static ILog AssetsLog = LogManager.GetLogger(typeof(Startup));
        public void ConfigureAssets()
        {
            var storage = new AzureFileStorage(new AzureFileStorageConfiguration() {ConnectionString=ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString});
            storage.EnsureDirectoryExists(ConfigurationManager.AppSettings["storageContainer"]);
            storage.EnsureDirectoryExists(ConfigurationManager.AppSettings["backgroundsContainer"]);
            EnsureFileExists(storage, "red-brick.jpg");
            EnsureFileExists(storage, "white-brick.jpg");
        }

        private static void EnsureFileExists(AzureFileStorage storage, string filename)
        {
            AssetsLog.DebugFormat("Check existence of {0}", filename);
            var filePath = string.Concat(ConfigurationManager.AppSettings["backgroundsContainer"], "/", filename);
            if (!storage.Exists(filePath))
            {
                AssetsLog.DebugFormat("Creating azure asset {0}", filename);
                using (
                    var file = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/backgrounds/", filename), FileMode.Open,
                        FileAccess.Read))
                {
                    var data = new byte[file.Length];
                    file.Read(data, 0, data.Length);
                    storage.Store(filePath, data, false);
                    AssetsLog.DebugFormat("Created azure asset {0}", filename);
                }
            }
        }
    }
}