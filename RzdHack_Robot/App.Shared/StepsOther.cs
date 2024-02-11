using RzdHack_Robot.Core;

namespace RzdHack_Robot.App.Shared
{
    public static class StepsOther
    {
        /// <summary>
        /// Формирование ответа пользователю
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ResponseToUser GetResponseToUser(long UserId, string message)
        {
            return new ResponseToUser
            {
                Message = message,
                UserId = UserId
            };
        }

        /// <summary>
        /// Формирует сообщение пользователю в случае если появилось свободное место
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static ResponseToUser GetNotificationResponseToUser(RailwayInput input)
        {
            ResponseToUser messageToUser = new ResponseToUser
            {
                Message = $"{char.ConvertFromUtf32(0x2705)} Появилось место на рейс:\n{input.DepartureStation} - {input.ArrivalStation} \n{input.CurrentTime} \n{input.DateFrom} ",
                UserId = input.UserID
            };
            return messageToUser;
        }

    }
}
