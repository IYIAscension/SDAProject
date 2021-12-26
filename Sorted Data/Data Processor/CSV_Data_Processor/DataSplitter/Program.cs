using System;
using System.IO;
using System.Collections.Generic;
using CSV_Processor;

namespace DataSplitter
{
    class Program
    {
        private static readonly string parentDir = "..";

        static void Main(string[] args)
        {
            string expectedDir = Path.Combine(
                parentDir, parentDir, parentDir,
                parentDir, parentDir, parentDir
            );

            string expectedPath = Path.Combine(
                expectedDir,
                "owid-covid-data.csv"
            );

            // If the CSV isn't where we expect it, then just quit.
            try
            {
                CSVFile file = new CSVFile(expectedPath);

                // Manually bind an interpreter to the columns.
                file.BindTextColumns("iso_code", "continent", "location");
                file.BindValueColumn("date", BuiltinParsers.ParseDateTime);
                file.BindValueColumns(BuiltinParsers.ParseDouble,
                    "total_cases",
                    "new_cases",
                    "new_cases_smoothed",
                    "total_deaths",
                    "new_deaths",
                    "new_deaths_smoothed",
                    "total_cases_per_million",
                    "new_cases_per_million",
                    "new_cases_smoothed_per_million",
                    "total_deaths_per_million",
                    "new_deaths_per_million",
                    "new_deaths_smoothed_per_million",
                    "reproduction_rate",
                    "icu_patients",
                    "icu_patients_per_million",
                    "hosp_patients",
                    "hosp_patients_per_million",
                    "weekly_icu_admissions",
                    "weekly_icu_admissions_per_million",
                    "weekly_hosp_admissions",
                    "weekly_hosp_admissions_per_million",
                    "new_tests",
                    "total_tests",
                    "total_tests_per_thousand",
                    "new_tests_per_thousand",
                    "new_tests_smoothed",
                    "new_tests_smoothed_per_thousand",
                    "positive_rate",
                    "tests_per_case"
                );
                file.BindTextColumn("tests_units");
                file.BindValueColumns(BuiltinParsers.ParseDouble,
                    "total_vaccinations",
                    "people_vaccinated",
                    "people_fully_vaccinated",
                    "total_boosters",
                    "new_vaccinations",
                    "new_vaccinations_smoothed",
                    "total_vaccinations_per_hundred",
                    "people_vaccinated_per_hundred",
                    "people_fully_vaccinated_per_hundred",
                    "total_boosters_per_hundred",
                    "new_vaccinations_smoothed_per_million",
                    "new_people_vaccinated_smoothed",
                    "new_people_vaccinated_smoothed_per_hundred",
                    "stringency_index",
                    "population",
                    "population_density",
                    "median_age",
                    "aged_65_older",
                    "aged_70_older",
                    "gdp_per_capita",
                    "extreme_poverty",
                    "cardiovasc_death_rate",
                    "diabetes_prevalence",
                    "female_smokers",
                    "male_smokers",
                    "handwashing_facilities",
                    "hospital_beds_per_thousand",
                    "life_expectancy",
                    "human_development_index",
                    "excess_mortality_cumulative_absolute",
                    "excess_mortality_cumulative",
                    "excess_mortality",
                    "excess_mortality_cumulative_per_million"
                );
                // Yes, the OWID file is really THAT large. :|

                // Given the sheer size of the file, ask the user to confirm.
                Console.WriteLine("Press any key to continue with file import.");
                Console.ReadKey();
                file.BeginRead();

                string[] colNames = file.GetColumnNames();
                int numColumns = colNames.Length;
                string extension = ".data";

                // Now filter by country.
                Dictionary<string, StreamCollection> countryStreams = new Dictionary<string, StreamCollection>();
                try
                {
                    foreach(object[] row in file)
                    {
                        // Index 2: country.
                        string country = row[2] as string;
                        try
                        {
                            countryStreams[country].Append(row);
                        }
                        catch(KeyNotFoundException)
                        {
                            // Generate filepaths.
                            string countryDir = Path.Combine(expectedDir, country);
                            if (!Directory.Exists(countryDir))
                                Directory.CreateDirectory(countryDir);

                            string[] filepaths = new string[numColumns - 3];
                            for (int i = 3; i < numColumns; i++)
                            {
                                filepaths[i - 3] = Path.Combine(
                                    countryDir,
                                    Path.ChangeExtension(colNames[i], extension)
                                );
                            }

                            StreamCollection collection = new StreamCollection(
                                filepaths
                            );
                            collection.Append(row);
                            countryStreams.Add(country, collection);
                        }
                    }
                }
                finally
                {
                    // Dispose all of the streams in any scenario.
                    foreach (var entry in countryStreams)
                        entry.Value.Dispose();
                }
            }
            catch (FileNotFoundException)
            {
                // Target file is missing; abort!
                Console.WriteLine(
                    "CSV file is not at the expected location! Aborting!"
                );
            }
            finally
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
