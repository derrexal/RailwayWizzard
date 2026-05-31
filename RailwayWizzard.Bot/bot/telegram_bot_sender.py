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

    future = asyncio.run_coroutine_threadsafe(
        _send_message_to_user(user_id, message),
        bot_loop,
    )

    await asyncio.wrap_future(future)

    logger.info(
        "Пользователю %s отправлено следующее сообщение:\n%s",
        user_id,
        message,
    )