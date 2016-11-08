using System;
using System.IO;
using System.Collections.Generic;

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

        public List<LogCurveInfo> LogCurveInfos; // чето ругается
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
        private char[] separator = new char[] {'~','.',':',' '};

        public void Process()
        {
            while (!InputStream.EndOfStream)
            {
                var stringFile = NextLine();
                var section = '0';
                if (stringFile != null)
                {
                    section = char.ToLower(stringFile[1]);
                }

                switch (section)
                {                    
                    case 'w':
                        SectionWell(stringFile);
                        break;
                    case 'c':
                        SectionCurve(stringFile);
                        break;
                    case 'p':
                        SectionParameter(stringFile);
                        break;
                }
            }
        }

        public string NextLine()
        {
            return InputStream.ReadLine();
        }

        public string PreviousLine(string line)
        {
            return "Вот это я пока не понял как сделать"; 
        }
              
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
          
        public void SectionWell(string line)
        {
            while (line[0] != '~')
            {
                line = NextLine();
                string[] mnemonicLine = line.Split(separator); // Тут ОШИБКА - нужно правильно рассечь строку на 4 части, видимо нужно использовать регулярные выражения
                string mnemonic = mnemonicLine[0]; // Навзание мнемоники
                string unit = mnemonicLine[1]; // Единицы измерения
                string data = mnemonicLine[2]; // Значение
                string info = mnemonicLine[3]; // Описание

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
                            StartDateTimeIndex = ParseDate(data, info, unit); // тут ошибка пока-что = нужно парсить дату или секунды - хз пока
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
                            StopDateTimeIndex = ParseDate(data, info, unit); // тут ошибка пока-что = нужно парсить дату или секунды - хз пока
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
            }
            PreviousLine(line); //// "Вот это я пока не понял как сделать"
        }

        public void SectionCurve(string line)
        {
            while (line[0] != '~')
            {
                line = NextLine();
                string[] mnemonicLine = line.Split(separator); // Тут ОШИБКА - нужно правильно рассечь строку на 3 части, видимо нужно использовать регулярные выражения
                string mnemonic = mnemonicLine[0]; // Навзание мнемоники
                string unit     = mnemonicLine[1]; // Единицы измерения
                string info     = mnemonicLine[2]; // Описание

                LogCurveInfos.Add(new LogCurveInfo(mnemonic, unit.ToLower(), info));
            }
            PreviousLine(line); //// "Вот это я пока не понял как сделать"
        }

        public void SectionParameter(string line)
        {
            // У меня такое чуство, что все эти параметры разработчики взяли откуда то непонятно, и их не надо сюда писать
            // но для примера пусть будут
            while (line[0] != '~')
            {
                line = NextLine();
                string[] mnemonicLine = line.Split(separator); // Тут ОШИБКА - нужно правильно рассечь строку на 3 части, видимо нужно использовать регулярные выражения
                string mnemonic = mnemonicLine[0]; // Навзание мнемоники
                string unit     = mnemonicLine[1]; // Единицы измерения
                string data     = mnemonicLine[2]; // Значение

                switch(mnemonic)
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
            
            PreviousLine(line); //// "Вот это я пока не понял как сделать"
        }
        
    }
}