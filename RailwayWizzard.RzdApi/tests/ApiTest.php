<?php

use PHPUnit\Framework\TestCase;
use Rzd\Api;

class ApiTest extends TestCase
{
    private Api $api;
    private Datetime $date0;
    private Datetime $date1;

    protected function setUp(): void
    {
        $this->api = new Api();

        $start = new DateTime();
        $this->date0 = $start->modify('+1 day');
        $this->date1 = $start->modify('+5 day');
    }

    /**
     * Тест получения маршрутов
     *
     * @runInSeparateProcess
     */
    public function testTrainRoutes(): void
    {
        $params = [
            'dir'        => 0,
            'tfl'        => 3,
            'checkSeats' => 1,
            'code0'      => '2004000',
            'code1'      => '2000000',
            'dt0'        => $this->date0->format('d.m.Y'),
        ];

        $trainRoutes = $this->api->trainRoutes($params);

        $this->assertIsArray($trainRoutes);
        $this->assertObjectHasAttribute('route0', $trainRoutes[0]);
        $this->assertSame('С-ПЕТЕР-ГЛ', $trainRoutes[0]->route0);
    }

    /**
     * Тест получения маршрутов туда-обратно
     *
     * @runInSeparateProcess
     */
    public function testTrainRoutesReturn(): void
    {
        $params = [
            'dir'        => 1,
            'tfl'        => 3,
            'checkSeats' => 1,
            'code0'      => '2004000',
            'code1'      => '2000000',
            'dt0'        => $this->date0->format('d.m.Y'),
            'dt1'        => $this->date1->format('d.m.Y'),
        ];

        $trainRoutesReturn = $this->api->trainRoutesReturn($params);

        $this->assertIsArray($trainRoutesReturn['forward']);
        $this->assertIsArray($trainRoutesReturn['back']);
        $this->assertObjectHasAttribute('route0', $trainRoutesReturn['forward'][0]);
        $this->assertSame('С-ПЕТЕР-ГЛ', $trainRoutesReturn['forward'][0]->route0);
    }

    /**
     * Тест получения вагонов
     *
     * @runInSeparateProcess
     */
    public function testTrainCarriages(): void
    {
        $params = [
            'dir'        => 0,
            'tfl'        => 3,
            'checkSeats' => 1,
            'code0'      => '2004000',
            'code1'      => '2000000',
            'dt0'        => $this->date0->format('d.m.Y'),
        ];

        $routes = $this->api->trainRoutes($params);

        if ($routes) {
            $params = [
                'dir'   => 0,
                'code0' => '2004000',
                'code1' => '2000000',
                'dt0'   => $routes[0]->date0,
                'time0' => $routes[0]->time0,
                'tnum0' => $routes[0]->number,
            ];

            $trainCarriages = $this->api->trainCarriages($params);

            $this->assertIsArray($trainCarriages);
            $this->assertArrayHasKey('cars', $trainCarriages);
            $this->assertObjectHasAttribute('cnumber', $trainCarriages['cars'][0]);
        }
    }

    /**
     * Тест просмотра станций
     *
     * @runInSeparateProcess
     */
    public function testTrainStationList(): void
    {
        $params = [
            'trainNumber' => '054Г',
            'depDate'     => $this->date0->format('d.m.Y'),
        ];

        $trainStationList = $this->api->trainStationList($params);

        $this->assertIsArray($trainStationList);
        $this->assertArrayHasKey('train', $trainStationList);
        $this->assertArrayHasKey('routes', $trainStationList);
        $this->assertSame('054Г', $trainStationList['train']->number);
    }

    /**
     * Тест кодов станций
     *
     * @runInSeparateProcess
     */
    public function testStationCode(): void
    {
        $params = [
            'stationNamePart' => 'ЧЕБ',
            'compactMode'     => 'y',
        ];

        $stationCode = $this->api->stationCode($params);

        $this->assertIsArray($stationCode);

        $cities = [];
        foreach($stationCode as $station) {
            $cities[] = $station['station'];
        }

        $this->assertContains('ЧЕБОКСАРЫ', $cities);
    }
}
