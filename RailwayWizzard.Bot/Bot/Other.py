from Bot.Handlers.Start import *


def log_user_message(update, context):
    answer = update.message.text
    idTg = update.message.from_user['id']
    username = update.message.from_user['username']
    print('User: {} ID: {} Message:{}'.format(username, idTg, answer))


async def check_stop(update, context):
    answer = update.message.text

    if answer == '/stop':
        await stop(update, context)
        return True
    return False


async def stop(update, context):
    await update.message.reply_text("Диалог прерван\n")
    await start_buttons_handler(update, context)


async def unknown_handler(update, context):
    await update.message.reply_text('Неизвестная команда')
    log_user_message(update, context)