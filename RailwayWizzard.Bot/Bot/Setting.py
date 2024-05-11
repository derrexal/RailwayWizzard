from telebot.types import InlineKeyboardButton
from telegram import InlineKeyboardMarkup

(CALLBACK_NOTIFICATION, CALLBACK_ACTIVE_TASK, CALLBACK_DATA_CANCEL_NOTIFICATION, CALLBACK_DATA_CORRECT_NOTIFICATION,
 CALLBACK_DATA_INCORRECT_NOTIFICATION, CALLBACK_RESERVATION, CALLBACK_SUPPORT,
 CALLBACK_CAR_TYPE_SEDENTARY, CALLBACK_CAR_TYPE_RESERVED_SEAT, CALLBACK_CAR_TYPE_COMPARTMENT, CALLBACK_CAR_TYPE_LUXURY,
 CALLBACK_CAR_TYPE_SELECT_ALL, CALLBACK_CAR_TYPE_REMOVE_ALL, CALLBACK_CAR_TYPE_CONTINUE) = range(14)

message_error = 'Системная ошибка.\nМы уже знаем о проблеме и скоро её решим\nПопробуйте еще раз позже'
message_success = 'Уведомление о поездке успешно создано. \nЗадача №'
message_failure = 'Создание уведомления о поездке прекращено'
message_start = '\U00002388 Добро пожаловать на борт'

start_inline_keyboards = InlineKeyboardMarkup(
        inline_keyboard=[
            [InlineKeyboardButton(text='\U00002709 Уведомление об появлении мест',
                                  callback_data=str(CALLBACK_NOTIFICATION))],
            [InlineKeyboardButton(text='\U0001F5C2 Список активных задач', callback_data=str(CALLBACK_ACTIVE_TASK))],
])

notification_confirm_inline_buttons = InlineKeyboardMarkup([[
            InlineKeyboardButton(text='Назад', callback_data=str(CALLBACK_DATA_CANCEL_NOTIFICATION)),
            InlineKeyboardButton(text='Данные верны', callback_data=str(CALLBACK_DATA_CORRECT_NOTIFICATION)),
            InlineKeyboardButton(text='Отменить создание уведомлений',
                                 callback_data=str(CALLBACK_DATA_INCORRECT_NOTIFICATION)),
]])

car_type_inline_buttons_old = InlineKeyboardMarkup([
    [InlineKeyboardButton(text='\U000025FB Сидячий', callback_data=str(CALLBACK_CAR_TYPE_SEDENTARY))],
    [InlineKeyboardButton(text='\U000025FB Плацкарт', callback_data=str(CALLBACK_CAR_TYPE_RESERVED_SEAT))],
    [InlineKeyboardButton(text='\U000025FB Купе', callback_data=str(CALLBACK_CAR_TYPE_COMPARTMENT))],
    [InlineKeyboardButton(text='\U000025FB СВ', callback_data=str(CALLBACK_CAR_TYPE_LUXURY))],
    [InlineKeyboardButton(text='Продолжить', callback_data=str(CALLBACK_CAR_TYPE_CONTINUE))]
])

footer_menu_car_type_inline_buttons = [
    InlineKeyboardButton(text='Продолжить', callback_data=str(CALLBACK_CAR_TYPE_CONTINUE)),
]
