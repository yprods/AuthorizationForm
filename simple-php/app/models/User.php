<?php
/**
 * User Model
 */

class User
{
    private $db;
    
    public function __construct()
    {
        $this->db = Database::getInstance();
    }
    
    public function findById($id)
    {
        return $this->db->fetchOne("SELECT * FROM users WHERE id = ?", [$id]);
    }
    
    public function findByEmail($email)
    {
        return $this->db->fetchOne("SELECT * FROM users WHERE email = ?", [$email]);
    }
    
    public function create($data)
    {
        $sql = "INSERT INTO users (name, email, password, full_name, department, is_manager, is_admin, manager_id, created_at) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
        
        $this->db->execute($sql, [
            $data['name'],
            $data['email'],
            password_hash($data['password'], PASSWORD_DEFAULT),
            $data['full_name'] ?? null,
            $data['department'] ?? null,
            $data['is_manager'] ?? false,
            $data['is_admin'] ?? false,
            $data['manager_id'] ?? null,
            date('Y-m-d H:i:s'),
        ]);
        
        return $this->db->lastInsertId();
    }
    
    public function update($id, $data)
    {
        $fields = [];
        $values = [];
        
        foreach ($data as $key => $value) {
            if ($key === 'password') {
                $value = password_hash($value, PASSWORD_DEFAULT);
            }
            $fields[] = "$key = ?";
            $values[] = $value;
        }
        
        $values[] = $id;
        $sql = "UPDATE users SET " . implode(', ', $fields) . " WHERE id = ?";
        
        return $this->db->execute($sql, $values);
    }
    
    public function getAll()
    {
        return $this->db->fetchAll("SELECT * FROM users ORDER BY created_at DESC");
    }
    
    public function getAdmins()
    {
        return $this->db->fetchAll("SELECT * FROM users WHERE is_admin = 1");
    }
    
    public function getManagers()
    {
        return $this->db->fetchAll("SELECT * FROM users WHERE is_manager = 1 OR is_admin = 1");
    }
}

