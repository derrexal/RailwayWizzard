FROM python:3

ENV PYTHONUNBUFFERED True

WORKDIR /app

COPY . .

RUN apt-get update && apt install -y libpq-dev build-essential python3 python3-pip1

RUN python3 -m pip install --upgrade pip

RUN python3 -m pip install -r requirements.txt

CMD [ "python3", "main.py" ]
