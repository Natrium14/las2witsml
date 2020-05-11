using Las2witsmlLIB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Las2witsmlLIB
{

    class UnrecognizedUnitException: Exception
    {}

    class ConverisionError : Exception
    {
        public Dictionary<string,string> badUnits { get; set; }
        public ConverisionError(Dictionary<string, string> badUnits)
        {
            this.badUnits = badUnits;
        }
    }
    
    /// <summary>
    /// Класс для описания сущности witsml-файла
    /// </summary>

    public class Witsml
    {
        private ConverisionError conversionError { get; set; }
        public StreamWriter OutputStream { get; set; }
        private Uom Uom { get; set; }
        private int Indent { get; set; }
        public XmlTextWriter xmlWriter { get; set; }

        public int WitsmlVersion { get; set; }
        public List<LogCurveInfo> LogCurveInfos;
        public Dictionary<string, string> badUnits;


        public Witsml(StreamWriter outputStream, int witsmlVersion, Uom uomFile)
        {
            this.OutputStream = outputStream;
            this.WitsmlVersion = witsmlVersion;
            //this.UomFile = uomFile;
            this.Indent = 0;
            this.Uom = uomFile;
            this.conversionError = new ConverisionError(badUnits);
            this.badUnits = new Dictionary<string, string>();
            this.xmlWriter = new XmlTextWriter(outputStream);
        }
        
        public string MakeDateFromDouble(string data, Las las)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime startDateTime = las.StartDateTimeIndex;
            var dtf = startDateTime.Subtract(dtDateTime).TotalSeconds;
            var offset = Parsing.ParseDouble(data);
            dtDateTime = dtDateTime.AddSeconds(dtf + offset);
            //if (dtf < 86400000 && offset != 0)
            //{
            //    dtDateTime = dtDateTime.AddSeconds(dtf + offset);
            //}
            //else
            //{
            //    dtDateTime = dtDateTime.AddSeconds(dtf);
            //}
            return dtDateTime.ToString();
        }

        // Главный процесс
        //
        public void FromLasFile(Las las, string uidWell, string uidWellbore, string uid, string name)
        {
            //var digestLas = DigestLas(las);
            //List<LogCurveInfo> newLcis = (List<LogCurveInfo>)digestLas[0];
            //LogCurveInfo indexLci = (LogCurveInfo)digestLas[1];
            //var isIndexIndex = digestLas[2];
            //var getIndex = digestLas[3];

            List<LogCurveInfo> newLcis = new List<LogCurveInfo>();
            newLcis = las.LogCurveInfos;

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
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = 4;

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("logs");
            xmlWriter.WriteAttributeString("xmlns", ns);
            xmlWriter.WriteAttributeString("version", vers);

            xmlWriter.WriteStartElement("log");
            xmlWriter.WriteAttributeString("uidWell", uidWell);
            xmlWriter.WriteAttributeString("uidWellbore", uidWellbore);
            xmlWriter.WriteAttributeString("uid", uid);

            xmlWriter.WriteElementString("nameWell", name);
            xmlWriter.WriteElementString("nameWellbore", name);
            xmlWriter.WriteElementString("name", name);
            xmlWriter.WriteElementString("serviceCompany", las.ServiceCompany ?? "");
            xmlWriter.WriteElementString("description", "Created by lab212");

            var measureDepthUnit = "";
            try {
                measureDepthUnit = NormalizeUnit(las.MeasuredDepthUnit);
            }
            catch (UnrecognizedUnitException e)
            {
                conversionError.badUnits.Add("measured depth", e.Message);
                measureDepthUnit = "unitless";
            }


            if (!String.IsNullOrEmpty(measureDepthUnit))
            {
                xmlWriter.WriteElementString("indexType", "measured depth");
                //xmlWriter.WriteElementString("startIndex", las.StartMeasuredDepthIndex.ToString().Replace(',','.'));
                xmlWriter.WriteStartElement("startIndex");
                xmlWriter.WriteAttributeString("uom", measureDepthUnit);
                xmlWriter.WriteString(las.StartMeasuredDepthIndex.ToString().Replace(',', '.'));
                xmlWriter.WriteEndElement();
                //xmlWriter.WriteElementString("endIndex", las.StopMeasuredDepthIndex.ToString().Replace(',', '.'));
                xmlWriter.WriteStartElement("endIndex");
                xmlWriter.WriteAttributeString("uom", measureDepthUnit);
                xmlWriter.WriteString(las.StopMeasuredDepthIndex.ToString().Replace(',', '.'));
                xmlWriter.WriteEndElement();
            }
            else
            {
                xmlWriter.WriteElementString("indexType", "date time");
                xmlWriter.WriteElementString("startDateTimeIndex", las.StartDateTimeIndex.ToString().Replace(',', '.'));
                xmlWriter.WriteElementString("endDateTimeIndex", las.StopDateTimeIndex.ToString().Replace(',', '.'));
            }

            if (WitsmlVersion >= 1410)
            {
                xmlWriter.WriteElementString("indexCurve", indexCurve);
            }
            else
            {
                //xmlWriter.WriteStartElement("indexCurve", indexLci.Mnemonic);
                //xmlWriter.WriteAttributeString("columnIndex", "1");
                //xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteElementString("nullValue", las.NullValue.ToString());
            
            foreach (var lci in newLcis.Select((x, i) => new { Value = x, Index = i }))
            {
                try
                {
                    if (String.IsNullOrEmpty(las.MeasuredDepthUnit))
                    {
                        var start = las.StartDateTimeIndex.ToString();
                        var stop = las.StopDateTimeIndex.ToString();
                        AddLogCurveInfo(lci.Value, lci.Index, start, stop, measureDepthUnit); // error
                    }
                    else
                    {
                        var start = las.StartMeasuredDepthIndex.ToString();
                        var stop = las.StopMeasuredDepthIndex.ToString();
                        AddLogCurveInfo(lci.Value, lci.Index, start, stop, measureDepthUnit); // error
                    }
                }
                catch(UnrecognizedUnitException ee)
                {
                    badUnits.Add(lci.Value.Mnemonic, ee.Message);
                }
            }

            if (badUnits.Count == 0)
            {
                xmlWriter.WriteStartElement("logData");
                if (WitsmlVersion >= 1410)
                {
                    var listMnemonic = "";
                    var listUnit = "";
                    foreach (var lci in newLcis)
                    {
                        listMnemonic += lci.Mnemonic + ",";
                        listUnit += lci.Unit + ",";
                    }

                    xmlWriter.WriteElementString("mnemonicList", listMnemonic);
                    xmlWriter.WriteElementString("unitList", listUnit);
                }

                // Вот здесь записываются все значения data
                las.EachDataLine(this);
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.Close();
            OutputStream.Close();
        }

        /*
        private object[] DigestLas(Las las)
        {
            LogCurveInfos = las.LogCurveInfos;
            List<LogCurveInfo> newLcis = new List<LogCurveInfo>();
            Func<int, bool> isIndexIndex;
            Func<DateTime[], DateTime> getIndex;
            LogCurveInfo indexLci;

            if (!String.IsNullOrEmpty(las.MeasuredDepthUnit))
            {
                newLcis = LogCurveInfos;
                indexLci = LogCurveInfos[0];
                isIndexIndex = (i) => { return (i == 0); };
                getIndex = (values) => { return values[0]; };
            }
            else // это временной отрезок
            {
                object[] timeIndexes = MakeTimeIndexes(LogCurveInfos);
                int dateIndex = (int)timeIndexes[0];
                int timeIndex = (int)timeIndexes[1];
                string dateFormat = (string)timeIndexes[2];

                indexLci = new LogCurveInfo();
                indexLci.Mnemonic = "DATETIME";

                // rest_lcis = lcis.reject {|lci| ['time', 'date'].member?(lci.mnemonic.downcase)}  
                List<LogCurveInfo> restLcis = LogCurveInfos.Where(x => x.Mnemonic.ToLower() != "time" && x.Mnemonic.ToLower() != "date").ToList();
                restLcis.Insert(0, indexLci);
                newLcis = restLcis;

                isIndexIndex = (i) =>
                {
                    return i == timeIndex || i == dateIndex;
                };

                getIndex = (values) =>
                {
                    DateTime Date = values[dateIndex];
                    DateTime Time;
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    var date = "";
                    var time = "";

                    if (dateFormat == "1")
                    {
                        double offset = DateTime.UtcNow.Subtract(las.StartDateTimeIndex).TotalSeconds;
                        double dtf = DateTime.UtcNow.Subtract(Date).TotalSeconds;
                        if (dtf < 86400000 && offset != 0)
                        {
                            dtDateTime = dtDateTime.AddSeconds(dtf + offset);
                        }
                        else
                        {
                            dtDateTime = dtDateTime.AddSeconds(dtf);
                        }
                    }
                    else
                    {
                        if (dateFormat == "yymmdd")
                        {
                            date = Regex.Replace(date, @"(\d\d)(\d\d)(\d\d)", @"$2/$3/$1");
                        }

                        Time = values[timeIndex];
                        //time = time.Substring("(\d\d)(\d\d)(\d\d)", "\1:\2:\3");
                        time = Regex.Replace(date, @"(\d\d)(\d\d)(\d\d)", @"$1/$2/$3");
                        dtDateTime = Parsing.ParseDateTime(time + " " + date);
                    }
                    return dtDateTime;
                };
            }

            return new object[] { newLcis, indexLci, isIndexIndex, getIndex };
        }


        private object[] MakeTimeIndexes(List<LogCurveInfo> lcis)
        {
            int dateIndex = lcis.FindIndex(x => x.Mnemonic.ToLower() == "date"); 
            int timeIndex = lcis.FindIndex(x => x.Mnemonic.ToLower() == "time"); 
            string dateFormat = "1";

            if (timeIndex == 0)
            {
                timeIndex = (dateIndex + 1);
            }

            if (dateIndex <= 0)
            {
                dateIndex = timeIndex;
                dateFormat = "1";
            }
            else
            {
                 dateFormat = lcis[dateIndex].Unit.ToLower();
            }

            return new object[] { dateIndex, timeIndex, dateFormat };
        }

        private void AddElement(string name, string text, Dictionary<string, string> attributes)
        {
            OutputStream.Write("<" + name);
            OutputStream.Write(" ");
            

            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    OutputStream.Write(attr.Key + "=" + escapeText(attr.Value) + " ");
                }
            }

            OutputStream.Write(">\n");

            OutputStream.Write(escapeText(text));

            OutputStream.Write("</" + name + ">\n");
            OutputStream.Flush();
        }

        public void AddTextElement(string name, string text, Dictionary<string, string> attributes)
        {
            //AddElement(name, attributes);
            //OutputStream.Write("<" + name + ">");
            OutputStream.Write(escapeText(text));
            //OutputStream.Write("</" + name + ">\n");
            OutputStream.Flush();
        }

        private string escapeText(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                return HttpUtility.HtmlEncode(text.Trim());
            }
            return "";
        }
        */

        private void AddLogCurveInfo(LogCurveInfo lasLci, int columnIndex, string minIndex, string maxIndex, string measureDepthUnit)
        {
            xmlWriter.WriteStartElement("logCurveInfo");
            xmlWriter.WriteAttributeString("uid", lasLci.Mnemonic);
            xmlWriter.WriteElementString("mnemonic", lasLci.Mnemonic);
            xmlWriter.WriteElementString("unit", lasLci.Unit);

            if (!String.IsNullOrEmpty(measureDepthUnit))
            {
                //xmlWriter.WriteElementString("minIndex", minIndex.ToString().Replace(',', '.'));
                xmlWriter.WriteStartElement("minIndex");
                xmlWriter.WriteAttributeString("uom", measureDepthUnit);
                xmlWriter.WriteString(minIndex.ToString().Replace(',', '.'));
                xmlWriter.WriteEndElement();
                //xmlWriter.WriteElementString("maxIndex", maxIndex.ToString().Replace(',', '.'));
                xmlWriter.WriteStartElement("maxIndex");
                xmlWriter.WriteAttributeString("uom", measureDepthUnit);
                xmlWriter.WriteString(maxIndex.ToString().Replace(',', '.'));
                xmlWriter.WriteEndElement();
            }
            else
            {
                xmlWriter.WriteElementString("minDateTimeIndex", minIndex.ToString());
                xmlWriter.WriteElementString("maxDateTimeIndex", maxIndex.ToString());
            }

            if (WitsmlVersion < 1410)
            {
                xmlWriter.WriteElementString("columnIndex", columnIndex.ToString());
            }

            xmlWriter.WriteElementString("curveDescription", lasLci.Description);
            xmlWriter.WriteElementString("typeLogData", "float");

            xmlWriter.WriteEndElement();
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
                    throw new UnrecognizedUnitException();
                }
                return retval;
            }
        }
    }
}
