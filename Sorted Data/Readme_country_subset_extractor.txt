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