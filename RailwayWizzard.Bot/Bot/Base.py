from telegram import Update
from telegram.ext import ConversationHandler
from telegram.constants import ParseMode

from logger import get_unique_uuid_error, logger


async def base_error_handler(update: Update, e: Exception, next_step: int, text: str, is_custom_error=False) -> int:
    """ Базовый обработчик ошибок для step функций"""
    message_to_log = e
    message_to_user = text
    if is_custom_error:
        error_uuid = get_unique_uuid_error()
        message_to_log = f"ERROR [{error_uuid}] {e} "
        message_to_user = f"{text} <code>{error_uuid}</code>"

    logger.error(message_to_log)
    await update.callback_query.message.reply_text(text=message_to_user,
                                                   parse_mode=ParseMode.HTML)
    if next_step == 1:
        return ConversationHandler.END
    return next_step - 1


