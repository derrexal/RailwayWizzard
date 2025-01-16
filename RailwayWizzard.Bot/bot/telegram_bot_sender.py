from telegram.error import Forbidden
from telegram.constants import ParseMode

from bot.telegram_bot import application
from logger import logger


async def send_message_to_user(user_id, message):

    try:
        await application.bot.send_message(user_id, message, parse_mode=ParseMode.HTML)
        logger.info("Пользователю " + str(user_id) + " отправлено сообщение:\n" + str(message))

    except Forbidden as eF:
        logger.warning("Пользователь " + str(user_id) + " заблокировал бота\n" + eF.message)
        raise eF

    except Exception as e:
        logger.exception("В ходе отправки сообщения пользователю возникла ошибка:")
        logger.exception(e)
        raise
