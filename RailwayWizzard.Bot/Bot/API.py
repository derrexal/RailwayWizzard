import requests

API_URL = "http://railwaywizzardapp:80/"


# TODO: переименовать все 'add' -> 'create'
# TODO: если ответ не 200 - выбрасывать Exception

async def add_user(idTg, username):
    """ Добавляет пользователя в БД """
    myObj = {'IdTg': idTg, 'Username': username}
    try:
        response = requests.post(API_URL + 'Users/CreateOrUpdate', json=myObj)
    except Exception as e:
        raise e
    print('Users/CreateOrUpdate' + str(response.status_code) + ' ' + response.text)


async def create_and_get_id_notification_task(record_json):
    """ Отправляет информацию о созданном таске в АПИ чтобы та сохранила в БД и отдала нам ID записи"""
    try:
        #TODO: где правильнее формировать данные для запроса. Здесь или выше по стеку?

        response = requests.post(API_URL + 'NotificationTask/CreateAndGetId', json=record_json)
        status = response.status_code
        print('NotificationTask/CreateAndGetId ' + str(status) + ' ')
        if status != 200:
            raise Exception("Не удалось создать задачу. Ошибка записи в БД")
        print('NotificationTask/CreateAndGetId ' + str(response.status_code) + ' ' + response.text)
        return response.text  # ID записи в БД
    except Exception as e:
        raise e


async def add_station_info(record_station_info):
    """ Создает сущность StationInfo """
    myObj = {'ExpressCode': record_station_info['c'], 'StationName': record_station_info['n']}
    try:
        response = requests.post(API_URL + 'StationInfo/CreateOrUpdate', json=myObj)
    except Exception as e:
        raise e
    print('StationName/CreateOrUpdate ' + str(response.status_code) + ' ' + response.text)


async def get_station_info_by_name(station_info_name):
    """ Возвращает expressCode сущности StationInfo по полю Name """
    myObj = {'StationName': station_info_name}

    try:
        response = requests.get(API_URL + 'StationInfo/GetByName', json=myObj)
        status = response.status_code
        print('StationInfo/GetByName ' + str(status) + ' ' + response.text)
        if status == 200:
            return response.json()['expressCode']
        return None
    except Exception as e:
        raise e


async def get_active_task_by_user_id(user_id):
    """ Возвращает expressCode сущности StationInfo по полю Name """
    myObj = {'userId': user_id}

    try:
        response = requests.get(API_URL + 'NotificationTask/GetActiveByUser', params=myObj)
        status = response.status_code
        print('NotificationTask/GetActiveByUser' + str(status) + ' ' + response.text)
        if status == 200:
            return response.json()
        return None
    except Exception as e:
        raise e


async def delete_task_by_id(task_id):
    """ Останавливает задачу по ее айди """
    myObj = {'idNotificationTask': task_id}

    try:
        response = requests.get(API_URL + 'NotificationTask/SetIsStopped', params=myObj)
        status = response.status_code
        print('NotificationTask/SetIsStopped' + str(status) + ' ' + response.text)
        if status == 200:
            return response.json()
        return None
    except Exception as e:
        raise e


async def set_is_stopped_notification_task(id_notification_task):
    """ Устанавливает статус 'Остановлен' для конкретного таска"""
    myObj = {'idNotificationTask': id_notification_task}

    try:
        response = requests.get(API_URL + 'NotificationTask/SetIsStopped', json=myObj)
        status = response.status_code
        print('NotificationTask/SetIsStopped' + str(status) + ' ' + response.text)
        if status == 200:
            return True
        return False
    except Exception as e:
        raise e
