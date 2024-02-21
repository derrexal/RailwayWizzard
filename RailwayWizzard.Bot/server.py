from fastapi import FastAPI, Body
from fastapi.responses import HTMLResponse
import uvicorn
from Bot.TelegramBot import send_message_to_user


app = FastAPI()


# ID admin:
@app.post("/api/sendMessageForUser")
async def read_root(data=Body()):
    if data is not None:
        user_id = data["userId"]
        message = data["message"]
        await send_message_to_user(user_id, message)
    else:
        print("Ошибка получения данных")

@app.get("/")
def test():
    html_content = "<h2>Hello METANIT.COM!</h2>"
    return HTMLResponse(content=html_content)

#todo поменять порт!
async def run():
    config = uvicorn.Config("server:app", port=5000, log_level="info")
    server = uvicorn.Server(config)
    await server.serve()
