import asyncio

from telegram.ext import (
    Application,
    CallbackQueryHandler,
    CommandHandler,
    ConversationHandler,
    MessageHandler,
    filters,
)

from app_config import settings
from bot.data.NotificationConversationState import (
    NOTIFICATION_STATE_KEY,
    NotificationConversationState,
)
from bot.handlers.active_task_handler.active_task_handler import (
    active_task_handler,
    one_step_active_task,
)
from bot.handlers.donate_handler.donate_handler import donate_handler
from bot.handlers.help_handler.help_handler import help_handler
from bot.handlers.notification_handler.notification_handler import (
    fifth_step_notification,
    first_step_notification,
    fourth_step_notification,
    notification_handler,
    second_step_notification,
    seventh_step_notification,
    sixth_step_notification,
    third_step_notification,
)
from bot.handlers.start_handler.start_handler import start_buttons_handler
from bot.handlers.unknown_handler.unknown_handler import unknown_handler
from bot.setting import CALLBACK_ACTIVE_TASK, CALLBACK_NOTIFICATION
from logger import logger


token = settings.token
if not token:
    raise RuntimeError("Environment variable TOKEN is not set")


_bot_loop = None
_handlers_are_set = False


NOTIFICATION_MESSAGE_HANDLERS = {
    1: first_step_notification,
    2: second_step_notification,
    3: third_step_notification,
    4: fourth_step_notification,
    5: fifth_step_notification,
}

NOTIFICATION_CALLBACK_HANDLERS = {
    6: sixth_step_notification,
    7: seventh_step_notification,
}


def get_bot_loop():
    if _bot_loop is None:
        raise RuntimeError("Telegram bot loop is not initialized yet")

    return _bot_loop


application = (
    Application.builder()
    .read_timeout(15)
    .token(token)
    .build()
)


def _get_notification_state(context) -> NotificationConversationState:
    state = context.user_data.get(NOTIFICATION_STATE_KEY)
    if isinstance(state, NotificationConversationState):
        return state
    return None


def _apply_notification_stage(context, result):
    state = _get_notification_state(context)
    if state is None:
        return

    if result == ConversationHandler.END:
        context.user_data.pop(NOTIFICATION_STATE_KEY, None)
        return

    if isinstance(result, int):
        state.stage = result


async def callback_query_router(update, context):
    query_data = update.callback_query.data

    if query_data == str(CALLBACK_NOTIFICATION):
        result = await notification_handler(update, context)
        _apply_notification_stage(context, result)
        return

    if query_data == str(CALLBACK_ACTIVE_TASK):
        await active_task_handler(update, context)
        return

    if query_data.startswith("active_task_callback"):
        await one_step_active_task(update, context)
        return

    state = _get_notification_state(context)
    if state and state.stage in NOTIFICATION_CALLBACK_HANDLERS:
        result = await NOTIFICATION_CALLBACK_HANDLERS[state.stage](update, context)
        _apply_notification_stage(context, result)


async def text_message_router(update, context):
    state = _get_notification_state(context)
    if state and state.stage in NOTIFICATION_MESSAGE_HANDLERS:
        result = await NOTIFICATION_MESSAGE_HANDLERS[state.stage](update, context)
        _apply_notification_stage(context, result)
        return

    await unknown_handler(update, context)


def setup_handlers():
    global _handlers_are_set

    if _handlers_are_set:
        return

    application.add_handler(CommandHandler("start", start_buttons_handler))
    application.add_handler(CommandHandler("help", help_handler))
    application.add_handler(CommandHandler("donate", donate_handler))
    application.add_handler(CallbackQueryHandler(callback_query_router))
    application.add_handler(
        MessageHandler(filters.TEXT & ~filters.COMMAND, text_message_router)
    )
    application.add_handler(
        MessageHandler(
            filters.COMMAND
            & ~filters.Regex("/start")
            & ~filters.Regex("/help")
            & ~filters.Regex("/donate"),
            unknown_handler,
        )
    )

    _handlers_are_set = True


async def start():
    global _bot_loop

    setup_handlers()
    _bot_loop = asyncio.get_running_loop()
    logger.info("Starting Telegram bot polling")
    await application.initialize()
    await application.bot.delete_webhook(drop_pending_updates=True)
    bot_info = await application.bot.get_me()
    logger.info("Telegram bot authorized: @%s", bot_info.username)
    await application.updater.start_polling(drop_pending_updates=True)
    await application.start()


async def stop():
    if application.updater is not None and getattr(application.updater, "running", False):
        await application.updater.stop()
    if getattr(application, "running", False):
        await application.stop()
    await application.shutdown()
    if application.updater is not None:
        await application.updater.shutdown()


def run():
    global _bot_loop

    setup_handlers()
    _bot_loop = asyncio.get_event_loop()
    logger.info("Starting Telegram bot polling")
    application.run_polling(drop_pending_updates=True)
