from datetime import datetime, date, timedelta

from Bot.Setting import moscow_tz
from RZD import API
from bs4 import BeautifulSoup
from Bot.API import *


# TODO: объединить проверки по группам(время, дата, город) и в каждом методе (условно main_date_validate)
#  возвращать сообщение, которое мы напечатаем юзеру. Улучшим читабельность кода бота


def json_serial(obj):
    """JSON serializer for objects not serializable by default json code"""
    """ Вспомогательная функция форматирующая дату в JSON """
    if isinstance(obj, (datetime, date)):
        return obj.isoformat()
    raise TypeError("Type %s not serializable" % type(obj))


# TODO: эту проверку можно упростить и уточнить одновременно.
# Отправить запрос к АПИ для получения списка поездок на этот день
# И если таких нет - значит и билет купить нельзя - значит и валидацию не проходит
def date_limits_validate(input_date_text, station_from, station_to, station_from_name, station_to_name, date_from):
    try:
        # Если на указанную дату уже нет билетов (н-р сегодня в 23:59)
        available_time = get_available_times(station_from, station_to, station_from_name, station_to_name, date_from)
        if len(available_time) == 0:
            return None

        input_date = datetime.strptime(input_date_text, '%d.%m.%Y')
        # купить билет на вчера, очевидно, нельзя
        if input_date.date() < datetime.now().date():
            return None
        # 90 суток от текущей даты - окончание срока продажи билетов на поезда по России
        # 120 суток от текущей даты - на позда "Красная стрела", "Экспресс"
        # и на летний период для поездов "Сапсан", "Невский экспресс", "Премиум", "Океан"
        if input_date.date() >= datetime.now().date() + timedelta(120):
            return None
        return input_date  # возвращаю это только чтобы не было None. Нигде не используется
    except Exception as e:
        print(e)
        raise e


def date_format_validate(input_date_text):
    try:
        if len(input_date_text.split('.')) == 3:
            # преобразуем строку в объект datetime
            input_date_json = json_serial(datetime.strptime(input_date_text, '%d.%m.%Y'))
            return {'date_text': input_date_text, 'date': input_date_json}
        # TODO: Пока отключаю успешную валидацию даты без указания года
        # TODO: Обязательно в валидацию добавить соответствие временному интервалу: сегодня + год (н-р)
        # elif len(input_date.split('.')) == 2:
        # Todo: Баг. Если в конце года создавать уведомление на январь без указания года непосредственно - дата будет н.р. 07.01.2023. А Должно быть 2024
        #     this_year = datetime.today().year
        #     input_date += '.' + this_year.__str__()  # добавляем год к дате
        #     datetime.strptime(input_date, '%d.%m.%Y')
        #     return input_date
        else:
            return None
    # Не актуально?
    except ValueError:
        return None
    except Exception as e:
        print(e)
        raise e


def time_format_validate(input_time) -> bool:
    """ Проверка времени, введенного пользователем на валидность """
    try:
        datetime.strptime(input_time, '%H:%M')
        return True
    except ValueError:
        return False
    except Exception as e:
        print(e)
        raise e


def time_check_validate(input_time, station_from, station_to, station_from_name, station_to_name, date_from):
    """
    Проверяет время введенное пользователем на наличие его в списке рейсов на запрошенный день.
    Возвращает список доступных, если время некорректно
    """

    try:
        available_time = get_available_times(station_from, station_to, station_from_name, station_to_name, date_from)
        if len(available_time) == 0:
            raise Exception("Сервис: get_available_times вернул пустой ответ")
        for time in available_time:
            if time == input_time:
                return True
        return available_time

    except Exception as e:
        print(e)
        raise e


def get_available_times(station_from, station_to, station_from_name, station_to_name, date_from) -> list:
    """ Получает список доступного времени для бронирования в выбранный день """

    available_time = []
    today_time = datetime.now(moscow_tz).time().strftime('%H:%M')

    try:
        response_text = API.get_schedule(station_from, station_to, station_from_name, station_to_name, date_from)
        soup = BeautifulSoup(response_text, 'html.parser')
        table = soup.find('table', class_='basicSched_trainsInfo_table')
        tr = table.find_all('tr')
        for row in tr:
            td = row.find_all('td')
            if td:
                time_railway_str = td[1].text
                if date_from == datetime.now(moscow_tz).date().strftime('%d.%m.%Y'):
                    # Если время из расписания больше чем сейчас - добавляем его в список доступных для выбора
                    if time_railway_str > today_time:
                        available_time.append(time_railway_str)
                else:
                    available_time.append(time_railway_str)
        return available_time
    except Exception as e:
        print(e)
        raise e


def language_input_validation(input_station) -> bool:
    """ Проверка ввода на русские символы и цифры """
    try:
        alphabet = set('абвгдеёжзийклмнопрстуфхцчшщъыьэюя')
        return not alphabet.isdisjoint(input_station.lower())

    except Exception as e:
        print(e)
        raise e


async def station_validate(input_station):
    try:
        input_station = input_station.upper()

        # Если в базе есть станция с таким именем
        expressCode = await get_express_code_station_by_name(input_station)
        if not expressCode is None:
            return expressCode
        # Иначе
        result_json = API.get_stations(input_station)
        if result_json is None:
            return None

        # Записываем информацию о станциях в базу
        for city in result_json:
            await add_station_info(city)

        for city in result_json:
            if city['n'] == input_station:
                return city['c']
        return None
    except Exception as e:
        print(e)
        raise e
