# School: University of Amsterdam
# Course: Scientific Data Analysis 2021/2022
# Programmed by: Team 10
# Description: Predicts the 2021 index values and writes them to
#              data/2021IndexPredictions.txt. Using "reuse" as an argument
#              reuses the 2020 values as 2021 predictions.
#              If not, amountYears, endMult and order can adjust predictions.

import scipy.interpolate as intrp
import numpy as np
import csv
import sys

if __name__ == "__main__":
    with open("data/EIU_Data.csv") as f:
        array = csv.reader(f)
        array = np.array(list(array))
    # If "reuse" is the argument, predicts 2021 to be 2020 values.
    if len(sys.argv) > 1:
        if sys.argv[1] == "reuse":
            prediction2021 = np.concatenate(([["country", "VA", "PV", "GE", "RQ", "RL", "CC"]], np.concatenate((array[2:-1,:1], array[2:-1,2:8]), axis=1)), axis=0)
        else:
            print("Use the \"reuse\" argument to reuse 2020 data, use no argument to use the predictor.")
    else:
        # amountYears is the amount of years prior to 2021
        # you want to interpolate through.
        amountYears = 3
        # Purely the values of years prior to 2021
        indexesArray = array[2:-1,2:(2+amountYears*6)]
        # List of countries. (At the start of each row.)
        countryArray = array[2:-1,:1]
        countryArray = [country.split(",")[0] for [country] in countryArray]
        # List of headers.
        prediction2021 = [["country", "VA", "PV", "GE", "RQ", "RL", "CC"]]
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
            # Adds row of extrapolated(predicted) indexes.
            prediction2021.append([str(countryArray[i]), vaInterpolation,
                                pvInterpolation,
                                geInterpolation,
                                rqInterpolation,
                                rlInterpolation,
                                ccInterpolation])
        # Clips values between 0 and 1.
        prediction2021 = [[prediction if isinstance(prediction, str) or prediction == np.isnan or prediction > 0 else 0 for prediction in indexesCountry] for indexesCountry in prediction2021]
        prediction2021 = [[prediction if isinstance(prediction, str) or prediction == np.isnan or prediction < 1 else 1 for prediction in indexesCountry] for indexesCountry in prediction2021]
    # Writes predictions to file with ", " delimiter.
    with open("data/2021IndexPredictions.txt", "w+") as f:
        f.writelines(', '.join(str(elem) for elem in row) + '\n'for row in prediction2021)
