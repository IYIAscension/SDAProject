
"""
Scientific Data Analysis - 2021-22 - Project

With this file a bootstrapped histogram and mean is produced for all countries at once 
from the sorted data directory.

The program is run by:
python bootstrap.py 
Whem prompted input the exact filename of the variable you want to analyse, followed by three optional prompts:
- minimal_value for the data input, with default value = 0.0
- subset_size, which controls the size of the bootstrapped subset, with default value = 5
- bootstrap_samples, which controls the number of bootstrapped samples, with default value = 10000

As a result of the shown histogram decisions can be made how to manipulate the data and calculate the p-values.
"""



import data_importer
import numpy as np
import matplotlib.pyplot as plt

if __name__ == "__main__":
    # Import country names from the countries.txt file
    with open('countries.txt') as countries:
        countries_list = countries.read().splitlines()

    # Prompt the user for four different inputs
    user_input = input("Country Data File Name: ")
    minimal_value = float(input("Minimal value:") or '0.0')
    subset_size = int(input('Bootstrap Subset Size: ') or '5')
    bootstrap_samples = int(input('Number Of Bootstrap Samples: ') or '10000')
    
    country_dict = {}
    data_list = []
    for country in countries_list:
        # Select the user-input data
        data = np.array(data_importer.import_numerics_handle_none(
            str(country + '/' + user_input + '.data'), 0.0
        ))

        # Default data of the country
        country_dict[country] = data[-1]

        # Get the first value larger than 0.0 or the minimal_value
        for i in range(1, 8):
            temp = data[-i]
            if temp <= minimal_value:
                continue
            elif temp > minimal_value:
                data_list.append(temp)
                country_dict[country] = temp
                break

    # Set up orginial data and bootstrapping parameters and perform the bootstrap resampling
    data_mean = sum(data_list)/len(data_list)
    resampled_data_list = []
    for i in range(bootstrap_samples):
        temp = np.random.choice(data_list, subset_size)
        resampled_data_list.append(sum(temp)/len(temp))

    resampled_data_mean = sum(resampled_data_list)/len(resampled_data_list)
    plot_mean = str('Mean: ' + str(round(resampled_data_mean, 4)))

    # Plot histogram of the data
    bins = 50
    plt.title(str(bootstrap_samples) + ' subsamples of ' + user_input + ' with subset-size ' + str(subset_size))
    plt.xlabel(user_input)
    plt.ylabel('Frequency')
    plt.hist(resampled_data_list, bins=bins)
    plt.vlines(resampled_data_mean, ymin=0, ymax=(bootstrap_samples), color='r', label=plot_mean)
    plt.legend()
    plt.show()

















"""
    # Read the data from the file.
    with open('Countries Data File.txt', 'r+') as f:
        f.seek(0)
        f_lines = f.readlines()
        first_row = []
        try: 
            first_row = f_lines[0].split(',')
        except:
            first_row == 'Empty'

    # Write the data to the file if the file has data
    try:
        if first_row[0] == 'Country':
            index_row = str(', ' + user_input + '\n')

            # Check if the user input values already exist in the data file
            replacement_index = 0
            user_input_string = str(' ' + user_input)
            for count, element in enumerate(first_row):
                if count == len(first_row) - 1:
                    stripped_element = element.strip('\n')
                    if stripped_element == user_input_string:
                        replacement_index = count
                if element == user_input_string:
                    replacement_index = count

            # If he user input is not already in the file, write the new data into the file.
            if replacement_index == 0:

                # Create list to add to file
                input_list = [index_row]
                for country in countries_list:
                    input_list.append(', ' + str(country_dict[country]) + '\n')

                # Combine original text file with new input
                total_lines = [f_lines[i].strip('\n') + input_list[i] for i in range(len(input_list))]
                
                # Write the total_lines to the text file
                f = open('Countries Data File.txt', 'w+')
                f.writelines(total_lines)
                f.close()

            # If the user input data is already in the file, overwrite the old data with the new data.
            if replacement_index != 0:
                if replacement_index == len(first_row) - 1:
                    replacement_list = [str(str(country_dict[country]) + '\n') for country in countries_list]
                else:
                    replacement_list = [str(country_dict[country]) for country in countries_list]

                # Replace old values with the new values.
                new_lines_strings = [first_row]
                old_lines = f_lines[1:]
                for count, line in enumerate(old_lines):
                    new_line = line.split(',')
                    new_value = replacement_list[count]
                    new_line[replacement_index] = new_value
                    new_lines_strings.append(new_line)
                
                # Write the new data to the file.
                input_list = [', '.join(line) for line in new_lines_strings]
                f = open('Countries Data File.txt', 'w+')
                f.writelines(input_list)
                f.close()

    except:
        # Write the data to the file if the file is empty.
        if first_row == [] or first_row == 'Empty':
            index_row = str('Country, ' + user_input + '\n')
            
            # Create list to add to the text file.
            input_list = [index_row]
            for country in countries_list:
                one = str(country)
                two = str(country_dict[country])
                input_list.append(str(one + ', ' + two + '\n'))

            # Write input to the text file.
            f = open('Countries Data File', 'w+')
            f.writelines(input_list)
            f.close()
"""