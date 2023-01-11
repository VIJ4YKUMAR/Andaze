using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace BeerProduction
{
    class Program
    {
        static void Main(string[] args)
        {
            // Read in the data from the input CSV file
            using (var reader = new StreamReader("Input_v1.0.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Map the input file to the InputRecord class using attributes
                csv.Context.RegisterClassMap<InputRecordMap>();
                var records = csv.GetRecords<InputRecord>().ToList();

                // Calculate the year mean and median for each record
                foreach (var record in records)
                {
                    record.YearMean = (records.Where(r => r.Date.Year == record.Date.Year).Average(r => r.BeerProduction));
                    record.YearMedian = (records.Where(r => r.Date.Year == record.Date.Year).Select(r => r.BeerProduction).Median());
                    record.IsBeerProductionGreaterThanYearMean = record.BeerProduction > record.YearMean;
                    //record.YearMean = double.Parse(string.Format("{0:0.00}", records.Where(r => r.Date.Year == record.Date.Year).Average(r => r.BeerProduction)));
                    //record.YearMedian = double.Parse(string.Format("{0:0.00}", records.Where(r => r.Date.Year == record.Date.Year).Select(r => r.BeerProduction).Median()));
                }
                // Write the updated data to the output CSV file
                using (var writer = new StreamWriter("Output.csv", false, new UTF8Encoding(true)))
                using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // Write the header row
                    csvWriter.WriteField("Grain");
                    csvWriter.WriteField("Date");
                    csvWriter.WriteField("Manager");
                    csvWriter.WriteField("BeerProduction");
                    csvWriter.WriteField("YearMean");
                    csvWriter.WriteField("YearMedian");
                    csvWriter.WriteField("IsBeerProductionGreaterThanYearMean");
                    csvWriter.NextRecord();

                    // Write the data rows
                    foreach (var record in records)
                    {
                        csvWriter.WriteField(record.Grain);
                        csvWriter.WriteField(record.Date.ToString("yyyy/dd/MM", new CultureInfo("en-US")));
                        //csvWriter.WriteField("\"" + record.FactoryManagerName + "\"");
                        csvWriter.WriteField(record.FactoryManagerName, true);
                        csvWriter.WriteField(record.BeerProduction);
                        csvWriter.WriteField(string.Format("{0:0.00}", record.YearMean));
                        csvWriter.WriteField(string.Format("{0:0.00}", record.YearMedian));
                        csvWriter.WriteField((record.IsBeerProductionGreaterThanYearMean ? "yes" : "no"), true);
                        csvWriter.NextRecord();
                    }
                }
            }
        }
    }

    // Class to hold the data for each record in the input CSV file
    public class InputRecord
    {
        public string Grain { get; set; }
        public DateTime Date { get; set; }
        public string FactoryManagerName { get; set; }
        public double BeerProduction { get; set; }
        public double YearMean { get; set; }
        public double YearMedian { get; set; }
        public bool IsBeerProductionGreaterThanYearMean { get; set; }
    }

    // Map the input file to the InputRecord class using attributes
    public sealed class InputRecordMap : ClassMap<InputRecord>
    {
        public InputRecordMap()
        {
            Map(m => m.Grain).Name("grain");
            Map(m => m.Date).Name("DATE");
            Map(m => m.FactoryManagerName).Name("FactoryManagerName");
            Map(m => m.BeerProduction).Name("BeerProduction");
        }
    }

    // Extension method to calculate the median of a list of doubles
    public static class EnumerableExtensions
    {
        public static double Median(this IEnumerable<double> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (!source.Any())
                throw new InvalidOperationException("Cannot calculate median for an empty set.");

            var sortedList = from number in source
                             orderby number
                             select number;

            int itemIndex = sortedList.Count() / 2;
            if (sortedList.Count() % 2 == 0)
            {
                // If there is an even number of items, return the average of the two middle items
                return (sortedList.ElementAt(itemIndex) + sortedList.ElementAt(itemIndex - 1)) / 2;
            }
            else
            {
                // If there is an odd number of items, return the middle item
                return sortedList.ElementAt(itemIndex);
            }
        }
    }
}