<?php
/**
 * AuthorizationRequest Model
 */

class AuthorizationRequest
{
    private $db;
    
    // Status constants
    const STATUS_DRAFT = 0;
    const STATUS_PENDING_MANAGER_APPROVAL = 1;
    const STATUS_PENDING_FINAL_APPROVAL = 2;
    const STATUS_APPROVED = 3;
    const STATUS_REJECTED = 4;
    const STATUS_CANCELLED_BY_USER = 5;
    const STATUS_CANCELLED_BY_MANAGER = 6;
    const STATUS_MANAGER_CHANGED = 7;
    
    // Service level constants
    const SERVICE_LEVEL_USER = 1;
    const SERVICE_LEVEL_OTHER_USER = 2;
    const SERVICE_LEVEL_MULTIPLE_USERS = 3;
    
    public function __construct()
    {
        $this->db = Database::getInstance();
    }
    
    public function findById($id)
    {
        return $this->db->fetchOne(
            "SELECT ar.*, 
                    u.name as user_name, u.full_name as user_full_name, u.email as user_email,
                    m.name as manager_name, m.full_name as manager_full_name,
                    fa.name as final_approver_name, fa.full_name as final_approver_full_name
             FROM authorization_requests ar
             LEFT JOIN users u ON ar.user_id = u.id
             LEFT JOIN users m ON ar.manager_id = m.id
             LEFT JOIN users fa ON ar.final_approver_id = fa.id
             WHERE ar.id = ?",
            [$id]
        );
    }
    
    public function create($data)
    {
        $sql = "INSERT INTO authorization_requests 
                (user_id, service_level, selected_employees, selected_systems, comments, 
                 manager_id, status, disclosure_acknowledged, created_at, updated_at) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        
        $this->db->execute($sql, [
            $data['user_id'],
            $data['service_level'],
            is_array($data['selected_employees']) ? json_encode($data['selected_employees']) : $data['selected_employees'],
            is_array($data['selected_systems']) ? json_encode($data['selected_systems']) : $data['selected_systems'],
            $data['comments'] ?? null,
            $data['manager_id'],
            $data['status'] ?? self::STATUS_DRAFT,
            $data['disclosure_acknowledged'] ?? false,
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
            if (in_array($key, ['selected_employees', 'selected_systems']) && is_array($value)) {
                $value = json_encode($value);
            }
            $fields[] = "$key = ?";
            $values[] = $value;
        }
        
        $fields[] = "updated_at = ?";
        $values[] = date('Y-m-d H:i:s');
        $values[] = $id;
        
        $sql = "UPDATE authorization_requests SET " . implode(', ', $fields) . " WHERE id = ?";
        
        return $this->db->execute($sql, $values);
    }
    
    public function getAll($filters = [])
    {
        $sql = "SELECT ar.*, 
                u.name as user_name, u.full_name as user_full_name,
                m.name as manager_name, m.full_name as manager_full_name
                FROM authorization_requests ar
                LEFT JOIN users u ON ar.user_id = u.id
                LEFT JOIN users m ON ar.manager_id = m.id
                WHERE 1=1";
        
        $params = [];
        
        if (isset($filters['user_id'])) {
            $sql .= " AND ar.user_id = ?";
            $params[] = $filters['user_id'];
        }
        
        if (isset($filters['manager_id'])) {
            $sql .= " AND ar.manager_id = ?";
            $params[] = $filters['manager_id'];
        }
        
        if (isset($filters['status'])) {
            $sql .= " AND ar.status = ?";
            $params[] = $filters['status'];
        }
        
        $sql .= " ORDER BY ar.created_at DESC";
        
        return $this->db->fetchAll($sql, $params);
    }
    
    public function addHistory($requestId, $previousStatus, $newStatus, $actionPerformedById, $comments = null)
    {
        $sql = "INSERT INTO request_histories 
                (request_id, previous_status, new_status, action_performed_by_id, comments, created_at) 
                VALUES (?, ?, ?, ?, ?, ?)";
        
        return $this->db->execute($sql, [
            $requestId,
            $previousStatus,
            $newStatus,
            $actionPerformedById,
            $comments,
            date('Y-m-d H:i:s'),
        ]);
    }
    
    public function getHistory($requestId)
    {
        return $this->db->fetchAll(
            "SELECT * FROM request_histories WHERE request_id = ? ORDER BY created_at ASC",
            [$requestId]
        );
    }
}

