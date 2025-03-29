import datetime

from telegram import InlineKeyboardMarkup

from bot.data.CarType import CarType
from bot.handlers.notification_handler.notification_handler_helper import NotificationHandlerDialog
from bot.setting import *
from bot.validators.validators import *
from bot.handlers.notification_handler.base_notification_handler import *
from bot.handlers.error_handler.base_error_handler import *
from bot.queries.robot_queries import station_validate, create_and_get_id_notification_task, \
    get_popular_cities_by_user_id
from bot.data.NotificationTask import NotificationTask

car_types = {car_type: car_type.is_sitting for car_type in CarType}
popular_cities = []


def generate_car_type_markup():
    buttons = [create_car_type_button(car_type) for car_type in car_types]

    grouped_buttons = [buttons[i:i + 2] for i in range(0, len(buttons), 2)]

    grouped_buttons.append(FOOTER_MENU_CAR_TYPE_INLINE_BUTTONS)

    return InlineKeyboardMarkup(grouped_buttons)


def create_car_type_button(car_type: CarType) -> InlineKeyboardButton:
    """ Helper Functions """
    selected = car_types[car_type]
    emoji = SELECT_SMILE if selected else NON_SELECT_SMILE
    return InlineKeyboardButton(text=f"{emoji} {car_type.display_name}",
                                callback_data=f"callback_{car_type.name.lower()}")


def set_default_car_types():
    global car_types
    global car_type_markup

    car_types = {car_type: car_type.is_sitting for car_type in CarType}

    car_type_markup = generate_car_type_markup()


async def notification_handler(update: Update, context: CallbackContext):
    """ Начало взаимодействия по клику на inline-кнопку 'Уведомление' """
    next_step = 1
    global popular_cities
    try:
        if update.callback_query.data != CALLBACK_NOTIFICATION:
            raise Exception('ERROR CONCAT CALLBACK QUERY')

        user_id = update.callback_query.message.chat.id
        popular_cities = await get_popular_cities_by_user_id(user_id)

        await update.callback_query.message.reply_text(text="Обратите внимание, по умолчанию не приходят уведомления о "
                                                            "местах для инвалидов. Если вам необходимо получать "
                                                            "уведомления и в таком случае - пожалуйста сообщите об этом"
                                                            "\n\nДля возврата в главное меню введите /stop")

        await update.callback_query.message.reply_text(
            text="Укажите <strong>станцию отправления</strong>.\nНапример:\n\n" +
                 '    '.join(
                '<code>' + str(city) + '</code>' for city in popular_cities),
            parse_mode=ParseMode.HTML)

        return next_step

    except Exception as e:
        await base_error_handler(update, e, next_step)


async def first_step_notification(update: Update, context: CallbackContext):
    """ Обрабатывает станцию отправления """
    next_step = 2
    expected_station_name = update.message.text.upper()
    global popular_cities

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        language_input_validation(expected_station_name)

        stations = await station_validate(expected_station_name)

        if len(stations) == 0:
            await update.message.reply_text(
                text="Такой станции на сайте РЖД не котируется.\n"
                     "Укажите <strong>станцию отправления</strong>.\nНапример:\n\n" +
                     '    '.join(
                    '<code>' + str(city) + '</code>' for city in popular_cities),
                parse_mode=ParseMode.HTML)
            return next_step - 1

        elif len(stations) > 1:
            message_text = "По вашему запросу найдено несколько станций:\n"
            for station in stations:
                station_name = station['name']
                message_text += f"<code>{station_name}</code>\n"
            message_text += "\nПожалуйста укажите название станции в соответствие с предлагаемыми"
            await update.message.reply_text(
                text=message_text,
                parse_mode=ParseMode.HTML)
            return next_step - 1

        elif len(stations) == 1:
            station = stations[0]
            context.user_data[0] = station['name']
            context.user_data[10] = station['expressCode']
            await update.message.reply_text(
                text="Укажите <strong>станцию прибытия</strong>.\nНапример:\n\n" +
                     '    '.join(
                    '<code>' + str(city) + '</code>' for city in popular_cities),
                parse_mode=ParseMode.HTML)

            return next_step

        raise Exception("Непредвиденная ошибка в методе обработки станции")

    except ValueError as e:
        return await base_error_handler(update, e, next_step, MESSAGE_FORMAT_ERROR)

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def second_step_notification(update: Update, context: CallbackContext):
    """ Обрабатывает станцию прибытия """
    next_step = 3
    expected_station_name = update.message.text.upper()
    global popular_cities

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        language_input_validation(expected_station_name)

        if expected_station_name == context.user_data[0]:
            await update.message.reply_text("Станции не могут совпадать.\n"
                                            "Укажите станцию прибытия.\nНапример:\n\n" +
                                            '    '.join(
                                            '<code>' + str(city) + '</code>' for city in popular_cities),
                                            parse_mode=ParseMode.HTML)
            return next_step - 1

        stations = await station_validate(expected_station_name)
        if len(stations) == 0:
            await update.message.reply_text(
                text="Такой станции на сайте РЖД не котируется.\n"
                     "Укажите станцию прибытия.\nНапример:\n\n" +
                     '    '.join(
                    '<code>' + str(city) + '</code>' for city in popular_cities),
                parse_mode=ParseMode.HTML)
            return next_step - 1

        elif len(stations) > 1:
            message_text = "По вашему запросу найдено несколько станций:\n"
            for station in stations:
                station_name = station['name']
                message_text += f"<code>{station_name}</code>\n"
            message_text += "\nПожалуйста укажите название станции в соответствие с предлагаемыми"
            await update.message.reply_text(text=message_text, parse_mode=ParseMode.HTML)
            return next_step - 1

        elif len(stations) == 1:
            station = stations[0]
            context.user_data[1] = station['name']
            context.user_data[11] = station['expressCode']
            await update.message.reply_text(
                text=NotificationHandlerDialog.enter_departure_date_text(), parse_mode=ParseMode.HTML)
            return next_step

        raise Exception("Непредвиденная ошибка в методе обработки станции")

    except ValueError as e:
        return await base_error_handler(update, e, next_step, MESSAGE_FORMAT_ERROR)

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def third_step_notification(update: Update, context: CallbackContext):
    """ Обрабатывает дату отправления """
    next_step = 4
    expected_date = update.message.text

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        date_and_date_json = date_format_validate(expected_date)

        # Получаем доступное время для бронирования
        available_times = await get_available_times(context.user_data[0], context.user_data[1], date_and_date_json['date'])
        date_limits = await date_limits_validate(date_and_date_json['date_text'])
        if not available_times or not date_limits:
            await update.message.reply_text("По указанному маршруту на указанную дату билеты не продаются")
            await update.message.reply_text(
                text=NotificationHandlerDialog.enter_departure_date_text(), parse_mode=ParseMode.HTML)
            return next_step - 1

        # Success
        context.user_data[2] = date_and_date_json['date']  # Дата в формате даты
        context.user_data[22] = date_and_date_json['date_text']  # Дата в формате строки

        await update.message.reply_text(text="Укажите <strong>время отправления</strong>.\n"
                                             "Доступное время для бронирования:\n" +
                                             '    '.join(
                                                 '<code>' + str(time) + '</code>' for time in available_times),
                                        parse_mode=ParseMode.HTML)
        return next_step

    except DateFormatError as value_error:
        await update.message.reply_text(text=str(value_error))
        await update.message.reply_text(
            text=NotificationHandlerDialog.enter_departure_date_text(), parse_mode=ParseMode.HTML)

        return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def fourth_step_notification(update: Update, context: CallbackContext):
    """ Обрабатывает время отправления """
    global car_type_markup
    next_step = 5
    expected_input_time = update.message.text

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        # обрабатываем время отправления
        # TODO: еще раз запрашиваем время чтобы сравнить его с тем что ввел пользователь...
        available_times = await get_available_times(context.user_data[0], context.user_data[1], context.user_data[2])
        validate_time = await time_check_validate(expected_input_time, available_times)

        if not time_format_validate(expected_input_time):
            await update.message.reply_text("Формат времени должен быть hh:mm ")
            await update.message.reply_text(text="Укажите <strong>время отправления</strong>.\n"
                                                 "Доступное время для бронирования:\n" +
                                                 '    '.join(
                                                     '<code>' + str(time) + '</code>' for time in available_times),
                                            parse_mode=ParseMode.HTML)
            return next_step - 1

        if validate_time is True:
            context.user_data[3] = expected_input_time
            await update.message.reply_text(
                text=MESSAGE_MIN_COUNT_SEATS,
                parse_mode=ParseMode.HTML)
            return next_step

        else:
            await update.message.reply_text("Не найдено поездки с таким временем")
            await update.message.reply_text(text="Укажите <strong>время отправления</strong>.\n"
                                                 "Доступное время для бронирования:\n" +
                                                 '    '.join(
                                                     '<code>' + str(time) + '</code>' for time in available_times),
                                            parse_mode=ParseMode.HTML)
            return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def fifth_step_notification(update: Update, context: CallbackContext):
    """ Обрабатывает минимальное количество мест в поезде на которое необходимо создать уведомление"""
    next_step = 6
    expected_amount_seats = update.message.text
    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        # обрабатываем количество мест которое ввел пользователь
        if expected_amount_seats.isdigit():
            expected_amount_seats = int(expected_amount_seats)

            if MAX_NUMBER_SEATS >= expected_amount_seats > 0:
                context.user_data[33] = expected_amount_seats
                set_default_car_types()
                await update.message.reply_text(
                    text="Выберите <strong>класс обсуживания</strong> который вас интересует.\n\n" +
                         "Обратите внимание, выбирая, например, <strong>\"Плац низ\"</strong> "
                         "вы выбираете плацкартные нижние места <strong>не включая</strong> боковых.\n\n"
                         "Для боковых мест определен отдельный фильтр.",
                    reply_markup=car_type_markup,
                    parse_mode=ParseMode.HTML)
                return next_step

            else:
                await update.message.reply_text(
                    text="Ошибка. Вы ввели число более 10 или менее 1. Если вы действительно хотите создать задачу на "
                         f"появление более 10 мест одновременно, пожалуйста, обратитесь к администратору бота {ADMIN_USERNAME}")
                await update.message.reply_text(text=MESSAGE_MIN_COUNT_SEATS, parse_mode=ParseMode.HTML)
                return next_step - 1

        else:
            await update.message.reply_text(text="Необходимо ввести цифру")
            await update.message.reply_text(text=MESSAGE_MIN_COUNT_SEATS, parse_mode=ParseMode.HTML)
            return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def sixth_step_notification(update: Update, context: CallbackContext):
    """ Обрабатывает типы вагонов """
    global car_type_markup
    global car_types

    next_step = 7
    query_data = update.callback_query.data

    try:
        if query_data == str(CALLBACK_CAR_TYPE_CONTINUE):
            car_types_text = ''  # Строка для отображения
            car_types_list = []  # [1,2,3,4] массив для отправки на сервер
            for car_type, selected in car_types.items():
                if selected:
                    car_types_text += f"{car_type.display_name}, "
                    car_types_list.append(
                        list(CarType).index(car_type) + 1)  # порядковый номер элемента в enum`е CarType

            if not car_types_list:
                message_warning = "Необходимо выбрать хотя бы 1 тип вагона"
                if message_warning not in update.callback_query.message.text:
                    await update.callback_query.edit_message_text(message_warning, reply_markup=car_type_markup)
                return next_step - 1

            context.user_data[4] = update.callback_query.message.chat.id
            context.user_data[5] = car_types_list
            await update.callback_query.edit_message_text(
                text="Пожалуйста, проверьте введенные данные:"
                     f"\n\nСтанция отправления: <strong>{context.user_data[0]}</strong>"
                     f"\nСтанция прибытия: <strong>{context.user_data[1]}</strong>"
                     f"\nДата отправления: <strong>{context.user_data[22]}</strong>"
                     f"\nВремя отправления: <strong>{context.user_data[3]}</strong>"
                     f"\nКласс обслуживания: <strong>{car_types_text[:-2]}</strong>"
                     f"\nКоличество необходимых мест: <strong>{str(context.user_data[33])}</strong>",
                reply_markup=NOTIFICATION_CONFIRM_INLINE_BUTTONS,
                parse_mode=ParseMode.HTML)
            return next_step
        elif query_data == str(CALLBACK_CAR_TYPE_SELECT_ALL):
            car_types = {car_type: True for car_type in CarType}

        elif query_data == str(CALLBACK_CAR_TYPE_REMOVE_ALL):
            car_types = {car_type: False for car_type in CarType}

        else:
            for car_type in CarType:
                if query_data == f"callback_{car_type.name.lower()}":
                    car_types[car_type] = not car_types[car_type]

        current_car_type_markup = update.callback_query.message.reply_markup
        car_type_markup = generate_car_type_markup()

        if car_type_markup.to_dict() != current_car_type_markup.to_dict():
            await update.callback_query.edit_message_reply_markup(car_type_markup)

        return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def seventh_step_notification(update: Update, context: CallbackContext):
    """ Обрабатывает решение пользователя о создании задачи """

    query_data = update.callback_query.data
    text_message_html = update.callback_query.message.text_html
    body_text_task = str(text_message_html)[str(text_message_html).find("Станция отправления"):]
    next_step = 7

    try:
        if query_data == str(CALLBACK_DATA_CORRECT_NOTIFICATION):  # Уведомление успешно создано
            notification_data_id = await send_notification_data_to_robot(context)  # Отправляем данные о задаче в app
            update_text_message = (
                    MESSAGE_SUCCESS + "<strong>" + notification_data_id + "</strong>" + "\n" + body_text_task)
            await update.callback_query.edit_message_text(update_text_message, parse_mode=ParseMode.HTML)

        elif query_data == str(CALLBACK_DATA_INCORRECT_NOTIFICATION):  # Создание уведомления отменено
            update_text_message = (MESSAGE_CANCEL + "\n\n" + body_text_task)
            await update.callback_query.edit_message_text(update_text_message, parse_mode=ParseMode.HTML)

        await start_buttons(update, context)  # Возвращаемся в главное меню
        return ConversationHandler.END

    except Exception as e:
        await base_error_handler(update, e, next_step)
        return next_step


async def send_notification_data_to_robot(context: CallbackContext):
    notification_task_data = NotificationTask(
        DepartureStation=context.user_data[0],
        ArrivalStation=context.user_data[1],
        DateFrom=context.user_data[2],
        TimeFrom=context.user_data[3],
        UserId=context.user_data[4],
        CarTypes=context.user_data[5],
        NumberSeats=context.user_data[33])

    try:
        return await create_and_get_id_notification_task(notification_task_data)

    except Exception as e:
        raise e


# INIT
car_type_markup = generate_car_type_markup()
