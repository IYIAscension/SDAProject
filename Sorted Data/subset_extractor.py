    """ 
    Scientific Data Analysis - 2021-22- Project
    
    This program runs in the Sorted Data directory and uses two files that
    must also be in the Sorted Data directory. 
    
    The first one is Subset_Input_File.txt where all the countries of a particular
    subset are placed. This program extracts the selected data from these countries,
    calculates the average and places the result in the second file, Subset_Results.txt.
    
    python subset_extractor.py
    To create a lower limit cutoff-value the user is asked to enter the minimal usable value cut (everything lower will be entered as 0.0) 

    
    """


import data_importer
import numpy as np

if __name__ == "__main__":

    # Opens M's Countries file
    with open('countries.txt') as countries:
        countries_list = countries.read().splitlines()

    minimal_value = float(input("To create a lower limit cutoff-value please enter the minimal usable value (everything lower will be entered as 0.0): ") or '0.0')

    user_input_subset = map(str, input('Enter one or more subsets, seperated by space: '))
    user_input_subset_list = list(user_input_subset).split()

    if len(user_input_subset_list) == 0:
        print('Error: no subsets were given.')

    elif len(user_input_subset_list) > 0:
        with open('Subset_Input_File.txt') as subset:
            subset_list = subset.read().splitlines()
        subset_input = user_input_subset_list

        subset_title = subset_list[0]
        subset_countries = subset_list[1:]
        print(subset_list)

        country_dict = {}
        total_data = []
        for country in subset_countries:
            data = np.array(data_importer.import_numerics_handle_none(
                str(country + '/' + user_input + '.data'), 0.0
            ))
            country_dict[country] = data[-1]
            total_data.append(data[-1])

            # Get the first value larger than 0.0 or the minimal_value to align the weeks from the sorted data directory
            for i in range(1, 8):
                temp = data[-i]
                if temp <= minimal_value:
                    continue
                elif temp > minimal_value:
                    country_dict[country] = temp
                    break
            
        # Get average of datapoints   (effort might be needed here to individualize approach per category)
        subset_average = sum(total_data)/len(total_data)

        # Write data to file, starting with a empty line and placing the values under the index row
        index_row = '\n' + user_input_subset + ', ' + user_input

        # Create list to add to the Subset_Result.txt file.
        input_list = [index_row + '\n' + 'Average: ' + str(subset_average) + '\n']
        for country in subset_countries:
            one = str(country)
            two = str(country_dict[country])
            input_list.append(str(one + ', ' + two + '\n'))

        # Write input to the text file.
        f = open('Subset_Results.txt', 'w+')
        f.writelines(input_list)
        f.close()

            