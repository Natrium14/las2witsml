using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace ClassLibrary1
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
            StartDateTimeIndex = 0;
            StopDateTimeIndex = 0;
            StepIncrement = 0;
            NullValue = null;
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
        public double StartDateTimeIndex { get; set; }
        public double StopDateTimeIndex { get; set; }
        public double StepIncrement { get; set; }
        public string NullValue { get; set; }
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

        // Главный метод данного класса, где будет считываться весь файл по секциям
        //
        public void Process()
        {
            while (!InputStream.EndOfStream)
            {
                var line = ReadNextLine();
                var section = '0';
                if (line != null && line[0] == '~')
                {
                    section = char.ToLower(line[1]);
                }

                switch (section)
                {                    
                    case 'w':
                        nextLine = ReadNextLine();
                        SectionWell(nextLine);
                        break;
                    case 'c':
                        nextLine = ReadNextLine();
                        SectionCurve(nextLine);
                        break;
                    case 'p':
                        nextLine = ReadNextLine();
                        SectionParameter(nextLine);
                        break;
                }
            }
        }

        // Переход к следующей строке файла
        //
        public string ReadNextLine()
        {
            string line = "";
            if (!string.IsNullOrEmpty(nextLine))
            {
                line = nextLine;
                nextLine = null;
            }
            else
            {
                line = InputStream.ReadLine();
            }
            return line;
        }

        // Переход к предыдущей строке файла
        //
        public void PreviousLine(string line)
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
                (unit.ToLower() == "in"))
            {
                return unit;
            }
            else return null;
        }

        // Проверка - является ли измеряемый промежуток отрезком времени 
        //
        public double ParseDate(string data, string info, string unit)
        {
            if (unit.ToLower() == "s" || unit.ToLower() == "sec")
            {
                return Convert.ToDouble(data);
            }
            if (unit.ToLower() == "date" || unit.ToLower() == "d")
            {
                // HZ 
                return Convert.ToDouble(data);
            }
            return -1;
        }
          
        // Обработка секции W
        //
        public void SectionWell(string line)
        {
            while (line[0] != '~')
            {             
                var regex = new Regex(@"^([^~][^\.]+?)\.([^\s]*)(.*?):(.*)$", RegexOptions.IgnoreCase);
                var m = regex.Match(line).Groups.Cast<Group>().Skip(1).Take(4).Select(x => x.Value.Trim()).ToList();
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
                            StartMeasuredDepthIndex = Convert.ToDouble(data);
                        }
                        else
                        {
                            //StartDateTimeIndex = ParseDate(data, info, unit); // тут ошибка пока-что = нужно парсить дату или секунды - хз пока
                        }
                        break;
                    case "STOP":
                        if (IsDepthUnit(unit) != null)
                        {
                            MeasuredDepthUnit = unit;
                            StopMeasuredDepthIndex = Convert.ToDouble(data);
                        }
                        else
                        {
                            //StopDateTimeIndex = ParseDate(data, info, unit); // тут ошибка пока-что = нужно парсить дату или секунды - хз пока
                        }
                        break;
                    case "STEP":
                        StepIncrement = Convert.ToDouble(data);
                        break;
                    case "NULL":
                        NullValue = data;
                        break;
                    case "SRVC":
                        ServiceCompany = data;
                        break;
                }

                line = ReadNextLine();
            }
            PreviousLine(line); 
        }

        // Обработка секции C
        //
        public void SectionCurve(string line)
        {
            while (line[0] != '~')
            {                
                var regex = new Regex(@"^([^~][^\.]+)\.([^: \t]*)\s*([^:]*):(.*)$", RegexOptions.IgnoreCase);
                var m = regex.Match(line).Groups.Cast<Group>().Skip(1).Take(4).Select(x => x.Value.Trim()).ToList();
                var mnemonic = m[0];
                var unit = m[1];
                var _ = m[2];
                var info = m[3];
                
                LogCurveInfos.Add(new LogCurveInfo(mnemonic, unit.ToLower(), info));
                line = ReadNextLine();
            }
            PreviousLine(line); //// "Вот это я пока не понял как сделать"
        }

        // Обработка секции P
        //
        public void SectionParameter(string line)
        {
            // У меня такое чуство, что все эти параметры разработчики взяли откуда то непонятно, и их не надо сюда писать
            // но для примера пусть будут
            while (line[0] != '~')
            {                
                var regex = new Regex(@"^([^~][^\.]+)\.([^\s]*)(.*):(.*)$", RegexOptions.IgnoreCase);
                var m = regex.Match(line).Groups.Cast<Group>().Skip(1).Take(4).Select(x => x.Value.Trim()).ToList();
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

                line = ReadNextLine();
            }
            
            PreviousLine(line); 
        }
        
    }
}