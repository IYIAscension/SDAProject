using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CSV_Processor;
using DataReader;

namespace DataMerger
{
    /// <summary>
    /// This Console program merges the democracy index .CSV data with
    /// specific Covid-19 datasets for each repsective country.
    /// </summary>
    internal sealed class Program
    {
        // Cached string for parent directory navigation.
        private static readonly string parentDir = "..";

        // We gotta move up a few folders because of where Visual Studio
        // places the executable.
        private static readonly string expectedDir = Path.Combine(
            parentDir, parentDir, parentDir,
            parentDir, parentDir, parentDir
        );

        /// <summary>
        /// Cached method for writing <see cref="double"/> values with the
        /// correct <see cref="System.Globalization.CultureInfo"/> setting.
        /// </summary>
        /// <param name="value">The value to convert to text.</param>
        private static string Write(double value)
            => value.ToString(BuiltinParsers.culture);

        /// <summary>
        /// The main method of the program, invoked by the executable.
        /// </summary>
        /// <param name="args">The command line arguments, which go unused
        /// in this case.</param>
        private static void Main(string[] args)
        {
            // Target the democracy data .csv file.
            string file_a = Path.Combine(
                expectedDir, "inputdata.csv"
            );

            CSVFile original_file = new CSVFile(file_a);

            // Create columns with the appropriate backing value types.
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

            // Import all of the data from the disk into memory.
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

            // Change the write function for all columns, to respect the us-en writing culture.
            original_file.GetColumn<ValueColumn<double>>("VA").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("PV").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("GE").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("RQ").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("RL").SetWriter(Write);
            original_file.GetColumn<ValueColumn<double>>("CC").SetWriter(Write);

            // Rename the columns for human readability.
            original_file.RenameColumn("country", "Country");
            original_file.RenameColumn("VA", "Voice & Accountability");
            original_file.RenameColumn("PV", "Pol. Stability & Abs. of Violence");
            original_file.RenameColumn("GE", "Govt. Effectiveness");
            original_file.RenameColumn("RQ", "Regulatory Quality");
            original_file.RenameColumn("RL", "Rule of Law");
            original_file.RenameColumn("CC", "Control of Corruption");

            // Create new Columns that ought to be appended.
            ValueColumn<double>
                total_tests = new ValueColumn<double>("Total Tests", null, Write),
                total_deaths = new ValueColumn<double>("Total Deaths", null, Write),
                total_cases = new ValueColumn<double>("Total Cases", null, Write),
                total_vaccinations = new ValueColumn<double>("Total Vaccinations", null, Write),
                fully_vaccinated = new ValueColumn<double>("Fully Vaccinated", null, Write);
            
            // Pack them into an array for speed-appending.
            ValueColumn<double>[] columns = new ValueColumn<double>[]
            {
                total_tests,
                total_deaths,
                total_cases,
                total_vaccinations,
                fully_vaccinated
            };
            // Also create a buffer of names for the data files.
            string[] datasets = new string[]
            {
                "total_tests.data",
                "total_deaths.data",
                "total_cases.data",
                "total_vaccinations.data",
                "people_fully_vaccinated.data"
            };

            // Grab the column of names so we can append to each row.
            TextColumn countries = original_file.GetColumn<TextColumn>(0);

            // Not all of the country names match. Create a dictionary for the remapping.
            Dictionary<string, string> remaps = new Dictionary<string, string>();
            // Also prepare a path for a file to save the bindings to, for later use in Python.
            string remapfilePath = Path.Combine(expectedDir, "binder.txt");

            // If a remap file exists already, then import it! Efficiency! :)
            if (File.Exists(remapfilePath))
            {
                foreach(string line in File.ReadAllLines(remapfilePath))
                {
                    string[] parts = line.Split(':');
                    remaps.Add(parts[0].Trim(), parts[1].Trim());
                }
            }

            // Now we loop over the CSV File, row-wise.
            for (int i = 0; i < countries.Length; i++)
            {
                // Grab the name of the country being inspected.
                string country = countries[i];
                // Check if the given name is present in the data files.
                bool invalid = !Gatherer.countryNames.Contains(country);

                // Name is unknown, so we gotta use a mapping.
                if (invalid)
                {
                    // Use an extant remapping if it exist.
                    if (remaps.TryGetValue(country, out string boundName))
                    {
                        string hold = country;
                        country = boundName;

                        // Check for safety if the bound name exists in the files.
                        invalid = !Gatherer.countryNames.Contains(country);
                        if (invalid)
                        {
                            // Welp, the bound name is also invalid. We're going to have to update it.
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
                        // Keep asking for a binding name until a valid name is supplied.
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
                
                /*  Now we have the name of the directory to import covid data
                 *  from. Loop over all covid datasets that are requested,
                 *  import the recentmost statistic and append it to this
                 *  country's row in the CSV File. */
                for (int c = 0; c < columns.Length; c++)
                    columns[c].Append(Gatherer.GetFinalTimeSeries(country, datasets[c]));
            }

            // Data was imported for each country. Append the columns to the CSVFile.
            original_file.AddColumns(
                total_tests,
                total_deaths,
                total_cases,
                total_vaccinations,
                fully_vaccinated
            );

            // Now write the final CSV File to the disk.
            File.WriteAllText(
                Path.Combine(expectedDir, "Merged.csv"),
                original_file.ToStringAligned()
            );

            // Also write the binder, for Python's sake.
            File.WriteAllLines(
                remapfilePath,
                remaps.Select(x => $"{x.Key}:{x.Value}")
            );
        }
    }
}
