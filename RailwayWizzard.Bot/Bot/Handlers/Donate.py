from telegram import *
from Bot.Base import base_error_handler
from Bot.Setting import CALLBACK_DONATE, DONATE_URL
from Bot import API


async def donate_handler(update, context):
    
    inline_buttons = InlineKeyboardMarkup(
        inline_keyboard=[[InlineKeyboardButton(
                    text='\U0001F4B3 Поддержать проект (СБП)',
                    callback_data=str(CALLBACK_DONATE),
                    url=DONATE_URL)]])

    try:
        await API.create_user(update.message.from_user['id'], update.message.from_user['username'])

        await update.message.reply_text(
            '\U0001F4B0 Если вы хотите внести свой вклад в развитие проекта нажмите на кнопку ниже',
            reply_markup=inline_buttons)

    except Exception as e:
        return await base_error_handler(update, e, 1)

