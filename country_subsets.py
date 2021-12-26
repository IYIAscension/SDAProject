

"""
Scientific Data Analysis - 2021-22 - Project
This file uses the input from subset_input.txt to create four different subset inputs, bootstrapped the samples,
perform the Kolmogorov-Smirnov test and Mann-Whitney U test and plot the results.

The user is prompted to give the variable name and the size of the bootstrapped samples.
Next, country subsets are created based on the subset_input.txt file, bootstrapped as many times as the user wishes
and are used to compare different COVID variables. Finally a Kolmogorov-Smirnov test is done to test the normality
of the subsets, and the non-parametric Mann-Whitney U test is used to compare the subsets using the Scipy package for p=0.95, 
with the results printed in the terminal. 
The final subset results are plotted in overlapping histograms together with their respective means.

*Note*

To run this file run the following:
python subsets_pvalues_histograms.py
input the file name for the variable data
input the number of bootstrap samples per subset (the default is 10000)

required files:
subset_input.txt
Sorted Data directory
data_imported.py
"""

import re
import data_importer
import numpy as np
import matplotlib.pyplot as plt
from scipy import stats
from itertools import groupby
                                                                                                                                                                                                                                                                                                                                                                                                                 
if __name__ == "__main__":
    # Import country names from the covid database countries.txt file
    with open('countries.txt') as countries:
        countries_list = countries.read().splitlines()

    # Import country names and data from the democracy index (DI) countries file
    with open('subset_input.txt') as f:
        subset_input = f.read().splitlines()

    # Prompt the user for the data variable and the number of samples
    user_input = input("Country Data File Name: ")
    bootstrap_samples = int(input('Number Of Bootstrap Samples: ') or '100000')    

    split_indeces = []
    split_country_list = []
    for count, i in enumerate(subset_input):
        if i == 'END':
            split_indeces.append(count)

    s1 = subset_input[1:split_indeces[0]]
    s2 = subset_input[split_indeces[0]+2:split_indeces[1]]
    s3 = subset_input[split_indeces[1]+2:split_indeces[2]]
    s4 = subset_input[split_indeces[2]+3:split_indeces[3]]
    subset_countries_dict = {
        'Subset 1': s1, 
        'Subset 2': s2, 
        'Subset 3': s3,
        'Subset 4': s4
    }

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


    subset1 = [subset_countries_dict['Subset 1']][0]
    subset2 = [subset_countries_dict['Subset 2']][0]
    subset3 = [subset_countries_dict['Subset 3']][0]
    subset4 = [subset_countries_dict['Subset 4']][0]

    print(subset1)

    # Select each country subset data and place it in the list
    subset1_data = [data_selector(i, user_input) for i in subset1]
    subset2_data = [data_selector(i, user_input) for i in subset2]
    subset3_data = [data_selector(i, user_input) for i in subset3]
    subset4_data = [data_selector(i, user_input) for i in subset4]

    # Calculate bootstraps for each subset
    s1_bootstrapped = bootstrapper(subset1_data, len(subset1))[0]
    s2_bootstrapped = bootstrapper(subset2_data, len(subset2))[0]
    s3_bootstrapped = bootstrapper(subset3_data, len(subset3))[0]
    s4_bootstrapped = bootstrapper(subset4_data, len(subset4))[0]

    # Check normality with Kolmogorov-Smirnovtest
    s1_vs_s2_n = stats.kstest(subset1_data, subset2_data)
    s1_vs_s3_n = stats.kstest(subset1_data, subset3_data)
    s1_vs_s4_n = stats.kstest(subset1_data, subset4_data)
    s2_vs_s3_n = stats.kstest(subset2_data, subset3_data)

    # Calculating the statistical significance of the results with the non-parametric Mann-Whitney U test 
    s1_vs_s2 = stats.mannwhitneyu(subset1_data, subset2_data)
    s1_vs_s3 = stats.mannwhitneyu(subset1_data, subset3_data)
    s1_vs_s4 = stats.mannwhitneyu(subset1_data, subset4_data)
    s2_vs_s3 = stats.mannwhitneyu(subset2_data, subset3_data)
    s2_vs_s4 = stats.mannwhitneyu(subset2_data, subset4_data)
    s3_vs_s4 = stats.mannwhitneyu(subset3_data, subset4_data)

    print('P-value: 0.95')
    print('Subset 1 vs 2: ', s1_vs_s2)
    print('Subset 1 vs 3: ', s1_vs_s3)
    print('Subset 1 vs 4', s1_vs_s4)
    print('Subset 2 vs 3: ', s2_vs_s3)
    print('Subset 2 va 4: ', s2_vs_s4)
    print('Subset 3 vs 4: ', s3_vs_s4)
    print('S1: ', s1_vs_s2_n)
    print('S2: ', s1_vs_s3_n)
    print('S3', s1_vs_s4_n)
    print('S4: ', s2_vs_s3_n
    )

    # Plot the subset histograms of the data.
    bins = 50
    plt.title('Histograms of four country subsets for vaccinations per thousand inhabitants')
    plt.xlabel('Number of vaccinated people per thousand inhabitants')
    plt.ylabel('Frequency')
    plt.hist(subset1_data, bins=bins, label='Subset 1', color='b', alpha=0.8)
    plt.hist(subset2_data, bins=bins, label='Subset 2', color='g', alpha=0.8)
    plt.hist(subset3_data, bins=bins, label='Subset 3', color='r', alpha=0.8)
    plt.hist(subset4_data, bins=15, label='Subset 4', color='k', alpha=0.6)
    plt.legend()
    plt.show()