# Small script file to write the names of all countries to a plaintext file
# for easy indexing in Python.
import os

if __name__ == "__main__":
    names = []
    # Loop over all directories.
    for x in os.walk('.'):
        name = x[0]
        # Ignore certain directories.
        if name == '.':
            continue

        if 'Data Processor' in name:
            continue

        if '__pycache__' in name:
            continue

        # We want country data, not income data.
        if 'income' in name:
            continue

        names.append(name[2:])
    # List of names has been established. Dump it to a file.
    with open('countries.txt', 'w', encoding='utf8') as file:
        file.write('\n'.join(names))
