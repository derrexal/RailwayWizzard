from telegram import *
from telegram.ext import *
from telegram.constants import *
from Bot.Setting import (message_success, message_failure, CALLBACK_ACTIVE_TASK, notification_confirm_inline_buttons)
from Bot.Other import *
from Bot.API import *


async def active_task_handler(update: Update, context: CallbackContext):
    """ Начало взаимодействия по клику на inline-кнопку 'Список активных задач' """
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
                callback = "active_task_callback" + str(task["id"])
                # Init keyboard
                keyboard = InlineKeyboardMarkup(inline_keyboard=[[
                    InlineKeyboardButton(text='Остановить поиск по задаче', callback_data=callback)]])

                # Send message for user include task info
                await update.callback_query.message.reply_text(
                    text="Задача №" + "<strong>" + str(task["id"]) + "</strong>"
                         + "\nСтанция отправления: " + "<strong>" + task["arrivalStation"] + "</strong>"
                         + "\nСтанция прибытия: " + "<strong>" + task["departureStation"] + "</strong>"
                         + "\nДата отправления: " + "<strong>" + task["dateFromString"] + "</strong>"
                         + "\nВремя отправления: " + "<strong>" + task["timeFrom"] + "</strong>",
                    reply_markup=keyboard,
                    parse_mode=ParseMode.HTML)

        return 1

    except Exception as e:
        print(e)
        await update.callback_query.message.reply_text(text=message_error)
        return ConversationHandler.END


async def one_step_active_task(update: Update, context: CallbackContext):
    query = update.callback_query.data
    text_message_html = update.callback_query.message.text_html
    task_number = int(''.join(filter(str.isdigit, query)))
    try:
        if "active_task_callback" not in query:  # Если нажали куда-то не туда - выходим из диалога
            return ConversationHandler.END
        if task_number is None:
            print('ERROR: task_number is none')
            await update.callback_query.message.reply_text(text=message_error)  # TODO: Добавить к каждой ошибке ее номер?
            return ConversationHandler.END

        response = await delete_task_by_id(task_number)
        if response is None:
            print('ERROR stopped task number: ' + str(task_number))
            await update.callback_query.message.reply_text(text=message_error)
            return ConversationHandler.END
        await update.callback_query.edit_message_text(text_message_html + "\n Остановлена", parse_mode=ParseMode.HTML)
        await update.callback_query.message.reply_text("Задача №" +
                                                       "<strong>" + str(task_number) + "</strong>"
                                                       + " успешно остановлена.", parse_mode=ParseMode.HTML)
        return 1

    except Exception as e:
        print(e)
        raise e
