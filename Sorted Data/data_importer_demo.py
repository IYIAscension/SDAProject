import data_importer
import numpy as np
import matplotlib.pyplot as plt

if __name__ == "__main__":
    new_cases = np.array(data_importer.import_numerics_handle_none(
        'Netherlands/new_cases.data', 0.0
    ))
    vaccinations = np.array(data_importer.import_numerics_handle_none(
        'Netherlands/people_fully_vaccinated.data', 0.0
    ))

    x_axis = np.array(list(range(len(vaccinations))))
    plt.plot(
        x_axis, vaccinations, 'g-'
    )
    plt.show()
