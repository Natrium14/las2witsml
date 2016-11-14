using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    /// <summary>
    /// Класс для описания сущности witsml-файла
    /// </summary>
    class Witsml
    {
        private StreamWriter OutputStream { get; set; }
        private int Indent { get; set; }
      //  private Las Las { get; set; }

        public int WitsmlVersion { get; set; }
        public string UomFile { get; set; }
        public List<LogCurveInfo> LogCurveInfos;


        public Witsml(StreamWriter outputStream, int witsmlVersion, string uomFile)
        {
            this.OutputStream = outputStream;
            this.WitsmlVersion = witsmlVersion = 1410;
            this.UomFile = uomFile;
            this.Indent = 0;
        }

        // Главный процесс
        //
        public void FromLasFile(Las las, string uidWell, string uidWellbore, string uid, string name)
        {
            
        }

        public void DigestLas(Las las)
        {
            LogCurveInfos = las.LogCurveInfos;
            List<LogCurveInfo> newLcis = new List<LogCurveInfo>();

            // Set these variables, which depend on whether we have a time or a depth log:
            // newLcis : possibly modified (to merge date+time) list of logCurveInfos
            // indexLci: the LCI of the index curve
            // is_index_index: proc to test the given integer to see whether it is one of the possibly one or two index curve indexes
            // get_index: proc to extract the index from an array of values

            if (String.IsNullOrEmpty(las.MeasuredDepthUnit))
                // Не знаю как корректно сделать - if las_file.measured_depth_unit then
            {
                newLcis = LogCurveInfos;
                var indexLci = LogCurveInfos[0];
                // var isIndexIndex = LambdaExpression
                // var getIndex = LambdaExpression
            }
            else // это временной отрезок
            {
                
            }
        }

        public void MakeTimeIndexes(List<LogCurveInfo> lcis)
        {
// Typically we see DATE and TIME
// We can also see only TIME in which case we expect long integer seconds since 1970
// (There's no spec that says that; but this data comes from SLB's IDEAL which uses Unix epoch)
// M/D Totco declares one curve named DATE. It has space separated data and time.

            var dateIndex = lcis.Where(x => x.Mnemonic == "date"); //?? Тут какой то лямбда запрос, пока не разобрался
            var timeIndex = lcis.Where(x => x.Mnemonic == "time"); //?? видимо получить номер колонны из lci
            char dateFormat;

            if (timeIndex == null)
            {
             //   timeIndex = dateIndex+1 ?? dateIndex // Следующая колонка
            }

            if (dateIndex == null)
            {
                // dateIndex = timeIndex ?? timeIndex
                dateFormat = '1';
            }
            else
            {
                // dateFormat = lcis[dateIndex].Unit.ToLower();
            }

            // return dateIndex, timeIndex, dateFormat
        }
    }
}
