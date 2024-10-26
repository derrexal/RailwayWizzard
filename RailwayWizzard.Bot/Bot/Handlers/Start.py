from Bot import API
from Bot.Base import base_error_handler
from Bot.Setting import MESSAGE_START, START_INLINE_KEYBOARDS


async def start_buttons_handler(update, context):
    """ Создаёт начальные inline-кнопки """
    user_id = user = update.message.from_user.id
    try:
        await update.message.reply_text(MESSAGE_START + user_id, reply_markup=START_INLINE_KEYBOARDS)
        await API.create_user(update.message.from_user['id'], update.message.from_user['username'])

    except Exception as e:
        return await base_error_handler(update, e, 1)


async def start_buttons(update, context):
    """ Создаёт начальные inline-кнопки """
    chat_id = update.callback_query.message.chat.id
    try:
        await context.bot.send_message(chat_id=chat_id,
                                       text='\U0001F441 Добро пожаловать на борт',
                                       reply_markup=START_INLINE_KEYBOARDS)
        await API.create_user(update.callback_query.message.chat.id, update.callback_query.message.chat.username)

    except Exception as e:
        return await base_error_handler(update, e, 1)
