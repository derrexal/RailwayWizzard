<?php

require dirname(__DIR__) . '/vendor/autoload.php';

$config = new Rzd\Config();

// Устанавливаем язык
$config->setLanguage('en');

// Изменяем userAgent
$config->setUserAgent('Mozilla/5.0 (iPhone; CPU iPhone OS 12_1_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.0 Mobile/15E148 Safari/604.2');

// Изменяем referer
$config->setReferer('https://ticket.rzd.ru/');

// Enable debug mode
//$config->setDebugMode(true);

// Enable proxy
//$config->setProxy('https://username:password@192.168.16.1:10');

$start = new DateTime();
$date0 = $start->modify('+1 day');

$params = [
    'dir'        => 0,
    'tfl'        => 3,
    'checkSeats' => 1,
    'code0'      => '2004000',
    'code1'      => '2000000',
    'dt0'        => $date0->format('d.m.Y'),
];

$api = new Rzd\Api($config);

echo $api->trainRoutes($params);
