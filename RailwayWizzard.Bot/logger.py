import json
import logging
from contextvars import ContextVar
from datetime import datetime, timezone

from nanoid import generate

from app_config import settings


request_id_var: ContextVar[str] = ContextVar("request_id", default="")


def set_request_id(request_id: str):
    return request_id_var.set(request_id)


def reset_request_id(token):
    request_id_var.reset(token)


class JsonFormatter(logging.Formatter):
    def format(self, record: logging.LogRecord) -> str:
        payload = {
            "timestamp": datetime.fromtimestamp(
                record.created,
                tz=timezone.utc,
            ).isoformat(),
            "service": settings.service_name,
            "environment": settings.app_environment,
            "level": record.levelname,
            "logger": record.name,
            "message": record.getMessage(),
        }

        request_id = getattr(record, "request_id", None) or request_id_var.get()
        if request_id:
            payload["request_id"] = request_id

        for field_name in (
            "endpoint",
            "user_id",
            "http_method",
            "path",
            "status_code",
            "component",
            "telegram_username",
        ):
            value = getattr(record, field_name, None)
            if value is not None:
                payload[field_name] = value

        if record.exc_info:
            exc_type, exc_value, _ = record.exc_info
            payload["exception"] = {
                "type": exc_type.__name__ if exc_type else None,
                "message": str(exc_value) if exc_value else None,
            }

        return json.dumps(payload, ensure_ascii=False)


logger = logging.getLogger("app")


def setup_logging():
    root_logger = logging.getLogger()
    root_logger.setLevel(logging.INFO)

    handler = logging.StreamHandler()
    handler.setFormatter(JsonFormatter())

    root_logger.handlers.clear()
    root_logger.addHandler(handler)

    for logger_name in ("uvicorn", "uvicorn.error", "uvicorn.access"):
        named_logger = logging.getLogger(logger_name)
        named_logger.handlers.clear()
        named_logger.propagate = True
        named_logger.setLevel(logging.INFO)

    app_logger = logging.getLogger("app")
    app_logger.handlers.clear()
    app_logger.propagate = True
    app_logger.setLevel(logging.INFO)



def get_unique_uuid_error() -> str:
    """ Генерирует UUID в формате строки"""
    number_alphabet = '0123456789'
    return str(generate(number_alphabet, 4))


setup_logging()
