using System.Net;
using System.Net.Http.Json;
using RzdHack.Robot.Core;


namespace RzdHack.Robot.App
{
    public class StepsUsingHttpClient : ISteps
    {
        private const string API_BOT_URL = "http://localhost:5000/";
        public async Task Notification(NotificationTask input)
        {
            string railwayDataText = $"{input.DepartureStation} - {input.ArrivalStation} \n{input.DateFrom.ToShortDateString()} {input.TimeFrom}";
            try
            {
                int countNotification = 0;  //счетчик отправленных уведомлений. Шлем н штук и закрываем таску

                Console.WriteLine($"Запустили процесс поиска мест на рейс:\n {railwayDataText}");

                while (countNotification != 100)
                {
                    //test
                    railwayDataText = $"{input.DepartureStation} - {input.ArrivalStation} \n{input.DateFrom.ToShortDateString()} {input.TimeFrom}";
                    await GetFreeSeats(input);
                    // Для срочного уведомления о наличии мест
                    ResponseToUser messageToUser = new ResponseToUser
                    {
                        Message = $"{char.ConvertFromUtf32(0x2705)} {railwayDataText} \n\nПоявилось место на рейс",
                        UserId = input.UserId
                    };

                    //TODO:Вынести в метод? А лучше в отдельный класс взаимодействия с АПИ бота, так же как сделано и там
                    HttpClient httpClient = new HttpClient();
                    // определяем данные запроса
                    using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_BOT_URL + "api/sendMessageForUser");
                    request.Content = JsonContent.Create(messageToUser);
                    // выполняем запрос
                    var response = await httpClient.SendAsync(request);

                    if (response.StatusCode != HttpStatusCode.OK)
                        //Это погасит весь метод!
                        //throw new Exception("Не удалось отправить сообщение пользователю");

                    //TODO:Тут может записать это число в БД?.Вдруг крашнется приложение
                    countNotification++;
                }

                //Если достигли лимита в 100 сообщений
                ResponseToUser messageToUserCountLimit = new ResponseToUser
                {
                    Message = $"{char.ConvertFromUtf32(0x2705)} Выполнено задание по поиску свободных мест на рейс:\n{railwayDataText}\n(Достигнут лимит в 100 сообщений)\n\n Если уведомления ещё нужны - пожалуйста, создайте новое задание",
                    UserId = input.UserId
                };
                //TODO: Тут в базе выставляем статус фолс и количество отправленных уведомлений=100
                HttpClient client = new HttpClient();
                using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, API_BOT_URL + "/api/sendMessageForUser");
                message.Content = JsonContent.Create(messageToUserCountLimit);
                // выполняем запрос
                var responseMessage = await client.SendAsync(message);

                if (responseMessage.StatusCode != HttpStatusCode.OK)
                    //Это погасит весь сервис?
                    throw new Exception("Не удалось отправить сообщение пользователю");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task GetFreeSeats(NotificationTask task)
        {
            Robot robot = new Robot();
            string? result;
            do
            {
                result = await robot.GetTicket(task);
                Thread.Sleep(1000 * 60); //60 секунд
            }
            while (result == null);

        }
    }
}
