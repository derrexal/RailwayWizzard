from fastapi import FastAPI, Body, HTTPException
import uvicorn
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
        error_message = "Ошибка валидации входящего запроса по адресу /api/sendMessageForUser"
        logger.exception(error_message)
        raise HTTPException(status_code=400, detail=error_message)
    await send_message_to_user(data["userId"], data["message"])
