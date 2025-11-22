# תיעוד API - טופס הרשאות
## API Documentation

---

## סקירה כללית

מערכת טופס הרשאות משתמשת ב-RESTful-like routes עם תמיכה ב-HTTP methods:
- **GET** - קריאת נתונים
- **POST** - יצירה/עדכון/מחיקה

---

## Authentication

### התחברות
```http
POST /login
Content-Type: application/x-www-form-urlencoded

email=admin@example.com&password=Qa123456
```

**Response Success:**
- HTTP 302 Redirect to `/`

**Response Error:**
- HTTP 200 עם הודעת שגיאה

---

### התנתקות
```http
POST /logout
```

**Response:**
- HTTP 302 Redirect to `/login`

---

## Authorization Requests

### יצירת בקשה חדשה
```http
POST /requests/store
Content-Type: application/x-www-form-urlencoded

service_level=1
&selected_employees=["1","2"]
&selected_systems=["1","3"]
&comments=הערות
&disclosure_acknowledged=on
```

**Parameters:**
- `service_level` (required) - 1, 2, or 3
- `selected_employees` (required) - JSON array of employee IDs
- `selected_systems` (required) - JSON array of system IDs
- `comments` (optional) - Text comments
- `disclosure_acknowledged` (required) - Checkbox value

**Response:**
- HTTP 302 Redirect to `/requests/{id}` or success page

---

### רשימת בקשות
```http
GET /requests
```

**Query Parameters (optional):**
- `user_id` - Filter by user ID
- `status` - Filter by status (0-7)
- `manager_id` - Filter by manager ID

**Response:**
- HTML page with requests table

---

### פרטי בקשה
```http
GET /requests/{id}
```

**Response:**
- HTML page with request details

**Status Codes:**
- 200 - Success
- 403 - Access denied
- 404 - Request not found

---

### אישור מנהל
```http
GET /requests/{id}/manager-approve
```

**Response:**
- HTML form for manager approval

```http
POST /requests/{id}/manager-approve
Content-Type: application/x-www-form-urlencoded

approved=1
&rejection_reason=
&comments=הערות
```

**Parameters:**
- `approved` (required) - 1 for approve, 0 for reject
- `rejection_reason` (optional) - Reason if rejected
- `comments` (optional) - Additional comments

**Response:**
- HTTP 302 Redirect to `/requests/{id}`

**Required Permission:** Manager or Admin

---

### אישור סופי
```http
GET /requests/{id}/final-approve
```

**Response:**
- HTML form for final approval

```http
POST /requests/{id}/final-approve
Content-Type: application/x-www-form-urlencoded

approved=1
&comments=הערות
```

**Parameters:**
- `approved` (required) - 1 for approve, 0 for reject
- `comments` (optional) - Additional comments

**Response:**
- HTTP 302 Redirect to `/requests/{id}`

**Required Permission:** Admin only

---

### שינוי מנהל
```http
GET /requests/{id}/change-manager
```

**Response:**
- HTML form for changing manager

```http
POST /requests/{id}/change-manager
Content-Type: application/x-www-form-urlencoded

manager_id=5
```

**Parameters:**
- `manager_id` (required) - New manager user ID

**Response:**
- HTTP 302 Redirect to `/requests/{id}`

**Required Permission:** Admin only

---

## Admin Endpoints

### ניהול משתמשים

#### רשימת משתמשים
```http
GET /admin/users
```

#### יצירת משתמש
```http
GET /admin/users/create
POST /admin/users
Content-Type: application/x-www-form-urlencoded

name=username
&email=user@example.com
&password=password123
&full_name=שם מלא
&department=מחלקה
&is_manager=0
&is_admin=0
&manager_id=
```

#### עריכת משתמש
```http
GET /admin/users/{id}/edit
POST /admin/users/{id}
Content-Type: application/x-www-form-urlencoded

name=username
&email=user@example.com
&full_name=שם מלא
&department=מחלקה
&is_manager=1
&is_admin=0
```

---

### ניהול עובדים

#### רשימת עובדים
```http
GET /admin/employees
```

#### יצירת עובד
```http
GET /admin/employees/create
POST /admin/employees
Content-Type: application/x-www-form-urlencoded

employee_id=EMP004
&first_name=יוסי
&last_name=כהן
&department=IT
&position=מפתח
&email=yossi@example.com
&phone=050-1234567
&is_active=1
```

#### עריכת עובד
```http
GET /admin/employees/{id}/edit
POST /admin/employees/{id}
Content-Type: application/x-www-form-urlencoded

employee_id=EMP004
&first_name=יוסי
&last_name=כהן
&department=IT
&is_active=1
```

---

### ניהול מערכות

#### רשימת מערכות
```http
GET /admin/systems
```

#### יצירת מערכת
```http
GET /admin/systems/create
POST /admin/systems
Content-Type: application/x-www-form-urlencoded

name=מערכת חדשה
&description=תיאור
&category=קטגוריה
&is_active=1
```

#### עריכת מערכת
```http
GET /admin/systems/{id}/edit
POST /admin/systems/{id}
Content-Type: application/x-www-form-urlencoded

name=מערכת מעודכנת
&description=תיאור חדש
&is_active=1
```

---

## Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 302 | Redirect |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 500 | Internal Server Error |

---

## Error Handling

### שגיאות נפוצות

**403 Forbidden:**
```
Access denied. Admin privileges required.
Access denied. Manager privileges required.
```

**404 Not Found:**
```
Request not found
User not found
```

**Validation Errors:**
- מוצגים בטופס עם הודעת שגיאה
- שדות חובה מסומנים

---

## Data Formats

### JSON Arrays
בקשות משתמשות ב-JSON arrays עבור:
- `selected_employees`: `["1","2","3"]`
- `selected_systems`: `["1","2"]`

### Dates
פורמט תאריך: `YYYY-MM-DD HH:MM:SS`
דוגמה: `2024-01-15 14:30:00`

### Status Values
- `0` - Draft
- `1` - Pending Manager Approval
- `2` - Pending Final Approval
- `3` - Approved
- `4` - Rejected
- `5` - Cancelled By User
- `6` - Cancelled By Manager
- `7` - Manager Changed

---

## Security Notes

1. כל ה-POST requests דורשים CSRF protection (מומלץ להוסיף)
2. Authentication נדרש לרוב ה-endpoints
3. Authorization נבדק בכל request
4. Input validation מתבצע על כל הקלטים
5. SQL injection מונע עם PDO prepared statements

---

## Rate Limiting

כרגע אין rate limiting. מומלץ להוסיף:
- מקסימום X requests לדקה
- מקסימום Y login attempts לשעה

---

**גרסה:** 1.0  
**תאריך עדכון:** 2024

