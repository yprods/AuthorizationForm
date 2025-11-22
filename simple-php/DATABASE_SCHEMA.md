# סכמת מסד נתונים - טופס הרשאות
## Database Schema Documentation

---

## סקירה כללית

מסד הנתונים משתמש ב-SQLite עם תמיכה ב-Foreign Keys. ניתן להמיר ל-MySQL בקלות.

---

## טבלאות

### 1. users

משתמשי המערכת

```sql
CREATE TABLE users (
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
```

**Indexes:**
- `email` - UNIQUE
- `manager_id` - Foreign key index

**Relationships:**
- `manager_id` → `users.id` (self-referencing)

---

### 2. employees

רשימת עובדים ארגוניים

```sql
CREATE TABLE employees (
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
```

**Indexes:**
- `employee_id` - UNIQUE

---

### 3. systems

מערכות ארגוניות

```sql
CREATE TABLE systems (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    description TEXT,
    category TEXT,
    is_active INTEGER DEFAULT 1,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);
```

**Seed Data:**
```sql
INSERT INTO systems (id, name, description, category, is_active) VALUES
(1, 'מערכת HR', 'מערכת ניהול משאבי אנוש', 'ניהול', 1),
(2, 'מערכת כספים', 'מערכת ניהול כספים', 'כספים', 1),
(3, 'מערכת מכירות', 'מערכת ניהול מכירות', 'מכירות', 1),
(4, 'מערכת לוגיסטיקה', 'מערכת ניהול לוגיסטיקה', 'לוגיסטיקה', 1);
```

---

### 4. authorization_requests

בקשות הרשאה

```sql
CREATE TABLE authorization_requests (
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
```

**Indexes:**
- `user_id` - Foreign key index
- `manager_id` - Foreign key index
- `status` - For filtering
- `created_at` - For sorting

**JSON Fields:**
- `selected_employees` - JSON array: `["1","2","3"]`
- `selected_systems` - JSON array: `["1","2"]`

**Status Values:**
- `0` - Draft
- `1` - Pending Manager Approval
- `2` - Pending Final Approval
- `3` - Approved
- `4` - Rejected
- `5` - Cancelled By User
- `6` - Cancelled By Manager
- `7` - Manager Changed

**Service Level Values:**
- `1` - User Level
- `2` - Other User Level
- `3` - Multiple Users

---

### 5. request_histories

היסטוריית שינויים בבקשות

```sql
CREATE TABLE request_histories (
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
```

**Indexes:**
- `request_id` - Foreign key index
- `created_at` - For sorting

---

### 6. email_templates

תבניות אימייל

```sql
CREATE TABLE email_templates (
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
```

**Trigger Types:**
- `1` - Request Created
- `2` - Manager Approval Request
- `3` - Manager Approved
- `4` - Manager Rejected
- `5` - Final Approval Request
- `6` - Final Approved
- `7` - Final Rejected
- `8` - Request Cancelled By User
- `9` - Request Cancelled By Manager
- `10` - Status Changed

**Recipient Types:**
- `User` - Request creator
- `Manager` - Request manager
- `FinalApprover` - Final approver
- `Custom` - Custom recipients from `custom_recipients` field

---

### 7. form_templates

תבניות טפסים

```sql
CREATE TABLE form_templates (
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
```

---

## Foreign Key Relationships

```
users
├── manager_id → users.id (self-reference)
└── (referenced by)
    ├── authorization_requests.user_id
    ├── authorization_requests.manager_id
    ├── authorization_requests.final_approver_id
    ├── authorization_requests.changed_by_admin_id
    ├── email_templates.created_by_id
    └── form_templates.created_by_id

authorization_requests
└── (referenced by)
    └── request_histories.request_id
```

---

## Indexes

### Primary Indexes
- כל הטבלאות: `id` (PRIMARY KEY)

### Unique Indexes
- `users.email`
- `employees.employee_id`

### Foreign Key Indexes
- `users.manager_id`
- `authorization_requests.user_id`
- `authorization_requests.manager_id`
- `authorization_requests.final_approver_id`
- `request_histories.request_id`

### Performance Indexes
- `authorization_requests.status`
- `authorization_requests.created_at`
- `request_histories.created_at`

---

## Data Types

### SQLite Types
- `INTEGER` - מספרים שלמים
- `TEXT` - מחרוזות (תמיכה בעברית)
- `REAL` - מספרים עשרוניים (לא בשימוש)

### Boolean Representation
- `0` = false
- `1` = true

### Date/Time Format
- `TEXT` format: `YYYY-MM-DD HH:MM:SS`
- Example: `2024-01-15 14:30:00`

---

## Constraints

### NOT NULL Constraints
- `users.name`, `users.email`, `users.password`
- `employees.employee_id`, `employees.first_name`, `employees.last_name`
- `systems.name`
- `authorization_requests.user_id`, `authorization_requests.manager_id`
- `request_histories.request_id`, `request_histories.previous_status`, `request_histories.new_status`

### UNIQUE Constraints
- `users.email`
- `employees.employee_id`

### Foreign Key Constraints
- כל ה-Foreign Keys עם `ON DELETE RESTRICT` או `ON DELETE CASCADE`
- `request_histories.request_id` - CASCADE (מחיקת בקשה מוחקת היסטוריה)
- כל השאר - RESTRICT (מונע מחיקה אם יש תלויות)

---

## Default Values

### Timestamps
- `created_at` - `CURRENT_TIMESTAMP`
- `updated_at` - `CURRENT_TIMESTAMP` (מתעדכן ידנית)

### Boolean Fields
- `is_active` - `1` (true)
- `is_manager` - `0` (false)
- `is_admin` - `0` (false)
- `disclosure_acknowledged` - `0` (false)

### Status Fields
- `authorization_requests.status` - `0` (Draft)
- `reminder_count` - `0`

---

## Seed Data

### Default Admin User
```sql
INSERT INTO users (name, email, password, full_name, is_admin, is_manager) 
VALUES ('admin', 'admin@example.com', '$2y$10$...', 'מנהל מערכת', 1, 0);
```
Password hash for `Qa123456`

### Default Systems
4 מערכות ברירת מחדל (ראה לעיל)

### Sample Employees
3 עובדים לדוגמה:
- EMP001 - יוסי כהן (IT)
- EMP002 - שרה לוי (HR)
- EMP003 - דוד ישראלי (כספים)

---

## Migration to MySQL

להמרה ל-MySQL, שנה:

1. **Data Types:**
   - `TEXT` → `TEXT` או `VARCHAR(n)`
   - `INTEGER` → `INT` או `BIGINT`
   - `AUTOINCREMENT` → `AUTO_INCREMENT`

2. **Timestamps:**
   - `TEXT DEFAULT CURRENT_TIMESTAMP` → `DATETIME DEFAULT CURRENT_TIMESTAMP`
   - `updated_at` → `DATETIME ON UPDATE CURRENT_TIMESTAMP`

3. **Boolean:**
   - `INTEGER` → `TINYINT(1)` או `BOOLEAN`

4. **Character Set:**
   - הוסף: `CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci`

---

## Backup & Restore

### Backup
```bash
sqlite3 database/authorization.db .dump > backup.sql
```

### Restore
```bash
sqlite3 database/authorization.db < backup.sql
```

---

**גרסה:** 1.0  
**תאריך עדכון:** 2024

