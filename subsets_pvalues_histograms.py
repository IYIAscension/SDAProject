
"""
Scientific Data Analysis - 2021-2022 - Project


This program filters data from the democracy_index.txt file, that I created to have list of the total Democracy Index values, 
takes the values of 2020 and places them in the democracy_index_2020.txt file. 
(This part of the code has been commented out
to prevent overlapping data placements if accidentaly ran another time.)

The user is prompted to give a variable name and the size of the bootstrapped samples 
(approximatly between 1000-10.000 to create a smooth distribution).
Next, country subsets are created based on the Total Democracy Index value, bootstrapped as many times as the user wishes
and are used to test the spread of the different distributions. 
Finally a Kolmogorov-Smirnov test is done to test the normality
of the subsets, and the non-parametric Mann-Whitney U test is used to compare the subsets using the Scipy package for p=0.95, 
with the results printed in the terminal. 
The final subset results are plotted in overlapping histograms together with their respective means.

*Note*
According to Wikipedia, 167 countries have a democracy index score. In the democracy_index_2020.txt file
there are 163 countries, in the countries.txt file there are 233 countries.
Manually added four countries in democracy_index_2020.txt with data from Wikipedia. These countries are
Algeria, Lithuania, Iran and Ukraine.

To run this file run the following:
python subsets_pvalues_histograms.py
input the file name for the variable data
input the number of bootstrap samples per subset (the default is 10000)

required files:
democracy_index_2020.txt
democracy_index.txt
Sorted Data directory
data_imported.py
"""

import re
import data_importer
import numpy as np
import matplotlib.pyplot as plt
from scipy import stats

if __name__ == "__main__":
    # Import country names from the covid database countries.txt file
    with open('countries.txt') as countries:
        countries_list = countries.read().splitlines()

    """ 
    ***This code was used to transform raw data from the democracy_index.txt file to the
    sorted democracy_index_2020.txt file and change country names if a synonim was used 
    in the countries.txt file.***

    # Imports the DI values
    with open('democracy_index.txt') as di:
        democracy_index = di.read().splitlines() 

    # Copied binder.txt file and code from M to convert sonims of country names
    binder = dict()
    with open("binder.txt", "r") as file:
        for line in file.readlines():
            parts = line.split(':')
            binder[parts[0].strip()] = parts[1].strip()

    democracy_index_split = [line.split('\t') for line in democracy_index]
    democracy_index_2020 = []
    for line in democracy_index_split:
        if line[1] == '2020':
            temp = [line[0], line[2]]
            if temp[0] in binder:
                temp[0] = binder[temp[0]]
            democracy_index_2020.append(temp)

    # Writes the DI 2020 values to the corresponding text file.
    f = open('democracy_index_2020.txt', 'w+')
    for line in democracy_index_2020:
        temp = str(', '.join(line))
        f.write(temp + '\n')
    f.close()
    """

    # Import country names and data from the democracy index (DI) countries file
    with open('democracy_index_2020.txt') as f:
        democracy_index = f.read().splitlines()

    # Create a dictionary with all countries' DI
    democracy_index_dict = {}
    for line in democracy_index:
        temp = line.split(', ')
        democracy_index_dict[temp[0]] = temp[1]

    # Divide the countries in subsets by their democracy index and create for each subset
    # a list and dictionary.
    full_t = 90.00
    full_b = 80.00
    flaw_t = 70.00
    flaw_b = 60.00
    hybr_t = 50.00
    hybr_b = 40.00
    auth_t = 30.00
    auth_m = 20.00
    auth_b = 0.00

    full_t_dict = {}
    full_b_dict = {}
    flaw_t_dict = {}
    flaw_b_dict = {}
    hybr_t_dict = {}
    hybr_b_dict = {}
    auth_t_dict = {}
    auth_m_dict = {}
    auth_b_dict = {}

    full_t_list = []
    full_b_list = []
    flaw_t_list = []
    flaw_b_list = []
    hybr_t_list = []
    hybr_b_list = []
    auth_t_list = []
    auth_m_list = []
    auth_b_list = []

    # Add data to each list according to the regime category scores.
    for element in democracy_index_dict:
        key = element
        try:
            value = float(democracy_index_dict[element])
        except:
            print(key, democracy_index_dict[key])
            continue
        if value > full_t:
            full_t_dict[key] = value
            full_t_list.append(value)
        elif value > full_b:
            full_b_dict[key] = value
            full_b_list.append(value)
        elif value > flaw_t:
            flaw_t_dict[key] = value
            flaw_t_list.append(value)
        elif value > flaw_b:
            flaw_b_dict[key] = value
            flaw_b_list.append(value)
        elif value > hybr_t:
            hybr_t_dict[key] = value
            hybr_t_list.append(value)
        elif value > hybr_b:
            hybr_b_dict[key] = value
            hybr_b_list.append(value)
        elif value > auth_t:
            auth_t_dict[key] = value
            auth_t_list.append(value)
        elif value > auth_m:
            auth_m_dict[key] = value
            auth_m_list.append(value)
        else:
            auth_b_dict[key] = value
            auth_b_list.append(value)

    len_full_t = len(full_t_list)
    len_full_b = len(full_b_list)
    len_flaw_t = len(flaw_t_list)
    len_flaw_b = len(flaw_b_list)
    len_hybr_t = len(hybr_t_list)
    len_hybr_b = len(hybr_b_list)
    len_auth_t = len(auth_t_list)
    len_auth_m = len(auth_m_list)
    len_auth_b = len(auth_b_list)

    # The following lines create dictionaries and lists of the different country subsets
    full_democracies_dict = dict(list(full_t_dict.items()) + list(full_b_dict.items()))
    flawed_democracies_dict = dict(list(flaw_t_dict.items()) + list(flaw_b_dict.items()))
    hybrid_regimes_dict = dict(list(hybr_t_dict.items()) + list(hybr_b_dict.items()))
    authoritarian_regimes_dict = dict(list(auth_t_dict.items()) + list(auth_m_dict.items()) + list(auth_b_dict.items()))

    full_democracies_list = full_t_list + full_b_list
    flawed_democracies_list = flaw_t_list + flaw_b_list
    hybrid_regimes_list = hybr_t_list + hybr_b_list
    authoritarian_regimes_list = auth_t_list + auth_m_list + auth_b_list

    full_democracies_len = len(full_democracies_list)
    flawed_democracies_len = len(flawed_democracies_list)
    hybrid_regimes_len = len(hybrid_regimes_list)
    authoritarian_regimes_len = len(authoritarian_regimes_list)

    full_democracies = [i for i in full_democracies_dict]
    flawed_democracies = [i for i in flawed_democracies_dict]
    hybrid_regimes = [i for i in hybrid_regimes_dict]
    authoritarian_regimes = [i for i in authoritarian_regimes_dict]
    all_democracy_index_countries = [i for i in democracy_index_dict]
        
    """ The next part performs bootstraps for the four main regime types, calculating normality, p-values
     and plotting the data in histograms. """

    # Prompt the user for the data variable and the number of samples
    user_input = input("Country Data File Name: ")
    bootstrap_samples = int(input('Number Of Bootstrap Samples: ') or '10000')    

    # Returns data for a specified country and variable
    def data_selector(country, user_input):
        # Select the user-input data
        data = np.array(data_importer.import_numerics_handle_none(
            str(country + '/' + user_input + '.data'), 0.0
        ))

        # Get the first value larger than 0.0 or the minimal_value
        for i in range(1, 8):
            temp = data[-i]
            if temp <= 0.0:
                continue
            elif temp > 0.0:
                return temp

    # Set up orginial data and bootstrapping parameters and perform bootstrap resampling for each subset
    def bootstrapper(data_list, subset_size, bootstrap_samples=bootstrap_samples):
        # Filter out possible None values from the data_list
        for count, i in enumerate(data_list):
            if i is None:
                data_list[count] = 0.0

        resampled_data_list = []
        for i in range(bootstrap_samples):
            temp = np.random.choice(data_list, subset_size)
            resampled_data_list.append(sum(temp)/len(temp))

        resampled_data_mean = np.mean(resampled_data_list)

        return resampled_data_list, resampled_data_mean

    # Calculate bootstraps for each subset
    subset1 = [data_selector(country, user_input) for country in full_democracies]
    subset2 = [data_selector(country, user_input) for country in flawed_democracies]
    subset3 = [data_selector(country, user_input) for country in hybrid_regimes]
    subset4 = [data_selector(country, user_input) for country in authoritarian_regimes]

    # Create bootstrapped subsets, which each resampling being equal in size of the original sample.
    # The exception for this is subset4_data, since its distribution was less smooth than the other three.
    # This is compensated by using a resampling size of 200.
    subset1_data = bootstrapper(subset1, len(subset1))[0]
    subset2_data = bootstrapper(subset2, len(subset2))[0]
    subset3_data = bootstrapper(subset3, len(subset3))[0]
    subset4_data = bootstrapper(subset4, 200)[0]

    # Check normality with Kolmogorov-Smirnovtest
    fullks = stats.kstest(subset1_data, subset2_data)
    flawedks = stats.kstest(subset1_data, subset3_data)
    hybridks = stats.kstest(subset1_data, subset4_data)
    authks = stats.kstest(subset2_data, subset3_data)

    # Calculating the statistical significance of the results with the non-parametric Mann-Whitney U test 
    full_vs_flawed = stats.mannwhitneyu(subset1_data, subset2_data)
    full_vs_hybrid = stats.mannwhitneyu(subset1_data, subset3_data)
    full_vs_authoritarian = stats.mannwhitneyu(subset1_data, subset4_data)
    flawed_vs_hybrid = stats.mannwhitneyu(subset2_data, subset3_data)
    flawed_vs_authoritarian = stats.mannwhitneyu(subset2_data, subset4_data)
    hybrid_vs_authoritarian = stats.mannwhitneyu(subset3_data, subset4_data)

    print('P-value: 0.95')
    print('full_vs_flawed: ', full_vs_flawed)
    print('full_vs_hybrid: ', full_vs_hybrid)
    print('full_vs_authoritarian', full_vs_authoritarian)
    print('flawed_vs_hybrid: ', flawed_vs_hybrid)
    print('flawed_vs_authoritarian: ', flawed_vs_authoritarian)
    print('hybrid_vs_authoritarian: ', hybrid_vs_authoritarian)
    print('fullks: ', fullks)
    print('flawedks: ', flawedks)
    print('hybridks', hybridks)
    print('authks: ', authks)

    # Plot the subset histograms of the data.
    # The Authoritarian Regime subset data is less equally distributed
    # compared to the other three subsets. This is compensated by using 
    # less bins and a higher degree of opacity (alpha).
    bins = 50
    plt.title('Four subsets ' + str(user_input) + ' with ' + str(bootstrap_samples) + ' samples ')
    plt.xlabel(user_input)
    plt.ylabel('Frequency')
    plt.hist(subset1_data, bins=bins, label='Full Democracy', color='b', alpha=0.5)
    plt.hist(subset2_data, bins=bins, label='Flawed Democracy', color='orange', alpha=0.5)
    plt.hist(subset3_data, bins=bins, label='Hybrid Regime', color='green', alpha=0.5)
    plt.hist(subset4_data, bins=15, label='Authoritarian Regime', color='r', alpha=0.4)
    plt.legend()
    plt.show()