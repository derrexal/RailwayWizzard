from datetime import datetime, date, timedelta

from bot.setting import MESSAGE_FORMAT_ERROR
from bot.queries.robot_queries import get_available_times


def json_serial(obj):
    """JSON serializer for objects not serializable by default json code"""
    """ Вспомогательная функция форматирующая дату в JSON """
    if isinstance(obj, (datetime, date)):
        return obj.isoformat()
    raise TypeError("Type %s not serializable" % type(obj))


async def date_limits_validate(input_date_text, available_times):
    """
    Валидация даты, введенной пользователем.
    Проверяется на допустимые границы:
    - введенная дата уже в прошлом
    - 90 суток от текущей даты - окончание срока продажи билетов на поезда по России
    - 120 суток от текущей даты - на поезда "Красная стрела", "Экспресс" и на летний период
        для поездов "Сапсан", "Невский экспресс", "Премиум", "Океан"
    Сравнивается со списком поездок на этот день.
    Если таких нет - значит и билет купить нельзя - значит и валидацию не проходит
    @param input_date_text: Дата введенная пользователем
    @param available_times: Список доступных рейсов на запрошенный день
    @return: Результат валидации
    """
    try:
        input_date = datetime.strptime(input_date_text, '%d.%m.%Y').date()
        today_date = datetime.now().date()

        if input_date < today_date:
            return None

        if input_date >= today_date + timedelta(120):
            return None

        if len(available_times) == 0:
            return None

        return True

    except Exception as e:
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
        raise e


def time_format_validate(input_time) -> bool:
    """
    Валидация времени, введенного пользователем на соответствие формату
    @param input_time: Время отправления
    @return: Результат валидации
    """
    try:
        datetime.strptime(input_time, '%H:%M')
        return True
    except ValueError:
        return False
    except Exception as e:
        raise e


async def get_times(station_from_name, station_to_name, date_from):
    """
    Получает список доступного для бронирования времени. Если таковых нет - возвращает exception
    @param station_from_name: Станция отправления
    @param station_to_name: Станция прибытия
    @param date_from: Дата отправления
    @return: Список доступных рейсов на запрошенный день
    """
    try:
        available_times = await get_available_times(station_from_name, station_to_name, date_from)
        if len(available_times) == 0:
            raise Exception("Сервис: get_available_times вернул пустой ответ")
        return available_times
    except Exception as e:
        raise e


async def time_check_validate(input_time, available_times) -> bool:
    """ Валидация времени введенное пользователем на наличие его в списке рейсов на запрошенный день.
    @rtype: Boolean
    @param input_time: Время отправления
    @param available_times: Список доступных рейсов на запрошенный день
    @return: Результат валидации
    """
    for time in available_times:
        if time == input_time:
            return True
    return False


def language_input_validation(input_station):
    """
    Проверка ввода на допустимые символы. Если проверка не пройдена - бросается Exception
    @param input_station: Название станции(UPPER)
    """
    alphabet = set('()[].,- '
                   '0123456789'
                   'АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ')
    input_station_set = set(input_station)
    if not input_station_set.issubset(alphabet):
        raise ValueError(MESSAGE_FORMAT_ERROR)
