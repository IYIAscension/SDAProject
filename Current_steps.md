MBT de vaccinatie-dataset:
https://covid.ourworldindata.org/data/owid-covid-data.csv
Bovenstaande OWIDCOVID 32mb datafile importeren en een pandas df van maken
Alle data filteren op week (staan alleen data per dag nu, met dag-datum; weet niet of daar een makkelijkere manier voor is dan per zeven selecteren?), hernoemen naar iets als 'df_weekly'
wat er dan vervolgens moet gebeuren:
- dfweekly filteren per land, creeer een df per land als volgt ' df[landnaam]'
- df_[landnaam] filteren voor hoeveelheid vaccinaties, noem dit 'dfvaccin[landnaam]'
- uit df_[landnaam] de bevolkingsgrootte filteren, noem deze 'dfpopulation[landnaam]'

Daarna kunne we al analyses starten.
