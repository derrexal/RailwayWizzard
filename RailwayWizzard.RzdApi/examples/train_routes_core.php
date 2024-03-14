<?php

require dirname(__DIR__) . '/vendor/autoload.php';

$start = new DateTime();
$date0 = $start->modify('+1 day');
$api = new Rzd\Api();

//Ожидаемые параметры
// $params = [
//     'dir'        => 0,
//     'tfl'        => 3,
//     'checkSeats' => 1,
//     'code0'      => '2004000',
//     'code1'      => '2000000',
//     'dt0'        => $date0->format('d.m.Y'),
// ];

// Получаем параметры из ссылки
$url = ((!empty($_SERVER['HTTPS'])) ? 'https' : 'http') . '://' . $_SERVER['HTTP_HOST'] . $_SERVER['REQUEST_URI'];
$parts = parse_url($url); 
parse_str($parts['query'], $query); 

echo $api->trainRoutes($query);
