import logging
from nanoid import generate

logger = logging.getLogger()


def set_logger(__name__):
    """ Устанавливает настройки логирования """
    global logger

    logger = logging.getLogger(__name__)
    logger.setLevel(logging.DEBUG)
    ch = logging.StreamHandler()
    formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
    ch.setFormatter(formatter)
    logger.addHandler(ch)


def get_unique_uuid_error() -> str:
    """ Генерирует UUID в формате строки"""
    number_alphabet = '0123456789'
    return str(generate(number_alphabet, 4))


set_logger(__name__)
