# מדריך התקנה והגדרה ב-IIS

## דרישות מערכת

### 1. התקנת IIS
1. פתח **Windows Features** (כיבוי והפעלה של תכונות Windows)
2. סמן:
   - **Internet Information Services**
   - **Internet Information Services Hostable Web Core**
   - תחת IIS:
     - **Web Management Tools** > **IIS Management Console**
     - **World Wide Web Services** > **Application Development Features**:
       - **ASP.NET 4.8** (או הגרסה האחרונה)
       - **ISAPI Extensions**
       - **ISAPI Filters**
     - **World Wide Web Services** > **Security**:
       - **Windows Authentication**
       - **Basic Authentication** (אופציונלי)

### 2. התקנת .NET Runtime/Hosting Bundle
1. הורד את **.NET 8.0 Hosting Bundle** מ:
   - https://dotnet.microsoft.com/download/dotnet/8.0
   - בחר "Hosting Bundle" (כולל Runtime + ASP.NET Core Module)
2. הרץ את ההתקנה (תצטרך להפעיל מחדש את IIS)

### 3. התקנת SQLite Runtime (אופציונלי)
- SQLite כלול ב-.NET 8.0, אין צורך בהתקנה נוספת

## הגדרת IIS

### 1. יצירת אתר חדש
1. פתח **IIS Manager**
2. לחץ ימני על **Sites** > **Add Website**
3. מלא את הפרטים:
   - **Site name:** AuthorizationForm
   - **Application pool:** השאר את זה שיוצר אוטומטית
   - **Physical path:** נתיב לתיקיית `publish` של הפרויקט (לדוגמה: `C:\inetpub\wwwroot\AuthorizationForm`)
   - **Binding:**
     - Type: `http` או `https`
     - IP address: `All Unassigned`
     - Port: `80` (או `443` ל-HTTPS)
     - Host name: (ריק או שם דומיין)

### 2. הגדרת Application Pool
1. לחץ על **Application Pools**
2. בחר את ה-Pool של האתר
3. לחץ **Advanced Settings**
4. הגדר:
   - **.NET CLR Version:** `No Managed Code` (חשוב!)
   - **Managed Pipeline Mode:** `Integrated`
   - **Identity:** `ApplicationPoolIdentity` או חשבון עם הרשאות

### 3. הפעלת Windows Authentication
1. בחר את האתר ב-IIS Manager
2. לחץ פעמיים על **Authentication**
3. לחץ ימני על **Windows Authentication** > **Enable**
4. לחץ ימני על **Anonymous Authentication** > **Enable** (חובה!)
5. וודא ש-Windows Authentication מופיע לפני Anonymous Authentication (גרור למעלה)

### 4. הגדרת הרשאות תיקייה
1. לחץ ימני על תיקיית האתר > **Properties** > **Security**
2. הוסף הרשאות:
   - **IIS_IUSRS:** Read & Execute
   - **Application Pool Identity:** Read & Execute
   - **Users:** Read & Execute (אופציונלי)

### 5. הגדרת web.config
הקובץ `web.config` צריך להיות בתיקיית ה-publish (נוצר אוטומטית, אך וודא שהתוכן נכון):

**אם הקובץ לא נוצר אוטומטית, צור אותו ידנית:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\AuthorizationForm.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
      <security>
        <authentication>
          <windowsAuthentication enabled="true" />
          <anonymousAuthentication enabled="true" />
        </authentication>
      </security>
      <staticContent>
        <mimeMap fileExtension=".json" mimeType="application/json" />
      </staticContent>
    </system.webServer>
  </location>
</configuration>
```

**מיקום:** `C:\inetpub\wwwroot\AuthorizationForm\web.config` (או הנתיב שהגדרת)

## תהליך Publish

### 1. יצירת Publish

**אופציה A - דרך Visual Studio:**
1. לחץ ימני על הפרויקט > **Publish**
2. בחר **Folder**
3. בחר תיקייה (לדוגמה: `C:\inetpub\wwwroot\AuthorizationForm`)
4. לחץ **Publish**

**אופציה B - דרך Command Line:**
```powershell
cd "C:\Users\yprod\OneDrive\Desktop\טופס הרשאות"
dotnet publish -c Release -o C:\inetpub\wwwroot\AuthorizationForm
```

### 2. העתקת קבצים ל-IIS
- כל הקבצים מתיקיית ה-Publish מועתקים אוטומטית
- ודא שהקובץ `web.config` נוצר (אם לא, צור אותו ידנית לפי הדוגמה למעלה)
- ודא שיש הרשאות כתיבה לתיקייה (ליצירת מסד נתונים)

## הגדרות נוספות

### 1. הגדרת Connection String
בקובץ `appsettings.json` בתיקיית ה-Publish:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=C:\\inetpub\\wwwroot\\AuthorizationForm\\authorization.db"
  }
}
```

### 2. הגדרת Email Settings
עדכן את `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.yourdomain.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "מערכת הרשאות",
    "Username": "your-username",
    "Password": "your-password",
    "EnableSsl": true
  }
}
```

### 3. הגדרת Active Directory
```json
{
  "ActiveDirectory": {
    "Domain": "yourdomain.com",
    "LdapPath": "LDAP://yourdomain.com",
    "ManagementGroup": "ניהול"
  }
}
```

## בדיקה ואימות

### 1. בדיקת Windows Authentication
1. פתח את האתר בדפדפן
2. אם Windows Authentication עובד, אתה אמור להתחבר אוטומטית
3. אם לא - תראה טופס התחברות ידני

### 2. בדיקת התחברות ידנית
- שם משתמש: `admin`
- סיסמה: `Qa123123!@#@WS`

### 3. בדיקת לוגים
- לוגים נמצאים ב: `C:\inetpub\wwwroot\AuthorizationForm\logs\stdout*.log`
- בדוק את הקובץ אם יש בעיות
- ודא שתיקיית `logs` קיימת ויש הרשאות כתיבה אליה

## פתרון בעיות

### בעיה: "HTTP Error 500.0 - ANCM In-Process Handler Load Failure"
**פתרון:**
- וודא שה-.NET 8.0 Hosting Bundle מותקן
- בדוק שה-Application Pool מוגדר ל-"No Managed Code"
- הפעל מחדש את IIS: `iisreset`

### בעיה: Windows Authentication לא עובד
**פתרון:**
1. וודא ש-Windows Authentication מופעל ב-IIS
2. וודא ש-Anonymous Authentication גם מופעל
3. בדוק את סדר ה-Authentication (Windows לפני Anonymous)
4. בדוק את `web.config` שהגדרות Authentication נכונות

### בעיה: "SQLite database is locked"
**פתרון:**
- ודא שהמסד נתונים לא בשימוש על ידי תהליך אחר
- בדוק הרשאות כתיבה לתיקיית המסד נתונים

### בעיה: "Access Denied"
**פתרון:**
- בדוק הרשאות תיקייה (IIS_IUSRS צריך Read & Execute)
- בדוק את Identity של ה-Application Pool

## תחזוקה

### 1. עדכון האפליקציה
1. עצור את ה-Application Pool
2. החלף קבצים חדשים
3. הפעל מחדש את ה-Application Pool

### 2. גיבוי מסד נתונים
- גבה את הקובץ `authorization.db` באופן קבוע
- הקובץ נמצא בתיקיית האפליקציה

## קישורים מועילים
- [ASP.NET Core IIS Deployment](https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/)
- [Windows Authentication in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/windowsauth)
- [.NET 8.0 Download](https://dotnet.microsoft.com/download/dotnet/8.0)

