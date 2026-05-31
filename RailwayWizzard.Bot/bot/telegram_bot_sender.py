import asyncio
from telegram.constants import ParseMode

from bot.telegram_bot import application, get_bot_loop
from logger import logger


async def _send_message_to_user(user_id, message):
    await application.bot.send_message(
        chat_id=user_id,
        text=message,
        parse_mode=ParseMode.HTML,
    )


async def send_message_to_user(user_id, message):
    bot_loop = get_bot_loop()
    try:
        current_loop = asyncio.get_running_loop()
    except RuntimeError:
        current_loop = None

    if current_loop is bot_loop:
        await _send_message_to_user(user_id, message)
    else:
        future = asyncio.run_coroutine_threadsafe(
            _send_message_to_user(user_id, message),
            bot_loop,
        )
        await asyncio.wrap_future(future)

    logger.info(
        "Message sent to Telegram user",
        extra={
            "component": "telegram_sender",
            "user_id": user_id,
        },
    )
