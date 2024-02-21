<h1>Примеры запросов</h1>

<div style="background: sandybrown; padding: 5px; font-weight: bold">
    Обращаем внимание на то, что по новым условиям RZD.RU дату отправки нужно указывать с учетом часового пояса станции отправления
</div>

<h3>В каждый запрос можно добавлять свои параметры (Все параметры не обязательны)</h3>

<pre style="background: aliceblue; padding: 5px; border: 1px solid brown">
$config = new Rzd\Config();

// Устанавливаем язык
$config->setLanguage('en');

// Добавляем прокси
$config->setProxy('https://username:password@192.168.16.1:10');

// Изменяем userAgent
$config->setUserAgent('Mozilla 5');

// Изменяем referer
$config->setReferer('https://rzd.ru');

$api = new Rzd\Api($config);
</pre>

<h3>Выбор маршрута в одну сторону</h3>
<a href="train_routes.php">Просмотр</a><br>
В примере выполняется поиск маршрута САНКТ-ПЕТЕРБУРГ - МОСКВА (только с билетами) на завтра

<pre style="background: aliceblue; padding: 5px; border: 1px solid brown">
$params = [
    'dir'        => 0,
    'tfl'        => 3,
    'checkSeats' => 1,
    'code0'      => '2004000',
    'code1'      => '2000000',
    'dt0'        => 'Дата отправления',
];

$routes = $api->trainRoutes($params)
</pre>

<h3>Выбор маршрута в одну сторону c измененными параметрами</h3>
<a href="train_routes_params.php">Просмотр</a><br>
В примере выполняется аналогичный поиск маршрута SANKT-PETERBURG - MOSKVA (только с билетами) на завтра, но с установленными параметрами:<br>
<pre style="background: aliceblue; padding: 5px; border: 1px solid brown">
Язык: en
UserAgent: Mozilla 5
Referer: rzd.ru
</pre>

<h3>Выбор маршрута туда-обратно</h3>
<a href="train_routes_return.php">Просмотр</a><br>
В примере выполняется поиск маршрута САНКТ-ПЕТЕРБУРГ - МОСКВА (только с билетами) на завтра туда и через 5 дней обратно

<pre style="background: aliceblue; padding: 5px; border: 1px solid brown">
$params = [
    'dir'        => 1,
    'tfl'        => 3,
    'checkSeats' => 1,
    'code0'      => '2004000',
    'code1'      => '2000000',
    'dt0'        => 'Дата отправления',
    'dt1'        => 'Дата возврата',
];

$routes = $api->trainRoutesReturn($params);
</pre>

<h3>Выбор маршрута в одну сторону с пересадками</h3>
<a href="train_routes_transfer.php">Просмотр</a><br>
В примере выполняется поиск маршрута НОВЫЙ УРЕНГОЙ - АБАКАН (только с билетами) (с пересадками) на завтра

<pre style="background: aliceblue; padding: 5px; border: 1px solid brown">
$params = [
    'dir'        => 0,
    'tfl'        => 3,
    'checkSeats' => 1,
    'code0'      => '2030319',
    'code1'      => '2038230',
    'dt0'        => 'Дата отправления',
    'md'         => 1,
];

$routes = $api->trainRoutes($params)
</pre>

<h3>Выбор вагонов</h3>
<a href="train_carriages.php">Просмотр</a><br>
В примере выполняется просмотр всех вагонов в поезде с направлением САНКТ-ПЕТЕРБУРГ - МОСКВА на завтра

<pre style="background: aliceblue; padding: 5px; border: 1px solid brown">
$params = [
    'dir'   => 0,
    'code0' => '2004000',
    'code1' => '2000000',
    'dt0'   => 'Дата отправления',
    'time0' => 'Время отправления',
    'tnum0' => 'Номер вагона',
];

$carriages = $api->trainCarriages($params)
</pre>

<h3>Просмотр станций</h3>
<a href="train_station_list.php">Просмотр</a><br>
В примере выполняется поиск всех станций остановок для поезда номер 072E на завтра

<pre style="background: aliceblue; padding: 5px; border: 1px solid brown">
$params = [
    'trainNumber' => '054Г',
    'depDate'     => 'Дата отправления',
];

$stations = $api->trainStationList($params);
</pre>


<h3>Просмотр списка кодов станций</h3>
<a href="station_code.php">Просмотр</a><br>
В примере выполняется поиск кодов станций начинающихся с ЧЕБ

<pre style="background: aliceblue; padding: 5px; border: 1px solid brown">
$api = new Rzd\Api();

$params = [
    'stationNamePart' => 'ЧЕБ',
    'compactMode'     => 'y',
];

$stations = $api->stationCode($params);
</pre>
