from fastapi import FastAPI, Body
from fastapi.responses import HTMLResponse
import uvicorn
from Bot.TelegramBot import send_message_to_user
from logger import logger

app = FastAPI()


@app.post("/api/sendMessageForUser")
async def send_message(data=Body()):
    if data is not None:
        user_id = data["userId"]
        message = data["message"]
        await send_message_to_user(user_id, message)
    else:
        logger.info("Ошибка получения данных")


async def run():
    config = uvicorn.Config("server:app", host="0.0.0.0", port=5000, log_level="info", loop="none")
    server = uvicorn.Server(config)
    await server.serve()
