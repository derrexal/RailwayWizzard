FROM php:8.3.2

ENV COMPOSER_ALLOW_SUPERUSER=1

WORKDIR /app
COPY ./RailwayWizzard.RzdApi /app

RUN apt-get update && apt install -y curl
RUN curl -sS https://getcomposer.org/installer | sudo php -- --install-dir=/usr/local/bin --filename=composer

RUN composer install \
    --ignore-platform-reqs \
    --no-interaction \
    --no-plugins \
    --no-scripts \
    --prefer-dist

EXPOSE 8088

CMD [ "php", "-S", "0.0.0.0:8088"]