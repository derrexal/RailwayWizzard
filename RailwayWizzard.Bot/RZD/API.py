import requests
from RZD.Config import *

suggestion_path_old = 'https://ticket.rzd.ru/api/v1/suggests/'
suggestion_path = 'https://pass.rzd.ru/suggester'
schedule_path = 'https://pass.rzd.ru/basic-schedule/public/ru'
main_path = 'pass.rzd.ru'


def get_railway_list():
    try:
        headers['Host'] = 'ticket.rzd.ru'
        path = 'https://ticket.rzd.ru/apib2b/p/Railway/V1/Search/TrainPricing?service_provider=B2B_RZD'
        params = {
            'CarGrouping': 'DontGroup',
            'CarIssuingType': 'PassengersAndBaggage',
            'DepartureDate': '2023-11-05T00:00:00',
            'Destination': '2000231',
            'Origin': '2000000',
            'SpecialPlacesDemand': 'StandardPlaces',
            'GetByLocalTime': True,
            'TimeFrom': 0,
            'TimeTo': 24
        }
        response = requests.get(
            url=path,
            params=params,
            headers=headers
            #proxies=proxies
        )
        if response is None:
            return None
        print(response.json())
        print(response.text)
        print(response.status_code)
        return response

    except Exception as e:
        print(e)
        return None


def get_stations(input_station):
    try:
        headers['Host'] = main_path
        cookies = get_cookies(suggestion_path)
        params = {
            'stationNamePart': input_station,
            'lang': 'ru'
        }

        response = requests.get(
            url=suggestion_path,
            params=params,
            headers=headers,
            #proxies=proxies,
            cookies=cookies
        )
        response_json = response.json()
        # если ответ пустой - такой станции нет
        if response_json is None:
            return None
        # TODO: попробовать сделать запросик ещё раз, а лучше парочку. Периодически сервис отдает {}
        print(response_json)
        return response_json

    except Exception as e:
        print(e)
        return None


def get_stations_old(input_station):
    try:

        cookies = get_cookies(suggestion_path_old)
        params = {
            'GroupResults': 'true',
            'RailwaySortPriority': 'true',
            'MergeSuburban': 'true',
            'Query': input_station,
            'Language': 'ru',
            'TransportType': {'rail', 'suburban', 'avia', 'boat', 'bus', 'aeroexpress'}
        }

        response = requests.get(
            url=suggestion_path_old,
            params=params,
            headers=headers,
            #proxies=proxies,
            cookies=cookies
        )
        response_json = response.json()['city']

        # если ответ пустой - такой станции нет
        if response_json is None:
            return None
        # TODO: попробовать сделать запросик ещё раз, а лучше парочку. Периодически сервис отдает {}

        return response_json

    except Exception as e:
        print(e)
        return None


def get_schedule(station_from, station_to, station_from_name, station_to_name, date):
    try:
        headers['Host'] = main_path
        cookies = get_cookies(schedule_path)

        params = {
            'STRUCTURE_ID': '5249',
            'layer_id': '5526',
            'refererLayerId': '5526',
            'st_from': station_from,
            'st_to': station_to,
            'st_from_name': station_from_name,
            'st_to_name': station_to_name,
            'day': date,
        }

        result = requests.get(
            url=schedule_path,
            headers=headers,
            params=params,
            #proxies=proxies,
            cookies=cookies
        )
        if result.status_code != 200:
            print(result.status_code)
            print(result.text)
            return None
        return result.text

    except Exception as e:
        print(e)
        return False

def get_cookies(path):
    try:
        result = requests.get(
            url=schedule_path,
            headers=headers,
            #proxies=proxies,
        )
        if result is None:
            return None
        return result.cookies

    except Exception as e:
        print(e)
        return False