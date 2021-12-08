from typing_extensions import final
import data_importer
import numpy as np

if __name__ == "__main__":
    # Countries
    with open('countries.txt') as countries:
        countries_list = countries.read().splitlines()

    country_dict = {}
    for count, k in enumerate(countries_list):

        # People fully vaccinated
        fully_vaccinated = np.array(data_importer.import_numerics_handle_none(
            str(k + '/people_fully_vaccinated.data'), 0.0
        ))

        # New cases
        new_cases = np.array(data_importer.import_numerics_handle_none(
            str(k + '/new_cases.data'), 0.0
        ))

        # Population size
        population_size = np.array(data_importer.import_numerics_handle_none(
            str(k + '/population.data'), 0.0
        ))

        # Total vaccines
        total_vaccinations = np.array(data_importer.import_numerics_handle_none(
            str(k + '/total_vaccinations.data'), 0.0
        ))
        
        # Total cases
        total_cases = np.array(data_importer.import_numerics_handle_none(
            str(k + '/total_cases.data'), 0.0
        ))

        # Total deaths
        total_deaths = np.array(data_importer.import_numerics_handle_none(
            str(k + '/total_deaths.data'), 0.0
        ))

        # Find first non-zero or '1.0' data point from november 30th to november 23rd,
        # then add that data point to the dictionary.
        total_dict = {0: 0, 1: 0, 2: 0, 3: 0, 4: 0, 5: 0}
        for i in range(7):
            temp1 = population_size[-i]
            temp2 = new_cases[-i]
            temp3 = total_vaccinations[-i]
            temp4 = fully_vaccinated[-i]
            temp5 = total_cases[-i]
            temp6 = total_deaths[-i]
            total = [temp1, temp2, temp3, temp4, temp5, temp6]
            for count, j in enumerate(total):
                if j != 0 and j != 0.0 and j != 1.0 and total_dict[count] == 0:
                    total_dict[count] = j

        # Convert the dictionary values into a string and add it to the 
        # dictionary with all the country values.
        final_data_list = [str(' ' + str(total_dict[i]) + ',') for i in range(len(total))]
        final_data_list[-1] = str(final_data_list[-1].strip(',') + '\n')
        final_data_string = ''.join(map(str, final_data_list))
        country_dict[k] = final_data_string

    # Create the index row of the file.
    index_row = 'Country, Population Size, Latest Cases, Total Vaccinations, Fully Vaccinated, Total Cases, Total Deaths\n'

    # Write the data to the file.
    f = open('Countries Data File', 'w+')
    f.write(index_row)
    for country in countries_list:
        values = country_dict[country]
        f.write(str(country + ',' + values))
    f.close()

