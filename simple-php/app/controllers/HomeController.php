<?php
/**
 * Home Controller
 */

class HomeController extends Controller
{
    public function index($params = [])
    {
        // Check if setup is needed
        if ($this->auth->isLoggedIn()) {
            $userModel = new User();
            $adminCount = count($userModel->getAdmins());
            if ($adminCount === 0) {
                $this->redirect('/setup');
            }
        }
        
        // If not authenticated, redirect to create form
        if (!$this->auth->isLoggedIn()) {
            $this->redirect('/requests/create');
        }
        
        $user = $this->auth->getUser();
        
        // Redirect based on role
        if ($this->auth->isAdmin()) {
            $this->redirect('/admin');
        }
        
        if ($this->auth->isManager()) {
            $this->redirect('/manager');
        }
        
        // Regular users go to requests
        $this->redirect('/requests');
    }
}

