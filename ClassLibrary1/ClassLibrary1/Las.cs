using System;
using System.IO;

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

            LogCurveInfos = null;
            CurveValues = null;
            MeasuredDepthUnit = null;
            StartMeasuredDepthIndex = null;
            StopMeasuredDepthIndex = null;
            StartDateTimeIndex = null;
            StopDateTimeIndex = null;
            NullValue = null;
            ServiceCompany = null;
            ElevationKellyBushing = null;
            LogMeasuredFrom = null;
            PermanentDatum = null;
            AbovePermanentDatum = null;
            ElevationPermanentDatum = null;
        }

        public string LogCurveInfos { get; set; }
        public string CurveValues { get; set; }
        public string MeasuredDepthUnit { get; set; }
        public string StartMeasuredDepthIndex { get; set; }
        public string StopMeasuredDepthIndex { get; set; }
        public string StartDateTimeIndex { get; set; }
        public string StopDateTimeIndex { get; set; }
        public string NullValue { get; set; }
        public string ServiceCompany { get; set; }
        public string ElevationKellyBushing { get; set; }
        public string LogMeasuredFrom { get; set; }
        public string PermanentDatum { get; set; }
        public string AbovePermanentDatum { get; set; }
        public string ElevationPermanentDatum { get; set; }

        private StreamReader InputStream { get; }
        private string nextLine { get; set; }

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
                    case 'v':
                        BlockVersion();
                        break;
                    case 'w':
                        BlockWell();
                        break;
                    case 'c':
                        BlockCurve();
                        break;
                    case 'p':
                        BlockParameter();
                        break;
                    case 'o':
                        break;
                    case 'a':
                        BlockOther();
                        break;
                }
            }
        }

        public string NextLine()
        {
            return InputStream.ReadLine();
        }

        public void BlockVersion()
        {
        }

        public void BlockWell()
        {
        }

        public void BlockCurve()
        {
        }

        public void BlockParameter()
        {
        }

        public void BlockOther()
        {
        }

        public void SkipBlock()
        {
            
        }
    }
}