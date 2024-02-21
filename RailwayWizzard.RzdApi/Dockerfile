FROM composer:2.6.6 as vendor
WORKDIR /app

COPY . /app

RUN composer install \
    --ignore-platform-reqs \
    --no-interaction \
    --no-plugins \
    --no-scripts \
    --prefer-dist

FROM php:8.3.2

EXPOSE 8088

CMD [ "php", "-S", "0.0.0.0:8088"]