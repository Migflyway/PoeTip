using Newtonsoft.Json;
using System.Net;
using System.IO;
namespace tools.Dev
{
    class ninjaapi
    {
        public static string DownloadFromUrl(string url)
        {
            using (var web = new WebClient())
            {
                var json = web.DownloadString(url);
                return json;
            }
        }

        public class Json
        {
            public static string ReadJson(string filePath)
            {
                return File.ReadAllText(filePath);
            }

            public static void SaveJson(string filePath, string data)
            {
                using (var file = File.CreateText(filePath))
                {
                    file.WriteLine(data);
                }
            }

            public static TSettingType LoadSettingFile<TSettingType>(string fileName)
            {
                return !File.Exists(fileName) ? default(TSettingType) : JsonConvert.DeserializeObject<TSettingType>(File.ReadAllText(fileName));
            }

            public static void SaveSettingFile<TSettingType>(string fileName, TSettingType setting)
            {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        }
    }
}
