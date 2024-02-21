# from telegram.ext import ConversationHandler
#
# from Bot.Setting import CALLBACK_RESERVATION, message_error
#
#
# def reservation_handler(update, context):
#     """ Начало взаимодействия по клику на inline-кнопку 'Авто бронирование' """
#     init = update.callback_query.data
#     chat_id = update.callback_query.message.chat.id
#     try:
#         if init != CALLBACK_RESERVATION:
#             update.callback_query.bot.send_message( #Оставить такой вариант отправки сообщения, или тот который в except? Какой из них сработает? Если оба - оставить который в Exception
#                 chat_id=chat_id,
#                 text='Что-то пошло не так, обратитесь к администратору бота')
#             return ConversationHandler.END
#         # Проверяем, заполнены ли персональные данные пользователя
#         # Спрашивать информацию о поездке(В принципе - та же схема, как и в сценарии настройки уведомления)
#         # Отправляем кипу данных роботу. Или записываем в базу - а оттуда он сам возьмет)
#         # Или ему пришлет шина - если остановимся на шине)
#         # Все. Осталось дождаться ответа от робота.
#     except Exception as e:
#         print(e)
#         update.message.reply_text(message_error)
#         raise
