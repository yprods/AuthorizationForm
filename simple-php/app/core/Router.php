<?php
/**
 * Simple Router Class
 */

class Router
{
    private $routes = [];
    
    public function add($method, $path, $handler)
    {
        $this->routes[] = [
            'method' => strtoupper($method),
            'path' => $path,
            'handler' => $handler,
        ];
    }
    
    public function get($path, $handler)
    {
        $this->add('GET', $path, $handler);
    }
    
    public function post($path, $handler)
    {
        $this->add('POST', $path, $handler);
    }
    
    public function dispatch()
    {
        $method = $_SERVER['REQUEST_METHOD'];
        $path = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH);
        
        // Remove base path if exists
        $basePath = dirname($_SERVER['SCRIPT_NAME']);
        if ($basePath !== '/') {
            $path = substr($path, strlen($basePath));
        }
        
        foreach ($this->routes as $route) {
            if ($route['method'] === $method && $this->matchPath($route['path'], $path)) {
                $params = $this->extractParams($route['path'], $path);
                $this->callHandler($route['handler'], $params);
                return;
            }
        }
        
        // 404 Not Found
        http_response_code(404);
        include VIEWS_PATH . '/errors/404.php';
    }
    
    private function matchPath($routePath, $requestPath)
    {
        $routePath = preg_replace('/\{(\w+)\}/', '([^/]+)', $routePath);
        $routePath = '#^' . $routePath . '$#';
        return preg_match($routePath, $requestPath);
    }
    
    private function extractParams($routePath, $requestPath)
    {
        preg_match_all('/\{(\w+)\}/', $routePath, $paramNames);
        $routePath = preg_replace('/\{(\w+)\}/', '([^/]+)', $routePath);
        $routePath = '#^' . $routePath . '$#';
        preg_match($routePath, $requestPath, $matches);
        
        $params = [];
        if (isset($paramNames[1])) {
            foreach ($paramNames[1] as $index => $name) {
                $params[$name] = $matches[$index + 1] ?? null;
            }
        }
        
        return $params;
    }
    
    private function callHandler($handler, $params)
    {
        if (is_string($handler)) {
            // Format: "Controller@method"
            list($controller, $method) = explode('@', $handler);
            $controllerClass = ucfirst($controller) . 'Controller';
            $controllerFile = APP_PATH . '/controllers/' . $controllerClass . '.php';
            
            if (file_exists($controllerFile)) {
                require_once $controllerFile;
                $controller = new $controllerClass();
                if (method_exists($controller, $method)) {
                    $controller->$method($params);
                } else {
                    die("Method {$method} not found in {$controllerClass}");
                }
            } else {
                die("Controller {$controllerClass} not found");
            }
        } elseif (is_callable($handler)) {
            call_user_func($handler, $params);
        }
    }
}

