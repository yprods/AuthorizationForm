<?php
/**
 * Database Configuration
 */

$config = require __DIR__ . '/config.php';

// Database configuration based on type
if ($config['db_type'] === 'sqlite') {
    $dsn = 'sqlite:' . $config['db_file'];
    $options = [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
    ];
} else {
    $dsn = "mysql:host={$config['db_host']};dbname={$config['db_name']};charset=utf8mb4";
    $options = [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        PDO::ATTR_EMULATE_PREPARES => false,
    ];
}

// Store in global for Database class
$GLOBALS['db_config'] = [
    'dsn' => $dsn,
    'user' => $config['db_user'] ?? null,
    'pass' => $config['db_pass'] ?? null,
    'options' => $options,
];

