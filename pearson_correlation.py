
"""
Scientific Data Analysis - 2021-22 - Project

With this file two variables at a time for all countries at once from the sorted data directory can be compared.
For all comparissons a Pearson correlation is calculated and a scatterplot with proper title and axis lables is produced.

The program is run by:
python pearson_correlation.py 
Whem prompted input the exact filename of the variable you want to be the x-axis, followed by a prompt for input of variable two, the y-axis.
Initially a linear regression fit is applied
If regression line isn't fitted on the scatterplot correctly, the logarithmic regresion fit must be used by un-commenting that section below (and commenting the linear section)

"""



from typing import ValuesView
import data_importer
import numpy as np
import matplotlib.pyplot as plt
from scipy import stats

if __name__ == "__main__":
    # Importing the countries from  M's country file
    with open('countries.txt') as countries:
        countries_list = countries.read().splitlines()

    # Ask user input for variable names
    var1 = input('Variable 1 (x-axis):')
    var2 = input('Variable 2 (y-axis):')

    # Select data from the Country Data File
    with open('Countries Data File.txt', 'r+') as f:
        lines = f.readlines()
        lines_list = [line.rstrip() for line in lines]

    # Select the line with each country's data
    index_row = lines_list[0]
    index_list = index_row.split(', ')
    var1_index = 0
    for count, element in enumerate(index_list):
        if element == var1:
            var1_index = count
    var2_index = 0
    for count, element in enumerate(index_list):
        if element == var2:
            var2_index = count

    values = lines_list[1:]
    var1_data = [float(line.split(',')[var1_index]) for line in values]
    var2_data = [float(line.split(',')[var2_index]) for line in values]

    # Compare the two variables
    result = stats.linregress(var1_data, var2_data)
    print('The Pearson R is: ', result.rvalue)
    regression_values = [(result.intercept + result.slope*i) for i in var1_data]

    # Logarithmic variables
    var1_data_log = [np.log10(i+1) for i in var1_data]
    var2_data_log = [np.log10(i+1) for i in var2_data]

    result_log = stats.linregress(var1_data_log, var2_data_log)
    x_values = np.linspace(10**4, 10**9)
    regression_values_log = [(result_log.intercept + result_log.slope*i) for i in x_values]
    print(result_log.intercept)
    print(result_log.slope)
    print(regression_values_log)

    # Plot the points

    # If linear fit doesn't work; uncomment this section instead and comment the linear section

    # title = str(index_list[var1_index] + ' vs ' + index_list[var2_index] + ' for all countries')
    # xlabel = str(index_list[var1_index])
    # ylabel = str(index_list[var2_index])
    # plt.scatter(var1_data, var2_data)
    # plt.plot(x_values, regression_values_log, label='Regression', color='r')
    # plt.title(title)
    # plt.xlabel(xlabel)
    # plt.ylabel(ylabel)
    # plt.xscale('log')
    # plt.yscale('log')
    # plt.show()


    title = str(index_list[var1_index] + ' vs ' + index_list[var2_index] + ' for all countries')
    xlabel = str(index_list[var1_index])
    ylabel = str(index_list[var2_index])
    plt.scatter(var1_data, var2_data)
    plt.plot(var1_data, regression_values, label='Regression', color='r')
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.xscale('linear')
    plt.yscale('linear')
    plt.show()
