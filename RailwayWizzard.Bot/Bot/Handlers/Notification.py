import datetime
from telegram import *
from Bot.Setting import *
from Bot.validators import *
from Bot.Other import *
from Bot.Base import *
from Bot import API
from Bot.Data.NotificationTaskData import NotificationTaskData

car_types = {
    CarType.SEDENTARY: True,
    CarType.RESERVED_SEAT: True,
    CarType.COMPARTMENT: True,
    CarType.LUXURY: False
}


def generate_car_type_buttons():
    return InlineKeyboardMarkup(
        [[create_car_type_button(CarType.SEDENTARY), create_car_type_button(CarType.RESERVED_SEAT),
          create_car_type_button(CarType.COMPARTMENT), create_car_type_button(CarType.LUXURY)],
         footer_menu_car_type_inline_buttons])


def create_car_type_button(car_type: CarType) -> InlineKeyboardButton:
    """ Helper Functions """
    selected = car_types[car_type]
    emoji = SELECT_SMILE if selected else NON_SELECT_SMILE
    return InlineKeyboardButton(text=f"{emoji} {car_type.value[0]}", callback_data=f"callback_{car_type.name.lower()}")


def set_default_car_types():
    global car_types
    global car_type_inline_buttons

    car_types = {
        CarType.SEDENTARY: True,
        CarType.RESERVED_SEAT: True,
        CarType.COMPARTMENT: True,
        CarType.LUXURY: False
    }

    car_type_inline_buttons = generate_car_type_buttons()


#init
car_type_inline_buttons = generate_car_type_buttons()


async def notification_handler(update: Update, context: CallbackContext):
    """ Начало взаимодействия по клику на inline-кнопку 'Уведомление' """
    next_step = 1
    try:
        if update.callback_query.data != CALLBACK_NOTIFICATION:
            raise Exception('ERROR CONCAT CALLBACK QUERY')
        await update.callback_query.message.reply_text(text="Обратите внимание, по умолчанию не приходят уведомления о "
                                                            "местах для инвалидов. Если вам необходимо получать "
                                                            "уведомления и в таком случае - пожалуйста, обратитесь к "
                                                            f"администратору бота {admin_username}"
                                                            "\n\nДля возврата в главное меню введите /stop")

        await update.callback_query.message.reply_text(
            text="Укажите <strong>станцию отправления</strong>.\nНапример, <code>Москва</code>",
            parse_mode=ParseMode.HTML)
        return next_step

    except Exception as e:
        await base_error_handler(update, e, next_step)


async def first_step_notification(update: Update, context: CallbackContext):
    next_step = 2
    expected_station_name = update.message.text.upper()

    try:
        await base_step_notification(update, context)
        language_input_validation(expected_station_name)

        stations = await API.station_validate(expected_station_name)
        if len(stations) == 0:
            await update.message.reply_text(text="Такой станции на сайте РЖД не котируется.\n"
                                                 "Укажите <strong>станцию отправления</strong>.\n"
                                                 "Например, <code>Москва</code>",
                                            parse_mode=ParseMode.HTML)
            return next_step - 1
        elif len(stations) > 1:
            message_text = "По вашему запросу найдено несколько станций:\n"
            for station in stations:
                station_name = station['stationName']
                message_text += f"<code>{station_name}</code>\n"
            message_text += "Пожалуйста укажите название станции в соответствие с предлагаемыми"
            await update.message.reply_text(text=message_text, parse_mode=ParseMode.HTML)
            return next_step - 1
        elif len(stations) == 1:
            station = stations[0]
            context.user_data[0] = station['stationName']
            context.user_data[10] = station['expressCode']
            await update.message.reply_text(text="Укажите <strong>станцию прибытия</strong>.\n"
                                                 "Например, <code>Курск</code>",
                                            parse_mode=ParseMode.HTML)
            return next_step
        raise Exception("Непредвиденная ошибка в методе обработки станции")

    except ValueError as e:
        return await base_error_handler(update, e, next_step, message_format_error)

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def second_step_notification(update: Update, context: CallbackContext):
    next_step = 3
    expected_station_name = update.message.text.upper()

    try:
        await base_step_notification(update, context)
        language_input_validation(expected_station_name)

        if expected_station_name == context.user_data[0]:
            await update.message.reply_text("Станции не могут совпадать"
                                            "Укажите станцию прибытия\n"
                                            "Например, <code>Курск</code>",
                                            parse_mode=ParseMode.HTML)
            return next_step - 1

        stations = await API.station_validate(expected_station_name)
        if len(stations) == 0:
            await update.message.reply_text(text="Такой станции на сайте РЖД не котируется.\n"
                                                 "Укажите станцию прибытия\n"
                                                 "Например, <code>Курск</code>",
                                            parse_mode=ParseMode.HTML)
            return next_step - 1
        elif len(stations) > 1:
            message_text = "По вашему запросу найдено несколько станций:\n"
            for station in stations:
                station_name = station['stationName']
                message_text += f"<code>{station_name}</code>\n"
            message_text += "Пожалуйста укажите название станции в соответствие с предлагаемыми"
            await update.message.reply_text(text=message_text, parse_mode=ParseMode.HTML)
            return next_step - 1
        elif len(stations) == 1:
            station = stations[0]
            context.user_data[1] = station['stationName']
            context.user_data[11] = station['expressCode']
            tomorrow = (datetime.now(moscow_tz) + timedelta(days=1)).strftime("%d.%m.%Y")
            await update.message.reply_text(text="Укажите <strong>дату отправления</strong>.\n"
                                                 f"Например, <code>{tomorrow}</code>",
                                            parse_mode=ParseMode.HTML)
            return next_step
        raise Exception("Непредвиденная ошибка в методе обработки станции")

    except ValueError as e:
        return await base_error_handler(update, e, next_step, message_format_error)

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def third_step_notification(update: Update, context: CallbackContext):
    next_step = 4
    tomorrow = (datetime.now(moscow_tz) + timedelta(days=1)).strftime("%d.%m.%Y")
    try:
        await base_step_notification(update, context)

        date_and_date_json = date_format_validate(update.message.text)

        if date_and_date_json is None:
            await update.message.reply_text("Формат даты должен быть dd.mm.yyyy")
            await update.message.reply_text(text="Укажите <strong>дату отправления</strong>.\n"
                                                 f"Например, <code>{tomorrow}</code>",
                                            parse_mode=ParseMode.HTML)
            return next_step - 1

        date_limits = await date_limits_validate(update.message.text, context.user_data[0],
                                                 context.user_data[1], date_and_date_json['date_text'])
        if date_limits is None:
            await update.message.reply_text("По указанному маршруту на указанную дату билеты не продаются")
            await update.message.reply_text(text="Укажите <strong>дату отправления</strong>.\n"
                                                 f"Например, <code>{tomorrow}</code>",
                                            parse_mode=ParseMode.HTML)
            return next_step - 1
        context.user_data[2] = date_and_date_json['date']  # Дата в формате даты
        context.user_data[22] = date_and_date_json['date_text']  # Дата в формате строки

        available_time = await time_check_validate(context.user_data[0], context.user_data[1], context.user_data[22])

        await update.message.reply_text(text="Укажите <strong>время отправления</strong>\n"
                                             "Доступное время для бронирования:\n" +
                                             '    '.join(
                                                 '<code>' + str(time) + '</code>' for time in available_time),
                                        parse_mode=ParseMode.HTML)
        return next_step

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def fourth_step_notification(update: Update, context: CallbackContext):
    global car_type_inline_buttons
    next_step = 5
    try:
        await base_step_notification(update, context)

        # обрабатываем время отправления
        input_time = update.message.text
        available_time = await time_check_validate(context.user_data[0], context.user_data[1],
                                                   context.user_data[22], input_time)

        if not time_format_validate(input_time):
            await update.message.reply_text("Формат времени должен быть hh:mm ")
            await update.message.reply_text(text="Укажите <strong>время отправления</strong>\n"
                                                 "Доступное время для бронирования:\n" +
                                                 '    '.join(
                                                     '<code>' + str(time) + '</code>' for time in available_time),
                                            parse_mode=ParseMode.HTML)
            return next_step - 1

        if available_time is True:
            context.user_data[3] = input_time
            await update.message.reply_text(text="Укажите минимальное количество мест в поезде, "
                                                 "на которое вам необходимо создать уведомление.\n"
                                                 "Если не знаете что указать, напишите цифру 1")
            return next_step

        else:
            await update.message.reply_text("Не найдено поездки с таким временем")
            await update.message.reply_text(text="Укажите <strong>время отправления</strong>\n"
                                                 "Доступное время для бронирования:\n" +
                                                 '    '.join(
                                                     '<code>' + str(time) + '</code>' for time in available_time),
                                            parse_mode=ParseMode.HTML)
            return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def fifth_step_notification(update: Update, context: CallbackContext):
    next_step = 6
    try:
        await base_step_notification(update, context)

        # обрабатываем количество мест которое ввел пользователь
        input_amount_seats = update.message.text
        if input_amount_seats.isdigit():
            input_amount_seats = int(input_amount_seats)
            if MAX_NUMBER_SEATS >= input_amount_seats > 0:
                context.user_data[33] = input_amount_seats
                set_default_car_types()
                await update.message.reply_text(
                    text="Выберите тип вагона который вас интересует.\n"
                         "Проставьте все варианты, если не знаете что выбрать",
                    reply_markup=car_type_inline_buttons)
                return next_step
            else:
                await update.message.reply_text(
                    text="Ошибка. Вы ввели число более 10 или менее 1. Если вы действительно хотите создать задачу на "
                         f"появление 10 мест одновременно, пожалуйста, обратитесь к администратору бота {admin_username}")
                await update.message.reply_text(text="Укажите минимальное количество мест в поезде, "
                                                     "на которое вам необходимо создать уведомление.\n"
                                                     "Если не знаете что указать, напишите цифру 1")
                return next_step - 1
        else:
            await update.message.reply_text(text="Необходимо ввести цифру")
            await update.message.reply_text(text="Укажите минимальное количество мест в поезде, "
                                                 "на которое вам необходимо создать уведомление.\n"
                                                 "Если не знаете что указать, напишите цифру 1")
            return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def sixth_step_notification(update: Update, context: CallbackContext):
    global car_type_inline_buttons
    global car_types

    next_step = 7
    query_data = update.callback_query.data

    try:
        if query_data == str(CALLBACK_CAR_TYPE_CONTINUE):
            # Выбранные пользователем типы вагонов записываем в строку для отображения
            # и наполняем массив для отправки на сервер
            car_types_text = ''
            car_types_list = []  #schema: [1,2,3,4]
            for car_type, selected in car_types.items():
                if selected:
                    car_types_text += f"{car_type.value[0]}, "
                    car_types_list.append(
                        list(CarType).index(car_type) + 1)  #порядковый номер элемента в enum`е CarType

            if not car_types_list:
                message_warning = "Необходимо выбрать хотя бы 1 тип вагона"
                if message_warning not in update.callback_query.message.text:
                    await update.callback_query.edit_message_text(message_warning, reply_markup=car_type_inline_buttons)
                return 6

            context.user_data[4] = update.callback_query.message.chat.id
            context.user_data[5] = car_types_list
            await update.callback_query.edit_message_text(
                text="Пожалуйста, проверьте введенные данные:"
                     f"\n\nСтанция отправления: <strong>{context.user_data[0]}</strong>"
                     f"\nСтанция прибытия: <strong>{context.user_data[1]}</strong>"
                     f"\nДата отправления: <strong>{context.user_data[22]}</strong>"
                     f"\nВремя отправления: <strong>{context.user_data[3]}</strong>"
                     f"\nВыбранные типы вагонов: <strong>{car_types_text}</strong>"
                     f"\nКоличество мест: <strong>{str(context.user_data[33])}</strong>",
                reply_markup=notification_confirm_inline_buttons,
                parse_mode=ParseMode.HTML)
            return next_step

        # Отрисовка флажков у типов вагона по нажатию
        for car_type in CarType:
            if query_data == f"callback_{car_type.name.lower()}":
                car_types[car_type] = not car_types[car_type]
                car_type_inline_buttons = generate_car_type_buttons()
                await update.callback_query.edit_message_reply_markup(car_type_inline_buttons)
                return next_step - 1
    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def seventh_step_notification(update: Update, context: CallbackContext):
    global car_type_inline_buttons
    query_data = update.callback_query.data
    text_message_html = update.callback_query.message.text_html
    text_message = update.callback_query.message.text
    next_step = 7

    try:

        if query_data == str(CALLBACK_DATA_CORRECT_NOTIFICATION):
            notification_data_id = await send_notification_data_to_robot(update, context)
            # TODO: Зачем
            update_text_message = (message_success + "<strong>" + notification_data_id + "</strong>" + ".\n\n"
                                   + str(text_message_html)[str(text_message_html).find("Станция отправления"):])
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
        return await base_error_handler(update, e, next_step)


async def send_notification_data_to_robot(update: Update, context: CallbackContext):
    notification_task_data = NotificationTaskData(
        DepartureStation=context.user_data[0],
        ArrivalStation=context.user_data[1],
        DateFrom=context.user_data[2],
        TimeFrom=context.user_data[3],
        UserId=context.user_data[4],  # TODO:Проверить
        CarTypes=context.user_data[5],
        NumberSeats=context.user_data[33])

    try:
        return await API.create_and_get_id_notification_task(notification_task_data)

    except Exception as e:
        raise e
