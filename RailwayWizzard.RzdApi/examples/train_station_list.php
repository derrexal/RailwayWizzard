<?php
require dirname(__DIR__) . '/vendor/autoload.php';

$api = new Rzd\Api();

$start = new DateTime();
$date0 = $start->modify('+1 day');

$params = [
    'trainNumber' => '054Г',
    'depDate'     => $date0->format('d.m.Y'),
];

echo $api->trainStationList($params);
