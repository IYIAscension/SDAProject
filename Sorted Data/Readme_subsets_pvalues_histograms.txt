README
subsets_pvalues_histograms.py
This file must be placed in the Sorted Data directory.
It needs the following files to function:
democracy_index_2020.txt
democracy_index.txt
Sorted Data directory
data_imported.py

This file filters data from the democracy_index.txt file, 
in which raw data from the Democracy Index was placed,
and places the filtered results in the democracy_index_2020.txt 
file. This part of the code has been commented out
to prevent overlapping data placements.

The user is prompted to give a variable name and the size of the 
bootstrapped samples. Next, country subsets are created based on 
the Democracy Index, bootstrapped as many times as the user wishes
and are used to compare different COVID variables. Finally normality
is tested using a Kolmogorov-Smirnov test and the p-values of the 
subsets are calculated using a Mann-Whitney U-test from the 
Scipy package for p=0.95, with the results printed in the terminal. 
The final subset results are plotted in overlapping histograms.

To run:
pyhon subsets_pvalues_histograms.py
Input the file name for the variable data
Input the number of bootstrap samples per subset 
(the default is 10000)
