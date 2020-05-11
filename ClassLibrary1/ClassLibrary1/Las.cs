using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

namespace Las2witsmlLIB
{
    /// <summary>
    ///     Класс для описания сущности las-файла
    /// </summary>
    public class Las
    {
        public Las(StreamReader inputStream)
        {
            InputStream = inputStream;
            nextLine = null;

            LogCurveInfos = new List<LogCurveInfo>();
            CurveValues = null;
            MeasuredDepthUnit = null;
            StartMeasuredDepthIndex = 0;
            StopMeasuredDepthIndex = 0;
            StartDateTimeIndex = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            StopDateTimeIndex = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            StepIncrement = 0;
            NullValue = 0;
            ServiceCompany = null;
            ElevationKellyBushing = null;
            LogMeasuredFrom = null;
            PermanentDatum = null;
            AbovePermanentDatum = 0;
            ElevationPermanentDatum = 0;
            RunNumber = 0;
        }

        public List<LogCurveInfo> LogCurveInfos; // Лист параметров в секции А - берется по всем выделенным параметрам кривых в секции С
        public string CurveValues { get; set; }
        public string MeasuredDepthUnit { get; set; }
        public double StartMeasuredDepthIndex { get; set; }
        public double StopMeasuredDepthIndex { get; set; }
        public DateTime StartDateTimeIndex { get; set; }
        public DateTime StopDateTimeIndex { get; set; }
        public double StepIncrement { get; set; }
        public double NullValue { get; set; }
        public string ServiceCompany { get; set; }
        public string ElevationKellyBushing { get; set; }
        public string LogMeasuredFrom { get; set; }
        public string PermanentDatum { get; set; }
        public double AbovePermanentDatum { get; set; }
        public double ElevationPermanentDatum { get; set; }
        public double RunNumber { get; set; }
        public string ElevationUnit { get; set; }

        private StreamReader InputStream { get; }
        private string nextLine { get; set; }
        private string Line { get; set; }

        // Главный метод данного класса, где будет считываться весь файл по секциям
        //
        public void Process()
        {
            while (!InputStream.EndOfStream)
            {
                Line = ReadNextLine();
                if (String.IsNullOrEmpty(Line) || Line[0]=='#')
                {
                    Line = ReadNextLine();
                }
                var section = '0';
                if (Line != null && Line[0] == '~')
                {
                    section = char.ToLower(Line[1]);
                }

                switch (section)
                {                    
                    case 'w':
                        nextLine = ReadNextLine();
                        SectionWell();
                        break;
                    case 'c':
                        nextLine = ReadNextLine();
                        SectionCurve();
                        break;
                    case 'p':
                        nextLine = ReadNextLine();
                        SectionParameter();
                        break;
                }

                if (section == 'a')
                {
                    break;
                }
            }
        }

        public void EachDataLine(Witsml witsml)
        {
            while(!InputStream.EndOfStream)
            {
                try
                {
                    Line = ReadNextLine();
                    if (String.IsNullOrEmpty(Line) || Line[0] == '#')
                    {
                        Line = ReadNextLine();
                    }

                    string dataString = "";
                    Line = Line.Replace("\t", " ");
                    var dataLine = Line.Split(' ').Where(x=> x!= "").ToArray();
                    if (String.IsNullOrEmpty(this.MeasuredDepthUnit))
                    {
                        dataLine[0] = witsml.MakeDateFromDouble(dataLine[0], this);
                    }
                    
                    foreach(var str in dataLine)
                    {
                        if (str == dataLine.Last())
                        {
                            dataString += str;
                        }
                        else
                        {
                            dataString += str + ',';
                        }
                    }

                    witsml.xmlWriter.WriteElementString("data", dataString);
                }
                catch(Exception ee)
                {}
            }
        }

        // Переход к следующей строке файла
        //
        private string ReadNextLine()
        {
            Line = "";
            if (!string.IsNullOrEmpty(nextLine))
            {
                Line = nextLine;
                nextLine = null;
            }
            else
            {
                Line = InputStream.ReadLine();
            }
            return Line;
        }

        // Переход к предыдущей строке файла
        //
        private void PreviousLine(string line)
        {
            nextLine = line; 
        }
              
        // Проверка - является ли измеряемый промежуток отрезком глубины
        //
        public string IsDepthUnit(string unit)
        {
            if ((unit.ToLower() == "ft") || 
                (unit.ToLower() == "f")  || 
                (unit.ToLower() == "fm") || 
                (unit.ToLower() == "cm") || 
                (unit.ToLower() == "in") ||
                (unit.ToLower() == "m"))
            {
                return unit;
            }
            else return null;
        }

        // Если измеряемый промежуток не глубина, то парсим дату 
        //
        private static DateTime ParseDateTime2(string data, string info, string unit = "s")
        {
            DateTime dateTime;
            if (DateTime.TryParse(data, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out dateTime))
            {
                return dateTime;
            }
            
            var offset = Parsing.ParseDouble(data);
            if (double.TryParse(data, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out offset))
            {
                var secondsSince1970 = unit?.Equals("date", StringComparison.OrdinalIgnoreCase) == true;
                if (!secondsSince1970)
                {
                    var date = Parsing.ParseDateTime(info);
                    //return date.AddSeconds(offset);
                    return date;
                }
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(offset);
            }
            throw new ApplicationException($"Значение '{data}' не возможно представить как дату/время.");
        }

        // Обработка секции W
        //
        private void SectionWell()
        {
            nextLine = null;
            while (Line[0] != '~')
            {
                try {
                    var regex = new Regex(@"^([^~][^\.]+?)\.([^\s]*)(.*?):(.*)$", RegexOptions.IgnoreCase);
                    var m = regex.Match(Line).Groups.Cast<Group>().Skip(1).Take(4).Select(x => x.Value.Trim()).ToList();
                    var mnemonic = m[0];
                    var unit = m[1];
                    var data = m[2];
                    var info = m[3];

                    switch (mnemonic)
                    {
                        case "STRT":
                            if (IsDepthUnit(unit) != null)
                            {
                                MeasuredDepthUnit = unit;
                                StartMeasuredDepthIndex = Parsing.ParseDouble(data);
                            }
                            else
                            {
                                StartDateTimeIndex = ParseDateTime2(data, info, unit); // тут ошибка пока-что = нужно парсить дату или секунды - хз пока
                            }
                            break;
                        case "STOP":
                            if (IsDepthUnit(unit) != null)
                            {
                                MeasuredDepthUnit = unit;
                                StopMeasuredDepthIndex = Parsing.ParseDouble(data);
                            }
                            else
                            {
                                StopDateTimeIndex = ParseDateTime2(data, info, unit); // тут ошибка пока-что = нужно парсить дату или секунды - хз пока
                            }
                            break;
                        case "STEP":
                            StepIncrement = Parsing.ParseDouble(data);
                            break;
                        case "NULL":
                            NullValue = Parsing.ParseDouble(data);
                            break;
                        case "SRVC":
                            ServiceCompany = data;
                            break;
                    }
                }
                catch(Exception ee) { }

                Line = ReadNextLine();
                if (String.IsNullOrEmpty(Line))
                {
                    Line = ReadNextLine();
                }
            }
            PreviousLine(Line); 
        }

        // Обработка секции C
        //
        private void SectionCurve()
        {
            nextLine = null;
            while (Line[0] != '~')
            {
                try {
                    var regex = new Regex(@"^([^~][^\.]+)\.([^: \t]*)\s*([^:]*):(.*)$", RegexOptions.IgnoreCase);
                    var m = regex.Match(Line).Groups.Cast<Group>().Skip(1).Take(4).Select(x => x.Value.Trim()).ToList();
                    var mnemonic = m[0];
                    var unit = m[1];
                    var _ = m[2];
                    var info = m[3];

                    LogCurveInfos.Add(new LogCurveInfo(mnemonic, unit.ToLower(), info));
                }
                catch(Exception ee) { }

                Line = ReadNextLine();
                if (String.IsNullOrEmpty(Line) || Line[0] == '#')
                {
                    Line = ReadNextLine();
                }
            }
            PreviousLine(Line);
        }

        // Обработка секции P
        //
        private void SectionParameter()
        {
            // У меня такое чуство, что все эти параметры разработчики взяли откуда то непонятно, и их не надо сюда писать
            // но для примера пусть будут
            while (Line[0] != '~')
            {
                try {
                    var regex = new Regex(@"^([^~][^\.]+)\.([^\s]*)(.*):(.*)$", RegexOptions.IgnoreCase);
                    var m = regex.Match(Line).Groups.Cast<Group>().Skip(1).Take(4).Select(x => x.Value.Trim()).ToList();
                    var mnemonic = m[0];
                    var unit = m[1];
                    var data = m[2];

                    switch (mnemonic)
                    {
                        case "RUN":
                            RunNumber = Convert.ToDouble(data);
                            break;
                        case "PDAT":
                            PermanentDatum = data;
                            break;
                        case "EPD":
                            ElevationPermanentDatum = Convert.ToDouble(data);
                            ElevationUnit = unit;
                            break;
                        case "EGL":
                            ElevationPermanentDatum = Convert.ToDouble(data);
                            ElevationUnit = unit;
                            break;
                        case "LMF":
                            LogMeasuredFrom = data;
                            break;
                        case "APD":
                            AbovePermanentDatum = Convert.ToDouble(data);
                            ElevationUnit = unit;
                            break;
                        case "EKB":
                            ElevationKellyBushing = data;
                            ElevationUnit = unit;
                            break;
                    }
                }
                catch(Exception ee) { }

                Line = ReadNextLine();
                if (String.IsNullOrEmpty(Line) || Line[0] == '#')
                {
                    Line = ReadNextLine();
                }
            }
            
            PreviousLine(Line); 
        }
        
    }
}