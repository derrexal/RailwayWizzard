from fastapi import FastAPI, Body, HTTPException
import uvicorn
from telegram.error import Forbidden

from bot.telegram_bot_sender import send_message_to_user
from logger import logger


app = FastAPI()


def run():
    uvicorn.run(
        app,
        host="0.0.0.0",
        port=5000,
        log_level="info",
    )


@app.post("/api/sendMessageForUser")
async def send_message(data=Body()):
    if data is None:
        error_message = "Incorrect input parameters. Url: /api/sendMessageForUser"
        logger.error(error_message)
        raise HTTPException(status_code=400, detail=error_message)

    try:
        user_id = data["telegramUserId"]
        message = data["message"]
    except KeyError as e:
        error_message = f"Missing required field: {e}"
        logger.error(error_message)
        raise HTTPException(status_code=400, detail=error_message)

    try:
        await send_message_to_user(user_id, message)

    except Forbidden as e:
        error_message = f"User {user_id} has blocked bot. Details: {e.message}"
        logger.warning(error_message)
        raise HTTPException(status_code=409, detail=error_message)

    except Exception as e:
        error_message = (
            f"В ходе отправки сообщения {message} пользователю {user_id} "
            f"возникла следующая ошибка:\n{e}"
        )
        logger.exception(error_message)
        raise HTTPException(status_code=500, detail=error_message)

    return {"detail": f"Success send message {message} to user {user_id}"}