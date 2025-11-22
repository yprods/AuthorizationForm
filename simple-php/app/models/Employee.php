<?php
/**
 * Employee Model
 */

class EmployeeModel
{
    private $db;
    
    public function __construct()
    {
        $this->db = Database::getInstance();
    }
    
    public function findById($id)
    {
        return $this->db->fetchOne("SELECT * FROM employees WHERE id = ?", [$id]);
    }
    
    public function findByEmployeeId($employeeId)
    {
        return $this->db->fetchOne("SELECT * FROM employees WHERE employee_id = ?", [$employeeId]);
    }
    
    public function getAll($activeOnly = false)
    {
        $sql = "SELECT * FROM employees";
        if ($activeOnly) {
            $sql .= " WHERE is_active = 1";
        }
        $sql .= " ORDER BY first_name, last_name";
        
        return $this->db->fetchAll($sql);
    }
    
    public function create($data)
    {
        $sql = "INSERT INTO employees 
                (employee_id, first_name, last_name, department, position, email, phone, is_active, created_at, updated_at) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        
        $this->db->execute($sql, [
            $data['employee_id'],
            $data['first_name'],
            $data['last_name'],
            $data['department'] ?? null,
            $data['position'] ?? null,
            $data['email'] ?? null,
            $data['phone'] ?? null,
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
        
        $sql = "UPDATE employees SET " . implode(', ', $fields) . " WHERE id = ?";
        
        return $this->db->execute($sql, $values);
    }
}

