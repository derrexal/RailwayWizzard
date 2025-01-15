from logger import logger


def log_user_message(update):
    answer = update.message.text
    id_tg = update.message.from_user['id']
    username = update.message.from_user['username']

    logger.info('User: {} ID: {} Message: {}'.format(username, id_tg, answer))
