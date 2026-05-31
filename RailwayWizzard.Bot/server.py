from fastapi import FastAPI, Header, HTTPException, status
from fastapi import Request
from fastapi.responses import Response
from pydantic import BaseModel
import uvicorn
from telegram.error import Forbidden
from uuid import uuid4

from app_config import settings
from bot.telegram_bot_sender import send_message_to_user
from logger import logger, reset_request_id, set_request_id


app = FastAPI()


class SendMessageRequest(BaseModel):
    telegramUserId: int
    message: str


@app.middleware("http")
async def request_context_middleware(request: Request, call_next):
    request_id = request.headers.get("X-Request-Id") or str(uuid4())
    token = set_request_id(request_id)
    logger.info(
        "HTTP request started",
        extra={
            "component": "http",
            "request_id": request_id,
            "http_method": request.method,
            "path": request.url.path,
        },
    )

    response: Response = Response("Internal Server Error", status_code=500)
    try:
        response = await call_next(request)
        return response
    finally:
        response.headers["X-Request-Id"] = request_id
        logger.info(
            "HTTP request completed",
            extra={
                "component": "http",
                "request_id": request_id,
                "http_method": request.method,
                "path": request.url.path,
                "status_code": response.status_code,
            },
        )
        reset_request_id(token)


def create_server():
    config = uvicorn.Config(
        app,
        host=settings.server_host,
        port=settings.server_port,
        log_level="info",
        log_config=None,
    )
    return uvicorn.Server(config)


@app.post("/api/sendMessageForUser")
async def send_message(
    data: SendMessageRequest,
    x_api_token: str = Header(default="", alias="X-Api-Token"),
):
    if settings.send_message_auth_token and x_api_token != settings.send_message_auth_token:
        logger.warning(
            "Rejected request with invalid API token",
            extra={
                "component": "http",
                "endpoint": "/api/sendMessageForUser",
                "status_code": status.HTTP_401_UNAUTHORIZED,
            },
        )
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid API token",
        )

    user_id = data.telegramUserId
    message = data.message

    try:
        await send_message_to_user(user_id, message)
        logger.info(
            "Handled send message request",
            extra={
                "component": "http",
                "endpoint": "/api/sendMessageForUser",
                "user_id": user_id,
                "status_code": status.HTTP_200_OK,
            },
        )

    except Forbidden as e:
        error_message = f"User {user_id} has blocked bot. Details: {e.message}"
        logger.warning(
            error_message,
            extra={
                "component": "http",
                "endpoint": "/api/sendMessageForUser",
                "user_id": user_id,
                "status_code": status.HTTP_409_CONFLICT,
            },
        )
        raise HTTPException(status_code=409, detail=error_message)

    except Exception as e:
        error_message = (
            f"В ходе отправки сообщения {message} пользователю {user_id} "
            f"возникла следующая ошибка:\n{e}"
        )
        logger.exception(
            error_message,
            extra={
                "component": "http",
                "endpoint": "/api/sendMessageForUser",
                "user_id": user_id,
                "status_code": status.HTTP_500_INTERNAL_SERVER_ERROR,
            },
        )
        raise HTTPException(status_code=500, detail=error_message)

    return {"detail": f"Success send message {message} to user {user_id}"}
