from telegram import *
from Bot.Setting import CALLBACK_SUPPORT, message_error
from Bot.API import *


async def help_handler(update, context):
    """ Подсказка для пользователя """

    inline_buttons = InlineKeyboardMarkup(
        inline_keyboard=[[InlineKeyboardButton(
                    text='\U0001F58C Написать в поддержку',
                    callback_data=str(CALLBACK_SUPPORT),
                    url='telegram.me/derrexal')]])

    try:
        await update.message.reply_text(
            '\U0001F4D8 Создано при поддержке прекрасной лапшичной BỔ и английского портера.\n',
            reply_markup=inline_buttons)

        await add_user(update.message.from_user['id'], update.message.from_user['username'])

    except Exception as e:
        print(e)
        await update.message.reply_text(message_error)
        raise

