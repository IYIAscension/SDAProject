README
command_line_extractor.py
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