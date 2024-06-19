from telegram import Update

from Bot.Handlers.Start import *
from telegram.ext import CallbackContext, ConversationHandler
from telegram.constants import ChatAction


def log_user_message(update):
    answer = update.message.text
    id_tg = update.message.from_user['id']
    username = update.message.from_user['username']
    logger.info('User: {} ID: {} Message:{}'.format(username, id_tg, answer))


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
    log_user_message(update)


async def base_step_notification(update: Update, context: CallbackContext):
    """ Базовый step-класс, собирающий в себя общую функциональность"""
    try:
        log_user_message(update)
        await update.message.reply_chat_action(ChatAction.TYPING)

        if await check_stop(update, context):
            return ConversationHandler.END

    except Exception as e:
        raise e

