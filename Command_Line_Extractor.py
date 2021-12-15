""" 
Selects data from the Sorted Data (M's filtering and sorting of binary data) directory for all countries.
Prompts the user for the name of the data file to be extracted from all countries and
places the results in Countries Data File.txt.

Optionally, a minimal value for the data can be added so as to filter very small values which might be data-errors of any kind
"""


import data_importer
import numpy as np

if __name__ == "__main__":
    # Countries 
    with open('countries.txt') as countries:
        countries_list = countries.read().splitlines()

    user_input = input("Data you want to extract from Sorted Data ([exact filename] without .data): ")
    minimal_value = float(input("To create a lower limit cutoff-value please enter the minimal usable value (everything lower will be entered as 0.0) (default: 0.0): ") or '0.0')

    country_dict = {}
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
                country_dict[country] = temp
                break

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

