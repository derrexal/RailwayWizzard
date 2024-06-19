from fastapi import FastAPI, Body, HTTPException
from fastapi.responses import HTMLResponse
import uvicorn
from Bot.TelegramBot import send_message_to_user
from logger import logger

app = FastAPI()


@app.post("/api/sendMessageForUser")
async def send_message(data=Body()):
    if data is None:
        logger.error("Ошибка получения данных")
        raise HTTPException(status_code=400,detail="Not Value")
    await send_message_to_user(data["userId"], data["message"])


async def run():
    config = uvicorn.Config("server:app", host="0.0.0.0", port=5000, log_level="info", loop="none")
    server = uvicorn.Server(config)
    await server.serve()
