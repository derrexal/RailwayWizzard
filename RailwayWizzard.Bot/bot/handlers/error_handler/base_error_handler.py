from telegram import Update
from telegram.ext import ConversationHandler
from telegram.constants import ParseMode

from logger import get_unique_uuid_error, logger


async def base_error_handler(update: Update, e: Exception, next_step: int, message_to_user: str = "") -> int:
    """ Базовый обработчик ошибок для step функций"""
    error_uuid = get_unique_uuid_error()
    message_to_log = f"ERROR [{error_uuid}] {e}"
    message_to_user = f"{message_to_user}\nОшибка номер <code>{error_uuid}</code>"

    logger.exception(message_to_log)

    if update.callback_query is not None:
        await update.callback_query.message.reply_text(text=message_to_user, parse_mode=ParseMode.HTML)
    else:
        await update.message.reply_text(text=message_to_user, parse_mode=ParseMode.HTML)
    if next_step == 1:
        return ConversationHandler.END

    return next_step - 1

