import scipy.interpolate as intrp
import numpy as np
import csv

if __name__ == "__main__":
    with open("data/EIU_Data.csv") as f:
        array = csv.reader(f)
        array = np.array(list(array))
    indexesArray = array[2:-1,2:]
    countryArray = array[2:-1,:1]
    countryArray = [country.split(",")[0] for [country] in countryArray]
    prediction2021 = [["country", "VA", "PV", "GE", "RQ", "RL", "CC"]]
    order = 1
    for i, indexesCountry in enumerate(indexesArray):
        vaIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index*3 / 6)+1) if index % 6 == 5 and val != "#N/A" and val != ".."]
        pvIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index*3 / 6)+1) if index % 6 == 4 and val != "#N/A" and val != ".."]
        geIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index*3 / 6)+1) if index % 6 == 3 and val != "#N/A" and val != ".."]
        rqIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index*3 / 6)+1) if index % 6 == 2 and val != "#N/A" and val != ".."]
        rlIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index*3 / 6)+1) if index % 6 == 1 and val != "#N/A" and val != ".."]
        ccIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index*3 / 6)+1) if index % 6 == 0 and val != "#N/A" and val != ".."]
        vaX = np.arange(0, len(vaIndex))
        pvX = np.arange(0, len(pvIndex))
        geX = np.arange(0, len(geIndex))
        rqX = np.arange(0, len(rqIndex))
        rlX = np.arange(0, len(rlIndex))
        ccX = np.arange(0, len(ccIndex))
        if len(vaX) != 0:
            vaInterpolation = float(intrp.UnivariateSpline(vaX, list(vaIndex), k=order)(len(vaIndex)))
        else:
            vaInterpolation = np.isnan
        if len(pvX) != 0:
            pvInterpolation = float(intrp.UnivariateSpline(pvX, list(pvIndex), k=order)(len(pvIndex)))
        else:
            pvInterpolation = np.isnan
        if len(geX) != 0:
            geInterpolation = float(intrp.UnivariateSpline(geX, list(geIndex), k=order)(len(geIndex)))
        else:
            geInterpolation = np.isnan
        if len(rqX) != 0:
            rqInterpolation = float(intrp.UnivariateSpline(rqX, list(rqIndex), k=order)(len(rqIndex)))
        else:
            rqInterpolation = np.isnan
        if len(rlX) != 0:
            rlInterpolation = float(intrp.UnivariateSpline(rlX, list(rlIndex), k=order)(len(rlIndex)))
        else:
            rlInterpolation = np.isnan
        if len(ccX) != 0:
            ccInterpolation = float(intrp.UnivariateSpline(ccX, list(ccIndex), k=order)(len(ccIndex)))
        else:
            ccInterpolation = np.isnan
        prediction2021.append([str(countryArray[i]), vaInterpolation,
                               pvInterpolation,
                               geInterpolation,
                               rqInterpolation,
                               rlInterpolation,
                               ccInterpolation])
    prediction2021 = [[prediction if isinstance(prediction, str) or prediction == np.isnan or prediction > 0 else 0 for prediction in indexesCountry] for indexesCountry in prediction2021]
    with open("data/2021IndexPredictions.txt", "w+") as f:
        f.writelines(', '.join(str(elem) for elem in row) + '\n'for row in prediction2021)
