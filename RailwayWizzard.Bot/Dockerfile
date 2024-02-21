FROM python:3

WORKDIR /app
COPY /RailwayWizzard.Bot/ .

RUN apt-get update && apt install -y libpq-dev build-essential python3 python3-pip 

RUN python -m pip install --upgrade pip

RUN python -m pip install -r requirements.txt

RUN python main.py
 