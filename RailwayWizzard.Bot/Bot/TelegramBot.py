from telegram.constants import ParseMode
from telegram.ext import CommandHandler, MessageHandler, ConversationHandler, CallbackQueryHandler, Application, filters
from Bot.Handlers.Help import help_handler
from Bot.Handlers.Notification import notification_handler, first_step_notification, second_step_notification, \
    third_step_notification, fourth_step_notification, fifth_step_notification
from Bot.Handlers.Start import start_buttons_handler
from Bot.Other import unknown_handler
from Bot.Setting import CALLBACK_NOTIFICATION

import os

#Получаем токен из файла .env в текущей директории
load_dotenv()
token = os.getenv('TOKEN')

application = Application.builder().token(token).build()

#Перенести этот метод в др место не получается, т.к. тут создается application
async def send_message_to_user(user_id, message):
    global application
    await application.bot.send_message(user_id, message, parse_mode=ParseMode.HTML)
    print("Пользователю " + str(user_id) + " отправлено сообщение:\n" + str(message))


def run():
    print("INFO:        Bot started")
    try:
        application.add_handler(conv_handler_notification)
        application.add_handler(CommandHandler('start', start_buttons_handler))
        application.add_handler(CommandHandler('help', help_handler))
        application.add_handler(MessageHandler(filters.TEXT
                                      | filters.COMMAND & ~filters.Regex('/start')
                                      & ~filters.Regex('/help'), unknown_handler))  # обрабатываем неизвестные команды
        application.run_polling()

    except Exception as e:
        print(e)
        return None


# Описываем обработчики событий
conv_handler_notification = ConversationHandler(
    # Точка входа в диалог.
    entry_points=[CallbackQueryHandler(notification_handler, pattern=str(CALLBACK_NOTIFICATION))],

    # Словарь состояний внутри диалога.
    states={
        1: [MessageHandler(filters.TEXT, first_step_notification)],
        2: [MessageHandler(filters.TEXT, second_step_notification)],
        3: [MessageHandler(filters.TEXT, third_step_notification)],
        4: [MessageHandler(filters.TEXT, fourth_step_notification)],
        5: [CallbackQueryHandler(fifth_step_notification)]
    },
    fallbacks=[CommandHandler('start', start_buttons_handler)],  # '''CommandHandler('stop', stop)'''#Оно не работает( Вернул проверку на стоп
    allow_reentry=True
)
