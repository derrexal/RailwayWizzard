using Abp.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using RzdHack_Robot.App;
using RzdHack_Robot.Core;

public class Program
{
    static RailwayInput railwayInput_1 => new RailwayInput()
    {
        DepartureStation = "Москва",
        ArrivalStation = "Глазуновка",
        CurrentTime = "14:10",
        DateFrom = "29.12.2023",
        UserID = 1002541777
    };

    static RailwayInput railwayInput_2 => new RailwayInput()
    {
        DepartureStation = "Москва",
        ArrivalStation = "Глазуновка",
        CurrentTime = "14:10",
        DateFrom = "29.12.2023",
        UserID = 897097392
    };

    static RailwayInput railwayInput_3 => new RailwayInput()
    {
        DepartureStation = "Москва",
        ArrivalStation = "Глазуновка",
        CurrentTime = "16:33",
        DateFrom = "29.12.2023",
        UserID = 1002541777
    };

    static RailwayInput railwayInput_4 => new RailwayInput()
    {
        DepartureStation = "Москва",
        ArrivalStation = "Глазуновка",
        CurrentTime = "16:33",
        DateFrom = "29.12.2023",
        UserID = 897097392
    };

    static LoginDetails loginDetails_1 => new LoginDetails("ermol_A","Sosipisos07rzd");

    static void Main()
    {

        //var t1 = new Thread(() => new Steps().AutoReservationOfTheSeatOnATrip(loginDetails_1, railwayInput_1, 0));

        CheckActiveTask.Start();
        TCP.Listening();
    }
}

#region Bot

//TODO:
//Бот не должен позволять создавать задания на время раньше чем сейчас + 1 час, и на время больше чем период доступности билетов на РЖД (Или это можно? - доп фича не помешает, пусть смотрит, Че)
//TODO:
//Может получится ситуация когда информация будет записана в базу, а робот не доступен.
//Пользователь получит ошибку и следующим бот будет ждать снова время поездки. А должен сбрасывать?
/*
 * Решение - перед тем как пользователь начинает вводить данные делать проверку доступности робота и в случае чего сразу сообщать ему об этом(Пожалуйста, попробуйте позже)
 */

#endregion

#region Robot

//TODO: Алгоритм выбора места
// Оказывается если вагон подразумевает перевозку животных - он ниже классом, чем тот в котором запрещено
// В вагонах классом выше - есть розетки везде

//TODO: Хочу получать уведомления о поломке робота, но все вычисления в классе Page не обрабатываются. Как сделать чтобы обрабатывались?

//TODO: Очень важно - очередь заданий
//                    сколько одновременных сессий позволяет открывать селениум?

// TODO:
// Изменить логику работы системы. Бот отправляет "пинок" серверу - мол посмотри, что я тебе в базу положил.
// Сервер смотрит, забирает новую задачу и дальше с ней работает.

// Done:
// Кроме этого сервер один раз в начале запуска просматривает все активные задачи и запускает их (Эта фича на случай перезапуска сервера)

//TODO: Важное замечение: автобронирование ВКЛЮЧАЕТ В СЕБЯ уведомление об появлении места)
// (Пока, на данный момент - если что отключим)

//TODO: По максимуму убрать рекурсию, т.к. из-за неё падает приложение/ Хотя - в целом, оно все равно будет рестартиться при ошибке такого рода по моей идее

#endregion