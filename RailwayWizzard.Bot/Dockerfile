FROM python:3.9-slim as base

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE 1
ENV PYTHONUNBUFFERED 1

RUN apt-get update && apt install -y libpq-dev build-essential python3 python3-pip gcc


COPY ./RailwayWizzard.Bot/requirements.txt /app/requirements.txt
RUN pip install --upgrade pip
RUN pip install -r requirements.txt

COPY . /app

CMD ["python3", "main.py"]

EXPOSE 5000
