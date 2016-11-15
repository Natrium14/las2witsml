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
            var newLcis = DigestLas(las)[0];
            var isIndexIndex = DigestLas(las)[1];
            var getIndex = DigestLas(las)[2];

            var ns = "";
            var vers = "";

            if (WitsmlVersion >= 1410)
            {
                ns = "http://www.witsml.org/schemas/1series";
                vers = "1.4.1.0";
            }
            else
            {
                ns = "http://www.witsml.org/schemas/131";
                vers = "1.3.1.1";
            }

            var indexCurve = newLcis[0].Mnemonic;
            AddElement("logs", new Dictionary<string, string> { { "xmlns", ns }, { "version", vers} });
            AddElement("log", new Dictionary<string, string> { { "uidWell", uidWell }, { "uidWellbore", uidWellbore }, { "uid", uid } });
            AddTextElement("nameWell",name);
            AddTextElement("nameWellbore", name);
            AddTextElement("name", name);
            AddTextElement("serviceCompany", las.ServiceCompany ?? "");
            AddTextElement("description", "Created by lab212");

            var measureDepthUnit = las.MeasuredDepthUnit;
            if (String.IsNullOrEmpty(measureDepthUnit))
            {
                AddTextElement("indexType", "measured depth");
                AddTextElement("startIndex", las.StartMeasuredDepthIndex.ToString());
                AddTextElement("endIndex", las.StopMeasuredDepthIndex.ToString());
            }
            else
            {
                AddTextElement("indexType", "date time");
                AddTextElement("startDateTimeIndex", las.StartDateTimeIndex.ToString());
                AddTextElement("endDateTimeIndex", las.StopDateTimeIndex.ToString());
            }

            if (WitsmlVersion >= 1410)
            {
                AddTextElement("indexCurve", indexCurve);
            }
            else
            {
                AddTextElement("indexCurve", indexCurve); // исправить надо как то {'columnIndex'=>1}
            }

            AddTextElement("nullValue", las.NullValue);
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
                int dateIndex = MakeTimeIndexes(LogCurveInfos)[0];
                int timeIndex = MakeTimeIndexes(LogCurveInfos)[1];
                char dateFormat = MakeTimeIndexes(LogCurveInfos)[2];
                var indexLci = new LogCurveInfo();
                indexLci.Mnemonic = "DATETIME";
                var restLcis = LogCurveInfos; // rest_lcis = lcis.reject {|lci| ['time', 'date'].member?(lci.mnemonic.downcase)}  
                newLcis = [indexLci] + restLcis;

            }
        }

        public int[] MakeTimeIndexes(List<LogCurveInfo> lcis)
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

            return new int[] { dateIndex, timeIndex, dateFormat };
        }

        private void AddElement(string name, Dictionary<string, string> attributes)
        {
            OutputStream.Write("<" + name);

            foreach(var attr in attributes)
            {
                OutputStream.Write(attr.Key + "=" + attr.Value);
            }

            OutputStream.Write(">\n");
            OutputStream.Write("</" + name + ">\n");
        }

        private void AddTextElement(string name, string text)
        {
            OutputStream.Write("<" + name + ">");
            OutputStream.Write(text);
            OutputStream.Write("</" + name + ">\n");
        }
    }
}
