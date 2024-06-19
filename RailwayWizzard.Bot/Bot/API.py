import requests
from logger import logger


API_URL = "http://railwaywizzardapp:80/"


async def create_user(id_tg, username):
    """ Добавляет пользователя в БД """
    end_point_url = 'Users/CreateOrUpdate'
    json_data = {'IdTg': id_tg, 'Username': username}

    try:
        response = requests.post(API_URL + end_point_url, json=json_data)
        logger.info(f'{end_point_url} {str(response.status_code)} {response.text}')
        if response.status_code != 200:
            raise Exception(f'{end_point_url} {str(response.status_code)} {response.text}')

    except Exception as e:
        raise e


async def create_station_info(record_station_info):
    """ Создает сущность StationInfo """
    end_point_url = 'StationInfo/CreateOrUpdate'
    json_data = {'ExpressCode': record_station_info['c'], 'StationName': record_station_info['n']}

    try:
        response = requests.post(API_URL + end_point_url, json=json_data)
        logger.info(f'{end_point_url} {str(response.status_code)} {response.text}')
        if response.status_code != 200:
            raise Exception(f'{end_point_url} {str(response.status_code)} {response.text}')

    except Exception as e:
        raise e


# TODO: если ответ не 200 - выбрасывать Exception# TODO: если ответ не 200 - выбрасывать Exception
#TODO: вынести это в один общий метод - много повторений, выяснилось
# TODO: где правильнее формировать данные для запроса. Здесь или выше по стеку?
async def create_and_get_id_notification_task(record_json):
    """ Создает задачу и отдает ее ID"""
    end_point_url = 'NotificationTask/CreateAndGetId'

    try:
        response = requests.post(API_URL + end_point_url, json=record_json)
        logger.info(f'{end_point_url} {str(response.status_code)} {response.text}')
        if response.status_code != 200:
            raise Exception(f'{end_point_url} {str(response.status_code)} {response.text}')
        return response.text  # ID записи в БД

    except Exception as e:
        raise e


async def get_express_code_station_by_name(station_info_name):
    """ Возвращает expressCode сущности StationInfo по полю Name """
    end_point_url = 'StationInfo/GetByName'
    json_data = {'StationName': station_info_name}

    try:
        response = requests.get(API_URL + end_point_url, json=json_data)
        logger.info(f'{end_point_url} {str(response.status_code)} {response.text}')
        if response.status_code == 000: #TODO: Какой код присылает сервер в случае если ничего не нашел?
            return None
        elif response.status_code != 200:
            raise Exception(f'{end_point_url} {str(response.status_code)} {response.text}')
        return response.json()['expressCode']

    except Exception as e:
        raise e


async def get_active_task_by_user_id(user_id):
    """ Возвращает активные задачи конкретного пользователя"""
    end_point_url = 'NotificationTask/GetActiveByUser'
    json_data = {'userId': user_id}

    try:
        response = requests.get(API_URL + end_point_url, params=json_data)
        logger.info(f'{end_point_url} {str(response.status_code)} {response.text}')
        if response.status_code == 000:
            return None
        elif response.status_code != 200:
            raise Exception(f'{end_point_url} {str(response.status_code)} {response.text}')
        return response.json()

    except Exception as e:
        raise e


async def delete_task_by_id(task_id):
    """ Останавливает(Устанавливает статус Остановлен) задачу по ее ID """
    end_point_url = 'NotificationTask/SetIsStopped'
    json_data = {'idNotificationTask': task_id}

    try:
        response = requests.get(API_URL + end_point_url, params=json_data)
        logger.info(f'{end_point_url} {str(response.status_code)} {response.text}')

        if response.status_code != 200:
            raise ValueError(f'{end_point_url} {str(response.status_code)} {response.text} {str(task_id)}')
        return response.json()

    except Exception as e:
        raise e
