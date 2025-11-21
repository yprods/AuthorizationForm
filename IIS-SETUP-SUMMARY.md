# תקציר התקנה ב-IIS - Authorization Form

## ✅ מה צריך להתקין?

### 1. IIS (Internet Information Services)
```
Windows Features > Internet Information Services
  - Web Management Tools > IIS Management Console
  - World Wide Web Services > Application Development Features > ASP.NET 4.8
  - World Wide Web Services > Security > Windows Authentication
```

### 2. .NET 8.0 Hosting Bundle
**הורדה:** https://dotnet.microsoft.com/download/dotnet/8.0
- בחר **"Hosting Bundle"** (כולל Runtime + ASP.NET Core Module)
- הרץ התקנה (דורש הפעלה מחדש של IIS)

### 3. Windows Authentication
- כבר כלול ב-IIS אם התקנת Windows Authentication

## 🔧 הגדרות IIS

### 1. Application Pool
- **.NET CLR Version:** `No Managed Code` ⚠️ חשוב!
- **Managed Pipeline Mode:** `Integrated`
- **Identity:** `ApplicationPoolIdentity` (ברירת מחדל)

### 2. Authentication
- ✅ **Windows Authentication:** Enabled
- ✅ **Anonymous Authentication:** Enabled (חובה!)

### 3. Security/Folder Permissions
- **IIS_IUSRS:** Read & Execute
- **Application Pool Identity:** Read & Execute
- **Users:** Read (אופציונלי)

## 📦 תהליך Publish

```powershell
# מתוך תיקיית הפרויקט:
dotnet publish -c Release -o C:\inetpub\wwwroot\AuthorizationForm
```

## 📝 web.config (נוצר אוטומטית, אך וודא שזה קיים):

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
    </system.webServer>
  </location>
</configuration>
```

## 🔑 משתמש Admin כברירת מחדל
- **שם משתמש:** `admin`
- **סיסמה:** `Qa123123!@#@WS`

## ⚙️ קבצי Config

### appsettings.json
עדכן את הנתיב למסד נתונים:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=C:\\inetpub\\wwwroot\\AuthorizationForm\\authorization.db"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.yourdomain.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "מערכת הרשאות",
    "Username": "your-username",
    "Password": "your-password",
    "EnableSsl": true
  },
  "ActiveDirectory": {
    "Domain": "yourdomain.com",
    "LdapPath": "LDAP://yourdomain.com",
    "ManagementGroup": "ניהול"
  }
}
```

## 🐛 פתרון בעיות נפוצות

### בעיה: HTTP Error 500.0
**פתרון:**
1. וודא ש-.NET 8.0 Hosting Bundle מותקן
2. בדוק Application Pool = "No Managed Code"
3. הפעל מחדש IIS: `iisreset` (כמנהל)

### בעיה: Windows Authentication לא עובד
**פתרון:**
1. וודא ש-Windows Authentication מופעל ב-IIS
2. וודא ש-Anonymous Authentication גם מופעל
3. בדוק שהסדר נכון: Windows Authentication לפני Anonymous

### בעיה: Access Denied
**פתרון:**
- בדוק הרשאות תיקייה (IIS_IUSRS צריך Read & Execute)

## ✅ בדיקה אחרי התקנה

1. פתח דפדפן וגש לכתובת: `http://localhost` (או הכתובת שהגדרת)
2. אם Windows Authentication עובד - תתחבר אוטומטית
3. אם לא - תראה טופס התחברות:
   - שם משתמש: `admin`
   - סיסמה: `Qa123123!@#@WS`

## 📚 קישורים מועילים
- [מדריך מפורט](IIS-DEPLOYMENT.md)
- [ASP.NET Core IIS Deployment](https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/)
- [.NET 8.0 Download](https://dotnet.microsoft.com/download/dotnet/8.0)

