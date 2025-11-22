<?php
/**
 * Simple Database Class using PDO
 */

class Database
{
    private static $instance = null;
    private $pdo;
    
    private function __construct()
    {
        $config = $GLOBALS['db_config'];
        
        try {
            if (isset($config['user']) && isset($config['pass'])) {
                $this->pdo = new PDO($config['dsn'], $config['user'], $config['pass'], $config['options']);
            } else {
                $this->pdo = new PDO($config['dsn'], null, null, $config['options']);
            }
            
            // Enable foreign keys for SQLite
            if (strpos($config['dsn'], 'sqlite') === 0) {
                $this->pdo->exec('PRAGMA foreign_keys = ON');
            }
        } catch (PDOException $e) {
            die('Database connection failed: ' . $e->getMessage());
        }
    }
    
    public static function getInstance()
    {
        if (self::$instance === null) {
            self::$instance = new self();
        }
        return self::$instance;
    }
    
    public function getPdo()
    {
        return $this->pdo;
    }
    
    public function query($sql, $params = [])
    {
        $stmt = $this->pdo->prepare($sql);
        $stmt->execute($params);
        return $stmt;
    }
    
    public function fetchAll($sql, $params = [])
    {
        return $this->query($sql, $params)->fetchAll();
    }
    
    public function fetchOne($sql, $params = [])
    {
        return $this->query($sql, $params)->fetch();
    }
    
    public function execute($sql, $params = [])
    {
        return $this->query($sql, $params)->rowCount();
    }
    
    public function lastInsertId()
    {
        return $this->pdo->lastInsertId();
    }
}

