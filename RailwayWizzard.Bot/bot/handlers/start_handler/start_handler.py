from telegram.constants import ParseMode
from telegram.ext import ConversationHandler

from bot.queries.robot_queries import create_user
from bot.handlers.error_handler.base_error_handler import base_error_handler
from bot.setting import MESSAGE_START, START_INLINE_KEYBOARDS


async def start_buttons_handler(update, context):
    """ Создаёт начальные inline-кнопки """
    user_id = str(update.message.from_user.id)
    username = str(update.message.from_user.username)

    try:
        await update.message.reply_text(
            text=f"{MESSAGE_START}, <code>{user_id}</code>\nПо всем вопросам: <code>{admin_link}</code>",
            reply_markup=START_INLINE_KEYBOARDS,
            parse_mode=ParseMode.HTML)

        await create_user(user_id, username)

        return None

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
            text=f"{MESSAGE_START}, <code>{user_id}</code>\nПо всем вопросам: <code>{admin_link}</code>",
            reply_markup=START_INLINE_KEYBOARDS,
            parse_mode=ParseMode.HTML)

        await create_user(user_id, username)

        return ConversationHandler.END

    except Exception as e:
        return await base_error_handler(update, e, 1)
