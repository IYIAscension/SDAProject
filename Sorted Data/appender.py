from sys import argv
from data_importer import import_final

if __name__ == "__main__":
    # Get the name of the file to target.
    target_file = argv[-2]
    print(f"Source file: [{target_file}]")
    if not (target_file.endswith('.txt') or target_file.endswith('.csv')):
        print(
            "For safety, it is not allowed to append to a file that does not" +
            " have the TXT or CSV extension.")
        exit(1)

    # Get the name of the dataset to append.
    append_name = argv[-1]
    if not append_name.endswith('.data'):
        append_name += '.data'

    # Read all lines in the file.
    with open(target_file, "r") as file:
        lines = file.read().strip('\r').split('\n')
        # Catch trailing newline character.
        if len(lines[-1]) == 0:
            lines = lines[:-1]

    # Import binder.
    binder = dict()
    with open("binder.txt", "r") as file:
        for line in file.readlines():
            parts = line.split(':')
            binder[parts[0].strip()] = parts[1].strip()

    # Import the dataset.
    head = append_name[:-5]
    width = len(head)
    column = [head]
    for line in lines[1:]:
        parts = line.split(',')
        countryName = parts[0].strip()
        # If a binding is defined, use it.
        if countryName in binder:
            countryName = binder[countryName]
        # Now import the data.
        value = import_final(f'{countryName}/{append_name}')
        if value is None:
            column.append('')
        else:
            val = str(value)
            width = max(width, len(val))
            column.append(val)

    # Next step is to write to the file's lines.
    for i in range(len(column)):
        val = column[i]
        # Pad out the values so each value is of even width.
        padding = width - len(val)
        lines[i] = f"{lines[i]}, {' ' * padding}{val}"

    # Generate a new file name.
    newname = target_file[:-4] + f"_app_[{head}]" + target_file[-4:]
    print(f"Written to file [{newname}]")

    # Write the file.
    with open(newname, "w") as file:
        file.write('\n'.join(lines))
