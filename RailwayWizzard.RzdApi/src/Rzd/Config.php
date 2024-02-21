<?php

namespace Rzd;

class Config
{
    private bool $debug = false;
    private string $language = 'ru';
    private float $timeout = 5.0;
    private ?string $proxy = null;
    private ?string $userAgent = null;
    private ?string $referer = null;

    /**
     * Set Language
     *
     * @param string $language
     */
    public function setLanguage(string $language): void
    {
        $this->language = $language;
    }

    /**
     * Get Language
     *
     * @return string
     */
    public function getLanguage(): string
    {
        return $this->language;
    }

    /**
     * Set debug mode
     *
     * @param bool $debug
     */
    public function setDebugMode(bool $debug): void
    {
        $this->debug = $debug;
    }

    /**
     * Get debug mode
     *
     * @return bool
     */
    public function getDebugMode(): bool
    {
        return $this->debug;
    }

    /**
     * Get timeout
     *
     * @return float
     */
    public function getTimeout(): float
    {
        return $this->timeout;
    }

    /**
     * Set timeout
     *
     * @param float $timeout
     */
    public function setTimeout(float $timeout): void
    {
        $this->timeout = $timeout;
    }

    /**
     * Set Proxy
     *
     * @param string $proxy
     */
    public function setProxy(string $proxy): void
    {
        $this->proxy = $proxy;
    }

    /**
     * Get Proxy
     *
     * @return string|null
     */
    public function getProxy(): ?string
    {
        return $this->proxy;
    }

    /**
     * Set User Agent
     *
     * @param string $userAgent
     */
    public function setUserAgent(string $userAgent): void
    {
        $this->userAgent = $userAgent;
    }

    /**
     * Get User Agent
     *
     * @return string|null
     */
    public function getUserAgent(): ?string
    {
        return $this->userAgent;
    }

    /**
     * Set Referer
     *
     * @param string $referer
     */
    public function setReferer(string $referer): void
    {
        $this->referer = $referer;
    }

    /**
     * Set Referer
     *
     * @return string|null
     */
    public function getReferer(): ?string
    {
        return $this->referer;
    }
}
