# RailwayWizzard
Если вы когда-нибудь пытались найти место на рейс РЖД средствами push уведомлений от одноименного приложения, то вы однозначно знаете что такое боль. 
Данное приложение поможет пережить этот тяжелый этап вашей жизни.

## Использование
Вы можете ознакомиться с успешно функционирующим экземляром сервиса по [ссылке](https://t.me/RzdWizzardBot).

## Установка
Для начала нужно клонировать репозиторий
```sh
$ git clone https://github.com/derrexal/RailwayWizzard.git
```

Установить *docker* и *docker-compose*

Изменить токен бота в файле 
```sh
RailwayWizzard.Bot/Bot/.env
```

В директории с проектом ввести следующую команду:
   ```sh
   docker compose build &&
   docker compose up -d
   ```

## Благодарность
Данный проект был вдохновлен [rzd-api](https://github.com/visavi/rzd-api).

Спасибо автору [visavi](https://github.com/visavi).

## License
The class is open-sourced software licensed under the [MIT license](https://opensource.org/license/MIT).
