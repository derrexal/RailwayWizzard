from telegram import InlineKeyboardMarkup
from telebot.types import InlineKeyboardButton

from bot.handlers.error_handler.base_error_handler import base_error_handler
from bot.setting import CALLBACK_SUPPORT
from bot.queries.robot_queries import create_user


async def help_handler(update, context):
    """ Подсказка для пользователя """

    inline_buttons = InlineKeyboardMarkup(
        inline_keyboard=[[InlineKeyboardButton(
                    text='\U0001F58C Написать в поддержку',
                    callback_data=str(CALLBACK_SUPPORT),
                    url='telegram.me/derrexal')]])

    try:
        await create_user(
            update.message.from_user['id'],
            update.message.from_user['username'])

        await update.message.reply_text(
            '\U0001F4D8 Создано при поддержке прекрасной лапшичной BỔ и английского портера.\n',
            reply_markup=inline_buttons)

    except Exception as e:
        return await base_error_handler(update, e, 1)

