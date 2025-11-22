# אפיון מערכת - טופס הרשאות
## Authorization Form System Specification

---

## תוכן עניינים

1. [סקירה כללית](#סקירה-כללית)
2. [דרישות מערכת](#דרישות-מערכת)
3. [ארכיטקטורה](#ארכיטקטורה)
4. [מודלים וטבלאות](#מודלים-וטבלאות)
5. [תפקידים והרשאות](#תפקידים-והרשאות)
6. [תהליכים עסקיים](#תהליכים-עסקיים)
7. [ממשק משתמש](#ממשק-משתמש)
8. [API ו-Routes](#api-ורoutes)
9. [אבטחה](#אבטחה)
10. [התקנה והפעלה](#התקנה-והפעלה)

---

## סקירה כללית

### מטרת המערכת
מערכת לניהול בקשות הרשאות למערכות ארגוניות. המערכת מאפשרת למשתמשים ליצור בקשות הרשאה, למנהלים לאשר אותן, ולמנהלי מערכת לנהל את כל התהליך.

### קהל יעד
- **משתמשים רגילים** - יוצרי בקשות הרשאה
- **מנהלים** - מאשרי בקשות ברמה ראשונה
- **מנהלי מערכת** - ניהול מלא של המערכת

### טכנולוגיות
- **שפת תכנות:** PHP 7.4+ (ללא מסגרות)
- **מסד נתונים:** SQLite (ניתן להמיר ל-MySQL)
- **אימות:** Session-based authentication
- **פרונט-אנד:** HTML, CSS, JavaScript (Vanilla)

---

## דרישות מערכת

### דרישות חומרה
- **שרת:** כל שרת תומך PHP
- **זיכרון:** מינימום 128MB RAM
- **אחסון:** 50MB+ (תלוי בגודל המסד נתונים)

### דרישות תוכנה
- **PHP:** גרסה 7.4 ומעלה (מומלץ 8.0+)
- **הרחבות PHP:**
  - PDO
  - SQLite3 (או MySQL)
  - Session
  - JSON
  - mbstring (לעברית)

### דרישות תשתית
- שרת web (Apache/Nginx) או PHP Built-in Server
- גישה לכתיבה בתיקיית `database/`

---

## ארכיטקטורה

### מבנה תיקיות
```
simple-php/
├── index.php                 # נקודת כניסה ראשית
├── config/                   # קבצי הגדרות
│   ├── config.php           # הגדרות כלליות
│   ├── database.php         # הגדרות מסד נתונים
│   └── routes.php           # הגדרות נתיבים
├── app/
│   ├── core/                # מחלקות ליבה
│   │   ├── Router.php      # מערכת ניתוב
│   │   ├── Controller.php  # מחלקת בסיס לבקרים
│   │   ├── Auth.php        # מערכת אימות
│   │   └── Database.php    # ממשק למסד נתונים
│   ├── models/             # מודלים
│   │   ├── User.php
│   │   ├── AuthorizationRequest.php
│   │   ├── Employee.php
│   │   └── ApplicationSystem.php
│   └── controllers/        # בקרים
│       ├── HomeController.php
│       ├── AuthController.php
│       └── RequestsController.php
├── views/                   # תבניות תצוגה
│   ├── layout.php          # תבנית בסיס
│   ├── auth/
│   ├── requests/
│   └── errors/
├── database/               # מסד נתונים
│   ├── authorization.db   # קובץ SQLite
│   └── init.sql           # סקריפט יצירת מסד נתונים
└── public/                 # קבצים ציבוריים (CSS, JS, תמונות)
```

### תבנית MVC
- **Model** - מחלקות גישה לנתונים (`app/models/`)
- **View** - תבניות תצוגה (`views/`)
- **Controller** - לוגיקת בקרה (`app/controllers/`)

---

## מודלים וטבלאות

### 1. Users (משתמשים)

**תיאור:** טבלת משתמשי המערכת

**שדות:**
| שדה | סוג | תיאור | אילוצים |
|------|-----|-------|---------|
| id | INTEGER | מזהה ייחודי | PRIMARY KEY, AUTO_INCREMENT |
| name | TEXT | שם משתמש | NOT NULL |
| email | TEXT | כתובת אימייל | NOT NULL, UNIQUE |
| password | TEXT | סיסמה מוצפנת | NOT NULL |
| full_name | TEXT | שם מלא | NULL |
| department | TEXT | מחלקה | NULL |
| is_manager | INTEGER | האם מנהל | DEFAULT 0 |
| is_admin | INTEGER | האם מנהל מערכת | DEFAULT 0 |
| manager_id | INTEGER | מזהה מנהל | FOREIGN KEY → users.id |
| created_at | TEXT | תאריך יצירה | DEFAULT CURRENT_TIMESTAMP |

**יחסים:**
- `manager_id` → `users.id` (מנהל)
- משתמש יכול להיות מנהל של משתמשים אחרים

---

### 2. Employees (עובדים)

**תיאור:** רשימת עובדים ארגוניים

**שדות:**
| שדה | סוג | תיאור | אילוצים |
|------|-----|-------|---------|
| id | INTEGER | מזהה ייחודי | PRIMARY KEY |
| employee_id | TEXT | מספר עובד | NOT NULL, UNIQUE |
| first_name | TEXT | שם פרטי | NOT NULL |
| last_name | TEXT | שם משפחה | NOT NULL |
| department | TEXT | מחלקה | NULL |
| position | TEXT | תפקיד | NULL |
| email | TEXT | אימייל | NULL |
| phone | TEXT | טלפון | NULL |
| is_active | INTEGER | פעיל | DEFAULT 1 |
| created_at | TEXT | תאריך יצירה | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TEXT | תאריך עדכון | DEFAULT CURRENT_TIMESTAMP |

---

### 3. Systems (מערכות)

**תיאור:** רשימת מערכות ארגוניות

**שדות:**
| שדה | סוג | תיאור | אילוצים |
|------|-----|-------|---------|
| id | INTEGER | מזהה ייחודי | PRIMARY KEY |
| name | TEXT | שם המערכת | NOT NULL |
| description | TEXT | תיאור | NULL |
| category | TEXT | קטגוריה | NULL |
| is_active | INTEGER | פעיל | DEFAULT 1 |
| created_at | TEXT | תאריך יצירה | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TEXT | תאריך עדכון | DEFAULT CURRENT_TIMESTAMP |

**נתונים ראשוניים:**
- מערכת HR
- מערכת כספים
- מערכת מכירות
- מערכת לוגיסטיקה

---

### 4. AuthorizationRequests (בקשות הרשאה)

**תיאור:** בקשות הרשאה למערכות

**שדות:**
| שדה | סוג | תיאור | אילוצים |
|------|-----|-------|---------|
| id | INTEGER | מזהה ייחודי | PRIMARY KEY |
| user_id | INTEGER | מזהה משתמש | FOREIGN KEY → users.id |
| service_level | INTEGER | רמת שירות | NOT NULL (1-3) |
| selected_employees | TEXT | עובדים נבחרים (JSON) | NOT NULL |
| selected_systems | TEXT | מערכות נבחרות (JSON) | NOT NULL |
| comments | TEXT | הערות | NULL |
| manager_id | INTEGER | מזהה מנהל | FOREIGN KEY → users.id |
| final_approver_id | INTEGER | מזהה מאשר סופי | FOREIGN KEY → users.id |
| status | INTEGER | סטטוס | DEFAULT 0 (0-7) |
| manager_approved_at | TEXT | תאריך אישור מנהל | NULL |
| manager_approval_signature | TEXT | חתימת מנהל | NULL |
| final_approved_at | TEXT | תאריך אישור סופי | NULL |
| final_approval_decision | TEXT | החלטה סופית | NULL |
| final_approval_comments | TEXT | הערות אישור סופי | NULL |
| disclosure_acknowledged | INTEGER | אישור גילוי | DEFAULT 0 |
| disclosure_acknowledged_at | TEXT | תאריך אישור גילוי | NULL |
| rejection_reason | TEXT | סיבת דחייה | NULL |
| changed_by_admin_id | INTEGER | שונה על ידי | FOREIGN KEY → users.id |
| previous_manager_id | INTEGER | מנהל קודם | FOREIGN KEY → users.id |
| manager_changed_at | TEXT | תאריך שינוי מנהל | NULL |
| pdf_path | TEXT | נתיב קובץ PDF | NULL |
| last_reminder_sent_at | TEXT | תאריך תזכורת אחרונה | NULL |
| reminder_count | INTEGER | מספר תזכורות | DEFAULT 0 |
| created_at | TEXT | תאריך יצירה | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TEXT | תאריך עדכון | DEFAULT CURRENT_TIMESTAMP |

**סטטוסים:**
- `0` - טיוטה (Draft)
- `1` - ממתין לאישור מנהל (PendingManagerApproval)
- `2` - ממתין לאישור סופי (PendingFinalApproval)
- `3` - אושר (Approved)
- `4` - נדחה (Rejected)
- `5` - בוטל על ידי משתמש (CancelledByUser)
- `6` - בוטל על ידי מנהל (CancelledByManager)
- `7` - מנהל שונה (ManagerChanged)

**רמות שירות:**
- `1` - רמת משתמש (UserLevel)
- `2` - רמת משתמש אחר (OtherUserLevel)
- `3` - מספר משתמשים (MultipleUsers)

---

### 5. RequestHistories (היסטוריית בקשות)

**תיאור:** מעקב אחר שינויים בבקשות

**שדות:**
| שדה | סוג | תיאור | אילוצים |
|------|-----|-------|---------|
| id | INTEGER | מזהה ייחודי | PRIMARY KEY |
| request_id | INTEGER | מזהה בקשה | FOREIGN KEY → authorization_requests.id |
| previous_status | INTEGER | סטטוס קודם | NOT NULL |
| new_status | INTEGER | סטטוס חדש | NOT NULL |
| action_performed_by | TEXT | שם מבצע | NULL |
| action_performed_by_id | TEXT | מזהה מבצע | NULL |
| comments | TEXT | הערות | NULL |
| created_at | TEXT | תאריך יצירה | DEFAULT CURRENT_TIMESTAMP |

---

### 6. EmailTemplates (תבניות אימייל)

**תיאור:** תבניות אימייל לשליחה אוטומטית

**שדות:**
| שדה | סוג | תיאור | אילוצים |
|------|-----|-------|---------|
| id | INTEGER | מזהה ייחודי | PRIMARY KEY |
| name | TEXT | שם תבנית | NOT NULL |
| description | TEXT | תיאור | NULL |
| trigger_type | INTEGER | סוג טריגר | NOT NULL (1-10) |
| subject | TEXT | נושא | NOT NULL |
| body | TEXT | תוכן (HTML) | NOT NULL |
| is_active | INTEGER | פעיל | DEFAULT 1 |
| created_by_id | INTEGER | נוצר על ידי | FOREIGN KEY → users.id |
| recipient_type | TEXT | סוג נמען | DEFAULT 'User' |
| custom_recipients | TEXT | נמענים מותאמים | NULL |
| created_at | TEXT | תאריך יצירה | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TEXT | תאריך עדכון | DEFAULT CURRENT_TIMESTAMP |

**סוגי טריגרים:**
- `1` - כשבקשה נוצרת
- `2` - כשמבקשים אישור מנהל
- `3` - כשמנהל מאשר
- `4` - כשמנהל דוחה
- `5` - כשמבקשים אישור סופי
- `6` - כשמאשרים סופית
- `7` - כשדוחים סופית
- `8` - כשמשתמש מבטל
- `9` - כשמנהל מבטל
- `10` - כשהסטטוס משתנה

---

### 7. FormTemplates (תבניות טפסים)

**תיאור:** תבניות טפסים מותאמות

**שדות:**
| שדה | סוג | תיאור | אילוצים |
|------|-----|-------|---------|
| id | INTEGER | מזהה ייחודי | PRIMARY KEY |
| name | TEXT | שם תבנית | NOT NULL |
| description | TEXT | תיאור | NULL |
| template_content | TEXT | תוכן תבנית (JSON/HTML) | NOT NULL |
| pdf_template_path | TEXT | נתיב תבנית PDF | NULL |
| is_active | INTEGER | פעיל | DEFAULT 1 |
| created_by_id | INTEGER | נוצר על ידי | FOREIGN KEY → users.id |
| created_at | TEXT | תאריך יצירה | DEFAULT CURRENT_TIMESTAMP |
| updated_at | TEXT | תאריך עדכון | DEFAULT CURRENT_TIMESTAMP |

---

## תפקידים והרשאות

### 1. משתמש רגיל (User)
**הרשאות:**
- ✅ יצירת בקשות הרשאה חדשות
- ✅ צפייה בבקשות שלו
- ✅ עריכת בקשות בטיוטה
- ✅ ביטול בקשות שלו
- ❌ צפייה בבקשות של אחרים
- ❌ אישור/דחיית בקשות
- ❌ ניהול משתמשים/מערכות

### 2. מנהל (Manager)
**הרשאות:**
- ✅ כל ההרשאות של משתמש רגיל
- ✅ צפייה בבקשות של עובדיו
- ✅ אישור/דחיית בקשות ברמה ראשונה
- ✅ שינוי מנהל לבקשה
- ✅ צפייה בדוחות
- ❌ ניהול משתמשים/מערכות
- ❌ אישור סופי

### 3. מנהל מערכת (Admin)
**הרשאות:**
- ✅ כל ההרשאות של מנהל
- ✅ ניהול משתמשים (יצירה, עריכה, מחיקה)
- ✅ ניהול עובדים
- ✅ ניהול מערכות
- ✅ ניהול תבניות אימייל
- ✅ ניהול תבניות טפסים
- ✅ אישור סופי של בקשות
- ✅ שינוי כל פרטי בקשה
- ✅ צפייה בכל הבקשות
- ✅ גישה לדוחות מלאים

---

## תהליכים עסקיים

### תהליך 1: יצירת בקשה חדשה

**שלבים:**
1. משתמש נכנס לטופס יצירת בקשה
2. בוחר רמת שירות (1-3)
3. בוחר עובדים מהרשימה
4. בוחר מערכות מהרשימה
5. מוסיף הערות (אופציונלי)
6. מאשר תנאי גילוי
7. שולח בקשה

**תוצאה:**
- בקשה נוצרת בסטטוס "טיוטה" (0)
- רשומה נוצרת בטבלת `authorization_requests`
- רשומה נוצרת ב-`request_histories`

**תנאים:**
- חובה לבחור לפחות עובד אחד
- חובה לבחור לפחות מערכת אחת
- חובה לאשר תנאי גילוי

---

### תהליך 2: אישור מנהל

**שלבים:**
1. מנהל נכנס לדף הבקשות שלו
2. בוחר בקשה ממתינה לאישור
3. בודק פרטי הבקשה
4. מחליט: מאשר או דוחה
5. אם דוחה - מוסיף סיבת דחייה
6. שולח החלטה

**תוצאה אם מאשר:**
- סטטוס משתנה ל-"ממתין לאישור סופי" (2)
- `manager_approved_at` מתעדכן
- רשומה נוצרת ב-`request_histories`

**תוצאה אם דוחה:**
- סטטוס משתנה ל-"נדחה" (4)
- `rejection_reason` מתעדכן
- רשומה נוצרת ב-`request_histories`

**תנאים:**
- רק מנהל הבקשה יכול לאשר
- מנהל מערכת יכול לאשר כל בקשה

---

### תהליך 3: אישור סופי

**שלבים:**
1. מנהל מערכת נכנס לבקשות ממתינות
2. בוחר בקשה לאישור סופי
3. בודק פרטי הבקשה והאישור הראשוני
4. מחליט: מאשר או דוחה
5. מוסיף הערות (אופציונלי)
6. שולח החלטה

**תוצאה אם מאשר:**
- סטטוס משתנה ל-"אושר" (3)
- `final_approved_at` מתעדכן
- `final_approval_decision` = "Approved"
- רשומה נוצרת ב-`request_histories`

**תוצאה אם דוחה:**
- סטטוס משתנה ל-"נדחה" (4)
- `final_approval_decision` = "Rejected"
- רשומה נוצרת ב-`request_histories`

**תנאים:**
- רק מנהל מערכת יכול לאשר סופית
- הבקשה חייבת להיות באישור מנהל קודם

---

### תהליך 4: שינוי מנהל

**שלבים:**
1. מנהל מערכת בוחר בקשה
2. בוחר "שינוי מנהל"
3. בוחר מנהל חדש מהרשימה
4. שולח שינוי

**תוצאה:**
- `previous_manager_id` מתעדכן
- `manager_id` משתנה למנהל החדש
- `manager_changed_at` מתעדכן
- סטטוס משתנה ל-"מנהל שונה" (7)
- רשומה נוצרת ב-`request_histories`

**תנאים:**
- רק מנהל מערכת יכול לשנות מנהל
- המנהל החדש חייב להיות פעיל

---

## ממשק משתמש

### דף בית (/)
- אם לא מחובר → הפניה לטופס יצירת בקשה
- אם מחובר כמנהל מערכת → הפניה לדף ניהול
- אם מחובר כמנהל → הפניה לדף מנהל
- אם מחובר כמשתמש → הפניה לרשימת בקשות

### דף התחברות (/login)
- שדה אימייל
- שדה סיסמה
- כפתור "התחבר"
- הודעת שגיאה במידת צורך

### טופס יצירת בקשה (/requests/create)
- בחירת רמת שירות (dropdown)
- בחירת עובדים (multi-select)
- בחירת מערכות (checkboxes)
- שדה הערות (textarea)
- checkbox לאישור גילוי
- כפתור "שלח בקשה"

### רשימת בקשות (/requests)
- טבלה עם:
  - מספר בקשה
  - שם משתמש
  - סטטוס
  - תאריך יצירה
  - כפתור "צפה"
- פילטרים (למנהלי מערכת):
  - לפי משתמש
  - לפי סטטוס
  - לפי תאריך

### פרטי בקשה (/requests/{id})
- מידע כללי:
  - מספר בקשה
  - משתמש
  - מנהל
  - סטטוס
  - תאריכים
- פרטי הבקשה:
  - רמת שירות
  - עובדים נבחרים
  - מערכות נבחרות
  - הערות
- היסטוריה:
  - טבלת שינויים
  - תאריכים
  - מבצעים
- כפתורי פעולה (לפי הרשאות):
  - אישור מנהל
  - אישור סופי
  - שינוי מנהל
  - ביטול

---

## API ו-Routes

### Routes ציבוריים (ללא אימות)

| Method | Route | Controller | תיאור |
|--------|-------|------------|-------|
| GET | `/` | Home@index | דף בית |
| GET | `/login` | Auth@showLogin | טופס התחברות |
| POST | `/login` | Auth@login | ביצוע התחברות |
| GET | `/requests/create` | Requests@create | טופס יצירת בקשה |
| POST | `/requests/store` | Requests@store | שמירת בקשה |

### Routes מאומתים (דורש התחברות)

| Method | Route | Controller | הרשאה |
|--------|-------|------------|-------|
| POST | `/logout` | Auth@logout | כל משתמש |
| GET | `/requests` | Requests@index | כל משתמש |
| GET | `/requests/{id}` | Requests@show | בעל הבקשה/מנהל |
| GET | `/requests/{id}/manager-approve` | Requests@managerApprove | מנהל |
| POST | `/requests/{id}/manager-approve` | Requests@managerApproveStore | מנהל |
| GET | `/requests/{id}/final-approve` | Requests@finalApprove | מנהל מערכת |
| POST | `/requests/{id}/final-approve` | Requests@finalApproveStore | מנהל מערכת |
| GET | `/requests/{id}/change-manager` | Requests@changeManager | מנהל מערכת |
| POST | `/requests/{id}/change-manager` | Requests@changeManagerStore | מנהל מערכת |
| GET | `/manager` | Manager@index | מנהל |

### Routes מנהל מערכת

| Method | Route | Controller | תיאור |
|--------|-------|------------|-------|
| GET | `/admin` | Admin@index | דף ניהול ראשי |
| GET | `/admin/users` | Admin@users | רשימת משתמשים |
| GET | `/admin/users/create` | Admin@createUser | יצירת משתמש |
| POST | `/admin/users` | Admin@storeUser | שמירת משתמש |
| GET | `/admin/users/{id}/edit` | Admin@editUser | עריכת משתמש |
| POST | `/admin/users/{id}` | Admin@updateUser | עדכון משתמש |
| GET | `/admin/employees` | Admin@employees | רשימת עובדים |
| GET | `/admin/employees/create` | Admin@createEmployee | יצירת עובד |
| POST | `/admin/employees` | Admin@storeEmployee | שמירת עובד |
| GET | `/admin/employees/{id}/edit` | Admin@editEmployee | עריכת עובד |
| POST | `/admin/employees/{id}` | Admin@updateEmployee | עדכון עובד |
| GET | `/admin/systems` | Admin@systems | רשימת מערכות |
| GET | `/admin/systems/create` | Admin@createSystem | יצירת מערכת |
| POST | `/admin/systems` | Admin@storeSystem | שמירת מערכת |
| GET | `/admin/systems/{id}/edit` | Admin@editSystem | עריכת מערכת |
| POST | `/admin/systems/{id}` | Admin@updateSystem | עדכון מערכת |

---

## אבטחה

### אימות (Authentication)
- **שיטה:** Session-based authentication
- **סיסמאות:** מוצפנות עם `password_hash()` (bcrypt)
- **Session:** מוגן עם `httponly` cookies
- **Timeout:** 2 שעות ללא פעילות

### הרשאות (Authorization)
- בדיקת הרשאות בכל בקשה:
  - `requireAuth()` - דורש התחברות
  - `requireManager()` - דורש הרשאת מנהל
  - `requireAdmin()` - דורש הרשאת מנהל מערכת

### הגנה מפני התקפות

**SQL Injection:**
- שימוש ב-PDO Prepared Statements בכל השאילתות
- אין שימוש ב-string concatenation בשאילתות

**XSS (Cross-Site Scripting):**
- שימוש ב-`htmlspecialchars()` בכל פלט למשתמש
- Escaping של כל נתוני קלט

**CSRF (Cross-Site Request Forgery):**
- מומלץ להוסיף CSRF tokens (לא מיושם כרגע)

**Session Hijacking:**
- Session ID מוגן
- Regeneration של Session ID לאחר התחברות

---

## התקנה והפעלה

### שלב 1: העתקת קבצים
```bash
cp -r simple-php /path/to/web/directory
cd /path/to/web/directory/simple-php
```

### שלב 2: יצירת מסד נתונים
```bash
mkdir database
sqlite3 database/authorization.db < database/init.sql
```

### שלב 3: הגדרת הרשאות
```bash
chmod 755 database
chmod 666 database/authorization.db
```

### שלב 4: הגדרות
ערוך `config/config.php`:
- הגדר `app_url`
- הגדר `timezone`
- הגדר פרטי מנהל מערכת

### שלב 5: הפעלה

**אפשרות 1: PHP Built-in Server**
```bash
php -S localhost:8000
```

**אפשרות 2: Apache**
- הגדר DocumentRoot לתיקיית הפרויקט
- ודא ש-mod_rewrite מופעל

**אפשרות 3: Nginx**
- הגדר root לתיקיית הפרויקט
- הוסף rewrite rules

### שלב 6: גישה למערכת
פתח בדפדפן: `http://localhost:8000`

**התחברות ראשונית:**
- אימייל: `admin@example.com`
- סיסמה: `Qa123456`

---

## תחזוקה ותמיכה

### גיבויים
- גבה את קובץ `database/authorization.db` באופן קבוע
- מומלץ: גיבוי יומי אוטומטי

### לוגים
- שגיאות PHP נשמרות ב-PHP error log
- מומלץ להוסיף מערכת לוגים מותאמת

### עדכונים
- בדוק עדכוני אבטחה של PHP
- עדכן סיסמאות ברירת מחדל
- סקור הרשאות משתמשים באופן קבוע

---

## הרחבות עתידיות

### תכונות מומלצות להוספה:
1. ✅ מערכת תזכורות אימייל
2. ✅ יצירת PDF אוטומטית
3. ✅ דוחות ו-export ל-Excel
4. ✅ חיפוש מתקדם
5. ✅ היסטוריית פעילות מלאה
6. ✅ API RESTful
7. ✅ תמיכה ב-Active Directory
8. ✅ תמיכה בשפות נוספות
9. ✅ ממשק mobile-responsive
10. ✅ מערכת התראות

---

## מגבלות ידועות

1. **ללא CSRF Protection** - מומלץ להוסיף
2. **ללא Rate Limiting** - מומלץ להוסיף למניעת brute force
3. **ללא Email Sending** - תכונה לא מיושמת
4. **ללא PDF Generation** - תכונה לא מיושמת
5. **ללא Advanced Search** - חיפוש בסיסי בלבד

---

## קשר ותמיכה

לשאלות ותמיכה טכנית, פנה למנהל המערכת.

---

**גרסה:** 1.0  
**תאריך עדכון:** 2024  
**מחבר:** System Specification

