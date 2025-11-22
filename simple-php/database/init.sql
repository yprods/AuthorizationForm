-- Simple PHP Authorization Form Database Schema
-- Run this SQL file to create the database

PRAGMA foreign_keys = ON;

-- Users table
CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    full_name TEXT,
    department TEXT,
    is_manager INTEGER DEFAULT 0,
    is_admin INTEGER DEFAULT 0,
    manager_id INTEGER,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (manager_id) REFERENCES users(id) ON DELETE RESTRICT
);

-- Employees table
CREATE TABLE IF NOT EXISTS employees (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    employee_id TEXT UNIQUE NOT NULL,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    department TEXT,
    position TEXT,
    email TEXT,
    phone TEXT,
    is_active INTEGER DEFAULT 1,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

-- Systems table
CREATE TABLE IF NOT EXISTS systems (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    description TEXT,
    category TEXT,
    is_active INTEGER DEFAULT 1,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

-- Authorization Requests table
CREATE TABLE IF NOT EXISTS authorization_requests (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    service_level INTEGER NOT NULL,
    selected_employees TEXT NOT NULL,
    selected_systems TEXT NOT NULL,
    comments TEXT,
    manager_id INTEGER NOT NULL,
    final_approver_id INTEGER,
    status INTEGER DEFAULT 0,
    manager_approved_at TEXT,
    manager_approval_signature TEXT,
    final_approved_at TEXT,
    final_approval_decision TEXT,
    final_approval_comments TEXT,
    disclosure_acknowledged INTEGER DEFAULT 0,
    disclosure_acknowledged_at TEXT,
    rejection_reason TEXT,
    changed_by_admin_id INTEGER,
    previous_manager_id INTEGER,
    manager_changed_at TEXT,
    pdf_path TEXT,
    last_reminder_sent_at TEXT,
    reminder_count INTEGER DEFAULT 0,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (manager_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (final_approver_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (changed_by_admin_id) REFERENCES users(id) ON DELETE RESTRICT
);

-- Request Histories table
CREATE TABLE IF NOT EXISTS request_histories (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    request_id INTEGER NOT NULL,
    previous_status INTEGER NOT NULL,
    new_status INTEGER NOT NULL,
    action_performed_by TEXT,
    action_performed_by_id TEXT,
    comments TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (request_id) REFERENCES authorization_requests(id) ON DELETE CASCADE
);

-- Email Templates table
CREATE TABLE IF NOT EXISTS email_templates (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    description TEXT,
    trigger_type INTEGER NOT NULL,
    subject TEXT NOT NULL,
    body TEXT NOT NULL,
    is_active INTEGER DEFAULT 1,
    created_by_id INTEGER NOT NULL,
    recipient_type TEXT DEFAULT 'User',
    custom_recipients TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by_id) REFERENCES users(id) ON DELETE RESTRICT
);

-- Form Templates table
CREATE TABLE IF NOT EXISTS form_templates (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    description TEXT,
    template_content TEXT NOT NULL,
    pdf_template_path TEXT,
    is_active INTEGER DEFAULT 1,
    created_by_id INTEGER NOT NULL,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by_id) REFERENCES users(id) ON DELETE RESTRICT
);

-- Insert default admin user (password: Qa123456)
INSERT OR IGNORE INTO users (name, email, password, full_name, is_admin, is_manager) 
VALUES ('admin', 'admin@example.com', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'מנהל מערכת', 1, 0);

-- Insert default systems
INSERT OR IGNORE INTO systems (id, name, description, category, is_active) VALUES
(1, 'מערכת HR', 'מערכת ניהול משאבי אנוש', 'ניהול', 1),
(2, 'מערכת כספים', 'מערכת ניהול כספים', 'כספים', 1),
(3, 'מערכת מכירות', 'מערכת ניהול מכירות', 'מכירות', 1),
(4, 'מערכת לוגיסטיקה', 'מערכת ניהול לוגיסטיקה', 'לוגיסטיקה', 1);

-- Insert sample employees
INSERT OR IGNORE INTO employees (employee_id, first_name, last_name, department, email, is_active) VALUES
('EMP001', 'יוסי', 'כהן', 'IT', 'yossi@example.com', 1),
('EMP002', 'שרה', 'לוי', 'HR', 'sara@example.com', 1),
('EMP003', 'דוד', 'ישראלי', 'כספים', 'david@example.com', 1);

