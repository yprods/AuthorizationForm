<?php
/**
 * ApplicationSystem Model
 */

class ApplicationSystem
{
    private $db;
    
    public function __construct()
    {
        $this->db = Database::getInstance();
    }
    
    public function findById($id)
    {
        return $this->db->fetchOne("SELECT * FROM systems WHERE id = ?", [$id]);
    }
    
    public function getAll($activeOnly = false)
    {
        $sql = "SELECT * FROM systems";
        if ($activeOnly) {
            $sql .= " WHERE is_active = 1";
        }
        $sql .= " ORDER BY category, name";
        
        return $this->db->fetchAll($sql);
    }
    
    public function create($data)
    {
        $sql = "INSERT INTO systems (name, description, category, is_active, created_at, updated_at) 
                VALUES (?, ?, ?, ?, ?, ?)";
        
        $this->db->execute($sql, [
            $data['name'],
            $data['description'] ?? null,
            $data['category'] ?? null,
            $data['is_active'] ?? true,
            date('Y-m-d H:i:s'),
            date('Y-m-d H:i:s'),
        ]);
        
        return $this->db->lastInsertId();
    }
    
    public function update($id, $data)
    {
        $fields = [];
        $values = [];
        
        foreach ($data as $key => $value) {
            $fields[] = "$key = ?";
            $values[] = $value;
        }
        
        $fields[] = "updated_at = ?";
        $values[] = date('Y-m-d H:i:s');
        $values[] = $id;
        
        $sql = "UPDATE systems SET " . implode(', ', $fields) . " WHERE id = ?";
        
        return $this->db->execute($sql, $values);
    }
}

