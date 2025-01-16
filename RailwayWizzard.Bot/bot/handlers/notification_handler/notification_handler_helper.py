from datetime import timedelta, datetime

from bot.setting import MOSCOW_TZ


class NotificationHandlerDialog:

    ENTER_DEPARTURE_DATE_TEXT = (
        "Укажите <strong>дату отправления</strong> в одном из следующих форматов:\n"
        "дд      <code>{day}</code>\n"
        "дд.мм      <code>{day_month}</code>\n"
        "дд.мм.гггг      <code>{day_month_year}</code>\n")

    @classmethod
    def enter_departure_date_text(cls):
        """Текст для ввода даты пользователем."""
        tomorrow_date = datetime.now(MOSCOW_TZ) + timedelta(days=1)

        return cls.ENTER_DEPARTURE_DATE_TEXT.format(
            day=tomorrow_date.strftime('%d'),
            day_month=tomorrow_date.strftime('%d.%m'),
            day_month_year=tomorrow_date.strftime('%d.%m.%Y'))
