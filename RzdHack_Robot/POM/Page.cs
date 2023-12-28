using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Support.UI;
using System.Xml.Linq;
namespace RzdHack_Robot.POM;

public class Page
{
    public readonly string startUrl = "https://www.rzd.ru/";
    public string? currentUrl { get; set; }
    private readonly IWebDriver _driver;
    public Page(IWebDriver driver)
    {
        _driver = driver;
    }

    #region Локаторы

    private readonly By _departureStationInputBy = By.XPath("//*[@id=\"direction-from\"]");
    private readonly By _departureStationInputSuccessBy = By.XPath("//*[@id=\"rzd-search-widget\"]/div[1]/div[1]/div[1]/div[1]/ul/li[2]");
    private readonly By _arrivalStationInputBy = By.XPath("//*[@id=\"direction-to\"]");
    private readonly By _arrivalStationInputSuccessBy = By.XPath("//*[@id=\"rzd-search-widget\"]/div[1]/div[1]/div[1]/div[2]/ul/li[2]");
    private readonly By _dateFromInputBy = By.XPath("//*[@id=\"datepicker-from\"]");
    private readonly By _dateToInputBy = By.XPath("//*[@id=\"datepicker-to\"]");
    private readonly By _routeSearchButtonBy = By.XPath("//*[@id=\"rzd-search-widget\"]/div[1]/div[2]/a");
    private readonly By _noTicketsOnSaleButtonBy = By.XPath("//div[4]/ui-kit-button/button");
    private readonly By _timeRailwayListBy = By.XPath("//rzd-search-results-card-railway-flat-card");
    
    private readonly By _serviceClassDivListBy = By.XPath("//rzd-railway-service-class-selection-list/div/ui-kit-radio-group/ui-kit-radio-button");
        public readonly By _serviceClassNameBy = By.XPath("//label/div[2]/rzd-railway-service-class-selection-item/div/div[1]/div[1]/h3");
        public readonly By _serviceClassRadioButtonBy = By.XPath("//label/div[2]/rzd-railway-service-class-selection-item/div/div[1]/div[1]/bkit-radio-icon");
            
    private readonly By _continueShopFirstButtonBy = By.XPath("//rzd-class-selection-footer/div[1]/div/ui-kit-button/button");
    
    private readonly By _trainCarNumberBy = By.XPath("//rzd-car-scheme-coupled-container/div[1]/div[1]");

    private readonly By _goToListFreeSeedsButtonBy = By.XPath("//rzd-car-scheme-coupled-container/div[1]/div[2]/ui-kit-button[1]/button/div");
    private readonly By _trainCarNumberButtonListBy = By.XPath("//rzd-seats-rail-container/rzd-seats-scheme-container/div/ul/li");
    private readonly By _availableSeedListBy = By.ClassName("seat--available");
    private readonly By _continueShopSecondButtonBy = By.XPath("//rzd-seats-rail-container/rzd-seats-scheme-container/div/rzd-seats-summary-footer/div[1]/div/ui-kit-button/button/div/div");
    private readonly By _loginButtonFirstBy = By.XPath("//rzd-auth-notice/div/div/div/p/a[1]");
    private readonly By _loginUsernameInputBy = By.XPath("//form/rzd-form-control-wrapper[1]/div/div[2]/ui-kit-input/div/div/div/input");
    private readonly By _loginPasswordInputBy = By.XPath("//form/rzd-form-control-wrapper[2]/div/div[2]/ui-kit-input/div/div/div/input");
    private readonly By _loginButtonSecondBy = By.XPath("//rzd-unauth-login-form/div/form/div[1]/ui-kit-button/button/div/div");

    private readonly By _enteringDataForPurchaseButtonBy = By.XPath("//rzd-boarding-passenger-form-fields/div/div/div[1]/ui-kit-button/button");
    private readonly By _surnameInputBy = By.XPath("//rzd-passenger-general-info/div/form/div[1]/ui-kit-form-field[1]/div/div[2]/ui-kit-input/div/div/div/input");
    private readonly By _nameInputBy = By.XPath("//rzd-passenger-general-info/div/form/div[1]/ui-kit-form-field[2]/div/div[2]/ui-kit-input/div/div/div/input");
    private readonly By _middlenameInputBy = By.XPath("//rzd-passenger-general-info/div/form/div[1]/ui-kit-form-field[3]/div/div[2]/ui-kit-input/div/div/div/input");
    private readonly By _genderManInputBy = By.XPath("//rzd-passenger-general-info/div/form/div[2]/ui-kit-form-field[1]/div/div[2]/rzd-bkit-gender-selector/div/div[1]");
    private readonly By _ageInputBy = By.XPath("//rzd-passenger-general-info/div/form/div[2]/ui-kit-form-field[2]/div/div[2]/ui-kit-input/div/div/div/input");
    private readonly By _documentTypeListBoxBy = By.XPath("//rzd-passenger-document/div/form/div[1]/ui-kit-form-field/div/div[2]/ui-kit-select/div/div/div[2]");
    private readonly By _docementTypeBy = By.XPath("//ui-kit-options-container/div/div/ui-kit-option[1]");
    private readonly By _numberDocumentInputBy = By.XPath("//rzd-passenger-document/div/form/div[1]/div/ui-kit-form-field/div/div[2]/ui-kit-input/div/div/div/input");
    private readonly By _usingСurrentTelefonCheckBoxBy = By.XPath("//rzd-passenger-contacts/form/ui-kit-checkbox[1]/label/div/div[1]");
    private readonly By _addPassengerDataButtonBy = By.XPath("//rzd-passenger-create-container/div/div/div[2]/div/div/ui-kit-button/button");
    private readonly By _isNotPassengerBy = By.XPath("//rzd-boarding-passenger-form-fields/div/div/div[1]/ui-kit-button/button");
    private readonly By _telefonNumberInputBy = By.XPath("//rzd-passenger-contacts/form/div/ui-kit-form-field[1]/div/div[2]/ui-kit-input/div/div/div/input");
    private readonly By _emailInputBy = By.XPath("//rzd-passenger-contacts/form/div/ui-kit-form-field[2]/div/div[2]/ui-kit-input/div/div/div/input");

    //Выбор пассажиров
    private readonly By _passengerSelectBy = By.XPath("//rzd-boarding-passenger-form-fields/div/div/ui-kit-form-field/div/div[2]/ui-kit-select/div");
        private readonly By _passengerElementsBy = By.XPath("//ui-kit-option");

    private readonly By _tariffSelectListBy = By.XPath("//rzd-boarding-card/div/ui-kit-form-field/div/div[2]/ui-kit-select/div");
    private readonly By _tariffSelectBy = By.XPath("//*[@id=\"ui-kit-select-2-ui-kit-option-0\"]/div");
    private readonly By _continueShopThirdButtonBy = By.XPath("//rzd-boarding-cards-container/form/div/div/ui-kit-button[1]/button/div/div");

    private readonly By _confirmFirstBy = By.XPath("//rzd-order-details-container/rzd-order-agreements/ui-kit-checkbox[1]/label/div/div[1]/div/div");
    private readonly By _confirmSecondBy = By.XPath("//rzd-order-details-container/rzd-order-agreements/ui-kit-checkbox[2]/label/div/div[1]/div/div");
    private readonly By _continueShopFourthButtonBy = By.XPath("//*[@id=\"order-payment-container\"]/ui-kit-button[2]/button/div");

    private readonly By _cardNumberPaymentInputByOld = By.XPath("//*[@id=\"cp-pan-decor\"]");
    public readonly By _paumentSuccesLabelBy = By.XPath("//rzd-order-wrapper/rzd-order-details-container/bkit-payment-notification/div[1]");

    private readonly By _loginButtonBy = By.XPath("//rzd-header/header/div/div/div/div[2]/ul/li[3]/button/span");
    private readonly By _loginSideBarMenuBy = By.XPath("//rzd-app/rzd-side-menu-container");

    private readonly By _paymentFormDivBy = By.XPath("//*[@id=\"wrapper\"]/div[2]");


    //класс обслуживания при выборе поездки
    //(Сидячий, для инвалидов, бизнес)
    public readonly By _bodyClassDivsBy = By.XPath("//rzd-search-results-card-list/div/rzd-search-results-card-railway-flat-card[1]/div/div/div[3]/div[1]");
    #endregion

    #region Элементы страницы

    public IWebElement departureStationInputElement => _driver.FindElement(_departureStationInputBy);
    public IWebElement departureStationInputSuccessElement => _driver.FindElement(_departureStationInputSuccessBy);
    public IWebElement arrivalStationInputElement => _driver.FindElement(_arrivalStationInputBy);
    public IWebElement arrivalStationInputSuccessElement => _driver.FindElement(_arrivalStationInputSuccessBy);
    public IWebElement dateFromInputElement => _driver.FindElement(_dateFromInputBy);
    public IWebElement dateToInputElement => _driver.FindElement(_dateToInputBy);
    public IWebElement routeSearchButtonElement => _driver.FindElement(_routeSearchButtonBy);
    public IWebElement noTicketsOnSaleButtonElement => _driver.FindElement(_noTicketsOnSaleButtonBy);
    public IList<IWebElement> timeRailwayListElements => _driver.FindElements(_timeRailwayListBy);

    public IList<IWebElement> serviceClassDivListElementList => _driver.FindElements(_serviceClassDivListBy);
    public IWebElement serviceClassNameElement => _driver.FindElement(_serviceClassNameBy);
    public IWebElement serviceClassRadioButtonElement => _driver.FindElement(_serviceClassRadioButtonBy);

    public IWebElement continueShopFirstButtonElement => _driver.FindElement(_continueShopFirstButtonBy);

    public IWebElement trainCarNumberElement => _driver.FindElement(_trainCarNumberBy);

    public IWebElement selectRailwayElement { get; set; }
    public IWebElement selectSeedElement { get; set; }

    public IWebElement goToListFreeSeedsButtonElement => _driver.FindElement(_goToListFreeSeedsButtonBy);
    public IList<IWebElement> trainCarNumberButtonListElements => _driver.FindElements(_trainCarNumberButtonListBy);
    public IList<IWebElement> availableSeedListElements => _driver.FindElements(_availableSeedListBy);
    public IWebElement continueShopSecondButtonElement => _driver.FindElement(_continueShopSecondButtonBy);
    
    public IWebElement loginButtonFirstElement => _driver.FindElement(_loginButtonFirstBy);
    public IWebElement loginUsernameInputElement => _driver.FindElement(_loginUsernameInputBy);
    public IWebElement loginPasswordInputElement => _driver.FindElement(_loginPasswordInputBy);
    public IWebElement loginButtonSecondElement => _driver.FindElement(_loginButtonSecondBy);

    public IWebElement? isNotPassengerElement 
    { 
        get 
        {
            try { return _driver.FindElement(_isNotPassengerBy); }
            catch { return null; }
        }
    } 
    public IWebElement enteringDataForPurchaseButtonElement => _driver.FindElement(_enteringDataForPurchaseButtonBy);
    public IWebElement surnameInputElement => _driver.FindElement(_surnameInputBy);
    public IWebElement nameInputElement => _driver.FindElement(_nameInputBy);
    public IWebElement middlenameInputElement => _driver.FindElement(_middlenameInputBy);
    public IWebElement ageInputElement => _driver.FindElement(_ageInputBy);
    public IWebElement genderManInputElement => _driver.FindElement(_genderManInputBy);
    public IWebElement documentTypeListBoxElement => _driver.FindElement(_documentTypeListBoxBy);
    public IWebElement documentTypeElement => _driver.FindElement(_docementTypeBy);
    public IWebElement numberDocumentInputElement => _driver.FindElement(_numberDocumentInputBy);
    public IWebElement usingСurrentTelefonCheckBoxElement => _driver.FindElement(_usingСurrentTelefonCheckBoxBy);
    public IWebElement addPassengerDataButtonElement => _driver.FindElement(_addPassengerDataButtonBy);
    public IWebElement telefonNumberInputElement => _driver.FindElement(_telefonNumberInputBy);
    public IWebElement emailInputElement => _driver.FindElement(_emailInputBy);

    public IWebElement passengerSelectElement=> _driver.FindElement(_passengerSelectBy);
        public IList<IWebElement> passengerElements => _driver.FindElements(_passengerElementsBy);
    public IWebElement tariffSelectListElement=> _driver.FindElement(_tariffSelectListBy);
        public IWebElement tariffSelectElement => _driver.FindElement(_tariffSelectBy);
    public IWebElement continueShopThirdButtonElement => _driver.FindElement(_continueShopThirdButtonBy);
        

    public IWebElement confirmFirstElement => _driver.FindElement(_confirmFirstBy);
    public IWebElement confirmSecondElement => _driver.FindElement(_confirmSecondBy);
    public IWebElement continueShopFourthButtonElement => _driver.FindElement(_continueShopFourthButtonBy);

    public IWebElement loginButtonElement => _driver.FindElement(_loginButtonBy);
    //хотел использовать для проверки авторизовались мы или нет, но через wait она не работает
    public IWebElement loginSideBarMenuElement => _driver.FindElement(_loginSideBarMenuBy);
    public IWebElement paymentFormDivElement=> _driver.FindElement(_paymentFormDivBy);




    #endregion

    public void ClickElement(IWebElement element)
    {
        var wait = new WebDriverWait(_driver,
            new TimeSpan(0, 0, 0, 10)); //ждем пока элемент появится - тайм-аут 5 секунд //увеличил до 10 из-за долгого отображения начальных инпутов (станция отправления, прибытия, ...)
        try
        {
            wait.Until(display => element.Displayed & element.Displayed);
            element.Click();
        }
        catch (ElementClickInterceptedException)//What is?
        {
            wait.Until(display => element.Displayed & element.Displayed);
            element.SendKeys(Keys.Return);
        }
        catch { throw; }
    }
    public void EnterInput(IWebElement elementInput, string input)
    {
        var wait = new WebDriverWait(_driver,
            new TimeSpan(0, 0, 0, 29));

        try
        {
            wait.Until(display => elementInput.Displayed & elementInput.Displayed);
            elementInput.Clear();
            elementInput.SendKeys(input);
        }
        catch { throw; }
    }
}