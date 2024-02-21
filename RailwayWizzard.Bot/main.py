import asyncio
from threading import Thread
import server
from Bot import TelegramBot


def main():
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    loop.run_until_complete(server.run())
    loop.close()

if __name__ == '__main__':
    Thread(target=main).start()
    TelegramBot.run()

#TODO-2: Наверное, было бы неплохо перед тем как пользователь начнет работу проверить соединение с необходимыми сервисами