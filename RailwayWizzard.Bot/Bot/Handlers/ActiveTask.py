from telegram import *

from Bot.Other import *
from Bot.Base import *
from Bot.Setting import (CALLBACK_ACTIVE_TASK, ADMIN_USERNAME)


def get_car_types_text(car_types: list) -> str:
    """ Convert a list of car types to a comma-separated string like "купе, плацкарт, св". """
    car_type_map = {1: "Сидячий", 2: "Плацкарт", 3: "Купе", 4: "СВ"}
    return ", ".join([car_type_map[car_type] for car_type in car_types if car_type in car_type_map])


async def active_task_handler(update: Update, context: CallbackContext):
    """ Начало взаимодействия по клику на inline-кнопку 'Список активных задач' """
    next_step = 1
    try:
        if update.callback_query.data != str(CALLBACK_ACTIVE_TASK):
            await update.callback_query.message.reply_text(
                text=f"Что-то пошло не так, обратитесь к администратору бота {ADMIN_USERNAME}")
            return ConversationHandler.END

        user_id = update.callback_query.message.chat.id
        active_tasks = await API.get_active_task_by_user_id(user_id)

        if not active_tasks:
            await update.callback_query.message.reply_text("У вас нет активных задач")
            return ConversationHandler.END

        # Send message for user include task info
        await update.callback_query.message.reply_text(text="Обратите внимание, в данном разделе не отображаются "
                                                            "устаревшие задания поиск по которым более не актуален.",
                                                       parse_mode=ParseMode.HTML)
        for task in active_tasks:
            await send_task_info(update, task)
        return next_step

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def send_task_info(update: Update, task: dict):
    """Send task information to the user."""

    try:
        task_id = task.get("id")
        car_types_text = get_car_types_text(task.get("carTypes", []))
        callback = f"active_task_callback{task_id}"
        keyboard = InlineKeyboardMarkup([[InlineKeyboardButton(text='Остановить поиск по задаче', callback_data=callback)]])

        task_info = (f"Задача № <strong> {task_id} </strong>\n"
                     f"Станция отправления: <strong> {task['departureStation']} </strong>\n"
                     f"Станция прибытия: <strong> {task['arrivalStation']} </strong>\n"
                     f"Дата отправления: <strong> {task['dateFromString']} </strong>\n"
                     f"Время отправления: <strong> {task['timeFrom']} </strong>\n"
                     f"Количество мест: <strong> {str(task['numberSeats'])} </strong>\n")

        await update.callback_query.message.reply_text(text=task_info, reply_markup=keyboard, parse_mode=ParseMode.HTML)

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

        response = await API.delete_task_by_id(task_number)
        if response is None:
            raise ValueError(f"ERROR stopped task number: {str(task_number)}")

        await update.callback_query.edit_message_text(f"{update.callback_query.message.text_html} \n\n<strong>Остановлена</strong>",
                                                      parse_mode=ParseMode.HTML)
        return next_step

    except Exception as e:
        return await base_error_handler(update, e, next_step)
