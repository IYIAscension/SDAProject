# Script voor aanpassen van de .txt analysedata-files
# 'werkt' nu nog even met 'TEST' als placeholder voor de input
# Ik maak er straks een OOP-versie van

# Open file for reading
text_file = open('Data/test_data_test2.txt', 'r')

# Get length of file
length_file = 0
for l in text_file:
    if l != '\n':
        length_file += 1

# Get back to the beginning of the file, read all the lines in the file
text_file.seek(0)
text_file_lines = text_file.readlines()

# Create list to add to file
input_list = [', DI\n']
for i in range(length_file):
    input_list.append(', TEST\n')

# Combine original lines with new input
total_lines = [text_file_lines[i].strip('\n') + input_list[i] for i in range(length_file)]

# Open file for writing
text_file = open('Data/test_data_test2.txt', 'w')
text_file.writelines(total_lines)

# Close the file
text_file.close()

