<?php

require __DIR__ . '/vendor/autoload.php';

use Monolog\Handler\StreamHandler;
use Monolog\Level;
use Monolog\Logger;
use PhpParser\ParserFactory;
use Psr\Http\Message\ServerRequestInterface;
use React\Http\HttpServer;
use React\Http\Message\Response;
use React\Socket\SocketServer;

const V1_CODEPATH = '/userfunc/user';
const V1_USER_FUNCTION = 'handler';
const HANDLER_DIVIDER = '::';

$codePath = null;
$userFunction = null;
$logger = new Logger("Function");
$logger->pushHandler(new StreamHandler('php://stdout', Level::Debug));


$server = new HttpServer(function (ServerRequestInterface $request) use (&$codePath, &$userFunction, $logger) {
    $path = $request->getUri()->getPath();
    $method = $request->getMethod();

    if ('/specialize' === $path && 'POST' === $method) {
        $codePath = V1_CODEPATH;
        $userFunction = V1_USER_FUNCTION;

        return new Response(201);
    }

    if ('/v2/specialize' === $path && 'POST' === $method) {
        $body = json_decode($request->getBody()->getContents(), true);
        if (!is_array($body) || empty($body['filepath']) || empty($body['functionName'])) {
            $logger->error('Invalid /v2/specialize payload: ' . json_last_error_msg());
            return new Response(400, [], 'Invalid specialize request: filepath and functionName required');
        }
        $filepath = $body['filepath'];
        // No HANDLER_DIVIDER means a legacy echo-style function (no named handler).
        $parts = explode(HANDLER_DIVIDER, $body['functionName']);
        $moduleName = $parts[0];
        $userFunction = $parts[1] ?? null;
        if (true === is_dir($filepath)) {
            $codePath = $filepath . DIRECTORY_SEPARATOR . $moduleName;

        } else {
            $codePath = $filepath;
        }

        return new Response(201);
    }
    if ('/' === $path) {
        if (null === $codePath) {
            $logger->error("$codePath not found");
            return new Response(500, [], 'Generic container: no requests supported');
        }

        ob_start();

        if (!file_exists($codePath)) {
            $logger->error("$codePath not found");
            ob_end_clean();
            return new Response(500, [], "$codePath not found");
        }

        try {
            $parser = (new ParserFactory)->createForNewestSupportedVersion();
            $parser->parse(file_get_contents($codePath));
        } catch (Throwable $throwable) {
            $logger->error($codePath . ' - ' . $throwable->getMessage());
            ob_end_clean();
            return new Response(500, [], $codePath . ' - ' . $throwable->getMessage());
        }

        //backwards compatibility: php code didn't have userFunction, return the content
        if ($userFunction === null) {

            require $codePath;
            $bodyRowContent = ob_get_contents();
            ob_end_clean();

            return new Response(200, [], $bodyRowContent);
        }

        require_once $codePath;

        //If the function as a handler class it will be called with request, response and logger
        if (function_exists($userFunction)) {
            $response = new Response();
            ob_end_clean();
            $userFunction(['request' =>$request, 'response' => $response, 'logger' => $logger]);
            return $response;
        }

        $logger->error("Handler function '$userFunction' not found in $codePath");
        ob_end_clean();
        return new Response(500, [], "Handler function '$userFunction' not found");
    }

    return new Response(404, ['Content-Type' => 'text/plain'], 'Not found');
});

// Log handler exceptions that react/http converts into generic 500 responses;
// the server keeps serving after these.
$server->on('error', function (Throwable $e) use ($logger) {
    $logger->error('Unhandled request error: ' . $e->getMessage(), ['exception' => (string) $e]);
});

$socket = new SocketServer('0.0.0.0:8888');
$server->listen($socket);
