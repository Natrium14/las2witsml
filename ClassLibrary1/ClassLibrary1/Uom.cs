using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ClassLibrary1
{
    class Uom
    {
        string path { get; set; }
        string uomMap { get; set; }

        public Uom(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                this.path = path;
            }
            else
            {
                this.path = defaultUomFile();
            }
            this.uomMap = loadUomMap(this.path);
        }

        public static string Translate(string in_uom)
        {
            return "string"; //uomMap[in_uom.ToLower()];
        }

        private string DefaultUomFile()
        {
            return Directory.GetCurrentDirectory() + "/uom.json";
        }

        private string LoadUomMap(string path)
        {
            return "string";
        }
        
    }
}
