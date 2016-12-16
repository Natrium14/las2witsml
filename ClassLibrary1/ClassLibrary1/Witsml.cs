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
            // new_lcis, _, is_index_index, get_index = digest_las(las_file)  #unused index_lci
            //var digestLas = DigestLas(las);
            //var newLcis = digestLas[0];
            //var indexLci = digestLas[1];
            //var isIndexIndex = digestLas[2];
            //var getIndex = digestLas[3];

            string ns = "";
            string vers = "";

            if (WitsmlVersion >= 1410)
            {
                ns = "http://www.witsml.org/schemas/1series";
                vers = "1.4.1.0";
            }
            else
            {
                ns = "http://www.witsml.org/schemas/131";
                vers = "1.3.1.1'";
            }
        }

        public void DigestLas(Las las)
        {
            LogCurveInfos = las.LogCurveInfos;
            List<LogCurveInfo> newLcis = new List<LogCurveInfo>();
            LogCurveInfo indexLci = new LogCurveInfo();
            bool isIndexIndex = false;
            int getIndex = -1;

            // Set these variables, which depend on whether we have a time or a depth log:
            // newLcis : possibly modified (to merge date+time) list of logCurveInfos
            // indexLci: the LCI of the index curve
            // is_index_index: proc to test the given integer to see whether it is one of the possibly one or two index curve indexes
            // get_index: proc to extract the index from an array of values

            if (String.IsNullOrEmpty(las.MeasuredDepthUnit))
            {
                newLcis = LogCurveInfos;
                indexLci = LogCurveInfos[0];
                // isIndexIndex = lambda {|i| (i == 0) }
                // getIndex = lambda { |values| values[0] }
            }
            else // это временной отрезок
            {
                //var timeIndexes = MakeTimeIndexes(newLcis);
                //int dateIndex = timeIndexes[0];
                //int timeIndex = timeIndexes[1];
                //string date_fmt = timeIndexes[2];
                indexLci = new LogCurveInfo("DATETIME",);

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
