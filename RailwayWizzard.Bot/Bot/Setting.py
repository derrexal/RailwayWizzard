from enum import Enum

from telebot.types import InlineKeyboardButton
from telegram import InlineKeyboardMarkup
import pytz


class CarType(Enum):
    SEDENTARY = ('Сидячий', True)
    RESERVED_SEAT = ('Плацкарт', True)
    COMPARTMENT = ('Купе', True)
    LUXURY = ('СВ', False)


NON_SELECT_SMILE = '\U000025FB'
SELECT_SMILE = '\U00002705'

CALLBACK_NOTIFICATION = 'callback_notification'
CALLBACK_CAR_TYPE_CONTINUE = 'callback_car_type_continue'
CALLBACK_DATA_CANCEL = 'callback_data_cancel'
CALLBACK_DATA_CORRECT_NOTIFICATION = 'callback_data_correct_notification'
CALLBACK_DATA_INCORRECT_NOTIFICATION = 'callback_data_incorrect_notification'

(CALLBACK_ACTIVE_TASK,
 CALLBACK_RESERVATION, CALLBACK_SUPPORT, CALLBACK_DONATE,
 CALLBACK_CAR_TYPE_SEDENTARY, CALLBACK_CAR_TYPE_RESERVED_SEAT, CALLBACK_CAR_TYPE_COMPARTMENT, CALLBACK_CAR_TYPE_LUXURY,
 CALLBACK_CAR_TYPE_SELECT_ALL, CALLBACK_CAR_TYPE_REMOVE_ALL) = range(10)

MOSCOW_TZ = pytz.timezone('Europe/Moscow')
MAX_NUMBER_SEATS = 10  # ни к чему не привязанное значение, в случае чего увеличить
ADMIN_USERNAME = '@derrexal'
DONATE_URL = 'pay.cloudtips.ru/p/c6b746f7'

MESSAGE_FORMAT_ERROR = "Недопустимый ввод."
MESSAGE_SUCCESS = 'Уведомление о поездке успешно создано. \nЗадача №'
MESSAGE_CANCEL = 'Уведомление о поездке отменено.'
MESSAGE_START = '\U00002388 Добро пожаловать на борт, '
MESSAGE_MIN_COUNT_SEATS = ("Укажите минимальное количество мест в поезде, "
                           "на которое вам необходимо создать уведомление (От 1 до 10).\n"
                           "Если не знаете что указать, напишите цифру 1")


START_INLINE_KEYBOARDS = InlineKeyboardMarkup(
    inline_keyboard=[
        [InlineKeyboardButton(text='\U00002709 Уведомление об появлении мест',
                              callback_data=str(CALLBACK_NOTIFICATION))],
        [InlineKeyboardButton(text='\U0001F5C2 Список активных задач', callback_data=str(CALLBACK_ACTIVE_TASK))]
    ])

NOTIFICATION_CONFIRM_INLINE_BUTTONS = InlineKeyboardMarkup([[
    InlineKeyboardButton(text='Данные верны', callback_data=str(CALLBACK_DATA_CORRECT_NOTIFICATION)),
    InlineKeyboardButton(text='Отменить создание уведомлений', callback_data=str(CALLBACK_DATA_INCORRECT_NOTIFICATION))
]])

FOOTER_MENU_CAR_TYPE_INLINE_BUTTONS = [
    InlineKeyboardButton(text='Продолжить', callback_data=str(CALLBACK_CAR_TYPE_CONTINUE)),
]

