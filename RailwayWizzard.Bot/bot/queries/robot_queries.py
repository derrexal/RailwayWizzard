import asyncio
from dataclasses import dataclass
from typing import Optional

import aiohttp

from app_config import settings
from logger import logger
from bot.data.NotificationTask import NotificationTask

API_URL = settings.api_url
REQUEST_TIMEOUT = aiohttp.ClientTimeout(total=settings.backend_timeout_seconds)


@dataclass
class BackendApiError(Exception):
    endpoint: str
    message: str
    status_code: Optional[int] = None
    response_text: Optional[str] = None

    def __str__(self) -> str:
        status = f" status={self.status_code}" if self.status_code is not None else ""
        return f"{self.endpoint}{status}: {self.message}"


async def make_request(
    method,
    endpoint,
    json_data=None,
    params=None,
    *,
    allow_not_found: bool = False,
    response_type: str = "json",
):
    """
    Базовый метод для выполнения асинхронного HTTP-запроса.
    """
    url = f"{API_URL.rstrip('/')}/{endpoint.lstrip('/')}"
    try:
        async with aiohttp.ClientSession(timeout=REQUEST_TIMEOUT) as session:
            async with session.request(
                method,
                url,
                json=json_data,
                params=params,
            ) as response:
                response_text = await response.text()
                logger.info("%s %s %s", endpoint, response.status, response_text)

                if allow_not_found and response.status == 404:
                    return None

                if response.status >= 400:
                    raise BackendApiError(
                        endpoint=endpoint,
                        message="Backend returned an error response",
                        status_code=response.status,
                        response_text=response_text,
                    )

                if response_type == "none":
                    return None
                if response_type == "text":
                    return response_text
                if response_type == "json":
                    return await response.json()
                raise ValueError(f"Invalid response_type {response_type}")
    except BackendApiError:
        logger.exception("Backend API error in %s", endpoint)
        raise
    except (aiohttp.ClientError, asyncio.TimeoutError) as e:
        logger.exception("Transport error in %s: %s", endpoint, e)
        raise BackendApiError(
            endpoint=endpoint,
            message=str(e),
        ) from e


async def station_validate(stationName) -> list:
    """  """
    endpoint = 'B2B/GetStationValidate'
    params = {'stationName': stationName}
    return await make_request('GET', endpoint, params=params, response_type="json")


async def get_available_times(station_from_name, station_to_name, date):
    """  """
    endpoint = 'B2B/GetAvailableTimes'
    json_data = {
        'DepartureStationName': station_from_name,
        'ArrivalStationName': station_to_name,
        'DepartureDate': date
    }
    return await make_request('GET', endpoint, json_data=json_data, response_type="json")


async def create_user(id_tg, username):
    """ Добавляет пользователя в БД """
    endpoint = 'User/CreateOrUpdate'
    json_data = {'IdTg': id_tg, 'Username': username}
    await make_request('POST', endpoint, json_data=json_data, response_type="none")


async def create_user_safe(id_tg, username) -> bool:
    """Пытается зарегистрировать пользователя, не роняя базовые команды бота."""
    try:
        await create_user(id_tg, username)
        return True
    except Exception:
        logger.warning(
            "Could not create or update user %s via %s. Bot will continue without backend sync.",
            id_tg,
            API_URL,
        )
        return False


async def create_and_get_id_notification_task(notification_task_data: NotificationTask):
    """Создает задачу"""
    endpoint = 'NotificationTask/Create'
    json_data = notification_task_data.__dict__  # Преобразование объекта в словарь
    logger.info(json_data)
    return await make_request('POST', endpoint, json_data=json_data, response_type="text")


async def get_active_task_by_user_id(user_id):
    """Возвращает активные задачи конкретного пользователя"""
    endpoint = 'NotificationTask/GetActiveByUser'
    params = {'telegramUserId': user_id}
    return await make_request(
        'GET',
        endpoint,
        params=params,
        allow_not_found=True,
        response_type="json",
    )


async def delete_task_by_id(task_id):
    """Останавливает (Устанавливает статус Остановлен) задачу по ее ID"""
    endpoint = 'NotificationTask/SetIsStopped'
    params = {'notificationTaskId': task_id}
    return await make_request('GET', endpoint, params=params, response_type="json")


async def get_popular_cities_by_user_id(user_id) -> list:
    """Возвращает популярные города конкретного пользователя"""
    endpoint = 'NotificationTask/GetPopularCities'
    params = {'telegramUserId': user_id}
    response = await make_request(
        'GET',
        endpoint,
        params=params,
        allow_not_found=True,
        response_type="json",
    )
    if response is None:
        raise BackendApiError(
            endpoint=endpoint,
            message="Popular cities were not found for the user",
            status_code=404,
        )
    return response
