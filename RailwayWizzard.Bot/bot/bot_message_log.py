from logger import logger


def log_user_message(update):
    answer = update.message.text
    id_tg = update.message.from_user['id']
    username = update.message.from_user['username']

    logger.info(
        "Received Telegram user message",
        extra={
            "component": "telegram_bot",
            "user_id": id_tg,
            "telegram_username": username,
        },
    )
