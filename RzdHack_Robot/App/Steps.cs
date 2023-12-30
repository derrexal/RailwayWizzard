using Microsoft.Toolkit.Uwp.Notifications;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using RzdHack_Robot.Browser;
using RzdHack_Robot.Core;
using RzdHack_Robot.POM;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;

namespace RzdHack_Robot.App;

public class Steps
{
    private readonly IWebDriver _driver;
    private readonly Page _page;
    private string _paymentUrl;
    private WebDriverWait _wait;

    #region Input Data

    //private readonly string _departureStation = "Глазуновка";
    //private readonly string _arrivalStation = "Москва";
    //private readonly string _dateFrom = "11.10.2023";
    //private readonly string _railwayCurrentTime = "17:54";

    //private readonly string _loginUsername = "ermol_a";
    //private readonly string _loginPassword = "Sosipisos07";

    //private readonly string _surname = "Ермолаев";
    //private readonly string _name = "Александр";
    //private readonly string _middlename = "Алексеевич";
    //private readonly string _ageInputElement = "13.10.1999";
    //private readonly string _numberDocument = "5419 550568";
    //private readonly string _telefonNumberInputElement = "79207026495";
    //private readonly string _emailInputElement = "ermolav2011@ya.ru";

    #endregion
    
    public Steps()
    {
        _driver = new BrowserDriver().Current;
        _page = new Page(_driver);
        _wait = new WebDriverWait(_driver,
            new TimeSpan(0, 0, 0, 5)); 
    }
    
    public async Task Notification(object? message)
    {
        try
        {
            int countNotification = 0;  //счетчик отправленных уведомлений. Шлем н штук и молчим
            if (message.GetType() != typeof(RailwayInput)) return; // если прислали не то что нужно - выходим
            RailwayInput input = (RailwayInput)message;

            Console.WriteLine($"Запустили процесс поиска мест на рейс:\n{input.DepartureStation} - {input.ArrivalStation} \n{input.CurrentTime} \n{input.DateFrom} ");

            // Получаем ссылку со списком поездок
            _page.currentUrl = GetCurrentUrl(input);

            while (countNotification!=100)
            {
                // Получаем нужную поездку
                var railway = GetSelectRailway(input);

                // Для срочного уведомления о наличии мест
                ResponseToUser messageToUser = new ResponseToUser
                {
                    Message = $"{char.ConvertFromUtf32(0x2705)} Появилось место на рейс:\n{input.DepartureStation} - {input.ArrivalStation} \n{input.CurrentTime} \n{input.DateFrom} ",
                    UserId = input.UserID
                };
                TCP.Sending(messageToUser);

                countNotification++;
                await Task.Delay(60000); //Шлем уведомление об новом месте 1 раз в 1 минуту. (При условии что его не выкупят раньше)
            }
            //Когда достигли лимита в 100 сообщений
            ResponseToUser messageToUserCountLimit = new ResponseToUser
            {
                Message = $"{char.ConvertFromUtf32(0x2705)} Выполнено задание по поиску свободных мест на рейс:\n{input.DepartureStation} - {input.ArrivalStation} \n{input.CurrentTime} \n{input.DateFrom}\n(Достигнут лимит в 100 сообщений)\n\n Если уведомления ещё нужны - пожалуйста, создайте новое задание",
                UserId = input.UserID
            };
            TCP.Sending(messageToUserCountLimit);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// DEBUG
    /// Отслеживание свободных билетов
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<IList<FreeSeedsDto>> GetFreeSeatRailway(RailwayInput input)
    {   
        // Получаем ссылку со списком поездок
        _page.currentUrl = GetCurrentUrl(input);
        // Получаем нужную поездку
        _page.selectRailwayElement = GetSelectRailway(input);
        //Получаем все свободные места
        var free_seed = GetFreeSeats();

        //Отдаем свободные места места
        //TODO: не забыть про то что вагоны бывают разные
        //TODO: да и в принципе вагонов несколько - в них всех нужно смотреть свободные места и предоставлять информацию о вагоне пользователю
        //TODO: Итого пользователь должен увидеть: ТИП ВАГОНА - НОМЕР ВАГОНА - НОМЕР МЕСТА 

        foreach (var seed in free_seed)
        {
            Console.WriteLine(seed.serviceClass + seed.trainCarNumber);
            foreach (var seedNumber in seed.seedsNumber)
            {
                Console.WriteLine(seedNumber);
            }
            
        }
        Console.WriteLine("             End");
        return free_seed;
    }

    /// <summary>
    /// Автоматическое бронирование места в поезде
    /// </summary>
    public void AutoReservationOfTheSeatOnATrip(LoginDetails loginDetails,RailwayInput railwayInput,int passenderId)
    {
        try
        {
            Console.WriteLine($"Запустили процесс автобронирования мест на рейс:\n{railwayInput.DepartureStation} - {railwayInput.ArrivalStation} \n{railwayInput.CurrentTime} \n{railwayInput.DateFrom} ");

            // Получаем ссылку со списком поездок
            _page.currentUrl = GetCurrentUrl(railwayInput);
            //Ключевой момент - авторизация должна происходить после перехода на страницу с выбором поездки! Если раньше - она слетает
            Autorization(loginDetails);
            //TODO:Здесь вместо слипа добавить проверку на то что мы действительно вошли
            Thread.Sleep(10000);

            // Получаем нужную поездку
            _page.selectRailwayElement = GetSelectRailway(railwayInput);
            
            // Для срочного уведомления о наличии мест
            var input = railwayInput;
            ResponseToUser messageToUser = new ResponseToUser
            {
                Message = $"{char.ConvertFromUtf32(0x2705)} Появилось место на рейс:\n{input.DepartureStation} - {input.ArrivalStation} \n{input.CurrentTime} \n{input.DateFrom} ",
                UserId = input.UserID
            };
            TCP.Sending(messageToUser);

            GoToSelectSeat();
            // Выбор места
            _page.selectSeedElement = SelectSeedFromAvailable();

            PrevShopTicketRailWay();

            // Выбираем пассажира
            if (_page.isNotPassengerElement == null) PassengerSelection(passenderId);
            //else PassengerDataEnter(passengerDetails); ввод данных о пассажире пока отключили

            //Получаем ссылку для оплаты
            _paymentUrl = GetUrlForPayment();

            ////отправляем ссылку на оплату пользователю
            var responseToUser = new ResponseToUser
            {
                Message = $"Забронирован билет на рейс:\n{railwayInput.DepartureStation}-{railwayInput.ArrivalStation}\n{railwayInput.DateFrom}\n{railwayInput.CurrentTime}\n\nСсылка для оплаты:\n{_paymentUrl}",
                UserId = railwayInput.UserID
            };
            TCP.Sending(responseToUser);

            ////проверяем страницу перед оплатой каждые N секунд
            CheckPaymentRailway(5); //тут можно проверять и почаще, уже не страшно
            ////Если оплата прошла - отправляем сообщение об успешной покупке
            var responseToUserSuccessPay = new ResponseToUser
            {
                Message = "Оплата успешно проведена, билет у вас в личном кабинете",
                UserId = railwayInput.UserID
            };
            TCP.Sending(responseToUserSuccessPay);

            //// Возвращаем билет юзеру

            /// Выставляем статус "Выполнено" данной задаче
        }
        catch { throw; }
    }


    private void GoToSelectSeat()
    {
        var wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 0, 5));
        try
        {
            _page.ClickElement(_page.selectRailwayElement);
            wait.Until(display => _page.continueShopFirstButtonElement.Displayed); //Ждем когда эта страница загрузиться
            _page.ClickElement(_page.serviceClassRadioButtonElement);
            _page.ClickElement(_page.continueShopFirstButtonElement);
        }
        catch { throw; };
    }

    //public void Start()
    //{
    //    // Получаем ссылку со списком поездок
    //    // build error
    //    //_page.currentUrl = GetCurrentUrl(input);
    //    // получаем нужную поездку
    //    //_page.selectRailwayElement = GetSelectRailway();


    //    GoToSelectSeat();

    //    // получаем выбранное место
    //    _page.selectSeedElement = SelectSeedFromAvailable();

    //    PrevShopTicketRailWay();
    //    // авторизация
    //    //build error 
    //    //Autorization();
    //    // Выбираем пассажира
    //    PassengerSelectionOrEnterData();
    //    //Получаем ссылку для оплаты
    //    _paymentUrl = GetUrlForPayment();
    //    //отправляем ссылку на оплату пользователю
    //    //проверяем страницу перед оплатой каждые N секунд
    //    CheckPaymentRailway();
    //    //Если оплата прошла -отправляем сообщение об успешной покупке
    //    // Возвращаем билет юзеру
    //}

    /// <summary>
    /// Получаем ссылку, на которой находится список поездок
    /// </summary>
    private string GetCurrentUrl(RailwayInput input)
    {
        try
        {
            _driver.Navigate().GoToUrl(_page.startUrl);
            _driver.Navigate().Refresh(); //Пробуем починить долгую загрузку

            _page.EnterInput(_page.departureStationInputElement, input.DepartureStation);
            Thread.Sleep(2500);
            _page.ClickElement(_page.departureStationInputSuccessElement);

            _page.EnterInput(_page.arrivalStationInputElement, input.ArrivalStation);
            Thread.Sleep(2500);
            _page.ClickElement(_page.arrivalStationInputSuccessElement);

            _page.EnterInput(_page.dateFromInputElement, input.DateFrom);
            _page.ClickElement(_page.routeSearchButtonElement);
            Thread.Sleep(3000);
            return _driver.Url;
        }
        //catch (WebDriverTimeoutException) //old            //Если отваливаемся по тайм-ауту - делаем аналогичный запрос
        //{
        //    return GetCurrentUrl(input);
        //}
        //Вернул потому что запускал на компе
        catch (WebDriverException)             //Если отваливаемся по тайм-ауту - делаем аналогичный запрос
        {
            return GetCurrentUrl(input);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Выбор нужной поездки
    /// </summary>
    /// <returns></returns>
    private IWebElement GetSelectRailway(RailwayInput input)
    {
        Thread currentThread = Thread.CurrentThread;
        int countScreenshot = 0;
        try
        {
            if (_page.currentUrl == _page.startUrl) throw new Exception("Не получили ссылку со списком маршрутов");
            if (_driver.Url != _page.currentUrl)
                _driver.Navigate().GoToUrl(_page.currentUrl); // Если сейчас мы в другом месте - вернемся на нужную страницу

            IWebElement? selectRailwayElement;
            do
            {
                _driver.Navigate().Refresh();
                Thread.Sleep(5000);
                selectRailwayElement = SelectTrueRailWay(_page.timeRailwayListElements, input.CurrentTime);
                //debug
                //_driver.TakeScreenshot().SaveAsFile($"screen{currentThread.ManagedThreadId}-{countScreenshot}.png", ScreenshotImageFormat.Png);
                Console.WriteLine($"время {DateTime.Today.ToString()} поток номер {currentThread.ManagedThreadId} чек номер {countScreenshot}");
                countScreenshot++;

            } while (selectRailwayElement == null); // как только поездка(а значит и билеты) появится в списке - выйдет из цикла

            #region DONT USING:Выводит уведомление на рабочий стол для ОС Windows
            //new ToastContentBuilder()
            //    .AddText("МЕСТО НАШЛОСЬ ")
            //    .AddText(input.CurrentTime)
            //    .Show(); // Not seeing the Show() method? Make sure you have version 7.0, and if you're using .NET 6 (or later), then your TFM must be net6.0-windows10.0.17763.0 or greater
            //return GetSelectRailway(input);
            #endregion

            return selectRailwayElement;
        }

        //попробовал отключить на время проведения проверки - действительно ли такое большое количество вызовов этой функции связано с этой рекурсией
        //catch (WebDriverTimeoutException) //old            //Если отваливаемся по тайм-ауту - делаем аналогичный запрос
        //{
        //    return GetSelectRailway(input);
        //}
        catch (WebDriverException)             //Если отваливаемся по тайм-ауту - делаем аналогичный запрос
        {
            return GetSelectRailway(input);
        }

        catch { throw; }
    }

    /// <summary>
    /// Выбор нужной поездки из списка
    /// Отсеиваются поездки с местами только для инвалидов, все остальные проходят
    /// </summary>
    /// <param name="listElements"></param>
    /// <returns></returns>
    private IWebElement? SelectTrueRailWay(IList<IWebElement> listElements, string CurrentTime)
    {
        try
        {
            foreach (var element in listElements)
            {
                //проверка на то, что в списке есть поездка с таким временем (значит есть хоть какие-то места)
                //и на то, что это места НЕ ДЛЯ инвалидов
                if (element.Text.Contains(CurrentTime))
                {
                    IList<IWebElement> bodyClassesList = element.FindElements(_page._bodyClassDivsBy);
                    //если мест нет
                    if (bodyClassesList.Count == 0) return null;
                    //если только 1 класс обслуживания и он - для инвалидов
                    if (bodyClassesList.Count == 1 && bodyClassesList[0].Text.Contains("инвалид"))
                        return null;
                    //если есть больше 1 класса обслуживания - нам подходит
                    else return element;
                }
                    
            }
            return null;
        }
        catch { throw; }
    }

    //TODO:Разбить на несколько методов
    /// <summary>
    /// Переход со списка поездок к выбору мест
    /// Этот метод предназначен для полноценного выбора мест
    /// Обходит все вагоны и места и предоставляет пользователю информацию об этом
    /// </summary>
    private IList<FreeSeedsDto> GetFreeSeats()
    {
        var freeSeedList = new List<FreeSeedsDto>(); // Класс, вагон, сиденье
        var wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 0, 5));
        try
        {
            _page.ClickElement(_page.selectRailwayElement);
            wait.Until(display => _page.continueShopFirstButtonElement.Displayed); //Ждем когда эта страница загрузиться
            var choiceServiceClassUrl = _driver.Url;

            var listServiceClass = _page.serviceClassDivListElementList;
            foreach (IWebElement serviceClass in listServiceClass)
            {
                _driver.Navigate().GoToUrl(choiceServiceClassUrl);
                var freeSeed = new FreeSeedsDto();

                var nameServiceClass = _page.serviceClassNameElement.Text;
                var radioButtonServiceClass = _page.serviceClassRadioButtonElement;
                
                _page.ClickElement(radioButtonServiceClass);
                _page.ClickElement(_page.continueShopFirstButtonElement);
                
                //Проходим по всем вагонам
                foreach (var trainCarNumberButton in _page.trainCarNumberButtonListElements)
                {//Расширить информацией об месте? (У стола, по ходу)

                    trainCarNumberButton.Click();
                    var seedsNumber = GetAllFreeSeed();
                    freeSeed.seedsNumber = seedsNumber;
                    freeSeed.trainCarNumber = ExtractNumberTextFromElement(_page.trainCarNumberElement);
                    freeSeed.serviceClass = nameServiceClass;
                    freeSeedList.Add(freeSeed);
                }
            }
            return freeSeedList;
        }//TODO: не печатаются номера сиденьев, при каждом проходе записывается один и тот же класс обслуживания и номер вагона, и как будто не прощелкивает их
        catch { throw; };
    }

    private IList<string> GetAllFreeSeed()
    {
        // Клик по кнопке спискового представления свободных мест
        //_page.goToListFreeSeedsButtonElement.Click();
        if (_page.availableSeedListElements.Count == 0) throw new Exception("Ошибка в логике получения свободных мест, дядь");
        return GetAllFreeSeedsNumber(_page.availableSeedListElements);
    }

    private IList<string> GetAllFreeSeedsNumber(IList<IWebElement> seeds)
    {
        var result = new List<string>();
        try
        {
            foreach (var seed in seeds)
            {
                var seedNumber = ExtractIdFromElement(seed);
                result.Add(seedNumber);
            }

            return result;
        }
        catch { throw; }
    }

    /// <summary>
    /// Метод извлечения номера места из ID элемента (в случае графического представления схемы)
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private string ExtractIdFromElement(IWebElement element)
    {
        try
        {
            var idElement = element.GetAttribute("id");
            return Regex.Match(idElement, @"\d+").Value; // в ID лежит номер места (Больше нигде нет)
        }
        catch (StaleElementReferenceException)
        {
            Console.WriteLine("Дерьмо случается(Не получилось извлечь номер кресла из id элемента)");
        }
        return "";
    }

    /// <summary>
    /// Метод извлечения номера места из текст элемента (В случае представления свободных мест списком)
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private string ExtractNumberTextFromElement(IWebElement element)
    {
        return Regex.Match(element.Text, @"\d+").Value;
    }

    /// <summary>
    /// Функционал выбора места в поезде
    /// </summary>
    /// <returns></returns>
    private IWebElement SelectSeedFromAvailable()
    {
        // Пока что возвращает первое попавшееся место
        // !Здесь должна быть логика определения наиболее приятного места
        // Или предложить выбор пользователю (Логика определения места по выбору пользователя)
        if (_page.availableSeedListElements.Count == 0) return _page.availableSeedListElements[0]; // если доступно всего 1 место - сразу его выбираем
        else return _page.availableSeedListElements[0];
    }

    /// <summary>
    /// Авторизация пользователя по вводимым им данным или (если есть) данным из БД
    /// </summary> 
    private void Autorization(LoginDetails loginDetails)
    {
        try
        {
            var loginButtonElement = _driver.FindElement(By.XPath("//rzd-header/header/div/div/div/div[2]/ul/li[3]/button"));
            //так должно быть в режиме оконного просмотра
            //            _page.loginButtonElement = _driver.FindElement(_page._loginButtonBy);
            _page.ClickElement(loginButtonElement);
            //Закоментировал кнопку относящуюся к старой логике когда авторизация происходила после того, как появится место
            //_page.ClickElement(_page.loginButtonFirstElement);
            var loginUsernameInputElement = _driver.FindElement(By.XPath("//ui-kit-input[@id=\"ui-kit-input-3\"]/div/div/div/input"));
            _page.EnterInput(loginUsernameInputElement, loginDetails.Username);
            var loginPasswordInputElement = _driver.FindElement(By.XPath("//ui-kit-input[@id=\"ui-kit-input-4\"]/div/div/div/input"));
            _page.EnterInput(loginPasswordInputElement, loginDetails.Password);
            var loginButtonSecondElement = _driver.FindElement(By.XPath("/html/body/rzd-app/rzd-side-menu-container/rzd-side-menu-wrapper/div/div[2]/div/rzd-unauth-login-form/div/form/div[1]/ui-kit-button"));
            _page.ClickElement(loginButtonSecondElement);
        }
        catch
        {
            _driver.TakeScreenshot().SaveAsFile("error.png", ScreenshotImageFormat.Png);
            throw;
        } //TODO: обработка тайм-аутов...
    }

    /// <summary>
    /// Авторизация пользователя по вводимым им данным или (если есть) данным из БД
    /// </summary> 
    private void Autorization_old(LoginDetails loginDetails)
    {
        try
        {
            _page.ClickElement(_page.loginButtonElement);
            //Закоментировал кнопку относящуюся к старой логике когда авторизация происходила после того, как появится место
            //_page.ClickElement(_page.loginButtonFirstElement);
            _page.EnterInput(_page.loginUsernameInputElement, loginDetails.Username);
            _page.EnterInput(_page.loginPasswordInputElement, loginDetails.Password);
            _page.ClickElement(_page.loginButtonSecondElement);
        }
        catch 
        {
           _driver.TakeScreenshot().SaveAsFile("error.png", ScreenshotImageFormat.Png);
            throw;
        } //TODO: обработка тайм-аутов...
    }

    /// <summary>
    /// Этап покупки билета
    /// </summary>
    /// <param name="sead"></param>
    private void PrevShopTicketRailWay()
    {
        try
        {
            _page.ClickElement(_page.selectSeedElement);
            _page.ClickElement(_page.continueShopSecondButtonElement);
        }
        catch { throw; }
    }

    /// <summary>
    /// Ввод данных об новом пассажире
    /// </summary>
    private void PassengerDataEnter(PassengerDetails passengerDetails)
    {
        try
        {
            _page.ClickElement(_page.enteringDataForPurchaseButtonElement);
            _page.EnterInput(_page.surnameInputElement, passengerDetails.Surname);
            _page.EnterInput(_page.nameInputElement, passengerDetails.Name);
            _page.EnterInput(_page.middlenameInputElement, passengerDetails.Middlename);
            _page.ClickElement(_page.genderManInputElement);
            _page.EnterInput(_page.ageInputElement, passengerDetails.Age);
            _page.ClickElement(_page.documentTypeListBoxElement);
            Thread.Sleep(2000);
            _page.ClickElement(_page.documentTypeElement);
            _page.EnterInput(_page.numberDocumentInputElement, passengerDetails.DocumentNumber);
            _page.EnterInput(_page.telefonNumberInputElement, passengerDetails.PhoneNumber);
            _page.EnterInput(_page.emailInputElement, passengerDetails.Email);
            _page.ClickElement(_page.addPassengerDataButtonElement);
        }
        catch { throw; }
    }

    /// <summary>
    /// Выбор пассажира для бронирования
    /// </summary>
    private void PassengerSelection(int passenger)
    {
        try
        {
            _page.ClickElement(_page.passengerSelectElement);
            _page.ClickElement(_page.passengerElements[passenger]);
            //_page.ClickElement(_page.tariffSelectListElement);
            //_page.ClickElement(_page.tariffSelectElement); // тут предоставить возможность выбора в будущем (тариф есть на случай ДР)
            // добавить РЖД бонус тут или в разделе заполнения информации
            _page.ClickElement(_page.continueShopThirdButtonElement);
        }
        catch { throw; }
    }

    /// <summary>
    /// Получение ссылки для оплаты
    /// </summary>
    /// <returns></returns>
    private string GetUrlForPayment()
    {
        try
        {
            //тут добавить доп багаж, если нужно
            // тут выбрать куда отправить чек
            _page.ClickElement(_page.confirmFirstElement);
            _page.ClickElement(_page.confirmSecondElement);
            _page.ClickElement(_page.continueShopFourthButtonElement);
            //Ждем, когда страница оплаты действительно прогрузится чтобы отдать ссылку на неё, а не на предыдущую
            _wait.Until(u => _page.paymentFormDivElement.Displayed);
            return _driver.Url;
        }
        catch { throw; }
    }

    /// <summary>
    /// Проверка оплаты
    /// </summary>
    /// <returns></returns>
    private bool CheckPaymentRailway(int second)
    {
        if (_driver.Url != _paymentUrl) _driver.Navigate().GoToUrl(_paymentUrl); // Если сейчас мы в другом месте - вернемся на страницу оплаты

        if (_driver.FindElement(By.XPath("//button[@id=\"OK\"]")) == null) return true; //кнопка оплатить
        else
        {
            Thread.Sleep(1000*second);
            _driver.Navigate().Refresh();
            return CheckPaymentRailway(second);
        }
    }
}