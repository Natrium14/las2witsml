using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        // Программа для проверки работоспособности библиотеки
        static void Main(string[] args)
        {
            StreamReader inputStream = new StreamReader(@"C:\Users\Борис\Desktop\las2witsmlLIB\ClassLibrary1\dt.las");
            Las lasFile = new Las(inputStream);
            lasFile.Process();

        }
    }
}
