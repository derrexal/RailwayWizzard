FROM php

ENV COMPOSER_ALLOW_SUPERUSER=1

WORKDIR /app
COPY ./RailwayWizzard.RzdApi /app

RUN apt-get update && apt install -y curl
RUN apt install -y git
RUN apt-get install -y zip unzip libzip-dev && docker-php-ext-install zip
RUN curl -sS https://getcomposer.org/installer | php -- --install-dir=/usr/local/bin --filename=composer

RUN composer install
EXPOSE 8088

CMD [ "php", "-S", "0.0.0.0:8088"]