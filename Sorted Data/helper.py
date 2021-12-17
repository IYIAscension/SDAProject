from data_importer import import_time_series, list_countries
import sys

if __name__ == "__main__":
    countries = list_countries()
    dataset = sys.argv[-1]
    print(f"--- COUNTRY REPORT FOR SET {dataset} ---")
    for country in countries:
        deaths = import_time_series(f"{country}/{dataset}.data")
        print(f'{country}: {deaths[-1]:.1f}')
