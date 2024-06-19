from Bot.API import *
from Bot.Base import base_error_handler
from Bot.Setting import message_error, message_start, start_inline_keyboards


async def start_buttons_handler(update, context):
    """ Создаёт начальные inline-кнопки """
    try:
        await update.message.reply_text(message_start,
                                        reply_markup=start_inline_keyboards)

        await create_user(update.message.from_user['id'],
                          update.message.from_user['username'])

    except Exception as e:
        return await base_error_handler(update, e, 1, message_error)


async def start_buttons(update, context):
    """ Создаёт начальные inline-кнопки """
    chat_id = update.callback_query.message.chat.id
    try:
        await context.bot.send_message(chat_id=chat_id,
                                       text='\U0001F441 Добро пожаловать на борт',
                                       reply_markup=start_inline_keyboards)
        await create_user(update.callback_query.message.chat.id,
                          update.callback_query.message.chat.username)

    except Exception as e:
        return await base_error_handler(update, e, 1, message_error)

