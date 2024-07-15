import aiohttp
from aiohttp import ClientResponse

from logger import logger
from Bot.Data.NotificationTaskData import NotificationTaskData

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
        logger.error(f'Error in {endpoint}: {str(e)}')
        raise e


async def station_validate(input_station) -> list:
    """  """
    endpoint = 'BTwoB/GetStationValidate'
    params = {'inputStation': input_station}
    response = await make_request('GET', endpoint, params=params)
    return await response.json()  # [{"expressCode":2000000,"stationName":"МОСКВА","id":4}]


async def get_available_times(station_from_name, station_to_name, date):
    """  """
    endpoint = 'BTwoB/GetAvailableTimes'
    json_data = {
        'StationFrom': station_from_name,
        'StationTo': station_to_name,
        'Date': date
    }
    response = await make_request('GET', endpoint, json_data=json_data)
    return await response.json()


async def create_user(id_tg, username):
    """ Добавляет пользователя в БД """
    endpoint = 'Users/CreateOrUpdate'
    json_data = {'IdTg': id_tg, 'Username': username}
    await make_request('POST', endpoint, json_data=json_data)


async def create_and_get_id_notification_task(notification_task_data: NotificationTaskData):
    """Создает задачу и отдает ее ID"""
    endpoint = 'NotificationTask/CreateAndGetId'
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
    params = {'idNotificationTask': task_id}
    response = await make_request('GET', endpoint, params=params)
    return await response.json()
