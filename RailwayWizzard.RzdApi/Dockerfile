FROM composer:2.6

WORKDIR /app

COPY ./examples /app
COPY ./src /app
COPY ./tests /app
COPY ./composer.json /app/composer.json
COPY ./composer.lock /app/composer.lock
COPY ./phpunit.xml /app/phpunit.xml
COPY ./Dockerfile /app/Dockerfile

RUN composer install \
    --ignore-platform-reqs \
    --no-interaction \
    --no-plugins \
    --no-scripts \
    --prefer-dist

RUN composer update --no-scripts

FROM php:8.3.2

EXPOSE 8088

CMD [ "php", "-S", "0.0.0.0:8088"]