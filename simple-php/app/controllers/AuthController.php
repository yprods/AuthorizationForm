<?php
/**
 * Authentication Controller
 */

class AuthController extends Controller
{
    public function showLogin($params = [])
    {
        if ($this->auth->isLoggedIn()) {
            $this->redirect('/');
        }
        
        $this->view('auth/login');
    }
    
    public function login($params = [])
    {
        if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
            $this->redirect('/login');
        }
        
        $email = $_POST['email'] ?? '';
        $password = $_POST['password'] ?? '';
        
        if ($this->auth->login($email, $password)) {
            $this->redirect('/');
        } else {
            $error = 'Invalid email or password';
            $this->view('auth/login', ['error' => $error]);
        }
    }
    
    public function logout($params = [])
    {
        $this->auth->logout();
        $this->redirect('/login');
    }
}

