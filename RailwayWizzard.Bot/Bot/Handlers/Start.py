from telegram.constants import ParseMode

from Bot import API
from Bot.Base import base_error_handler
from Bot.Setting import MESSAGE_START, START_INLINE_KEYBOARDS

async def start_buttons_handler(update, context):
    """ Создаёт начальные inline-кнопки """
    user_id = str(update.message.from_user.id)
    username = str(update.message.from_user.username)

    try:
        await update.message.reply_text(
            MESSAGE_START + '<code>' + user_id + '</code>',
            reply_markup=START_INLINE_KEYBOARDS,
            parse_mode=ParseMode.HTML)

        await API.create_user(user_id, username)

    except Exception as e:
        return await base_error_handler(update, e, 1)


async def start_buttons(update, context):
    """ Создаёт начальные inline-кнопки """
    chat_id = update.callback_query.message.chat.id
    user_id = str(update.callback_query.message.chat.id)
    username = str(update.callback_query.message.chat.username)

    try:
        await context.bot.send_message(
            chat_id=chat_id,
            text=MESSAGE_START + '<code>' + user_id + '</code>',
            reply_markup=START_INLINE_KEYBOARDS,
            parse_mode=ParseMode.HTML)

        await API.create_user(user_id, username)

    except Exception as e:
        return await base_error_handler(update, e, 1)
