<?php
/**
 * Application Routes
 */

if (!isset($router)) {
    die('Router not initialized');
}

// Home
$router->get('/', 'Home@index');

// Authentication
$router->get('/login', 'Auth@showLogin');
$router->post('/login', 'Auth@login');
$router->post('/logout', 'Auth@logout');

// Requests (public - allow anonymous)
$router->get('/requests/create', 'Requests@create');
$router->post('/requests/store', 'Requests@store');

// Requests (authenticated)
$router->get('/requests', 'Requests@index');
$router->get('/requests/{id}', 'Requests@show');
$router->get('/requests/{id}/manager-approve', 'Requests@managerApprove');
$router->post('/requests/{id}/manager-approve', 'Requests@managerApproveStore');
$router->get('/requests/{id}/final-approve', 'Requests@finalApprove');
$router->post('/requests/{id}/final-approve', 'Requests@finalApproveStore');
$router->get('/requests/{id}/change-manager', 'Requests@changeManager');
$router->post('/requests/{id}/change-manager', 'Requests@changeManagerStore');

// Manager routes
$router->get('/manager', 'Manager@index');

// Admin routes
$router->get('/admin', 'Admin@index');
$router->get('/admin/users', 'Admin@users');
$router->get('/admin/users/create', 'Admin@createUser');
$router->post('/admin/users', 'Admin@storeUser');
$router->get('/admin/users/{id}/edit', 'Admin@editUser');
$router->post('/admin/users/{id}', 'Admin@updateUser');

$router->get('/admin/employees', 'Admin@employees');
$router->get('/admin/employees/create', 'Admin@createEmployee');
$router->post('/admin/employees', 'Admin@storeEmployee');
$router->get('/admin/employees/{id}/edit', 'Admin@editEmployee');
$router->post('/admin/employees/{id}', 'Admin@updateEmployee');

$router->get('/admin/systems', 'Admin@systems');
$router->get('/admin/systems/create', 'Admin@createSystem');
$router->post('/admin/systems', 'Admin@storeSystem');
$router->get('/admin/systems/{id}/edit', 'Admin@editSystem');
$router->post('/admin/systems/{id}', 'Admin@updateSystem');

// Setup
$router->get('/setup', 'Setup@index');
$router->post('/setup', 'Setup@store');

