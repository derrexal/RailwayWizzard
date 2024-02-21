<?php

$url = explode('/',strtolower(substr($_SERVER['REQUEST_URI'], 1)));

switch($url[0]) {
    case '': {
        include 'index_old.php';
        break;
    }
    case 'routes':{
        include 'train_routes_core.php';
        break;
    }
}