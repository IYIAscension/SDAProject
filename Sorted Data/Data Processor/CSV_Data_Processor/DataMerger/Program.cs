using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CSV_Processor;
using DataReader;

namespace DataMerger
{
    class Program
    {
        private static readonly string parentDir = "..";

        private static readonly string expectedDir = Path.Combine(
            parentDir, parentDir, parentDir,
            parentDir, parentDir, parentDir
        );

        static void Main(string[] args)
        {
            string file_a = Path.Combine(
                expectedDir, "country_data_orig.csv"
            );

            CSVFile original_file = new CSVFile(file_a);
            original_file.BindTextColumn("Country");
            original_file.BindValueColumns(
                BuiltinParsers.ParseDouble,
                "total_tests",
                "total_deaths",
                "total_cases",
                "human_development_index"
            );
            try
            {
                original_file.BeginRead();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadKey();
                return;
            }

            TextColumn countries = original_file.GetColumn<TextColumn>(0);
            var development = original_file.GetColumn<ValueColumn<double>>(
                "human_development_index"
            );

            ValueColumn<double>
                total_tests = new ValueColumn<double>("Total Tests", null),
                total_deaths = new ValueColumn<double>("Total Deaths", null),
                total_cases = new ValueColumn<double>("Total Cases", null),
                population_size = new ValueColumn<double>("Population Size", null),
                latest_cases = new ValueColumn<double>("Latest Cases", null),
                total_vaccinations = new ValueColumn<double>("Total Vaccinations", null),
                fully_vaccinated = new ValueColumn<double>("Fully Vaccinated", null);
            
            ValueColumn<double>[] columns = new ValueColumn<double>[]
            {
                total_tests,
                total_deaths,
                total_cases,
                population_size,
                latest_cases,
                total_vaccinations,
                fully_vaccinated
            };
            string[] datasets = new string[]
            {
                "total_tests.data",
                "total_deaths.data",
                "total_cases.data",
                "population.data",
                "new_cases.data",
                "total_vaccinations.data",
                "people_fully_vaccinated.data"
            };

            for (int i = 0; i < countries.Length; i++)
            {
                string country = countries[i];
                while (!Gatherer.countryNames.Contains(country))
                {
                    Console.Write($"Country [{country}] not found. Enter the name to search under.\nName:");
                    country = Console.ReadLine();
                }

                for (int c = 0; c < 7; c++)
                    columns[c].Append(Gatherer.GetFinalTimeSeries(country, datasets[c]));
            }

            CSVFile output = new CSVFile(
                countries, development, total_tests, total_deaths,
                total_cases, population_size, latest_cases,
                total_vaccinations, fully_vaccinated
            );
            File.WriteAllText(
                Path.Combine(expectedDir, "Merged.csv"),
                output.ToStringAligned()
            );
        }
    }
}
