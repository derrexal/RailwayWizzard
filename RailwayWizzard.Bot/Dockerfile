FROM python:3

ENV PYTHONUNBUFFERED True

WORKDIR /app

COPY . .

RUN apt-get update && apt-get install -y libpq-dev build-essential

RUN python3 -m pip install --upgrade pip

RUN python3 -m pip install --no-cache-dir -r requirements.txt

CMD [ "python3", "main.py" ]
