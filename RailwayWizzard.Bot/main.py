import asyncio
from threading import Thread
import server
from bot import telegram_bot


#TODO: Перейти на другой более удобный фреймворк? Почитать про aiogram https://habr.com/ru/articles/757236/
#TODO: Изучить другой способ параллельного запуска бота и сервера (https://habr.com/ru/articles/709314/)
def main():
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    loop.run_until_complete(server.run())
    loop.close()


if __name__ == '__main__':
    Thread(target=main).start()
    telegram_bot.run()

