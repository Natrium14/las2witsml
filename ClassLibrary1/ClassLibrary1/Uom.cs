using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Las2witsmlLIB
{
    public class Uom
    {
        string path { get; set; }
        Dictionary<string, string> uomMap { get; set; }

        public Uom(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                this.path = path;
            }
            else
            {
                this.path = DefaultUomFile();
            }
            this.uomMap = LoadUomMap(this.path);
        }

        public string Translate(string in_uom)
        {
            return uomMap.First(x => x.Key == in_uom.ToLower()).Value;
        }

        private string DefaultUomFile()
        {
            return Directory.GetCurrentDirectory() + "/uom.json";
        }

        private Dictionary<string,string> LoadUomMap(string path)
        {
            // JSON.parse( File.open( path, 'r') { |f| f.read })
            StreamReader file = new StreamReader(path);
            string stringJson = file.ReadToEnd();
            var list = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringJson);
            return list;
        }
        
    }
    
}
