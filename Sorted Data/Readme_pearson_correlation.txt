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
of variable two, the y-axis.
Initially a linear regression fit is applied
If regression line isn't fitted on the scatterplot correctly, 
the logarithmic regresion fit must be used by un-commenting 
that section below (and commenting the linear section)