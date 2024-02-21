<?php

require dirname(__DIR__) . '/vendor/autoload.php';

$start = new DateTime();
$date0 = $start->modify('+1 day');

$params = [
    'dir'        => 0,
    'tfl'        => 3,
    'checkSeats' => 1,
    'code0'      => '2030319',
    'code1'      => '2038230',
    'dt0'        => $date0->format('d.m.Y'),
    'md'         => 1,
];

$api = new Rzd\Api();

echo $api->trainRoutes($params);
