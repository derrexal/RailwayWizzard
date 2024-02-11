//using RzdHack_Robot.Core;
//using RzdHack_Robot.App.Shared;

//namespace RzdHack_Robot.App
//{
//    public class StepsUsingHttpClient : ISteps
//    {
//        public async Task Notification(object? message)
//        {
//            try
//            {
//                int countNotification = 0;  //счетчик отправленных уведомлений. Шлем н штук и молчим
//                if (message.GetType() != typeof(RailwayInput)) return; // если прислали не то что нужно - выходим
//                RailwayInput input = (RailwayInput)message;

//                Console.WriteLine($"Запустили процесс поиска мест на рейс:\n{input.DepartureStation} - {input.ArrivalStation} \n{input.CurrentTime} \n{input.DateFrom} ");

//                // Получаем ссылку со списком поездок
//                _page.currentUrl = GetCurrentUrl(input);

//                while (countNotification != 100)
//                {
//                    // Получаем нужную поездку
//                    var railway = GetSelectRailway(input);

//                    // Для срочного уведомления о наличии мест
//                    ResponseToUser messageToUser = GetNotificationResponseToUser(input);
//                    TCP.Sending(messageToUser);

//                    countNotification++;
//                    await Task.Delay(60000); //Шлем уведомление об новом месте 1 раз в 1 минуту. (При условии что его не выкупят раньше)
//                }
//                //Когда достигли лимита в 100 сообщений
//                ResponseToUser messageToUserCountLimit = new ResponseToUser
//                {
//                    Message = $"{char.ConvertFromUtf32(0x2705)} Выполнено задание по поиску свободных мест на рейс:\n{input.DepartureStation} - {input.ArrivalStation} \n{input.CurrentTime} \n{input.DateFrom}\n(Достигнут лимит в 100 сообщений)\n\n Если уведомления ещё нужны - пожалуйста, создайте новое задание",
//                    UserId = input.UserID
//                };
//                TCP.Sending(messageToUserCountLimit);

//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                throw;
//            }
//        }
//    }
//}
