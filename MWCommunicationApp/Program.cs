using Seaware.GribCS.Grib2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seaware.GribCS.Grib1;
using System.IO;
using System.Net;
using System.Reflection;

namespace MWCommunicationApp
{
    class Program
    {
        static string strPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName; 
        //Just Rename with name
        private static System.IO.FileStream RandomAccessFile = new System.IO.FileStream(strPath + "/GribFiles/newone.grib2", System.IO.FileMode.Open);
        static void Main(string[] args)
        {
            #region Grib 2 Code

            Grib2Input input = new Grib2Input(RandomAccessFile);
           
            if (!input.scan(false, false))
            {
                Console.WriteLine("Failed to successfully scan grib file");
                return;
            }
            Grib2Data data = new Grib2Data(RandomAccessFile);

            var records = input.Records;

            foreach (Grib2Record record in records)
            {
                IGrib2IndicatorSection iis = record.Is;
                IGrib2IdentificationSection id = record.ID;
                IGrib2ProductDefinitionSection pdsv = record.PDS;
                IGrib2GridDefinitionSection gdsv = record.GDS;

                long time = id.RefTime.AddTicks(record.PDS.ForecastTime * 3600000).Ticks;

                Console.WriteLine("Record description at " + " forecast " + new DateTime(time) + ": " + string.Format("{0} {1} {2}", iis.Discipline, pdsv.ParameterCategory, pdsv.ParameterNumber));

               

                float[] values = data.getData(record.getGdsOffset(), record.getPdsOffset());


                if ((iis.Discipline == 0) && (pdsv.ParameterCategory == 2) && (pdsv.ParameterNumber == 2))
                {
                    // U-component_of_wind
                    int c = 0;
                    for (double lat = gdsv.La1; lat >= gdsv.La2; lat = lat - gdsv.Dy)
                    {
                        for (double lon = gdsv.Lo1; lon <= gdsv.Lo2; lon = lon + gdsv.Dx)
                        {
                            Console.WriteLine("U-Wind " + lat + "\t" + lon + "\t" + values[c]);
                            c++;
                        }
                    }
                }
                else if ((iis.Discipline == 0) && (pdsv.ParameterCategory == 2) && (pdsv.ParameterNumber == 3))
                {
                    // V-component_of_wind
                    int c = 0;
                    for (double lat = gdsv.La1; lat >= gdsv.La2; lat = lat - gdsv.Dy)
                    {
                        for (double lon = gdsv.Lo1; lon <= gdsv.Lo2; lon = lon + gdsv.Dx)
                        {
                            Console.WriteLine("V-Wind " + lat + "\t" + lon + "\t" + values[c]);
                            c++;
                        }
                    }
                }
                else if ((iis.Discipline == 0) && (pdsv.ParameterCategory == 1) && (pdsv.ParameterNumber == 1))
                {
                    // RH
                    int c = 0;
                    for (double lat = gdsv.La1; lat >= gdsv.La2; lat = lat - gdsv.Dy)
                    {
                        for (double lon = gdsv.Lo1; lon <= gdsv.Lo2; lon = lon + gdsv.Dx)
                        {
                            Console.WriteLine("RH "+lat + "\t" + lon + "\t" + values[c]);
                            c++;
                        }
                    }
                }
            }

            #endregion
            Console.ReadLine();
        }
        
        static void GribOne()
        {
            #region Grib 1 Code

            //FileStream fs = new FileStream(@"c:\seawaredev\routing 3.0\gribs\pinta_38634_51527m.grb", FileMode.Open);
            Grib1Input gi = new Grib1Input(RandomAccessFile);
            gi.scan(false, false);
            byte[] b = new byte[100];

            RandomAccessFile.Seek(129400, SeekOrigin.Begin);
            RandomAccessFile.Read(b, 0, 100);

            Grib1Data data = new Grib1Data(RandomAccessFile);
            var records1 = gi.Records;

            //foreach (Grib1Record record in records1)
            //{
            //    IGrib1IndicatorSection iis = record.Is;
            //    //IGrib1IdentificationSection id = record.ID;
            //    IGrib1ProductDefinitionSection pdsv = record.PDS;
            //    IGrib1GridDefinitionSection gdsv = record.GDS;

            //    // long time = id.RefTime.AddTicks(record.PDS.ForecastTime * 3600000).Ticks;

            //    // Console.WriteLine("Record description at " + " forecast " + new DateTime(time) + ": " + string.Format("{0} {1} {2}", iis.Discipline, pdsv.ParameterCategory, pdsv.ParameterNumber));

            //    float[] values = data.getData(record.DataOffset, record.PDS.DecimalScale, record.PDS.bmsExists());

            //    if ((iis.Length > 0) && (pdsv.ParameterNumber == 2))
            //    {
            //        // U-component_of_wind
            //        int c = 0;
            //        for (double lat = gdsv.La1; lat >= gdsv.La2; lat = lat - gdsv.Dy)
            //        {
            //            for (double lon = gdsv.Lo1; lon <= gdsv.Lo2; lon = lon + gdsv.Dx)
            //            {
            //                Console.WriteLine(lat + "\t" + lon + "\t" + values[c]);
            //                c++;
            //            }
            //        }
            //    }
            //    else if ((iis.Length > 0) && (pdsv.ParameterNumber == 3))
            //    {
            //        // V-component_of_wind
            //        int c = 0;
            //        for (double lat = gdsv.La1; lat >= gdsv.La2; lat = lat - gdsv.Dy)
            //        {
            //            for (double lon = gdsv.Lo1; lon <= gdsv.Lo2; lon = lon + gdsv.Dx)
            //            {
            //                Console.WriteLine(lat + "\t" + lon + "\t" + values[c]);
            //                c++;
            //            }
            //        }
            //    }
            //}

            for (int i = 0; i < gi.Records.Count; i++)
            {
                Grib1Record rec = (Grib1Record)gi.Records[i];
                Grib1Data gd = new Grib1Data(RandomAccessFile);
                float[] data1 = gd.getData(rec.DataOffset,
                                            rec.PDS.DecimalScale, rec.PDS.bmsExists());
                System.Console.WriteLine("Param=" + rec.PDS.ParameterNumber);
                System.Console.WriteLine("Time=" + rec.PDS.ForecastTime);
                for (int j = 0; j < data1.Length; j++)
                {
                    System.Console.WriteLine(data1[j]);
                }
                float dataMin = float.MaxValue;
                float dataMax = -float.MaxValue;
                for (int j = 0; j < data1.Length; j++)
                {
                    if (data1[j] > -9998.0)
                    {
                        dataMin = Math.Min(dataMin, data1[j]);
                        dataMax = Math.Max(dataMax, data1[j]);
                    }
                }

                string gridName = Grib1GridDefinitionSection.getName(rec.GDS.Gdtn);
            }
            #endregion
        }
    }


}
