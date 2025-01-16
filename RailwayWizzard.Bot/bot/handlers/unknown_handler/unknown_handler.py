from telegram import Update
from telegram.ext import CallbackContext
from bot.bot_message_log import log_user_message


async def unknown_handler(update: Update, context: CallbackContext):
    log_user_message(update)
    await update.message.reply_text('Неизвестная команда')
