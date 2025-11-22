<?php
/**
 * Simple PHP Authorization Form Application
 * No frameworks, no libraries - just plain PHP
 */

session_start();

// Define constants
if (!defined('ROOT_PATH')) {
    define('ROOT_PATH', __DIR__);
}
define('APP_PATH', ROOT_PATH . '/app');
define('CONFIG_PATH', ROOT_PATH . '/config');
define('VIEWS_PATH', ROOT_PATH . '/views');
define('PUBLIC_PATH', ROOT_PATH . '/public');

// Load configuration
require_once CONFIG_PATH . '/config.php';
require_once CONFIG_PATH . '/database.php';

// Load core classes
require_once APP_PATH . '/core/Router.php';
require_once APP_PATH . '/core/Controller.php';
require_once APP_PATH . '/core/Auth.php';
require_once APP_PATH . '/core/Database.php';

// Load models
require_once APP_PATH . '/models/User.php';
require_once APP_PATH . '/models/AuthorizationRequest.php';
require_once APP_PATH . '/models/Employee.php';
require_once APP_PATH . '/models/ApplicationSystem.php';
require_once APP_PATH . '/models/RequestHistory.php';
require_once APP_PATH . '/models/EmailTemplate.php';
require_once APP_PATH . '/models/FormTemplate.php';

// Initialize database
$db = Database::getInstance();

// Initialize router
$router = new Router();

// Define routes
require_once CONFIG_PATH . '/routes.php';

// Handle request
$router->dispatch();

