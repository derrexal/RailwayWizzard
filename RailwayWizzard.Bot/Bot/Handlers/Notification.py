from telegram import *
from telegram.ext import *
from telegram.constants import *
from Bot.Setting import (message_success, message_failure,
                         CALLBACK_DATA_CORRECT_NOTIFICATION,
                         CALLBACK_DATA_INCORRECT_NOTIFICATION, CALLBACK_NOTIFICATION,
                         CALLBACK_CAR_TYPE_SEDENTARY, CALLBACK_CAR_TYPE_RESERVED_SEAT, CALLBACK_CAR_TYPE_COMPARTMENT,
                         CALLBACK_CAR_TYPE_LUXURY, CALLBACK_CAR_TYPE_CONTINUE,
                         notification_confirm_inline_buttons, footer_menu_car_type_inline_buttons)
from Bot.validators import *
from Bot.Other import *
from Bot.API import *


non_select_smile = '\U000025FB'
select_smile = '\U00002705'
car_type_sedentary_text = select_smile + ' Сидячий'
car_type_reserved_seat_text = select_smile + ' Плацкарт'
car_type_compartment_text = select_smile + ' Купе'
car_type_luxury_text = non_select_smile + ' СВ'
car_type_sedentary_button = InlineKeyboardButton(text=car_type_sedentary_text,
                                                 callback_data=CALLBACK_CAR_TYPE_SEDENTARY)
car_type_reserved_seat_button = InlineKeyboardButton(text=car_type_reserved_seat_text,
                                                     callback_data=CALLBACK_CAR_TYPE_RESERVED_SEAT)
car_type_compartment_button = InlineKeyboardButton(text=car_type_compartment_text,
                                                   callback_data=CALLBACK_CAR_TYPE_COMPARTMENT)
car_type_luxury_button = InlineKeyboardButton(text=car_type_luxury_text, callback_data=CALLBACK_CAR_TYPE_LUXURY)
car_type_inline_buttons = InlineKeyboardMarkup([[car_type_sedentary_button, car_type_reserved_seat_button,
                                                 car_type_compartment_button, car_type_luxury_button],
                                                footer_menu_car_type_inline_buttons])
car_types = {'sedentary': True,
             'reserved_seat': True,
             'compartment': True,
             'luxury': False}

#TODO:duplicate
def set_default_car_types():
    global car_types
    global car_type_sedentary_text
    global car_type_reserved_seat_text
    global car_type_compartment_text
    global car_type_luxury_text
    global car_type_sedentary_button
    global car_type_reserved_seat_button
    global car_type_compartment_button
    global car_type_luxury_button
    global car_type_inline_buttons
    global select_smile
    global non_select_smile
    car_types = {'sedentary': True,
                 'reserved_seat': True,
                 'compartment': True,
                 'luxury': False}
    car_type_sedentary_text = select_smile + ' Сидячий'
    car_type_reserved_seat_text = select_smile + ' Плацкарт'
    car_type_compartment_text = select_smile + ' Купе'
    car_type_luxury_text = non_select_smile + ' СВ'
    car_type_sedentary_button = InlineKeyboardButton(text=car_type_sedentary_text,
                                                        callback_data=CALLBACK_CAR_TYPE_SEDENTARY)
    car_type_reserved_seat_button = InlineKeyboardButton(text=car_type_reserved_seat_text,
                                                         callback_data=CALLBACK_CAR_TYPE_RESERVED_SEAT)
    car_type_compartment_button = InlineKeyboardButton(text=car_type_compartment_text,
                                                       callback_data=CALLBACK_CAR_TYPE_COMPARTMENT)
    car_type_inline_buttons = InlineKeyboardMarkup([[car_type_sedentary_button, car_type_reserved_seat_button,
                                                     car_type_compartment_button, car_type_luxury_button],
                                                    footer_menu_car_type_inline_buttons])


async def notification_handler(update: Update, context: CallbackContext):
    """ Начало взаимодействия по клику на inline-кнопку 'Уведомление' """
    init = update.callback_query.data
    try:
        if init != str(CALLBACK_NOTIFICATION):
            await update.callback_query.message.reply_text(text='Что-то пошло не так, обратитесь к администратору бота')
            return ConversationHandler.END
        await update.callback_query.message.reply_text('Обратите внимание, по умолчанию не приходят уведомления о '
                                                       'местах для инвалидов. Если вам необходимо получать '
                                                       'уведомления и в таком случае - пожалуйста, обратитесь к '
                                                       'администратору.\n\nДля возврата в главное меню введите /stop')

        await update.callback_query.message.reply_text(
            text='Укажите <strong>станцию отправления</strong>.\n'
                 'Например, <strong>Москва</strong>',
            parse_mode=ParseMode.HTML)
        return 1

    except Exception as e:
        print(e)
        await update.callback_query.message.reply_text(text=message_error, parse_mode=ParseMode.HTML)
        raise


async def first_step_notification(update: Update, context: CallbackContext):
    try:
        #TODO: Duplicate Code(1)
        log_user_message(update, context)
        await update.message.reply_chat_action(ChatAction.TYPING)

        if await check_stop(update, context):
            return ConversationHandler.END

        if not language_input_validation(update.message.text):
            await update.message.reply_text('Недопустимый ввод.\nРазрешается вводить только символы кириллицы и цифры')
            return 1
        station_code = await station_validate(update.message.text)
        if station_code is None:
            await update.message.reply_text(text='Такой станции на сайте РЖД не котируется.\n'
                                                 'Укажите <strong>станцию отправления</strong>.\n'
                                                 'Например, <strong>Москва</strong>',
                                            parse_mode=ParseMode.HTML)
            return 1

        context.user_data[0] = update.message.text.upper()
        context.user_data[10] = station_code
        await update.message.reply_text(text='Укажите <strong>станцию прибытия</strong>.\n'
                                             'Например, <strong>Курск</strong>',
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
            await update.message.reply_text('Такой станции на сайте РЖД не котируется.\n'
                                            'Укажите станцию прибытия\n'
                                            'Например, <strong>Курск</strong>')
            return 2

        context.user_data[1] = update.message.text.upper()
        context.user_data[11] = station_code

        if context.user_data[0] == context.user_data[1]:
            await update.message.reply_text('Станции не могут совпадать')
            return 2

        await update.message.reply_text(text='Укажите <strong>дату отправления</strong>.\n'
                                             'Например, <strong>' + datetime.now().strftime("%d.%m.%Y") + '</strong>',
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
            await update.message.reply_text(text='Укажите <strong>дату отправления</strong>\n'
                                                 'Например, <strong>' + datetime.now().strftime("%d.%m.%Y")
                                                 + '</strong>',
                                            parse_mode=ParseMode.HTML)
            return 3
        if date_limits_validate(update.message.text) is None:
            await update.message.reply_text('На указанную дату билеты не продаются')
            await update.message.reply_text(text='Укажите <strong>дату отправления</strong>\n'
                                                 'Например, <strong>' + datetime.now().strftime("%d.%m.%Y")
                                                 + '</strong>',
                                            parse_mode=ParseMode.HTML)
            return 3

        context.user_data[2] = date_and_date_json['date']  # Дата в формате даты
        context.user_data[22] = date_and_date_json['date_text']  # Дата в формате строки
        await update.message.reply_text(text='Укажите <strong>время отправления</strong>\n'
                                             'Например, <strong>' + datetime.now().strftime("%H:%M") + '</strong>',
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
            await update.message.reply_text(text='Укажите <strong>время отправления</strong>\n'
                                                 'Например, <strong>' + datetime.now().strftime("%H:%M") + '</strong>',
                                            parse_mode=ParseMode.HTML)
            return 4
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
        set_default_car_types()
        await update.message.reply_text(text="Выберите тип вагона который вас интересует",
                                        reply_markup=car_type_inline_buttons)
        return 5

    except Exception as e:
        print(e)
        await update.message.reply_text(message_error)
        return 4


async def fifth_step_notification(update: Update, context: CallbackContext):
    global car_type_sedentary_text
    global car_type_reserved_seat_text
    global car_type_compartment_text
    global car_type_luxury_text
    global car_type_sedentary_button
    global car_type_reserved_seat_button
    global car_type_compartment_button
    global car_type_luxury_button
    global car_type_inline_buttons
    global car_types

    try:
        query_data = update.callback_query.data
        if query_data == str(CALLBACK_CAR_TYPE_CONTINUE):
            car_types_text = ''
            car_types_list = []
            if car_types['sedentary']:
                car_types_text = car_types_text + 'Сидячий, '
                car_types_list.append(1)
            if car_types['reserved_seat']:
                car_types_text = car_types_text + 'Плацкарт, '
                car_types_list.append(2)
            if car_types['compartment']:
                car_types_text = car_types_text + 'Купе, '
                car_types_list.append(3)
            if car_types['luxury']:
                car_types_text = car_types_text + 'СВ'
                car_types_list.append(4)

            if len(car_types_list) == 0:
                message_warning = "Необходимо выбрать хотя бы 1 тип вагона"
                if message_warning not in update.callback_query.message.text:
                    await update.callback_query.edit_message_text(message_warning, reply_markup=car_type_inline_buttons)
                return 5
            context.user_data[5] = car_types_list
            await update.callback_query.edit_message_text(text="Пожалуйста, проверьте введенные данные:"
                                                               + "\n\nСтанция отправления: " + "<strong>" +
                                                               context.user_data[0] + "</strong>"
                                                               + "\nСтанция прибытия: " + "<strong>" +
                                                               context.user_data[1] + "</strong>"
                                                               + "\nДата отправления: " + "<strong>" +
                                                               context.user_data[22] + "</strong>"
                                                               + "\nВремя отправления: " + "<strong>" +
                                                               context.user_data[3] + "</strong>"
                                                               + "\nВыбранные типы вагонов: " + "<strong>" +
                                                               car_types_text + "</strong>",
                                                          reply_markup=notification_confirm_inline_buttons,
                                                          parse_mode=ParseMode.HTML)
            set_default_car_types()
            return 6

        if query_data == str(CALLBACK_CAR_TYPE_SEDENTARY):
            if non_select_smile in car_type_sedentary_text:
                car_type_sedentary_text = car_type_sedentary_text.replace(non_select_smile, select_smile)
                car_types['sedentary'] = True
            elif select_smile in car_type_sedentary_text:
                car_type_sedentary_text = car_type_sedentary_text.replace(select_smile, non_select_smile)
                car_types['sedentary'] = False
            car_type_sedentary_button.text = car_type_sedentary_text
            car_type_inline_buttons = InlineKeyboardMarkup([[car_type_sedentary_button, car_type_reserved_seat_button,
                                                             car_type_compartment_button, car_type_luxury_button],
                                                            footer_menu_car_type_inline_buttons])
            await update.callback_query.edit_message_reply_markup(car_type_inline_buttons)
            return 5

        if query_data == str(CALLBACK_CAR_TYPE_RESERVED_SEAT):
            if non_select_smile in car_type_reserved_seat_text:
                car_type_reserved_seat_text = car_type_reserved_seat_text.replace(non_select_smile, select_smile)
                car_types['reserved_seat'] = True
            elif select_smile in car_type_reserved_seat_text:
                car_type_reserved_seat_text = car_type_reserved_seat_text.replace(select_smile, non_select_smile)
                car_types['reserved_seat'] = False
            car_type_reserved_seat_button.text = car_type_reserved_seat_text
            car_type_inline_buttons = InlineKeyboardMarkup([[car_type_sedentary_button, car_type_reserved_seat_button,
                                                             car_type_compartment_button, car_type_luxury_button],
                                                            footer_menu_car_type_inline_buttons])
            await update.callback_query.edit_message_reply_markup(car_type_inline_buttons)
            return 5

        if query_data == str(CALLBACK_CAR_TYPE_COMPARTMENT):
            if non_select_smile in car_type_compartment_text:
                car_type_compartment_text = car_type_compartment_text.replace(non_select_smile, select_smile)
                car_types['compartment'] = True
            elif select_smile in car_type_compartment_text:
                car_type_compartment_text = car_type_compartment_text.replace(select_smile, non_select_smile)
                car_types['compartment'] = False
            car_type_compartment_button.text = car_type_compartment_text
            car_type_inline_buttons = InlineKeyboardMarkup([[car_type_sedentary_button, car_type_reserved_seat_button,
                                                             car_type_compartment_button, car_type_luxury_button],
                                                            footer_menu_car_type_inline_buttons])
            await update.callback_query.edit_message_reply_markup(car_type_inline_buttons)
            return 5

        if query_data == str(CALLBACK_CAR_TYPE_LUXURY):
            if non_select_smile in car_type_luxury_text:
                car_type_luxury_text = car_type_luxury_text.replace(non_select_smile, select_smile)
                car_types['luxury'] = True
            elif select_smile in car_type_luxury_text:
                car_type_luxury_text = car_type_luxury_text.replace(select_smile, non_select_smile)
                car_types['luxury'] = False
            car_type_luxury_button.text = car_type_luxury_text
            car_type_inline_buttons = InlineKeyboardMarkup([[car_type_sedentary_button, car_type_reserved_seat_button,
                                                             car_type_compartment_button, car_type_luxury_button],
                                                            footer_menu_car_type_inline_buttons])
            await update.callback_query.edit_message_reply_markup(car_type_inline_buttons)
            return 5

    except Exception as e:
        print(e)
        await update.callback_query.message.reply_text(text=message_error)
        return 5


async def sixth_step_notification(update: Update, context: CallbackContext):
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
        await update.callback_query.message.reply_text(text=message_error)
        return 6


async def send_notification_data_to_robot(update: Update, context: CallbackContext):
    #TODO: вынести UserId в context
    record_json = {
        "DepartureStation": context.user_data[0],
        "ArrivalStation": context.user_data[1],
        "DateFrom": context.user_data[2],
        "TimeFrom": context.user_data[3],
        "UserId": update.callback_query.message.chat.id,
        "CarTypes": context.user_data[5],
    }
    try:
        return await create_and_get_id_notification_task(record_json)

    except Exception as e:
        raise e