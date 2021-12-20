from numpy.lib.function_base import diff
import scipy.interpolate as intrp
import numpy as np
import csv

def is_float(element):
    try:
        float(element)
        return True
    except TypeError:
        return False


if __name__ == "__main__":
    with open("data/EIU_Data.csv") as f:
        array = csv.reader(f)
        array = np.array(list(array))
    amountVals = 3
    indexesArray = array[2:-1,8:(8+amountVals*6)]
    true2020 = array[2:-1,2:8]
    indexes2019 = array[2:-1,8:14]
    countryArray = array[2:-1,:1]
    countryArray = [country.split(",")[0] for [country] in countryArray]
    prediction2020 = [["country", "VA", "PV", "GE", "RQ", "RL", "CC"]]
    endMult = 8
    order = 1
    for i, indexesCountry in enumerate(indexesArray):
        vaIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 5 and val != "#N/A" and val != ".."]
        pvIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 4 and val != "#N/A" and val != ".."]
        geIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 3 and val != "#N/A" and val != ".."]
        rqIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 2 and val != "#N/A" and val != ".."]
        rlIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 1 and val != "#N/A" and val != ".."]
        ccIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 0 and val != "#N/A" and val != ".."]
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
        prediction2020.append([str(countryArray[i]), vaInterpolation,
                               pvInterpolation,
                               geInterpolation,
                               rqInterpolation,
                               rlInterpolation,
                               ccInterpolation])
    prediction2020 = [[prediction if isinstance(prediction, str) or prediction == np.isnan or prediction > 0 else 0 for prediction in indexesCountry] for indexesCountry in prediction2020]
    prediction2020 = np.asarray(prediction2020)[1:, 1:].flatten()
    true2020 = true2020.flatten()
    indexes2019 = indexes2019.flatten()
    prediction2020 = np.asarray([x for x in prediction2020 if x != ".." and is_float(x)]).astype(np.float)
    true2020 = np.asarray([x for x in true2020 if x != ".." and is_float(x)]).astype(np.float)
    indexes2019 = np.asarray([x for x in indexes2019 if x != ".." and is_float(x)]).astype(np.float)
    print(len(prediction2020), len(true2020), len(indexes2019))
    diff2019 = 0
    diffPred = 0
    for i, true in enumerate(true2020):
            diff2019 += abs(true - indexes2019[i])
            diffPred += abs(true - prediction2020[i])
    print(diffPred, diff2019)
