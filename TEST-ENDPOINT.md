# מדריך לבדיקת Endpoint SearchAdUsers

## בדיקת ה-Endpoint

### 1. הרצת האפליקציה
```bash
cd "C:\Users\yprod\OneDrive\Desktop\טופס הרשאות"
dotnet run
```

האפליקציה תרוץ על:
- **HTTPS:** https://localhost:5001
- **HTTP:** http://localhost:5000

### 2. בדיקה ישירה בדפדפן

פתח בדפדפן את הכתובת:
```
https://localhost:5001/Requests/SearchAdUsers?term=admin&maxResults=5
```

**תגובה תקינה (JSON):**
```json
[
  {
    "username": "admin",
    "fullName": "Admin User",
    "email": "admin@domain.com",
    "department": "IT",
    "title": "Administrator"
  }
]
```

**אם אין תוצאות:**
```json
[]
```

**אם יש שגיאה:**
```json
{
  "error": true,
  "message": "שגיאה בחיפוש Active Directory: ...",
  "details": "..."
}
```

### 3. בדיקה דרך Console (F12)

1. פתח את האפליקציה בדפדפן
2. לחץ F12 (Developer Tools)
3. גש ל-Console
4. הרץ:
```javascript
fetch('/Requests/SearchAdUsers?term=admin&maxResults=5')
  .then(r => r.json())
  .then(data => console.log(data))
  .catch(err => console.error('Error:', err));
```

### 4. בדיקה עם curl (PowerShell)

```powershell
Invoke-WebRequest -Uri "https://localhost:5001/Requests/SearchAdUsers?term=admin&maxResults=5" -Method GET -SkipCertificateCheck | Select-Object -ExpandProperty Content
```

### 5. פתרון בעיות נפוצות

**בעיה: 404 Not Found**
- פתרון: ודא שה-controller action נקרא `SearchAdUsers` והנתיב הוא `/Requests/SearchAdUsers`

**בעיה: 500 Internal Server Error**
- פתרון: 
  - בדוק את `LdapPath` ב-`appsettings.json` (חייב להיות מוגדר לדומיין שלך)
  - בדוק את הלוגים בשרת
  - ודא שיש חיבור ל-Active Directory

**בעיה: [] (רשימה ריקה)**
- פתרון:
  - ודא שיש משתמשים ב-AD עם השם שחיפשת
  - בדוק שה-LdapPath נכון
  - נסה לחפש עם שם משתמש או אימייל

**בעיה: שגיאת Certificate (Self-signed)**
- פתרון: הוסף `-SkipCertificateCheck` ב-PowerShell או אישר את התעודה בדפדפן

### 6. הגדרת LdapPath

עדכן את `appsettings.json`:
```json
{
  "ActiveDirectory": {
    "Domain": "yourdomain.com",
    "LdapPath": "LDAP://yourdomain.com",  // <-- שנה לכתובת AD שלך
    "ManagementGroup": "ניהול"
  }
}
```

**דוגמאות:**
- `LDAP://dc1.yourdomain.com`
- `LDAP://10.0.0.1`
- `LDAP://yourdomain.com/DC=yourdomain,DC=com`

### 7. בדיקת לוגים

הלוגים יראו:
```
info: AuthorizationForm.Controllers.RequestsController[0]
      SearchAdUsers API called with term: 'admin', maxResults: 5
info: AuthorizationForm.Controllers.RequestsController[0]
      Calling AD service to search for users with term: admin
info: AuthorizationForm.Services.ActiveDirectoryService[0]
      Searching AD users with term: admin, LdapPath: LDAP://...
info: AuthorizationForm.Controllers.RequestsController[0]
      AD service returned X users
info: AuthorizationForm.Controllers.RequestsController[0]
      Returning X results to client
```

### 8. אם עדיין לא עובד

1. בדוק את הלוגים המלאים
2. בדוק את ה-Console בדפדפן
3. ודא שהאפליקציה רץ (לא נסגר)
4. בדוק את `appsettings.json` - `LdapPath` חייב להיות נכון

