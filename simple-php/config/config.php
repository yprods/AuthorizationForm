<?php
/**
 * Application Configuration
 */

if (!defined('ROOT_PATH')) {
    define('ROOT_PATH', dirname(__DIR__));
}

$config = [
    'app_name' => 'Authorization Form',
    'app_url' => 'http://localhost',
    
    // Database
    'db_type' => 'sqlite', // 'sqlite' or 'mysql'
    'db_file' => ROOT_PATH . '/database/authorization.db',
    'db_host' => 'localhost',
    'db_name' => 'authorization_db',
    'db_user' => 'root',
    'db_pass' => '',
    
    // Session
    'session_lifetime' => 7200, // 2 hours
    
    // Admin
    'admin_email' => 'admin@example.com',
    'admin_password' => 'Qa123456', // Will be hashed
    'admin_full_name' => 'מנהל מערכת',
    
    // Email (if needed)
    'email_enabled' => false,
    'smtp_host' => 'smtp.gmail.com',
    'smtp_port' => 587,
    'smtp_user' => '',
    'smtp_pass' => '',
    'smtp_from' => 'noreply@example.com',
    
    // Timezone
    'timezone' => 'Asia/Jerusalem',
    
    // Locale
    'locale' => 'he_IL',
    'charset' => 'UTF-8',
];

// Set timezone
date_default_timezone_set($config['timezone'] ?? 'UTC');

// Set locale
setlocale(LC_ALL, $config['locale'] ?? 'en_US');

return $config;

