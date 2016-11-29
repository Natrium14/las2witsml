using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    class Program
    {
        // Программа для проверки работоспособности библиотеки
        static void Main(string[] args)
        {
            //StreamReader inputStream = new StreamReader(@"C:\Users\Борис\Desktop\las2witsmlLIB\ClassLibrary1\dt.las");
            //Las lasFile = new Las(inputStream);
            //lasFile.Process();

            StreamReader file = new StreamReader(@"C:\Users\Борис\Desktop\las2witsml-master\lib\uom.json");
            string stringJson = file.ReadToEnd();
            var list = JsonConvert.DeserializeObject<Dictionary<string,string>>(stringJson);

            foreach (var i in list)
            {
                Console.WriteLine($"{i.Key} - {i.Value}");
            }


            Console.Write((new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(1480397995));
            Console.ReadKey();
        }
    }

    class ParseUom
    {
        public string lasUnit { get; set; }
        public string xmlUnit { get; set; }
    }
}
