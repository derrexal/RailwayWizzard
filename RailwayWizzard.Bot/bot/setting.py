from telebot.types import InlineKeyboardButton
from telegram import InlineKeyboardMarkup
import pytz


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
MESSAGE_SUCCESS = 'Уведомление о поездке успешно создано.\n\nЗадача №'
MESSAGE_CANCEL = 'Создание уведомления о поездке отменено.'
MESSAGE_START = '\U00002388 Добро пожаловать на борт, '

MESSAGE_MIN_COUNT_SEATS = ("Укажите <strong>количество необходимых мест</strong>.\n"
                           "Например, <code>1</code>\n\n"
                           "Пояснение: \n"
                           "Укажите <strong>1</strong> если хотите узнать о <strong>каждом</strong> появившемся месте.\n"
                           "Укажите <strong>2</strong> если хотите узнать только если появится <strong>сразу 2</strong> свободных места\n"
                           "Далее - по аналогии.")

START_INLINE_KEYBOARDS = InlineKeyboardMarkup(
    inline_keyboard=[
        [InlineKeyboardButton(text='\U00002709 Уведомление об появлении мест',
                              callback_data=str(CALLBACK_NOTIFICATION))],
        [InlineKeyboardButton(text='\U0001F5C2 Список активных уведомлений', callback_data=str(CALLBACK_ACTIVE_TASK))]
    ])

NOTIFICATION_CONFIRM_INLINE_BUTTONS = InlineKeyboardMarkup([[
    InlineKeyboardButton(text='Данные верны', callback_data=str(CALLBACK_DATA_CORRECT_NOTIFICATION)),
    InlineKeyboardButton(text='Отменить создание уведомления', callback_data=str(CALLBACK_DATA_INCORRECT_NOTIFICATION))
]])

FOOTER_MENU_CAR_TYPE_INLINE_BUTTONS = [
    InlineKeyboardButton(text='Выбрать все', callback_data=str(CALLBACK_CAR_TYPE_SELECT_ALL)),
    InlineKeyboardButton(text='Снять выбор', callback_data=str(CALLBACK_CAR_TYPE_REMOVE_ALL)),
    InlineKeyboardButton(text='Продолжить', callback_data=str(CALLBACK_CAR_TYPE_CONTINUE))]
