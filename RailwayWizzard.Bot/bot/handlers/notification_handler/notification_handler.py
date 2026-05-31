from telegram import InlineKeyboardButton, InlineKeyboardMarkup, Update
from telegram.constants import ParseMode
from telegram.ext import CallbackContext, ConversationHandler

from bot.data.CarType import CarType
from bot.data.NotificationConversationState import (
    NOTIFICATION_STATE_KEY,
    NotificationConversationState,
    build_default_car_type_selection,
)
from bot.data.NotificationTask import NotificationTask
from bot.errors.DateFormatError import DateFormatError
from bot.handlers.error_handler.base_error_handler import base_error_handler
from bot.handlers.notification_handler.base_notification_handler import (
    null_step_notification,
)
from bot.handlers.notification_handler.notification_handler_helper import (
    NotificationHandlerDialog,
)
from bot.handlers.start_handler.start_handler import start_buttons
from bot.queries.robot_queries import (
    create_and_get_id_notification_task,
    get_available_times,
    get_popular_cities_by_user_id,
    station_validate,
)
from bot.setting import (
    ADMIN_USERNAME,
    CALLBACK_CAR_TYPE_CONTINUE,
    CALLBACK_CAR_TYPE_REMOVE_ALL,
    CALLBACK_CAR_TYPE_SELECT_ALL,
    CALLBACK_DATA_CORRECT_NOTIFICATION,
    CALLBACK_DATA_INCORRECT_NOTIFICATION,
    CALLBACK_NOTIFICATION,
    FOOTER_MENU_CAR_TYPE_INLINE_BUTTONS,
    MAX_NUMBER_SEATS,
    MESSAGE_CANCEL,
    MESSAGE_FORMAT_ERROR,
    MESSAGE_MIN_COUNT_SEATS,
    MESSAGE_SUCCESS,
    NON_SELECT_SMILE,
    NOTIFICATION_CONFIRM_INLINE_BUTTONS,
    SELECT_SMILE,
)
from bot.validators.validators import (
    date_format_validate,
    date_limits_validate,
    language_input_validation,
    time_check_validate,
    time_format_validate,
)


MIN_LENGTH_STATION_NAME = 2


def get_notification_state(context: CallbackContext) -> NotificationConversationState:
    state = context.user_data.get(NOTIFICATION_STATE_KEY)
    if not isinstance(state, NotificationConversationState):
        state = NotificationConversationState()
        context.user_data[NOTIFICATION_STATE_KEY] = state
    return state


def reset_notification_state(
    context: CallbackContext,
    popular_cities=None,
) -> NotificationConversationState:
    state = NotificationConversationState(popular_cities=popular_cities or [])
    context.user_data[NOTIFICATION_STATE_KEY] = state
    return state


def format_popular_cities(popular_cities) -> str:
    return "    ".join(f"<code>{city}</code>" for city in popular_cities)


def generate_car_type_markup(car_type_selection) -> InlineKeyboardMarkup:
    buttons = [
        create_car_type_button(car_type, car_type_selection) for car_type in CarType
    ]
    grouped_buttons = [buttons[i : i + 2] for i in range(0, len(buttons), 2)]
    grouped_buttons.append(FOOTER_MENU_CAR_TYPE_INLINE_BUTTONS)
    return InlineKeyboardMarkup(grouped_buttons)


def create_car_type_button(
    car_type: CarType,
    car_type_selection,
) -> InlineKeyboardButton:
    selected = car_type_selection.get(car_type.name, car_type.is_sitting)
    emoji = SELECT_SMILE if selected else NON_SELECT_SMILE
    return InlineKeyboardButton(
        text=f"{emoji} {car_type.display_name}",
        callback_data=f"callback_{car_type.name.lower()}",
    )


async def notification_handler(update: Update, context: CallbackContext):
    """Начало взаимодействия по клику на inline-кнопку 'Уведомление'."""
    next_step = 1
    try:
        if update.callback_query.data != CALLBACK_NOTIFICATION:
            raise ValueError("Unexpected notification callback data")

        user_id = update.callback_query.message.chat.id
        popular_cities = await get_popular_cities_by_user_id(user_id)
        state = reset_notification_state(context, popular_cities=popular_cities)

        await update.callback_query.message.reply_text(
            text="Обратите внимание, по умолчанию не приходят уведомления о "
            "местах для инвалидов. Если вам необходимо получать "
            "уведомления и в таком случае - пожалуйста сообщите об этом"
            "\n\nДля возврата в главное меню введите /stop"
        )

        await update.callback_query.message.reply_text(
            text="Укажите <strong>станцию отправления</strong>.\nНапример:\n\n"
            + format_popular_cities(state.popular_cities),
            parse_mode=ParseMode.HTML,
        )

        return next_step

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def first_step_notification(update: Update, context: CallbackContext):
    """Обрабатывает станцию отправления."""
    next_step = 2
    expected_station_name = update.message.text.upper()
    state = get_notification_state(context)

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        if len(expected_station_name) < MIN_LENGTH_STATION_NAME:
            await update.message.reply_text(text="Введите строку более 1 символа")
            return next_step - 1

        language_input_validation(expected_station_name)

        stations = await station_validate(expected_station_name)

        if len(stations) == 0:
            await update.message.reply_text(
                text="Такой станции на сайте РЖД не котируется.\n"
                "Укажите <strong>станцию отправления</strong>.\nНапример:\n\n"
                + format_popular_cities(state.popular_cities),
                parse_mode=ParseMode.HTML,
            )
            return next_step - 1

        if len(stations) > 1:
            message_text = "По вашему запросу найдено несколько станций:\n"
            for station in stations:
                station_name = station["name"]
                message_text += f"<code>{station_name}</code>\n"
            message_text += (
                "\nПожалуйста укажите название станции в соответствие с предлагаемыми"
            )
            await update.message.reply_text(
                text=message_text,
                parse_mode=ParseMode.HTML,
            )
            return next_step - 1

        station = stations[0]
        state.departure_station = station["name"]
        state.departure_express_code = station["expressCode"]
        await update.message.reply_text(
            text="Укажите <strong>станцию прибытия</strong>.\nНапример:\n\n"
            + format_popular_cities(state.popular_cities),
            parse_mode=ParseMode.HTML,
        )
        return next_step

    except ValueError as e:
        return await base_error_handler(update, e, next_step, MESSAGE_FORMAT_ERROR)

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def second_step_notification(update: Update, context: CallbackContext):
    """Обрабатывает станцию прибытия."""
    next_step = 3
    expected_station_name = update.message.text.upper()
    state = get_notification_state(context)

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        if len(expected_station_name) < MIN_LENGTH_STATION_NAME:
            await update.message.reply_text(text="Введите строку более 1 символа")
            return next_step - 1

        language_input_validation(expected_station_name)

        if expected_station_name == state.departure_station:
            await update.message.reply_text(
                "Станции не могут совпадать.\n"
                "Укажите станцию прибытия.\nНапример:\n\n"
                + format_popular_cities(state.popular_cities),
                parse_mode=ParseMode.HTML,
            )
            return next_step - 1

        stations = await station_validate(expected_station_name)
        if len(stations) == 0:
            await update.message.reply_text(
                text="Такой станции на сайте РЖД не котируется.\n"
                "Укажите станцию прибытия.\nНапример:\n\n"
                + format_popular_cities(state.popular_cities),
                parse_mode=ParseMode.HTML,
            )
            return next_step - 1

        if len(stations) > 1:
            message_text = "По вашему запросу найдено несколько станций:\n"
            for station in stations:
                station_name = station["name"]
                message_text += f"<code>{station_name}</code>\n"
            message_text += (
                "\nПожалуйста укажите название станции в соответствие с предлагаемыми"
            )
            await update.message.reply_text(
                text=message_text,
                parse_mode=ParseMode.HTML,
            )
            return next_step - 1

        station = stations[0]
        state.arrival_station = station["name"]
        state.arrival_express_code = station["expressCode"]
        await update.message.reply_text(
            text=NotificationHandlerDialog.enter_departure_date_text(),
            parse_mode=ParseMode.HTML,
        )
        return next_step

    except ValueError as e:
        return await base_error_handler(update, e, next_step, MESSAGE_FORMAT_ERROR)

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def third_step_notification(update: Update, context: CallbackContext):
    """Обрабатывает дату отправления."""
    next_step = 4
    expected_date = update.message.text
    state = get_notification_state(context)

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        date_and_date_json = date_format_validate(expected_date)

        available_times = await get_available_times(
            state.departure_station,
            state.arrival_station,
            date_and_date_json["date"],
        )
        date_limits = await date_limits_validate(date_and_date_json["date_text"])
        if not available_times or not date_limits:
            await update.message.reply_text(
                "По указанному маршруту на указанную дату билеты не продаются"
            )
            await update.message.reply_text(
                text=NotificationHandlerDialog.enter_departure_date_text(),
                parse_mode=ParseMode.HTML,
            )
            return next_step - 1

        state.departure_date = date_and_date_json["date"]
        state.departure_date_text = date_and_date_json["date_text"]

        await update.message.reply_text(
            text="Укажите <strong>время отправления</strong>.\n"
            "Доступное время для бронирования:\n"
            + "    ".join(f"<code>{time}</code>" for time in available_times),
            parse_mode=ParseMode.HTML,
        )
        return next_step

    except DateFormatError as value_error:
        await update.message.reply_text(text=str(value_error))
        await update.message.reply_text(
            text=NotificationHandlerDialog.enter_departure_date_text(),
            parse_mode=ParseMode.HTML,
        )

        return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def fourth_step_notification(update: Update, context: CallbackContext):
    """Обрабатывает время отправления."""
    next_step = 5
    expected_input_time = update.message.text
    state = get_notification_state(context)

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        available_times = await get_available_times(
            state.departure_station,
            state.arrival_station,
            state.departure_date,
        )
        validate_time = await time_check_validate(
            expected_input_time,
            available_times,
        )

        if not time_format_validate(expected_input_time):
            await update.message.reply_text("Формат времени должен быть hh:mm ")
            await update.message.reply_text(
                text="Укажите <strong>время отправления</strong>.\n"
                "Доступное время для бронирования:\n"
                + "    ".join(f"<code>{time}</code>" for time in available_times),
                parse_mode=ParseMode.HTML,
            )
            return next_step - 1

        if validate_time is True:
            state.departure_time = expected_input_time
            await update.message.reply_text(
                text=MESSAGE_MIN_COUNT_SEATS,
                parse_mode=ParseMode.HTML,
            )
            return next_step

        await update.message.reply_text("Не найдено поездки с таким временем")
        await update.message.reply_text(
            text="Укажите <strong>время отправления</strong>.\n"
            "Доступное время для бронирования:\n"
            + "    ".join(f"<code>{time}</code>" for time in available_times),
            parse_mode=ParseMode.HTML,
        )
        return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def fifth_step_notification(update: Update, context: CallbackContext):
    """Обрабатывает минимальное количество мест для создания уведомления."""
    next_step = 6
    expected_amount_seats = update.message.text
    state = get_notification_state(context)

    try:
        base_check = await null_step_notification(update, context)
        if base_check is not None:
            return base_check

        if expected_amount_seats.isdigit():
            expected_amount_seats = int(expected_amount_seats)

            if MAX_NUMBER_SEATS >= expected_amount_seats > 0:
                state.number_seats = expected_amount_seats
                state.car_type_selection = build_default_car_type_selection()
                await update.message.reply_text(
                    text="Выберите <strong>класс обсуживания</strong> который вас интересует.\n\n"
                    'Обратите внимание, выбирая, например, <strong>"Плац низ"</strong> '
                    "вы выбираете плацкартные нижние места <strong>не включая</strong> боковых.\n\n"
                    "Для боковых мест определен отдельный фильтр.",
                    reply_markup=generate_car_type_markup(state.car_type_selection),
                    parse_mode=ParseMode.HTML,
                )
                return next_step

            await update.message.reply_text(
                text="Ошибка. Вы ввели число более 10 или менее 1. Если вы действительно хотите создать задачу на "
                f"появление более 10 мест одновременно, пожалуйста, обратитесь к администратору бота {ADMIN_USERNAME}"
            )
            await update.message.reply_text(
                text=MESSAGE_MIN_COUNT_SEATS,
                parse_mode=ParseMode.HTML,
            )
            return next_step - 1

        await update.message.reply_text(text="Необходимо ввести цифру")
        await update.message.reply_text(
            text=MESSAGE_MIN_COUNT_SEATS,
            parse_mode=ParseMode.HTML,
        )
        return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def sixth_step_notification(update: Update, context: CallbackContext):
    """Обрабатывает типы вагонов."""
    next_step = 7
    query_data = update.callback_query.data
    state = get_notification_state(context)

    try:
        if query_data == str(CALLBACK_CAR_TYPE_CONTINUE):
            selected_car_types = [
                car_type
                for car_type in CarType
                if state.car_type_selection.get(car_type.name)
            ]

            if not selected_car_types:
                message_warning = "Необходимо выбрать хотя бы 1 тип вагона"
                current_markup = generate_car_type_markup(state.car_type_selection)
                if message_warning not in update.callback_query.message.text:
                    await update.callback_query.edit_message_text(
                        message_warning,
                        reply_markup=current_markup,
                    )
                return next_step - 1

            state.user_id = update.callback_query.message.chat.id
            state.car_types = [
                index
                for index, car_type in enumerate(CarType, start=1)
                if state.car_type_selection.get(car_type.name)
            ]
            car_types_text = ", ".join(
                car_type.display_name for car_type in selected_car_types
            )
            await update.callback_query.edit_message_text(
                text="Пожалуйста, проверьте введенные данные:"
                f"\n\nСтанция отправления: <strong>{state.departure_station}</strong>"
                f"\nСтанция прибытия: <strong>{state.arrival_station}</strong>"
                f"\nДата отправления: <strong>{state.departure_date_text}</strong>"
                f"\nВремя отправления: <strong>{state.departure_time}</strong>"
                f"\nКласс обслуживания: <strong>{car_types_text}</strong>"
                f"\nКоличество необходимых мест: <strong>{state.number_seats}</strong>",
                reply_markup=NOTIFICATION_CONFIRM_INLINE_BUTTONS,
                parse_mode=ParseMode.HTML,
            )
            return next_step

        if query_data == str(CALLBACK_CAR_TYPE_SELECT_ALL):
            state.car_type_selection = {car_type.name: True for car_type in CarType}

        elif query_data == str(CALLBACK_CAR_TYPE_REMOVE_ALL):
            state.car_type_selection = {car_type.name: False for car_type in CarType}

        else:
            for car_type in CarType:
                if query_data == f"callback_{car_type.name.lower()}":
                    current_value = state.car_type_selection.get(
                        car_type.name,
                        car_type.is_sitting,
                    )
                    state.car_type_selection[car_type.name] = not current_value

        current_car_type_markup = update.callback_query.message.reply_markup
        car_type_markup = generate_car_type_markup(state.car_type_selection)

        if car_type_markup.to_dict() != current_car_type_markup.to_dict():
            await update.callback_query.edit_message_reply_markup(car_type_markup)

        return next_step - 1

    except Exception as e:
        return await base_error_handler(update, e, next_step)


async def seventh_step_notification(update: Update, context: CallbackContext):
    """Обрабатывает решение пользователя о создании задачи."""
    query_data = update.callback_query.data
    text_message_html = update.callback_query.message.text_html
    body_text_task = str(text_message_html)[
        str(text_message_html).find("Станция отправления") :
    ]
    next_step = 7
    state = get_notification_state(context)

    try:
        if query_data == str(CALLBACK_DATA_CORRECT_NOTIFICATION):
            notification_data_id = await send_notification_data_to_robot(state)
            update_text_message = (
                MESSAGE_SUCCESS
                + "<strong>"
                + notification_data_id
                + "</strong>\n"
                + body_text_task
            )
            await update.callback_query.edit_message_text(
                update_text_message,
                parse_mode=ParseMode.HTML,
            )

        elif query_data == str(CALLBACK_DATA_INCORRECT_NOTIFICATION):
            update_text_message = MESSAGE_CANCEL + "\n\n" + body_text_task
            await update.callback_query.edit_message_text(
                update_text_message,
                parse_mode=ParseMode.HTML,
            )

        context.user_data.pop(NOTIFICATION_STATE_KEY, None)
        await start_buttons(update, context)
        return ConversationHandler.END

    except Exception as e:
        await base_error_handler(update, e, next_step)
        return next_step


async def send_notification_data_to_robot(state: NotificationConversationState):
    notification_task_data = NotificationTask(
        DepartureStation=state.departure_station,
        ArrivalStation=state.arrival_station,
        DateFrom=state.departure_date,
        TimeFrom=state.departure_time,
        UserId=state.user_id,
        CarTypes=state.car_types,
        NumberSeats=state.number_seats,
    )

    return await create_and_get_id_notification_task(notification_task_data)
