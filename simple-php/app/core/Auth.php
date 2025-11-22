<?php
/**
 * Simple Authentication Class
 */

class Auth
{
    private static $instance = null;
    
    private function __construct()
    {
        // Start session if not already started
        if (session_status() === PHP_SESSION_NONE) {
            session_start();
        }
    }
    
    public static function getInstance()
    {
        if (self::$instance === null) {
            self::$instance = new self();
        }
        return self::$instance;
    }
    
    public function login($email, $password)
    {
        $db = Database::getInstance();
        $user = $db->fetchOne(
            "SELECT * FROM users WHERE email = ?",
            [$email]
        );
        
        if ($user && password_verify($password, $user['password'])) {
            $_SESSION['user_id'] = $user['id'];
            $_SESSION['user_email'] = $user['email'];
            $_SESSION['user_name'] = $user['name'];
            $_SESSION['is_admin'] = (bool)$user['is_admin'];
            $_SESSION['is_manager'] = (bool)$user['is_manager'];
            return true;
        }
        
        return false;
    }
    
    public function logout()
    {
        session_destroy();
        $_SESSION = [];
    }
    
    public function isLoggedIn()
    {
        return isset($_SESSION['user_id']);
    }
    
    public function isAdmin()
    {
        return isset($_SESSION['is_admin']) && $_SESSION['is_admin'] === true;
    }
    
    public function isManager()
    {
        return isset($_SESSION['is_manager']) && $_SESSION['is_manager'] === true;
    }
    
    public function getUser()
    {
        if (!$this->isLoggedIn()) {
            return null;
        }
        
        $db = Database::getInstance();
        return $db->fetchOne(
            "SELECT * FROM users WHERE id = ?",
            [$_SESSION['user_id']]
        );
    }
    
    public function getUserId()
    {
        return $_SESSION['user_id'] ?? null;
    }
}

