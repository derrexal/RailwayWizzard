import asyncio
from bot import telegram_bot
import server


#TODO: Перейти на другой более удобный фреймворк? Почитать про aiogram https://habr.com/ru/articles/757236/
#TODO: Изучить другой способ параллельного запуска бота и сервера (https://habr.com/ru/articles/709314/)
async def main():
    # Запускаем сервер и бота в одной event loop
    server_task = asyncio.create_task(server.run())
    bot_task = asyncio.create_task(telegram_bot.run_async())
    await asyncio.gather(server_task, bot_task)

if __name__ == '__main__':
    asyncio.run(main())
