FROM python:3

ENV PYTHONUNBUFFERED True

RUN mkdir /app
WORKDIR /app

RUN apt-get update && apt install -y libpq-dev build-essential python3 python3-pip 

RUN python3 -m pip install --upgrade pip

COPY ./requirements.txt /

RUN python3 -m pip install -r requirements.txt

COPY . .

CMD [ "python3", "main.py" ]
