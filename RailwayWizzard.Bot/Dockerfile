FROM python:3

ENV PYTHONUNBUFFERED True

WORKDIR /app

COPY . .

RUN apt-get update && apt-get install -y libpq-dev build-essential

RUN pip install --upgrade pip

RUN pip install --no-cache-dir -r requirements.txt

CMD [ "python", "-u", "main.py" ]
