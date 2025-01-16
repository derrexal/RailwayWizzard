#TODO: Доработать тексты!
class DateFormatError(Exception):
    INVALID_DAY = "Введен некорректный день: {day} для месяца: {month}."
    INVALID_DAY_CURRENT_MONTH = "Введен некорректный день: {day} для текущего месяца: {month}."
    INVALID_MONTH = "Введен некорректный месяц: {month}."
    UNACTUAL_DATE = "Введена неактуальная дата."
    INVALID_FORMAT = "Некорректный формат даты. Ожидается 'ДД', 'ДД.ММ' или 'ДД.ММ.ГГГГ'."
    CONVERSION_ERROR = "Ошибка преобразования: введенные данные должны быть числовыми."
    UNKNOWN_ERROR = "Непредвиденная ошибка: {error}"

    @classmethod
    def invalid_day(cls, day, month):
        """Создает исключение для недопустимого дня в указанном месяце."""
        return cls(cls.INVALID_DAY.format(day=day, month=month))

    @classmethod
    def invalid_day_current_month(cls, day, month):
        """Создает исключение для недопустимого дня в текущем месяце."""
        return cls(cls.INVALID_DAY_CURRENT_MONTH.format(day=day, month=month))

    @classmethod
    def invalid_month(cls, month):
        """Создает исключение для недопустимого месяца."""
        return cls(cls.INVALID_MONTH.format(month=month))

    @classmethod
    def unactual_date(cls):
        """Создает исключение для неактуальной даты."""
        return cls(cls.UNACTUAL_DATE)

    @classmethod
    def invalid_format(cls):
        """Создает исключение для недопустимого формата даты."""
        return cls(cls.INVALID_FORMAT)

    @classmethod
    def conversion_error(cls):
        """Создает исключение для ошибки преобразования."""
        return cls(cls.CONVERSION_ERROR)

    @classmethod
    def unknown(cls, error):
        """Создает исключение для непредвиденной ситуации."""
        return cls(cls.UNKNOWN_ERROR.format(error=error))
