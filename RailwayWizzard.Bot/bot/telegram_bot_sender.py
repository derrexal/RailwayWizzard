from telegram.constants import ParseMode
from telegram.error import NetworkError

from bot.telegram_bot import application
from logger import logger


async def send_message_to_user(user_id, message):
    """ Метод отправки сообщения пользователю. """
    try:
        await application.bot.send_message(user_id, message, parse_mode=ParseMode.HTML)
        logger.info(f'Пользователю {user_id} отправлено следующее сообщение: \n {message}')
    except NetworkError as ne:
        logger.error(f"Network error sending message: {ne}")