<?php
/**
 * Base Controller Class
 */

class Controller
{
    protected $db;
    protected $auth;
    
    public function __construct()
    {
        $this->db = Database::getInstance();
        $this->auth = Auth::getInstance();
    }
    
    protected function view($viewName, $data = [])
    {
        extract($data);
        $viewFile = VIEWS_PATH . '/' . $viewName . '.php';
        
        if (!file_exists($viewFile)) {
            die("View {$viewName} not found");
        }
        
        include $viewFile;
    }
    
    protected function redirect($url)
    {
        header('Location: ' . $url);
        exit;
    }
    
    protected function json($data, $statusCode = 200)
    {
        http_response_code($statusCode);
        header('Content-Type: application/json; charset=utf-8');
        echo json_encode($data, JSON_UNESCAPED_UNICODE);
        exit;
    }
    
    protected function requireAuth()
    {
        if (!$this->auth->isLoggedIn()) {
            $this->redirect('/login');
        }
    }
    
    protected function requireAdmin()
    {
        $this->requireAuth();
        if (!$this->auth->isAdmin()) {
            http_response_code(403);
            die('Access denied. Admin privileges required.');
        }
    }
    
    protected function requireManager()
    {
        $this->requireAuth();
        if (!$this->auth->isManager() && !$this->auth->isAdmin()) {
            http_response_code(403);
            die('Access denied. Manager privileges required.');
        }
    }
}

