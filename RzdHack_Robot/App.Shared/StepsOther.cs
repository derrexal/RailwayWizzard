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


    }
}
