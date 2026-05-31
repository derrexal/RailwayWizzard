import os
import asyncio
import warnings
from dotenv import load_dotenv
from telegram.ext import (
    CommandHandler,
    MessageHandler,
    ConversationHandler,
    CallbackQueryHandler,
    Application,
    filters,
)

from bot.handlers.active_task_handler.active_task_handler import (
    active_task_handler,
    one_step_active_task,
)
from bot.handlers.donate_handler.donate_handler import donate_handler
from bot.handlers.help_handler.help_handler import help_handler
from bot.handlers.notification_handler.notification_handler import (
    notification_handler,
    first_step_notification,
    second_step_notification,
    third_step_notification,
    fourth_step_notification,
    fifth_step_notification,
    sixth_step_notification,
    seventh_step_notification,
)
from bot.handlers.start_handler.start_handler import start_buttons_handler
from bot.handlers.unknown_handler.unknown_handler import unknown_handler
from bot.setting import CALLBACK_NOTIFICATION, CALLBACK_ACTIVE_TASK
from logger import logger

warnings.filterwarnings(
    "ignore",
    message=r"If 'per_message=.*",
    category=UserWarning,
)


token = os.getenv("TOKEN")
if not token:
    load_dotenv()
    token = os.getenv("TOKEN")

if not token:
    raise RuntimeError("Environment variable TOKEN is not set")

token = token.strip()
if len(token) >= 2 and token[0] == token[-1] and token[0] in {"'", '"'}:
    token = token[1:-1].strip()


_bot_loop = None
_handlers_are_set = False


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


conv_handler_notification = ConversationHandler(
    entry_points=[
        CallbackQueryHandler(notification_handler, pattern=str(CALLBACK_NOTIFICATION))
    ],
    states={
        1: [MessageHandler(filters.TEXT, first_step_notification)],
        2: [MessageHandler(filters.TEXT, second_step_notification)],
        3: [MessageHandler(filters.TEXT, third_step_notification)],
        4: [MessageHandler(filters.TEXT, fourth_step_notification)],
        5: [MessageHandler(filters.TEXT, fifth_step_notification)],
        6: [CallbackQueryHandler(sixth_step_notification)],
        7: [CallbackQueryHandler(seventh_step_notification)],
    },
    fallbacks=[CommandHandler("start", start_buttons_handler)],
    allow_reentry=True,
)

conv_handler_active_task = ConversationHandler(
    entry_points=[
        CallbackQueryHandler(active_task_handler, pattern=str(CALLBACK_ACTIVE_TASK))
    ],
    states={
        1: [CallbackQueryHandler(one_step_active_task)],
    },
    fallbacks=[CommandHandler("start", start_buttons_handler)],
    allow_reentry=True,
)


def setup_handlers():
    global _handlers_are_set

    if _handlers_are_set:
        return

    application.add_handler(CommandHandler("start", start_buttons_handler))
    application.add_handler(CommandHandler("help", help_handler))
    application.add_handler(CommandHandler("donate", donate_handler))

    application.add_handler(conv_handler_notification)
    application.add_handler(conv_handler_active_task)

    application.add_handler(
        MessageHandler(
            (filters.TEXT | filters.COMMAND)
            & ~filters.Regex("/start")
            & ~filters.Regex("/help"),
            unknown_handler,
        )
    )

    _handlers_are_set = True


def run():
    global _bot_loop

    setup_handlers()
    # `python-telegram-bot==20.0a0` does not support `ApplicationBuilder.post_init`.
    # Reuse the loop that `run_polling` will run on so cross-thread sends keep working.
    _bot_loop = asyncio.get_event_loop()
    logger.info("Starting Telegram bot polling")
    application.run_polling(drop_pending_updates=True)
