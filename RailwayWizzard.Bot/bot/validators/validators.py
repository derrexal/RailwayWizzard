from calendar import monthrange
from datetime import datetime, date, timedelta

from bot.errors.DateFormatError import DateFormatError
from bot.setting import MESSAGE_FORMAT_ERROR
from bot.queries.robot_queries import get_available_times


def json_serial(obj):
    """JSON serializer for objects not serializable by default json code"""
    """ Вспомогательная функция форматирующая дату в JSON """
    if isinstance(obj, (datetime, date)):
        return obj.isoformat()
    raise TypeError("Type %s not serializable" % type(obj))


async def date_limits_validate(input_date_text):
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
    @return: Результат валидации
    """
    try:
        input_date = datetime.strptime(input_date_text, '%d.%m.%Y').date()
        today_date = datetime.now().date()

        if input_date < today_date:
            return False

        if input_date >= today_date + timedelta(120):
            return False

        return True

    except Exception as e:
        raise e


def date_format_validate(input_date_text):
    today = date.today()

    try:
        # Разделяем входную строку по точке
        parts = input_date_text.split('.')

        if len(parts) == 1:
            # Только день
            day = int(parts[0])
            month_days = monthrange(today.year, today.month)[1]
            if 1 <= day <= month_days:
                date_obj = date(today.year, today.month, day)
            else:
                raise DateFormatError.invalid_day_current_month(day, today.month)

        elif len(parts) == 2:
            # День и месяц
            day, month = map(int, parts)
            if 1 <= month <= 12:
                month_days = monthrange(today.year, month)[1]
                if 1 <= day <= month_days:
                    date_obj = date(today.year, month, day)
                else:
                    raise DateFormatError.invalid_day(day, month)
            else:
                raise DateFormatError.invalid_month(month)

        elif len(parts) == 3:
            # Полный формат
            day, month, year = map(int, parts)
            if 1 <= month <= 12:
                month_days = monthrange(year, month)[1]
                if 1 <= day <= month_days:
                    date_obj = datetime(year, month, day).date()
                else:
                    raise DateFormatError.invalid_day(day, month)
            else:
                raise DateFormatError.invalid_month(month)
        else:
            raise DateFormatError.invalid_format()

        if date_obj < today:
            raise DateFormatError.unactual_date()

        return {
            'date_text': date_obj.strftime('%d.%m.%Y'),
            'date': json_serial(date_obj)
        }

    except ValueError:
        raise DateFormatError.conversion_error()
    except DateFormatError as date_format_error:
        raise date_format_error
    except Exception as e:
        raise DateFormatError.unknown(str(e))


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
