from telegram.ext import *
from telegram.constants import *
from Bot.Setting import (message_success,message_failure,
                         CALLBACK_DATA_CORRECT_NOTIFICATION,
                         CALLBACK_DATA_INCORRECT_NOTIFICATION, CALLBACK_NOTIFICATION, notification_confirm_inline_buttons)
from Bot.validators import *
from Bot.Other import *
from Bot.API import *


async def notification_handler(update: Update, context: CallbackContext):
    """ Начало взаимодействия по клику на inline-кнопку 'Уведомление' """
    init = update.callback_query.data

    try:
        if init != str(CALLBACK_NOTIFICATION):
            await update.callback_query.message.reply_text(text='Что-то пошло не так, обратитесь к администратору бота')
            return ConversationHandler.END
        await update.callback_query.message.reply_text('Обратите внимание, по умолчанию не приходят уведомления о местах для инвалидов. Если вам необходимо получать уведомления и в таком случае - пожалуйста, обратитесь к администратору бота.(/help)\n\nДля возврата в главное меню введите /stop')

        await update.callback_query.message.reply_text(text='Укажите <strong>станцию отправления</strong>',
                                                       parse_mode=ParseMode.HTML)
        return 1

    except Exception as e:
        print(e)
        await update.callback_query.message.reply_text(text='Укажите <strong>станцию отправления</strong>',
                                                       parse_mode=ParseMode.HTML)
        raise


async def first_step_notification(update: Update, context: CallbackContext):
    try:
        # Duplicate Code(1)
        log_user_message(update, context)
        await update.message.reply_chat_action(ChatAction.TYPING)

        if await check_stop(update, context):
            return ConversationHandler.END

        if not language_input_validation(update.message.text):
            await update.message.reply_text('Недопустимый ввод.\nРазрешается вводить только символы кириллицы и цифры')
            return 1
        # ToDo: Если станция есть в БД - не нужно ходить к РЖД
        station_code = await station_validate(update.message.text)
        if station_code is None:
            await update.message.reply_text(text="Такой станции на сайте РЖД не котируется.\n"
                                                 "Укажите <strong>станцию</strong> отправления",
                                            parse_mode=ParseMode.HTML)
            return 1

        context.user_data[0] = update.message.text.upper()
        context.user_data[10] = station_code
        await update.message.reply_text(text='Укажите <strong>станцию прибытия</strong>',
                                        parse_mode=ParseMode.HTML)
        return 2

    except Exception as e:
        print(e)
        await update.message.reply_text(message_error)
        return 1


async def second_step_notification(update: Update, context: CallbackContext):
    try:
        # Duplicate Code(1)
        log_user_message(update, context)
        await update.message.reply_chat_action(ChatAction.TYPING)

        if await check_stop(update, context):
            return ConversationHandler.END

        if not language_input_validation(update.message.text):
            await update.message.reply_text('Недопустимый ввод. \nРазрешается вводить только символы кириллицы и цифры')
            return 1

        station_code = await station_validate(update.message.text)
        if station_code is None:
            await update.message.reply_text('Такой станции на сайте РЖД не котируется.\nУкажите станцию прибытия')
            return 2

        context.user_data[1] = update.message.text.upper()
        context.user_data[11] = station_code

        if context.user_data[0] == context.user_data[1]:
            await update.message.reply_text('Станции не могут совпадать')
            return 2

        await update.message.reply_text(text='Укажите <strong>дату отправления</strong>',
                                        parse_mode=ParseMode.HTML)
        return 3

    except Exception as e:
        print(e)
        await update.message.reply_text(message_error)
        return 2


async def third_step_notification(update: Update, context: CallbackContext):
    try:
        log_user_message(update, context)
        await update.message.reply_chat_action(ChatAction.TYPING)

        if await check_stop(update, context):
            return ConversationHandler.END

        date_and_date_json = date_format_validate(update.message.text)
        if date_and_date_json is None:
            await update.message.reply_text('Формат даты должен быть dd.mm.yyyy')
            await update.message.reply_text(text='Укажите <strong>дату отправления</strong>',
                                            parse_mode=ParseMode.HTML)
            return 3
        if date_limits_validate(update.message.text) is None:
            await update.message.reply_text('На указанную дату билеты не продаются')
            await update.message.reply_text(text='Укажите <strong>дату отправления</strong>',
                                            parse_mode=ParseMode.HTML)
            return 3

        context.user_data[2] = date_and_date_json['date']  # Дата в формате даты
        context.user_data[22] = date_and_date_json['date_text'] #Дата в формате строки
        await update.message.reply_text(text='Укажите <strong>время отправления</strong>',
                                        parse_mode=ParseMode.HTML)
        return 4

    except Exception as e:
        print(e)
        await update.message.reply_text(message_error)
        return 3


async def fourth_step_notification(update: Update, context: CallbackContext):
    try:
        log_user_message(update, context)
        await update.message.reply_chat_action(ChatAction.TYPING)

        if await check_stop(update, context):
            return ConversationHandler.END

        # обрабатываем время отправления
        input_time = update.message.text
        if not time_format_validate(input_time):
            await update.message.reply_text('Формат времени должен быть hh:mm ')
            await update.message.reply_text(text='Укажите <strong>время отправления</strong>',
                                            parse_mode=ParseMode.HTML)
            return 4

        # True #ToDo:пока отключил проверку времени, т.к. ржд динамит меня
        available_time = time_check_validate(
            input_time,
            context.user_data[10], context.user_data[11],
            context.user_data[0], context.user_data[1],
            context.user_data[22])
        if available_time is not True:
            await update.message.reply_text('Не найдено поездки с таким временем')
            await update.message.reply_text('Доступное время для бронирования:\n' + available_time.__str__())
            await update.message.reply_text(text='Укажите <strong>время отправления</strong>',
                                            parse_mode=ParseMode.HTML)
            return 4
        context.user_data[3] = input_time

        await update.message.reply_text(text="Пожалуйста, проверьте введенные данные:"
                                        + "\n\nСтанция отправления: " + "<strong>" + context.user_data[0] + "</strong>"
                                        + "\nСтанция прибытия: " + "<strong>" + context.user_data[1] + "</strong>"
                                        + "\nДата отправления: " + "<strong>" + context.user_data[22] + "</strong>"
                                        + "\nВремя отправления: " + "<strong>" + context.user_data[3] + "</strong>",
                                        reply_markup=notification_confirm_inline_buttons,
                                        parse_mode=ParseMode.HTML)
        return 5

    except Exception as e:
        print(e)
        await update.message.reply_text(message_error)
        return 4


async def fifth_step_notification(update: Update, context: CallbackContext):
    query_data = update.callback_query.data
    text_message_html = update.callback_query.message.text_html
    text_message = update.callback_query.message.text

    try:
        if query_data == str(CALLBACK_DATA_CORRECT_NOTIFICATION):
            notification_data_id = await send_notification_data_to_robot(update, context)
            update_text_message = (message_success + '<strong>' + notification_data_id + '</strong>' + ".\n\n"
                                   + str(text_message_html)[str(text_message_html).find('Станция отправления'):])
            await update.callback_query.edit_message_text(update_text_message, parse_mode=ParseMode.HTML)
            # Возвращаемся в главное меню
            await start_buttons(update, context)
            return ConversationHandler.END
        elif query_data == str(CALLBACK_DATA_INCORRECT_NOTIFICATION):
            await update.callback_query.edit_message_text(text_message)
            await update.callback_query.message.reply_text(text=message_failure)
            # Возвращаемся в главное меню
            await start_buttons(update, context)
            return ConversationHandler.END
        if await check_stop(update, context):
            return ConversationHandler.END

    except Exception as e:
        print(e)
        await update.callback_query.edit_message_text(text_message)
        await update.callback_query.message.reply_text(text=message_error)
        await update.callback_query.message.reply_text(text=message_failure)
        return 5


async def send_notification_data_to_robot(update: Update, context: CallbackContext):
    record_json = {
        "DepartureStation": context.user_data[0],
        "ArrivalStation": context.user_data[1],
        "DateFrom": context.user_data[2],
        "TimeFrom": context.user_data[3],
        "UserId": update.callback_query.message.chat.id,
    }
    try:
        return await create_and_get_id_notification_task(record_json)

    except ConnectionRefusedError:
        return ConversationHandler.END

    except Exception as e:
        print(e)
        await update.callback_query.message.reply_text(text=message_error)
        raise