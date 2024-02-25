using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using RzdHack.Robot.Core;


namespace RzdHack.Robot.App
{
    public class StepsUsingHttpClient : ISteps
    {
        private const string API_BOT_URL = "http://bot_service:5000/";
        public async Task Notification(NotificationTask input)
        {
            string railwayDataText = $"{input.DepartureStation} - {input.ArrivalStation} {input.TimeFrom} {input.DateFrom.ToString("dd.MM.yyy", CultureInfo.InvariantCulture)}";
            try
            {
                Console.WriteLine($"Запустили процесс поиска мест на рейс:\n {railwayDataText}");

                var freeSeats = await GetFreeSeats(input);

                // Формируется текст уведомления о наличии мест
                ResponseToUser messageToUser = new ResponseToUser
                {
                    Message = $"{char.ConvertFromUtf32(0x2705)} {railwayDataText} " +
                              $"\n{String.Join("\n", freeSeats.ToArray())}" +
                              "\nОбнаружены свободные места\n",
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
                    throw new Exception("Не удалось отправить сообщение пользователю");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<string>> GetFreeSeats(NotificationTask task)
        {
            Robot robot = new();
            List<string> result;
            do
            {
                result = await robot.GetTicket(task);
                Thread.Sleep(1000 * 60*3); //3 минуты
            }
            while (result.Count == 0);

            return result;
        }
    }
}
