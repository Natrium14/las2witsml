using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    /// <summary>
    /// Класс для описания сущности лога кривой
    /// </summary>
    public class LogCurveInfo
    {
        public string Mnemonic { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }

        public LogCurveInfo(){}

        public LogCurveInfo(string mnemonic, string unit, string description)
        {
            this.Mnemonic = mnemonic;
            this.Unit = unit;
            this.Description = description;
        }
    }
}
