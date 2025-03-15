from telegram import InlineKeyboardMarkup
from telebot.types import InlineKeyboardButton

from bot.handlers.notification_handler.base_notification_handler import *
from bot.handlers.error_handler.base_error_handler import *
from bot.setting import (CALLBACK_ACTIVE_TASK, ADMIN_USERNAME)
from bot.queries.robot_queries import delete_task_by_id, get_active_task_by_user_id


async def active_task_handler(update: Update, context: CallbackContext):
    """ Начало взаимодействия по клику на inline-кнопку 'Список активных задач' """
    next_step = 1

    try:
        if update.callback_query.data != str(CALLBACK_ACTIVE_TASK):
            await update.callback_query.message.reply_text(
                text=f"Что-то пошло не так, обратитесь к администратору бота {ADMIN_USERNAME}")
            return ConversationHandler.END

        user_id = update.callback_query.message.chat.id
        active_tasks = await get_active_task_by_user_id(user_id)

        if not active_tasks:
            await update.callback_query.message.reply_text("У вас нет активных задач")
            return ConversationHandler.END

        for task in active_tasks:
            await send_task_info(update, task)

        return next_step

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def send_task_info(update: Update, task: dict):
    """Send task information to the user."""
    try:
        task_id = task.get("id")
        callback = f"active_task_callback{task_id}"
        keyboard = InlineKeyboardMarkup([[InlineKeyboardButton(text='Остановить поиск по задаче', callback_data=callback)]])

        task_info = f"Задача № <strong>{task_id}</strong>\n"

        if task['trainNumber'] is not None:
            task_info = task_info + f"Номер поезда: <strong> {task['trainNumber']} </strong>\n"

        task_info = task_info + (
            f"Станция отправления: <strong> {task['departureStation']} </strong>\n"
            f"Станция прибытия: <strong> {task['arrivalStation']} </strong>\n"
            f"Дата отправления: <strong> {task['dateFromString']} </strong>\n"
            f"Класс обслуживания: <strong>{task['carTypes']}</strong>\n"
            f"Количество необходимых мест: <strong> {str(task['numberSeats'])} </strong>\n")

        if str(task['updated']) != '':
            task_info = task_info + f"Время последней проверки: <strong> {str(task['updated'])} </strong>\n"

        await update.callback_query.message.reply_text(
            text=task_info,
            reply_markup=keyboard,
            parse_mode=ParseMode.HTML)

    except Exception as e:
        return await base_error_handler(update, e, 1)


async def one_step_active_task(update: Update, context: CallbackContext):
    query = update.callback_query.data
    next_step = 1

    try:
        if "active_task_callback" not in query:  # Если нажали куда-то не туда - выходим из диалога
            return ConversationHandler.END

        task_number = int(''.join(filter(str.isdigit, query)))
        if task_number is None:
            raise ValueError(f"ERROR: task_number:{task_number} is none")

        response = await delete_task_by_id(task_number)
        if response is None:
            raise ValueError(f"ERROR stopped task number: {str(task_number)}")

        await update.callback_query.message.delete()

        return next_step

    except Exception as e:
        return await base_error_handler(update, e, next_step)
