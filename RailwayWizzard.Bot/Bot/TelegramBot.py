from dotenv import load_dotenv
from telegram.error import Forbidden
from telegram.constants import ParseMode
from telegram.ext import CommandHandler, MessageHandler, ConversationHandler, CallbackQueryHandler, Application, filters
from Bot.Handlers.Help import help_handler
from Bot.Handlers.Notification import (notification_handler,
                                       first_step_notification, second_step_notification, third_step_notification,
                                       fourth_step_notification, sixth_step_notification, seventh_step_notification,
                                       fifth_step_notification)
from Bot.Handlers.Start import start_buttons_handler
from Bot.Handlers.ActiveTask import active_task_handler, one_step_active_task
from Bot.Other import unknown_handler
from Bot.Setting import CALLBACK_NOTIFICATION, CALLBACK_ACTIVE_TASK

import os
from logger import logger

# Получаем токен из файла .env в текущей директории
load_dotenv()
token = os.getenv('TOKEN')

application = Application.builder().token(token).build()


# Перенести этот метод в др место не получается, т.к. тут создается application
async def send_message_to_user(user_id, message):
    global application
    try:
        await application.bot.send_message(user_id, message, parse_mode=ParseMode.HTML)
        logger.info("Пользователю " + str(user_id) + " отправлено сообщение:\n" + str(message))
    except Forbidden as eF:
        logger.warning("Пользователь " + str(user_id) + " заблокировал бота\n" + eF.message)
        raise eF
    except Exception as e:
        logger.error(e)
        raise
  

def run():
    logger.info("Bot started...")
    try:
        application.add_handler(conv_handler_notification)
        application.add_handler(conv_handler_active_task)
        application.add_handler(CommandHandler('start', start_buttons_handler))
        application.add_handler(CommandHandler('help', help_handler))
        application.add_handler(MessageHandler(filters.TEXT
                                               | filters.COMMAND & ~filters.Regex('/start')
                                               & ~filters.Regex('/help'),
                                               unknown_handler))  # обрабатываем неизвестные команды
        application.run_polling()

    except Exception as e:
        logger.error(e)


conv_handler_notification = ConversationHandler(
    # Точка входа в диалог.
    entry_points=[CallbackQueryHandler(notification_handler, pattern=str(CALLBACK_NOTIFICATION))],

    # Словарь состояний внутри диалога.
    states={
        1: [MessageHandler(filters.TEXT, first_step_notification)],
        2: [MessageHandler(filters.TEXT, second_step_notification)],
        3: [MessageHandler(filters.TEXT, third_step_notification)],
        4: [MessageHandler(filters.TEXT, fourth_step_notification)],
        5: [MessageHandler(filters.TEXT, fifth_step_notification)],
        6: [CallbackQueryHandler(sixth_step_notification)],
        7: [CallbackQueryHandler(seventh_step_notification)]
    },
    fallbacks=[CommandHandler('start', start_buttons_handler)],  # Точка выхода из диалога - команда /start?
    # '''CommandHandler('stop', stop)'''#Оно не работает( Вернул проверку на стоп
    allow_reentry=True
)

conv_handler_active_task = ConversationHandler(
    # Точка входа в диалог.
    entry_points=[CallbackQueryHandler(active_task_handler, pattern=str(CALLBACK_ACTIVE_TASK))],

    # Словарь состояний внутри диалога.
    states={
        1: [CallbackQueryHandler(one_step_active_task)]
    },
    fallbacks=[CommandHandler('start', start_buttons_handler)],  # Точка выхода из диалога - команда /start?
    # '''CommandHandler('stop', stop)'''#Оно не работает( Вернул проверку на стоп
    allow_reentry=True
)
