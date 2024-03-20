import json

from telegram.ext import *
from telegram.constants import *
from Bot.Setting import (message_success, message_failure, CALLBACK_ACTIVE_TASK, notification_confirm_inline_buttons)
from Bot.validators import *
from Bot.Other import *
from Bot.API import *


async def active_task_handler(update: Update, context: CallbackContext):
    """ Начало взаимодействия по клику на inline-кнопку 'Уведомление' """
    init = update.callback_query.data
    user_id = update.callback_query.message.chat.id
    try:
        if init != str(CALLBACK_ACTIVE_TASK):
            await update.callback_query.message.reply_text(text="Что-то пошло не так, обратитесь к администратору бота")
            return ConversationHandler.END

        active_tasks = await get_active_task_by_user_id(user_id)
        if active_tasks is None or not active_tasks:
            await update.callback_query.message.reply_text("У вас нет активных задач")
        else:
            for task in active_tasks:
                print(task["dateFrom"])
                await update.callback_query.message.reply_text(
                    text="Задача №" + "<strong>" + str(task["id"]) + "</strong>"
                         + "\nСтанция отправления: " + "<strong>" + task["arrivalStation"] + "</strong>"
                         + "\nСтанция прибытия: " + "<strong>" + task["departureStation"] + "</strong>"
                         + "\nДата отправления: " + "<strong>" + task["dateFromString"] + "</strong>"
                         + "\nВремя отправления: " + "<strong>" + task["timeFrom"] + "</strong>",
                    parse_mode=ParseMode.HTML)

        await start_buttons(update, context)
        return ConversationHandler.END

    except Exception as e:
        print(e)
        raise
