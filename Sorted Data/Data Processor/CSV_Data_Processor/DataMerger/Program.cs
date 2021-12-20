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

        private static string Write(double value)
            => value.ToString(BuiltinParsers.culture);

        static void Main(string[] args)
        {
            string file_a = Path.Combine(
                expectedDir, "inputdata.csv"
            );

            CSVFile original_file = new CSVFile(file_a);
            original_file.BindTextColumn("country");
            original_file.BindValueColumns(
                BuiltinParsers.ParseDouble,
                "VA",
                "PV",
                "GE",
                "RQ",
                "RL",
                "CC"
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

            original_file.GetColumn<ValueColumn<double>>("VA").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("PV").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("GE").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("RQ").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("RL").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("CC").SetWriter(Write);

            original_file.RenameColumn("country", "Country");
            original_file.RenameColumn("VA", "Voice & Accountability");
            original_file.RenameColumn("PV", "Pol. Stability & Abs. of Violence");
            original_file.RenameColumn("GE", "Govt. Effectiveness");
            original_file.RenameColumn("RQ", "Regulatory Quality");
            original_file.RenameColumn("RL", "Rule of Law");
            original_file.RenameColumn("CC", "Control of Corruption");

            ValueColumn<double>
                total_tests = new ValueColumn<double>("Total Tests", null, Write),
                total_deaths = new ValueColumn<double>("Total Deaths", null, Write),
                total_cases = new ValueColumn<double>("Total Cases", null, Write),
                total_vaccinations = new ValueColumn<double>("Total Vaccinations", null, Write),
                fully_vaccinated = new ValueColumn<double>("Fully Vaccinated", null, Write);
            
            ValueColumn<double>[] columns = new ValueColumn<double>[]
            {
                total_tests,
                total_deaths,
                total_cases,
                total_vaccinations,
                fully_vaccinated
            };
            string[] datasets = new string[]
            {
                "total_tests.data",
                "total_deaths.data",
                "total_cases.data",
                "total_vaccinations.data",
                "people_fully_vaccinated.data"
            };

            TextColumn countries = original_file.GetColumn<TextColumn>(0);

            Dictionary<string, string> remaps = new Dictionary<string, string>();
            string remapfilePath = Path.Combine(expectedDir, "binder.txt");
            if (File.Exists(remapfilePath))
            {
                foreach(string line in File.ReadAllLines(remapfilePath))
                {
                    string[] parts = line.Split(':');
                    remaps.Add(parts[0].Trim(), parts[1].Trim());
                }
            }

            for (int i = 0; i < countries.Length; i++)
            {
                string country = countries[i];
                bool invalid = !Gatherer.countryNames.Contains(country);
                if (invalid)
                {
                    // Try a remap.
                    if (remaps.TryGetValue(country, out string boundName))
                    {
                        string hold = country;
                        country = boundName;
                        invalid = !Gatherer.countryNames.Contains(country);
                        if (invalid)
                        {
                            boundName = country;
                            while (invalid)
                            {
                                Console.Write(
                                    $"Country [{country}] not found. Enter the name to search under.\nName:"
                                );
                                country = Console.ReadLine();
                                invalid = !Gatherer.countryNames.Contains(country);
                            }

                            remaps[hold] = country;
                        }
                    }
                    else
                    {
                        string srcName = country;
                        while (invalid)
                        {
                            Console.Write(
                                $"Country [{country}] not found. Enter the name to search under.\nName:"
                            );
                            country = Console.ReadLine();
                            invalid = !Gatherer.countryNames.Contains(country);
                        }

                        remaps.Add(srcName, country);
                    }
                }
                

                for (int c = 0; c < columns.Length; c++)
                    columns[c].Append(Gatherer.GetFinalTimeSeries(country, datasets[c]));
            }

            original_file.AddColumns(
                total_tests,
                total_deaths,
                total_cases,
                total_vaccinations,
                fully_vaccinated
            );

            File.WriteAllText(
                Path.Combine(expectedDir, "Merged.csv"),
                original_file.ToStringAligned()
            );

            File.WriteAllLines(
                remapfilePath,
                remaps.Select(x => $"{x.Key}:{x.Value}")
            );
        }
    }
}
