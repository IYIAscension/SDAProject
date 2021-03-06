# Gathers, for each country, the following statistics:
# 1. Population size
# 2. Number of infections
# 3. Total casualties

import data_importer
from typing import Dict, Iterable
import numpy as np


class Dataset:
    def __init__(self, country: str):
        # Create filepaths and import all files.
        date = data_importer.import_dates(country + '/date.data')
        population = data_importer.import_time_series(
            country + '/population.data'
        )
        vaccinations = data_importer.import_time_series(
            country + '/total_vaccinations.data'
        )
        cases = data_importer.import_time_series(
            country + '/total_cases.data'
        )
        deaths = data_importer.import_time_series(
            country + '/total_deaths.data'
        )
        excess_mortality = data_importer.import_numerics_handle_none(
            country + '/excess_mortality.data', 0.0
        )

        # Now filter by date to get indices for data slicing.
        (start, stop) = data_importer.limit_by_date(
            date,
            # January 1st, 2021
            (2021, 1, 1),
            # November 30th, 2021
            (2021, 11, 30)
        )
        # Convert to Numpy for speed-processing.
        self.__population = np.array(population[start:stop])
        self.__vaccinations = np.array(vaccinations[start:stop])
        self.__cases = np.array(cases[start:stop])
        self.__deaths = np.array(deaths[start:stop])
        self.__excessmort = np.array(excess_mortality[start:stop])

    def get_population(self) -> Iterable[float]:
        """Returns the population statistics of this country as a Numpy
        array.

        Returns:
            Iterable[float]: The Numpy array of population data.
        """
        return self.__population

    def get_vaccinations(self) -> Iterable[float]:
        """Returns the vaccination statistics of this country as a Numpy
        array.

        Returns:
            Iterable[float]: The Numpy array of vaccination data.
        """
        return self.__vaccinations

    def get_cases(self) -> Iterable[float]:
        """Returns the number-of-covid-cases statistics of this country as
        a Numpy array.

        Returns:
            Iterable[float]: The Numpy array of number-of-cases data.
        """
        return self.__cases

    def get_deaths(self) -> Iterable[float]:
        """Returns the number-of-deaths statistics of this country as a Numpy
        array.

        Returns:
            Iterable[float]: The Numpy array of fatality data.
        """
        return self.__deaths

    def get_excess_mortality(self) -> Iterable[float]:
        """Returns the excess mortality statistics of this country as a Numpy
        array.

        Returns:
            Iterable[float]: The Numpy array of excess mortality.
        """
        return self.__excessmort


def gather() -> Dict[str, Dataset]:
    """Gathers all of the data for each country and returns a dictionary
    where the key is the country name and the value is the respective
    dataset.

    Returns:
        Dict[str, Dataset]: The returned dictionary of country-specific
        data.
    """
    output = dict()
    for country in data_importer.list_countries():
        print(f'Parsing data for country [{country}]')
        output[country] = Dataset(country)

    return output


if __name__ == "__main__":
    data = gather()
    print('Demo: first ten samples of case data from the Netherlands.')
    dataset = data['Netherlands']
    print(dataset.get_cases()[0:10])
