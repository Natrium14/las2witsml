using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ClassLibrary1
{
    /// <summary>
    /// Класс для описания сущности witsml-файла
    /// </summary>
    
    class Witsml
    {
        private StreamWriter OutputStream { get; set; }
        private Uom Uom { get; set; }
        private int Indent { get; set; }
      //  private Las Las { get; set; }

        public int WitsmlVersion { get; set; }
        public List<LogCurveInfo> LogCurveInfos;


        public Witsml(StreamWriter outputStream, int witsmlVersion, string uomFile)
        {
            this.OutputStream = outputStream;
            this.WitsmlVersion = witsmlVersion = 1410;
            //this.UomFile = uomFile;
            this.Indent = 0;
            this.Uom = new Uom(uomFile);
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
            int isIndexIndex = -1;
            int getIndex = -1;

            // Set these variables, which depend on whether we have a time or a depth log:
            // newLcis : possibly modified (to merge date+time) list of logCurveInfos
            // indexLci: the LCI of the index curve
            // is_index_index: proc to test the given integer to see whether it is one of the possibly one or two index curve indexes
            // get_index: proc to extract the index from an array of values

            if (String.IsNullOrEmpty(las.MeasuredDepthUnit))
            {
                newLcis = LogCurveInfos;
                var indexLci = LogCurveInfos[0];
                // isIndexIndex = LambdaExpression ( lambda {|i| (i == 0) } )
                // getIndex = LambdaExpression
            }
            else // это временной отрезок
            {
                object[] timeIndexes = MakeTimeIndexes(LogCurveInfos);
                LogCurveInfo dateIndex = (LogCurveInfo) timeIndexes[0];
                LogCurveInfo timeIndex = (LogCurveInfo) timeIndexes[1];
                string dateFormat = (string) timeIndexes[2];
                var indexLci = new LogCurveInfo();
                indexLci.Mnemonic = "DATETIME";
                var restLcis = LogCurveInfos; // rest_lcis = lcis.reject {|lci| ['time', 'date'].member?(lci.mnemonic.downcase)}  

                newLcis = [indexLci] + restLcis;
                isIndexIndex = 0 // lambda {|i| (i == time_index || i == date_index) }
                
                var date = "";
                var time = "";
                DateTime dt = new DateTime();
                if (dateFormat == "1")
                {
                    double offset = las.StartDateTimeIndex;
                    double dtf = Convert.ToDouble(date);
                    if (dtf < 86400000 && !Double.IsNaN(offset))
                    {
                        dt = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(dtf + offset);
                    }
                    else
                    {
                        dt = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(dtf);
                    }
                }
                else
                {
                    if (dateFormat == "yymmdd") {
                        date = date.Substring("(\d\d)(\d\d)(\d\d)", "\2/\3/\1");
                    }
                    time = values[timeIndex];
                    if (true) { // /\d\d\d\d\d\d/=~ time 
                        time = time.Substring("(\d\d)(\d\d)(\d\d)", "\1:\2:\3");
                    }
                    dt = DateTime.Parse(time + " " + date);
                }
            }

            return object[] { newLcis, indexLci, isIndexIndex,getIndex};
        }

        public object[] MakeTimeIndexes(List<LogCurveInfo> lcis)
        {
            // Typically we see DATE and TIME
            // We can also see only TIME in which case we expect long integer seconds since 1970
            // (There's no spec that says that; but this data comes from SLB's IDEAL which uses Unix epoch)
            // M/D Totco declares one curve named DATE. It has space separated data and time.

            LogCurveInfo dateIndex = lcis.First(x => x.Mnemonic.ToLower() == "date"); //?? Тут какой то лямбда запрос, пока не разобрался
            LogCurveInfo timeIndex = lcis.First(x => x.Mnemonic.ToLower() == "time"); //?? видимо получить номер колонны из lci
            string dateFormat = "1";

            if (timeIndex == null)
            {
                timeIndex = (dateIndex + 1) ?? dateIndex; // ни понятно
            }

            if (dateIndex == null)
            {
                dateIndex = timeIndex ?? timeIndex;
                dateFormat = "1";
            }
            else
            {
                 dateFormat = dateIndex.Unit.ToLower();
            }

            return new object[] { dateIndex, timeIndex, dateFormat };
        }

        private void AddElement(string name, Dictionary<string, string> attributes)
        {
            OutputStream.Write("<" + name);

            foreach(var attr in attributes)
            {
                OutputStream.Write(attr.Key + "=" + escapeText(attr.Value));
            }

            OutputStream.Write(">\n");
            OutputStream.Write("</" + name + ">\n");
        }

        private void AddTextElement(string name, string text)
        {
            OutputStream.Write("<" + name + ">");
            OutputStream.Write(escapeText(text));
            OutputStream.Write("</" + name + ">\n");
        }

        private string escapeText(string text)
        {
            return HttpUtility.HtmlEncode(text.Trim());
        }

        private void AddLogCurveInfo(LogCurveInfo lasLci, int columnIndex, int minIndex, int maxIndex, string measureDepthUnit)
        {
            Dictionary<string, string> mnemonicDict = new Dictionary<string, string>();
            mnemonicDict.Add("uid", lasLci.Mnemonic);
            AddElement("logCurveInfo", mnemonicDict);
            AddTextElement("mnemonic", lasLci.Mnemonic);
            AddTextElement("unit", lasLci.Unit);

            if (String.IsNullOrEmpty(measureDepthUnit))
            {
                AddTextElement("minIndex", minIndex.ToString());
                AddTextElement("maxIndex", maxIndex.ToString());
            }
            else
            {
                AddTextElement("minDateTimeIndex", minIndex.ToString());
                AddTextElement("maxDateTimeIndex", maxIndex.ToString());
            }

            if (WitsmlVersion < 1410)
            {
                AddTextElement("columnIndex", columnIndex.ToString());
            }

            AddTextElement("curveDescription", lasLci.Description);
            AddTextElement("typeLogData", "float");
        }

        private string NormalizeUnit(string lasUnit)
        {
            if (String.IsNullOrEmpty(lasUnit))
            {
                return lasUnit;
            }
            else
            {
                string retval = Uom.Translate(lasUnit);
                if (String.IsNullOrEmpty(retval))
                {
                    // raise UnrecognizedUnitException, las_unit
                    retval = "";
                }
                return retval;
            }
        }
    }
}
