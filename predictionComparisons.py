# School: University of Amsterdam
# Course: Scientific Data Analysis 2021/2022
# Programmed by: Team 10
# Description: Compares predictions of 2020 and true 2019 values with true 2020
#              to see which is better. Predictions can be adjusted with the
#              amountYears, endMult and order variables.

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
    # amountYears is the amount of years prior to 2020
    # you want to interpolate through.
    amountYears = 3
    # Purely the values of years prior to 2020, 2020 and 2019.
    indexesArray = array[2:-1,8:(8+amountYears*6)]
    true2020 = array[2:-1,2:8]
    indexes2019 = array[2:-1,8:14]
    # List of countries. (At the start of each row.)
    countryArray = array[2:-1,:1]
    countryArray = [country.split(",")[0] for [country] in countryArray]
    # List of headers.
    prediction2020 = [["country", "VA", "PV", "GE", "RQ", "RL", "CC"]]
    # Increases the weight of the later years exponentially.
    endMult = 8
    # 1=Linear, 2=quadratic, 3=cubic interpolation.
    order = 1
    # For each country's row.
    for i, indexesCountry in enumerate(indexesArray):
        # Divides the indexes into seperate arrays and removes invalid values.
        vaIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 5 and val != "#N/A" and val != ".."]
        pvIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 4 and val != "#N/A" and val != ".."]
        geIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 3 and val != "#N/A" and val != ".."]
        rqIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 2 and val != "#N/A" and val != ".."]
        rlIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 1 and val != "#N/A" and val != ".."]
        ccIndex = [float(val) for index, val in enumerate(reversed(indexesCountry)) for _ in range(int(index / 6+1)**endMult) if index % 6 == 0 and val != "#N/A" and val != ".."]
        # Creates the x-axis of fitting length for each.
        vaX = np.arange(0, len(vaIndex))
        pvX = np.arange(0, len(pvIndex))
        geX = np.arange(0, len(geIndex))
        rqX = np.arange(0, len(rqIndex))
        rlX = np.arange(0, len(rlIndex))
        ccX = np.arange(0, len(ccIndex))
        # If there are values left to interpolate through, creates
        # interpolation, then immediately extrapolates for the
        # index just outside the list.
        if len(vaX) != 0:
            vaExtrapolation = float(intrp.UnivariateSpline(vaX, list(vaIndex), k=order)(len(vaIndex)))
        else:
            vaExtrapolation = np.isnan
        if len(pvX) != 0:
            pvExtrapolation = float(intrp.UnivariateSpline(pvX, list(pvIndex), k=order)(len(pvIndex)))
        else:
            pvExtrapolation = np.isnan
        if len(geX) != 0:
            geExtrapolation = float(intrp.UnivariateSpline(geX, list(geIndex), k=order)(len(geIndex)))
        else:
            geExtrapolation = np.isnan
        if len(rqX) != 0:
            rqExtrapolation = float(intrp.UnivariateSpline(rqX, list(rqIndex), k=order)(len(rqIndex)))
        else:
            rqExtrapolation = np.isnan
        if len(rlX) != 0:
            rlExtrapolation = float(intrp.UnivariateSpline(rlX, list(rlIndex), k=order)(len(rlIndex)))
        else:
            rlExtrapolation = np.isnan
        if len(ccX) != 0:
            ccExtrapolation = float(intrp.UnivariateSpline(ccX, list(ccIndex), k=order)(len(ccIndex)))
        else:
            ccExtrapolation = np.isnan
        # Adds row of extrapolated(predicted) indexes.
        prediction2020.append([str(countryArray[i]), vaExtrapolation,
                               pvExtrapolation,
                               geExtrapolation,
                               rqExtrapolation,
                               rlExtrapolation,
                               ccExtrapolation])
    # Clips values between 0 and 1.
    prediction2020 = [[prediction if isinstance(prediction, str) or prediction == np.isnan or prediction > 0 else 0 for prediction in indexesCountry] for indexesCountry in prediction2020]
    prediction2020 = [[prediction if isinstance(prediction, str) or prediction == np.isnan or prediction < 1 else 1 for prediction in indexesCountry] for indexesCountry in prediction2020]
    prediction2020 = np.asarray(prediction2020)[1:, 1:].flatten()
    true2020 = true2020.flatten()
    indexes2019 = indexes2019.flatten()
    prediction2020 = np.asarray([x for x in prediction2020 if x != ".." and is_float(x)]).astype(np.float)
    true2020 = np.asarray([x for x in true2020 if x != ".." and is_float(x)]).astype(np.float)
    indexes2019 = np.asarray([x for x in indexes2019 if x != ".." and is_float(x)]).astype(np.float)
    # Calculates the differences between the 2019 and 2020 values, as well as
    # the predictions and true 2020 values and prints them.
    diff2019 = 0
    diffPred = 0
    for i, true in enumerate(true2020):
            diff2019 += abs(true - indexes2019[i])
            diffPred += abs(true - prediction2020[i])
    print(diff2019, diffPred)
