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
            Uom uom = new Uom(@"C:\Users\Борис\Desktop\las2witsml-master\lib\oum.json");
            
            StreamReader inputStream = new StreamReader(@"C:\Users\Борис\Desktop\las2witsml-master\Untitled_3 las.dat");
            Las lasFile = new Las(inputStream);
            lasFile.Process();

            StreamWriter outputStream = new StreamWriter(@"C:\Users\Борис\Desktop\las2witsml-master\c#Untitled3.xml");
            Witsml witsmlFile = new Witsml(outputStream,1411, uom);
            witsmlFile.FromLasFile(lasFile,"123","456","789","007");
            
            Console.Write("thats all");
            Console.ReadKey();
        }
    }
}
