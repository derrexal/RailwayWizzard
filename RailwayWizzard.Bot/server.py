from fastapi import FastAPI, Body, HTTPException
import uvicorn
from telegram.error import Forbidden

from bot.telegram_bot_sender import send_message_to_user
from logger import logger

# Сервер для получения и обработки запросов от других сервисов приложения
app = FastAPI()


async def run():
    config = uvicorn.Config("server:app", host="0.0.0.0", port=5000, log_level="info", loop="none")
    server = uvicorn.Server(config)
    await server.serve()


@app.post("/api/sendMessageForUser")
async def send_message(data=Body()):
    if data is None:
        error_message = "Incorrect input parameters. Url: /api/sendMessageForUser"
        logger.exception(error_message)
        raise HTTPException(status_code=400, detail=error_message)

    user_id = data["telegramUserId"]
    message = data["message"]

    try:
        await send_message_to_user(user_id, message)

    except Forbidden as eF:
        error_message = f'User {user_id} has blocked bot. Details: \n {eF.message}'
        logger.warning(error_message)
        raise HTTPException(status_code=409, detail=error_message)

    except Exception as e:
        error_message = f'В ходе отправки сообщения {message} пользователю {user_id} возникла следующая ошибка:\n{e}'
        logger.exception(error_message)
        raise HTTPException(status_code=500, detail=error_message)

    return {"detail": f"Success send message {message} to user {user_id}"}
