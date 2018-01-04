using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;

namespace Common
{
    public static class JsonApplicationConfig
    {
        private const string ConfigExtension = ".json";

        public static T Load<T>(IFileHelper fileHelper, string fileNameWithoutExtension) 
            where T : new()
        {
            var configPath = fileNameWithoutExtension;// + ConfigExtension;
            Console.WriteLine(configPath);

            var content = fileHelper.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<T>(content);
        }    
    }
}