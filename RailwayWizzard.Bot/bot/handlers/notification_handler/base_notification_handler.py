from telegram import Update

from bot.handlers.start_handler.start_handler import *
from telegram.ext import CallbackContext, ConversationHandler
from telegram.constants import ChatAction

from bot.log.log_user_message import log_user_message


async def null_step_notification(update: Update, context: CallbackContext):
    """ Базовый step-класс, собирающий в себя общую функциональность"""
    try:
        log_user_message(update)

        await update.message.reply_chat_action(ChatAction.TYPING)

        if await check_stop(update, context):
            return ConversationHandler.END

        return

    except Exception as e:
        raise e


async def check_stop(update, context):
    answer = update.message.text

    if answer == '/stop':
        await update.message.reply_text("Диалог прерван")
        await start_buttons_handler(update, context)
        return True

    return False
