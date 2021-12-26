# SDAProject

In this project we wish to answer the research question:
``Does the degree of authoritarianism (significantly) influence the rollout of vaccinations against COVID-19?''

We have the following hypotheses:
H0: A higher degree of authoritarianism in a country doesn’t lead to a higher rate of vaccination per capita.
H1: A higher degree of authoritarianism in a country leads to a higher rate of vaccination per capita.

The data map has the files which we used for our analysis.

2021DIPrediction.py predicts the 2021 index values from the data/EIU_Data.csv file and places them into the data/2021indexPredictions.txt file.
In it amountYears, endMult and order can be used to adjust predictions.
However, we used the "reuse" argument to reuse 2020 values as our 2021 predictions.
python3 2021DIPrediction.py reuse

predictionComparison.py prints out 2 values.
The first is the difference between 2019 values and 2020 values. The second is the difference between predicted 2020 values and true 2020 values.
In it amountYears, endMult and order can be used to adjust predictions.
python3 predictionComparison.py

regRegression.ipynb is a notebook which transforms Merged.csv into networks.
We used the first network, based on Ridge, four our poster and analysis.
