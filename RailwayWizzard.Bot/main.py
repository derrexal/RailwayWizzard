from threading import Thread

import server
from bot import telegram_bot


if __name__ == "__main__":
    Thread(target=server.run, daemon=True).start()
    telegram_bot.run()