from telegram import Update
from telegram.constants import ParseMode, ChatAction
from telegram.ext import CallbackContext, ConversationHandler

from Bot.Handlers.Start import *
from logger import get_unique_uuid_error, logger


def log_user_message(update, context):
    answer = update.message.text
    idTg = update.message.from_user['id']
    username = update.message.from_user['username']
    print('User: {} ID: {} Message:{}'.format(username, idTg, answer))


async def check_stop(update, context):
    answer = update.message.text

    if answer == '/stop':
        await stop(update, context)
        return True
    return False


async def stop(update, context):
    await update.message.reply_text("Диалог прерван\n")
    await start_buttons_handler(update, context)


async def unknown_handler(update, context):
    await update.message.reply_text('Неизвестная команда')
    log_user_message(update, context)


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


async def base_step_notification(update: Update, context: CallbackContext):
    """ Базовый step-класс, собирающий в себя общую функциональность"""
    try:
        log_user_message(update, context)
        await update.message.reply_chat_action(ChatAction.TYPING)

        if await check_stop(update, context):
            return ConversationHandler.END

    except Exception as e:
        raise e
