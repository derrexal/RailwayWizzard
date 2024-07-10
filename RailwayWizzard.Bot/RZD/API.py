import requests
from RZD.Config import *

suggestion_path_old = 'https://ticket.rzd.ru/api/v1/suggests/'
suggestion_path = 'https://pass.rzd.ru/suggester'
schedule_path = 'https://pass.rzd.ru/basic-schedule/public/ru'
main_path = 'pass.rzd.ru'


def get_schedule(station_from, station_to, station_from_name, station_to_name, date):
    try:
        headers['Host'] = main_path
        cookies = get_cookies()

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
