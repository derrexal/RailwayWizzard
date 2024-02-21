<?php

namespace Rzd;

use GuzzleHttp\Exception\GuzzleException;
use JsonException;

class Api
{
    public const ROUTES_LAYER = 5827;
    public const CARRIAGES_LAYER = 5764;
    public const STATIONS_STRUCTURE_ID = 704;

    /**
     * Путь получения маршрутов
     */
    protected string $path = 'https://pass.rzd.ru/timetable/public/';

    /**
     * Путь получения кодов станций
     */
    protected string $suggestionPath = 'https://pass.rzd.ru/suggester';

    /**
     * Путь получения станций маршрута
     */
    protected string $stationListPath = 'https://pass.rzd.ru/ticket/services/route/basicRoute';

    private Query $query;
    private string $lang;

    /**
     * Api constructor.
     *
     * @param Config|null $config
     */
    public function __construct(Config $config = null)
    {
        if (! $config) {
            $config = new Config();
        }

        $this->lang = $config->getLanguage();
        $this->path .= $this->lang;
        $this->query = new Query($config);
    }

    /**
     * Получает маршруты в 1 точку
     *
     * @param array $params Массив параметров
     *
     * @return string
     * @throws GuzzleException|JsonException
     */
    public function trainRoutes(array $params): string
    {
        $layer = [
            'layer_id' => static::ROUTES_LAYER,
        ];
        $routes = $this->query->get($this->path, $layer + $params);

        return json_encode($routes->tp[0]->list, JSON_THROW_ON_ERROR | JSON_UNESCAPED_UNICODE);
    }

    /**
     * Получает маршруты туда-обратно
     *
     * @param  array $params Массив параметров
     *
     * @return string
     * @throws GuzzleException|JsonException
     */
    public function trainRoutesReturn(array $params): string
    {
        $layer = [
            'layer_id' => static::ROUTES_LAYER,
        ];
        $routes = $this->query->get($this->path, $layer + $params);

        return json_encode([
                'forward' => $routes->tp[0]->list,
                'back'    => $routes->tp[1]->list,
            ],
            JSON_THROW_ON_ERROR | JSON_UNESCAPED_UNICODE
        );
    }

    /**
     * Получение списка вагонов
     *
     * @param  array $params Массив параметров
     *
     * @return string
     * @throws GuzzleException|JsonException
     */
    public function trainCarriages(array $params): string
    {
        $layer = [
            'layer_id' => static::CARRIAGES_LAYER,
        ];
        $carriages = $this->query->get($this->path, $layer + $params);

        return json_encode([
                'cars'           => $carriages->lst[0]->cars ?? null,
                'functionBlocks' => $carriages->lst[0]->functionBlocks ?? null,
                'schemes'        => $carriages->schemes ?? null,
                'companies'      => $carriages->insuranceCompany ?? null,
            ],
            JSON_THROW_ON_ERROR | JSON_UNESCAPED_UNICODE
        );

    }

    /**
     * Получение списка станций
     *
     * @param  array $params Массив параметров
     *
     * @return string
     * @throws GuzzleException|JsonException
     */
    public function trainStationList(array $params): string
    {
        $layer = [
            'STRUCTURE_ID' => static::STATIONS_STRUCTURE_ID,
        ];
        $stations = $this->query->get($this->stationListPath, $layer + $params);

        return json_encode([
                'train'  => $stations->data->trainInfo,
                'routes' => $stations->data->routes,
            ],
            JSON_THROW_ON_ERROR | JSON_UNESCAPED_UNICODE
        );
    }

    /**
     * Получение списка кодов станций
     *
     * @param  array $params Массив параметров
     *
     * @return string
     * @throws GuzzleException|JsonException
     */
    public function stationCode(array $params): string
    {
        $lang = [
            'lang' => $this->lang,
        ];

        $routes = $this->query->get($this->suggestionPath, $lang + $params, 'GET');
        $stations = [];

        if ($routes) {
            foreach ($routes as $station) {
                if (mb_stristr($station->n, $params['stationNamePart'])) {
                    $stations[] = [
                        'station' => $station->n,
                        'code' => $station->c,
                    ];
                }
            }
        }

        return json_encode($stations, JSON_THROW_ON_ERROR | JSON_UNESCAPED_UNICODE);
    }
}
