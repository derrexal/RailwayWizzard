from telebot.types import InlineKeyboardButton
from telegram import InlineKeyboardMarkup

(CALLBACK_NOTIFICATION, CALLBACK_ACTIVE_TASK, CALLBACK_DATA_CORRECT_NOTIFICATION,
 CALLBACK_DATA_INCORRECT_NOTIFICATION, CALLBACK_RESERVATION, CALLBACK_SUPPORT) = range(6)

message_error = 'Системная ошибка.\nМы уже знаем о проблеме и скоро её решим\nПопробуйте еще раз позже'
message_success = 'Уведомление о поездке успешно создано. \nЗадача №'
message_failure = 'Создание уведомления о поездке прекращено'

start_inline_keyboards = InlineKeyboardMarkup(
        inline_keyboard=[
            [InlineKeyboardButton(
                    text='\U00002709 Уведомление об появлении мест',
                    callback_data=str(CALLBACK_NOTIFICATION))],
            [InlineKeyboardButton(
                text='Список активных задач',
                callback_data=str(CALLBACK_ACTIVE_TASK))],
        ])

notification_confirm_inline_buttons = InlineKeyboardMarkup([[
            InlineKeyboardButton(text='Данные верны',
                                 callback_data=str(CALLBACK_DATA_CORRECT_NOTIFICATION)),
            InlineKeyboardButton(text='Отменить создание уведомлений',
                                 callback_data=str(CALLBACK_DATA_INCORRECT_NOTIFICATION)),
    ]])

