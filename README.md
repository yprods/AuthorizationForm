# מערכת הרשאות - Authorization Form System

מערכת מקצועית לניהול בקשות הרשאות גישה למערכות, בנויה על ASP.NET Core MVC עם SQLite.

## תכונות עיקריות

### 1. ניהול בקשות הרשאות
- יצירת בקשה חדשה עם בחירת רמת שירות (רמת משתמש, רמת משתמש אחר, ריבוי משתמשים)
- בחירת עובדים מרשימת עובדים עם חיפוש
- בחירת מערכות מרשימת מערכות
- הוספת הערות
- בחירת מנהל אחראי מרשימת מנהלים עם חיפוש
- גילוי נאות ואישור

### 2. תהליך אישור
- שליחת מייל אוטומטי למנהל לאשר בקשה
- אישור מנהל עם אימות Active Directory (שם משתמש וסיסמה)
- שליחת מייל למייל סופי לאישור אחרון
- אישור/דחייה סופית עם הערות

### 3. ניהול
- ביטול בקשה על ידי משתמש (בטיוטה או ממתין לאישור מנהל)
- ביטול בקשה על ידי מנהל
- החלפת מנהל אחראי על ידי אדמין לאחר שליחה
- מעקב אחר היסטוריית בקשות

### 4. ניהול נתונים (אדמין)
- ניהול עובדים - הוספה, עריכה, מחיקה
- ניהול מערכות - הוספה, עריכה, מחיקה
- ניהול מנהלים - צפייה ברשימת מנהלים
- ניהול תבניות טפסים - יצירה, עריכה, מחיקה

### 5. צפייה ודוחות
- משתמשים יכולים לראות את כל הבקשות שלהם עם סטטוס
- מנהלים יכולים לראות בקשות הממתינות לאישורם ובקשות שנשלחו
- היסטוריה מלאה של כל שינוי סטטוס
- יצירת PDF של בקשות מאושרות

### 6. אבטחה
- אימות Active Directory לאישורים
- הרשאות מבוססות תפקידים (Admin, Manager, User)
- הגנה על נתונים רגישים

### 7. ממשק משתמש
- עיצוב מודרני ונאה
- תמיכה מלאה בעברית (RTL)
- ממשק נוח ומקצועי
- חיפוש מתקדם בעובדים ומנהלים

## התקנה

### דרישות מערכת
- .NET 8.0 SDK או חדש יותר
- Visual Studio 2022 או Visual Studio Code

### שלבי התקנה

1. שכפל את הפרויקט או הורד את הקבצים

2. פתח את הפרויקט ב-Visual Studio

3. עדכן את `appsettings.json`:
   - עדכן את הגדרות Email (SMTP)
   - עדכן את הגדרות Active Directory (LDAP)

4. הרץ את הפקודות הבאות:
   ```bash
   dotnet restore
   dotnet ef database update
   dotnet run
   ```

5. התחבר עם:
   - **Admin**: admin@example.com / Admin@123
   - **Manager**: manager@example.com / Manager@123

## מבנה הפרויקט

```
AuthorizationForm/
├── Controllers/          # בקרים
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── RequestsController.cs
│   └── AdminController.cs
├── Models/              # מודלים
│   ├── ApplicationUser.cs
│   ├── AuthorizationRequest.cs
│   ├── Employee.cs
│   ├── System.cs
│   └── ...
├── Services/            # שירותים
│   ├── EmailService.cs
│   ├── PdfService.cs
│   ├── ActiveDirectoryService.cs
│   └── AuthorizationService.cs
├── Data/               # גישה לנתונים
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Views/              # תצוגות
│   ├── Home/
│   ├── Account/
│   ├── Requests/
│   └── Admin/
└── wwwroot/           # קבצים סטטיים
    ├── css/
    └── js/
```

## הגדרות מייל

עדכן את `appsettings.json` עם הפרטים שלך:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "מערכת הרשאות",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

## הגדרות Active Directory

עדכן את `appsettings.json` עם הפרטים שלך:

```json
{
  "ActiveDirectory": {
    "Domain": "yourdomain.com",
    "LdapPath": "LDAP://yourdomain.com"
  }
}
```

## תכונות מתקדמות

### יצירת PDF
המערכת יוצרת אוטומטית PDF של בקשות מאושרות. הקבצים נשמרים בתיקיית `Pdfs/`.

### תבניות
ניתן ליצור תבניות PDF מותאמות אישית עם פלס הולדרים דינמיים.

### חיפוש
חיפוש מתקדם בעובדים ומנהלים בזמן אמת.

## אבטחה

- כל המידע מוצפן במסד הנתונים
- אימות Active Directory לאישורים
- הגנת CSRF על כל הטופסים
- הרשאות מבוססות תפקידים

## תמיכה

לשאלות ותמיכה, אנא פנה למנהל המערכת.

## רישיון

פרויקט זה הוא קוד פתוח וזמין תחת רישיון MIT.

