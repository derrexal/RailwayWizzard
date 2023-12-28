using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using System.Net;

namespace RzdHack_Robot.Browser;

/// <summary>
/// Драйвером браузера
/// </summary>
public class BrowserDriver : IDisposable
{
    private readonly Lazy<IWebDriver> _currentWebDriverLazy;
    private bool _isDisposed;

    public BrowserDriver()
    {
        _currentWebDriverLazy = new Lazy<IWebDriver>(CreateChromeWebDriver);
    }

    /// <summary>
    /// Экземпляр IWebDriver
    /// </summary>
    public IWebDriver Current => _currentWebDriverLazy.Value;

    /// <summary>
    /// Создает экземпляр драйвера Firefox
    /// </summary>
    /// <returns></returns>
    private IWebDriver CreateFirefoxWebDriver()
    {
        var firefoxDriverService = FirefoxDriverService.CreateDefaultService();

        var firefoxOptions = new FirefoxOptions();
        //            когда будет нужно запускать в скрытом режиме
        //            firefoxOptions.AddArgument("headless");

        var firefoxDriver = new FirefoxDriver(firefoxDriverService, firefoxOptions);
        // частично помогло решить вопрос с ожиданием загрузки страницы
        firefoxDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        firefoxDriver.Manage().Window.Maximize();

        return firefoxDriver;
    }

    /// <summary>
    /// Создает экземпляр драйвера Chrome
    /// </summary>
    /// <returns></returns>
    private IWebDriver CreateChromeWebDriver()
    {
        var chromeDriverService = ChromeDriverService.CreateDefaultService();

        var chromeOptions = new ChromeOptions();

        var user_agent = "Только после того как сделаете нормальную систему уведомлений";
        chromeOptions.AddArgument($"user-agent={user_agent}");

        // добавить когда будет нужно запускать в скрытом режиме
        //chromeOptions.AddArgument("--headless=chrome");
        chromeOptions.AddArgument("--headless=new");
        chromeOptions.AddArgument("start-maximized");
        chromeOptions.AddArgument("--window-size=1920,1080");

        chromeOptions.AddArguments("--ignore-certificate-errors");
        chromeOptions.AddArguments("--ignore-ssl-errors");

        //chromeOptions.AddHttpProxy(
        //    host: "46.3.197.61",
        //    port: 9207,
        //    userName: "utCwtG",
        //    password: "h7krbG"
        //    );

        var chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions);
        // частично помогло решить вопрос с ожиданием загрузки страницы
        chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        return chromeDriver;
    }

    /// <summary>
    /// Удаляет объект драйвера и закрывает браузер после завершения сценария
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_currentWebDriverLazy.IsValueCreated)
        {
            Current.Quit();
        }

        _isDisposed = true;
    }
}