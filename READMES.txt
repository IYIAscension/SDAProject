By Jan-Joost Raedts - 5841801

README
txt_import_export.py
This file must be placed in the Sorted Data directory.
This is a test file that reads in text files, appends a new
character to the end of each line and writes the new lines
to a text file.


README
country_data_extractor.py
This file must be placed in the Sorted Data directory.
It needs the following files to function:
countries.txt
data_importer.py
Countries Data File.txt

This code uses the data_importer.py code to select for all countries
the following variables: 
-Population Size
-Latest Cases
-Total Vaccinations
-Fully Vaccinated
-Total Cases
-Total Deaths

Each variables is placed on the row to their corresponding country
and written to the Countries Data File.txt.

To run:
python country_data_extractor.py


README
subset_extractor.py
This file must be placed in the Sorted Data directory.
It needs the following files to function:
Subset_Input_File.txt
Subset_Results.txt
data_importer.py

This program extracts the selected data from each countries in the
Sorted Data directory, calculates the average and places the result 
in the Subset_Results.txt file.

To run:    
python subset_extractor.py
Whem prompted input the exact filename 
of the variable you want to use
To create a lower limit cutoff-value,
the user is asked to enter the minimal 
usable value cut (everything lower will be entered as 0.0)


README
command_line_extractor
This file must be placed in the Sorted Data directory.
It needs the following files to function:


Selects data from the Sorted Data (M's filtering and sorting 
of binary data) directory for all countries.
Prompts the user for the name of the data file to be extracted 
from all countries and places the results in Countries Data File.txt.

Optionally, a minimal value for the data can be added so as to filter very small values which might be data-errors of any kind

To run:
command_line_extractor.py
Whem prompted input the exact filename 
of the variable you want to use
To create a lower limit cutoff-value,
the user is asked to enter the minimal 
usable value cut (everything lower will be entered as 0.0)



README
pearson_correlation.py
This file must be placed in the Sorted Data directory.
It needs the following files to function:
countries.txt
Countries Data File.txt
data_importer.py

With this file two variables at a time for all countries 
at once from the sorted data directory can be compared.
For all comparissons a Pearson correlation is calculated 
and a scatterplot with proper title and axis lables is produced.

To run:
python pearson_correlation.py 
Whem prompted input the exact filename of the variable 
you want to be the x-axis, followed by a prompt for input 
of variable two, the y-axis. Next the user is prompted to 
select either a linear or logarithmic scale for the 
correlations and plots.


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
and are used to compare different COVID variables. Finally the 
p-values of the subsets are calculated using a two-sample
T-Test from the Scipy package for p=0.95, with the results printed 
in the terminal. The final subset results are plotted in overlapping 
histograms.

To run:
pyhon subsets_pvalues_histograms.py
Input the file name for the variable data
Input the number of bootstrap samples per subset 
(the default is 10000)


README
country_subsets.py
This file must be placed in the Sorted Data directory.
It needs the following files to function:
subset_input.txt
Sorted Data directory
data_imported.py

This file creates four country subsets based on the input from the
subset_input.txt file.
The user is prompted to give a variable name and the size of the 
bootstrapped samples. Next, country subsets are bootstrapped as many times as the user wishes
and are used to compare different COVID variables. Finally the 
p-values of the subsets are calculated using a two-sample
T-Test from the Scipy package for p=0.95, with the results printed 
in the terminal. The final subset results are plotted in overlapping 
histograms.

To run:
python country_subsets.py
Input the file name for the variable data
Input the number of bootstrap samples per subset 
(the default is 10000)
