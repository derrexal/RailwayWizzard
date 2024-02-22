<?php

require dirname(__DIR__) . '../vendor/autoload.php';

$start = new DateTime();
$date0 = $start->modify('+1 day');
$api = new Rzd\Api();

#получаем параметры из ссылки
$url = ((!empty($_SERVER['HTTPS'])) ? 'https' : 'http') . '://' . $_SERVER['HTTP_HOST'] . $_SERVER['REQUEST_URI'];
$parts = parse_url($url); 
parse_str($parts['query'], $query); 

echo $api->trainRoutes($query);
