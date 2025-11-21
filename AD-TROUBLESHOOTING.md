# פתרון בעיות - Active Directory Search

## בעיות נפוצות וחיפוש לא עובד

### 1. בדוק את הגדרות appsettings.json

וודא שה-`LdapPath` מוגדר נכון:

```json
{
  "ActiveDirectory": {
    "Domain": "yourdomain.com",
    "LdapPath": "LDAP://yourdomain.com",
    "ManagementGroup": "ניהול"
  }
}
```

**דוגמאות נכונות:**
- `LDAP://dc1.yourdomain.com` (שרת ספציפי)
- `LDAP://yourdomain.com/DC=yourdomain,DC=com` (עם DN מלא)
- `LDAP://10.0.0.1` (כתובת IP)

### 2. בדוק הרשאות

- ודא שהמשתמש שמריץ את האפליקציה יכול לגשת ל-Active Directory
- ב-IIS: ודא ש-Application Pool Identity יכול לגשת ל-AD
- ב-Development: ודא שאתה מחובר לדומיין

### 3. בדוק את הלוגים

פתח את Console של הדפדפן (F12) ובדוק:

1. **Console Logs:**
   - האם יש בקשות AJAX?
   - מה ה-Status Code?
   - האם יש שגיאות JavaScript?

2. **Network Tab:**
   - בדוק את הבקשה ל-`/Requests/SearchAdUsers`
   - מה ה-Response?
   - מה ה-Status Code?

3. **Server Logs:**
   - בדוק את לוגי השרת לראות אם יש שגיאות
   - חפש הודעות שמתחילות ב-"SearchAdUsers" או "Searching AD users"

### 4. בדוק חיבור ל-AD

**בדיקה ידנית:**
1. פתח PowerShell
2. הרץ:
   ```powershell
   $entry = [ADSI]"LDAP://yourdomain.com"
   $entry | Get-Member
   ```

אם זה לא עובד, בעיית חיבור ל-AD.

### 5. בדוק את ה-Endpoint

פתח בדפדפן (כשlogged in):
```
https://localhost:5001/Requests/SearchAdUsers?term=test&maxResults=10
```

אם אתה מקבל JSON - ה-endpoint עובד.
אם 404 - בעיית routing.
אם 500 - בעיית שרת (בדוק לוגים).

### 6. תיקון בעיות נפוצות

**בעיה: "Endpoint לא נמצא"**
- פתרון: ודא שה-controller action נקרא `SearchAdUsers` והנתיב הוא `/Requests/SearchAdUsers`

**בעיה: "שגיאת שרת"**
- פתרון: בדוק את `appsettings.json` - `LdapPath` חייב להיות נכון
- בדוק שהשרת מחובר לרשת הדומיין
- בדוק את הלוגים לפרטים נוספים

**בעיה: "לא נמצאו תוצאות"**
- פתרון: 
  - ודא שיש משתמשים ב-AD עם השם שחיפשת
  - בדוק שה-LDAP Filter נכון (בדוק בלוגים)
  - נסה לחפש עם שם משתמש או אימייל

**בעיה: Auto-complete לא מוצג**
- פתרון:
  - בדוק שה-CSS של `.autocomplete-dropdown` מוגדר נכון
  - בדוק ב-Console אם יש שגיאות JavaScript
  - ודא שהקונטיינר נוצר (בדוק ב-Elements inspector)

### 7. הוספת Debug Mode

ב-`appsettings.json`, שנה את רמת הלוג:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "AuthorizationForm": "Debug"
    }
  }
}
```

### 8. בדיקה ידנית של AD Service

צור endpoint בדיקה:
```csharp
[HttpGet("TestAdConnection")]
public async Task<IActionResult> TestAdConnection()
{
    try
    {
        var testUser = await _adService.GetUserInfoAsync("admin");
        return Json(new { success = true, user = testUser });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, error = ex.Message });
    }
}
```

גש ל-`/Requests/TestAdConnection` ובדוק מה קורה.

### 9. Windows Authentication

אם אתה משתמש ב-Windows Authentication, ודא:
- המשתמש מחובר לדומיין
- יש הרשאות לגשת ל-AD
- ה-Application Pool Identity יכול לגשת ל-AD

### 10. בדיקה עם ldp.exe

Windows כולל כלי `ldp.exe` לבדיקת חיבור ל-AD:
1. פתח `ldp.exe`
2. Connect > Connect
3. הזן את ה-LDAP server
4. Connect > Bind (כמשתמש מחובר)

אם זה לא עובד, יש בעיית תשתית.

## אם עדיין לא עובד

1. בדוק את הלוגים המלאים (Debug level)
2. בדוק את ה-Console בדפדפן
3. בדוק את ה-Network requests
4. נסה גישה ידנית ל-endpoint
5. בדוק את הגדרות ה-AD ב-`appsettings.json`

