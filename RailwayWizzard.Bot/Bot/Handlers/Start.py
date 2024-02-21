from Bot.API import *
from telegram import *
from telegram.ext import ConversationHandler

from Bot.Setting import CALLBACK_NOTIFICATION, message_error


async def start_buttons_handler(update, context):
    """ Создаёт начальные inline-кнопки """
    inline_buttons = InlineKeyboardMarkup(
        inline_keyboard=[[InlineKeyboardButton(
                    text='\U00002709 Уведомление об появлении мест',
                    callback_data=str(CALLBACK_NOTIFICATION))]])
    try:
        await update.message.reply_text(
            '\U0001F441 Добро пожаловать на борт',
            reply_markup=inline_buttons)

        await add_user(
            update.message.from_user['id'],
            update.message.from_user['username'])

    except Exception as e:
        print(e)
        await update.message.reply_text(message_error)
        return ConversationHandler.END


async def start_buttons(update, context):
    """ Создаёт начальные inline-кнопки """
    chat_id = update.callback_query.message.chat.id

    inline_buttons = InlineKeyboardMarkup(
        inline_keyboard=[
            [
                InlineKeyboardButton(
                    text='\U00002709 Уведомление об появлении мест',
                    callback_data=str(CALLBACK_NOTIFICATION))
            ]])
    try:
        await context.bot.send_message(
            chat_id=chat_id,
            text='\U0001F441 Добро пожаловать на борт',
            reply_markup=inline_buttons
        )
        await add_user(
            update.callback_query.message.chat.id,
            update.callback_query.message.chat.username)

    except Exception as e:
        print(e)
        await update.message.reply_text(message_error)
        return ConversationHandler.END


'''
                InlineKeyboardButton(
                    text='\U0001F9BE Авто бронирование мест',
                    callback_data=CALLBACK_RESERVATION)
'''
