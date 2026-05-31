import os
from dataclasses import dataclass
from typing import Optional

from dotenv import load_dotenv


load_dotenv()


def _get_env(
    name: str,
    default: Optional[str] = None,
    *,
    strip_quotes: bool = False,
) -> Optional[str]:
    value = os.getenv(name, default)
    if value is None:
        return None

    value = value.strip()
    if strip_quotes and len(value) >= 2 and value[0] == value[-1] and value[0] in {"'", '"'}:
        value = value[1:-1].strip()
    return value


@dataclass(frozen=True)
class AppConfig:
    service_name: str
    app_environment: str
    token: Optional[str]
    api_url: str
    admin_username: str
    donate_url: str
    server_host: str
    server_port: int
    backend_timeout_seconds: float
    send_message_auth_token: Optional[str]


settings = AppConfig(
    service_name=_get_env("SERVICE_NAME", "railwaywizzard-bot") or "railwaywizzard-bot",
    app_environment=_get_env("APP_ENV", "development") or "development",
    token=_get_env("TOKEN", strip_quotes=True),
    api_url=_get_env("API_URL", "http://railwaywizzardapp:80") or "http://railwaywizzardapp:80",
    admin_username=_get_env("ADMIN_USERNAME", "@derrexal") or "@derrexal",
    donate_url=_get_env("DONATE_URL", "https://pay.cloudtips.ru/p/c6b746f7") or "https://pay.cloudtips.ru/p/c6b746f7",
    server_host=_get_env("SERVER_HOST", "0.0.0.0") or "0.0.0.0",
    server_port=int(_get_env("SERVER_PORT", "5000") or "5000"),
    backend_timeout_seconds=float(_get_env("BACKEND_TIMEOUT_SECONDS", "15") or "15"),
    send_message_auth_token=_get_env("SEND_MESSAGE_AUTH_TOKEN"),
)
