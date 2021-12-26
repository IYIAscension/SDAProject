import data_importer
import numpy as np
import matplotlib.pyplot as plt

if __name__ == "__main__":
    new_cases = np.array(data_importer.import_time_series(
        'Netherlands/new_cases.data'
    ))
    vaccinations = np.array(data_importer.import_time_series(
        'Netherlands/people_fully_vaccinated.data'
    ))

    # Plot vaccination statistics for the Netherlands over time.
    x_axis = np.array(list(range(len(vaccinations))))
    plt.plot(
        x_axis, vaccinations, 'g-'
    )
    plt.xlabel('Data points')
    plt.ylabel('Fully vaccinated people')
    plt.title(
        'The amount of fully vaccinated people in the Netherlands.'
    )
    plt.tight_layout()
    plt.show()
