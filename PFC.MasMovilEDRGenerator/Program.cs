using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PFC.MasMovilEDRGenerator
{
    class Program
    {

        static void Main(string[] args)
        {
            //"C03004-0000|03/08/2015 10:26:11|01/07/2015 13:26:48|20150801|607561835|SMS NACIONAL|601219484|0,000000|0,080000|0,000000"

            const int year = 2017;
            const int MSISDN = 30;
            const string basePath = @"C:\temp\FicherosCSV";

            var random = new Random();

            // Generamos 500 nmeros de teléofono
            List<string> msisdn = new List<string>();
            for (int i = 0; i < MSISDN; i++)
                msisdn.Add(random.Next(600000000, 699999999).ToString());            

            // GEneramos 12 ficheros uno por cada mes del año
            for (int i = 1; i <= 12; i++)
            {
                var date = new DateTime(year, i, 1);
                using (var file = File.CreateText($@"{basePath}\{date.ToString("yyyyMMdd")}.txt"))
                {
                    for(int eventos = 0; eventos < MSISDN * 100; eventos++)
                    {
                        var eventDate = new DateTime(year, i, random.Next(1, 29), random.Next(0, 24), random.Next(0,60), random.Next(0, 60));
                        file.WriteLine($"C03004-0000|{date.ToString("dd/MM/yyy HH:mm:ss")}|{eventDate.ToString("dd/MM/yyy HH:mm:ss")}|{date.ToString("yyyyMMdd")}|{msisdn[random.Next(0, MSISDN)]}|VOZ NACIONAL|{random.Next(600000000, 699999999)}|{random.Next(1,3600)}|0,080000|0,000000");
                    }
                }
            }
        }
    }
}
