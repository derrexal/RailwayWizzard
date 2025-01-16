from typing import Any

import aiohttp
from aiohttp import ClientResponse

from logger import logger
from bot.data.NotificationTask import NotificationTask

API_URL = "http://railwaywizzardapp:80"


async def make_request(method, endpoint, json_data=None, params=None) -> ClientResponse:
    """
    Базовый метод для выполнения асинхронного HTTP-запроса.

    Args:
        method (str): HTTP метод запроса ('GET' или 'POST').
        endpoint (str): Конечная точка API (относительный URL).
        json_data (dict, optional): Данные для отправки в теле запроса в формате JSON. По умолчанию None.
        params (dict, optional): Параметры для отправки в URL запроса. По умолчанию None.

    Returns:
        aiohttp.ClientResponse: Ответ от сервера, если запрос был успешным.

    Raises:
        aiohttp.ClientError: Исключение, если произошла ошибка во время выполнения запроса.
        aiohttp.HttpProcessingError: Исключение, если статус код ответа не является успешным (2xx).
    """
    url = f"{API_URL}/{endpoint}"
    try:
        async with aiohttp.ClientSession() as session:
            if method == 'POST':
                async with session.post(url, json=json_data) as response:
                    text = await response.text()
                    logger.info(f'{endpoint} {response.status} {text}')
                    response.raise_for_status()
                    return response
            elif method == 'GET':
                async with session.get(url, json=json_data, params=params) as response:
                    text = await response.text()
                    logger.info(f'{endpoint} {response.status} {text}')
                    #todo: не работает проверка статус кода
                    response.raise_for_status()
                    return response
            else:
                raise Exception(f'Invalid method {method}')
    except Exception as e:
        logger.exception(f'Error in {endpoint}: {str(e)}')
        raise e


async def station_validate(stationName) -> list:
    """  """
    endpoint = 'B2B/GetStationValidate'
    params = {'stationName': stationName}
    response = await make_request('GET', endpoint, params=params)
    return await response.json()  # [{"expressCode":2000000,"stationName":"МОСКВА","id":4}]


async def get_available_times(station_from_name, station_to_name, date):
    """  """
    endpoint = 'B2B/GetAvailableTimes'
    json_data = {
        'DepartureStationName': station_from_name,
        'ArrivalStationName': station_to_name,
        'DepartureDate': date
    }
    response = await make_request('GET', endpoint, json_data=json_data)
    return await response.json()


async def create_user(id_tg, username):
    """ Добавляет пользователя в БД """
    endpoint = 'User/CreateOrUpdate'
    json_data = {'IdTg': id_tg, 'Username': username}
    await make_request('POST', endpoint, json_data=json_data)


async def create_and_get_id_notification_task(notification_task_data: NotificationTask):
    """Создает задачу"""
    endpoint = 'NotificationTask/Create'
    json_data = notification_task_data.__dict__  # Преобразование объекта в словарь
    logger.info(json_data)
    response = await make_request('POST', endpoint, json_data=json_data)
    return await response.text()  # ID записи в БД


async def get_active_task_by_user_id(user_id):
    """Возвращает активные задачи конкретного пользователя"""
    endpoint = 'NotificationTask/GetActiveByUser'
    params = {'userId': user_id}
    response = await make_request('GET', endpoint, params=params)
    if response.status == 404:
        return None
    return await response.json()


async def delete_task_by_id(task_id):
    """Останавливает (Устанавливает статус Остановлен) задачу по ее ID"""
    endpoint = 'NotificationTask/SetIsStopped'
    params = {'notificationTaskId': task_id}
    response = await make_request('GET', endpoint, params=params)
    return await response.json()


async def get_popular_cities_by_user_id(user_id) -> list:
    """Возвращает популярные города конкретного пользователя"""
    endpoint = 'NotificationTask/GetPopularCities'
    params = {'userId': user_id}
    response = await make_request('GET', endpoint, params=params)
    if response.status == 404:
        raise Exception("Непредвиденная ошибка в методе получения популярных станций")
    return await response.json()
