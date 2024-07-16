from datetime import datetime, date, timedelta

from Bot.Setting import MESSAGE_FORMAT_ERROR
from Bot import API


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
async def date_limits_validate(input_date_text, station_from_name, station_to_name, date_from):
    try:
        input_date = datetime.strptime(input_date_text, '%d.%m.%Y').date()
        today_date = datetime.now().date()
        # купить билет на вчера, очевидно, нельзя
        if input_date < today_date:
            return None
        # 90 суток от текущей даты - окончание срока продажи билетов на поезда по России
        # 120 суток от текущей даты - на поезда "Красная стрела", "Экспресс"
        # и на летний период для поездов "Сапсан", "Невский экспресс", "Премиум", "Океан"
        if input_date >= today_date + timedelta(120):
            return None

        # Если на указанную дату уже нет билетов (н-р сегодня в 23:59)
        available_time = await API.get_available_times(station_from_name, station_to_name, date_from)
        if len(available_time) == 0:
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
    """ Проверка времени, введенного пользователем на валидность """

    try:
        datetime.strptime(input_time, '%H:%M')
        return True
    except ValueError:
        return False
    except Exception as e:
        raise e


async def time_check_validate(station_from_name, station_to_name, date_from, input_time='00:00'):
    """
    Проверяет время введенное пользователем на наличие его в списке рейсов на запрошенный день.
    Возвращает список доступных, если время некорректно
    """
    try:
        available_time = await API.get_available_times(station_from_name, station_to_name, date_from)
        if len(available_time) == 0:
            raise Exception("Сервис: get_available_times вернул пустой ответ")

        if input_time == '00:00':
            return available_time

        for time in available_time:
            if time == input_time:
                return True

        return available_time

    except Exception as e:
        raise e


def language_input_validation(input_station):
    """
    Проверка ввода на допустимые символы
    @param input_station: имя станции(UPPER)
    """
    alphabet = set('[].,- '
                   '0123456789'
                   'АБВГДЕЁЖЗИЙКЛМНОПРСТУФЧЦЧШЩЪЫЬЭЮЯ')
    input_station_set = set(input_station)
    if not input_station_set.issubset(alphabet):
        raise ValueError(MESSAGE_FORMAT_ERROR)
