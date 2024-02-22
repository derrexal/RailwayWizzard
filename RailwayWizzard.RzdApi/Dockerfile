FROM composer:2.6.6 as vendor
WORKDIR /app

COPY ./examples /app
COPY ./src /app
COPY ./tests /app
COPY ./composer.json /app/composer.json
COPY ./composer.lock /app/composer.lock
COPY ./phpunit.xml /app/phpunit.xml
COPY ./Dockerfile /app/Dockerfile

FROM php:8.3.2

EXPOSE 8088