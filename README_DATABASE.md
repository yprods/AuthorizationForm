# Database Configuration - MySQL

המערכת משתמשת ב-MySQL במקום SQLite. ניתן להגדיר את מחרוזת החיבור באמצעות קובץ `.env` או משתני סביבה.

## הגדרת MySQL

### 1. התקנת MySQL

התקן MySQL Server על המחשב שלך:
- Windows: הורד מ-[MySQL Downloads](https://dev.mysql.com/downloads/mysql/)
- או השתמש ב-XAMPP/WAMP שכולל MySQL

### 2. יצירת מסד נתונים

צור מסד נתונים חדש:

```sql
CREATE DATABASE authorization_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 3. הגדרת מחרוזת החיבור

#### אפשרות א': קובץ .env (מומלץ)

1. צור קובץ `.env` בתיקיית הפרויקט (באותה תיקייה כמו `Program.cs`)
2. הוסף את השורה הבאה:

```
MYSQL_CONNECTION_STRING=Server=localhost;Database=authorization_db;User=root;Password=your_password;Port=3306;CharSet=utf8mb4;
```

**דוגמאות:**

```env
# חיבור מקומי עם סיסמה
MYSQL_CONNECTION_STRING=Server=localhost;Database=authorization_db;User=root;Password=mypassword;Port=3306;CharSet=utf8mb4;

# חיבור לשרת מרוחק
MYSQL_CONNECTION_STRING=Server=192.168.1.100;Database=authorization_db;User=dbuser;Password=securepass;Port=3306;CharSet=utf8mb4;

# חיבור עם SSL
MYSQL_CONNECTION_STRING=Server=localhost;Database=authorization_db;User=root;Password=password;Port=3306;CharSet=utf8mb4;SslMode=Required;
```

#### אפשרות ב': משתנה סביבה

הגדר משתנה סביבה בשם `MYSQL_CONNECTION_STRING`:

**Windows (PowerShell):**
```powershell
$env:MYSQL_CONNECTION_STRING="Server=localhost;Database=authorization_db;User=root;Password=password;Port=3306;CharSet=utf8mb4;"
```

**Windows (CMD):**
```cmd
set MYSQL_CONNECTION_STRING=Server=localhost;Database=authorization_db;User=root;Password=password;Port=3306;CharSet=utf8mb4;
```

**Linux/Mac:**
```bash
export MYSQL_CONNECTION_STRING="Server=localhost;Database=authorization_db;User=root;Password=password;Port=3306;CharSet=utf8mb4;"
```

#### אפשרות ג': appsettings.json

אם לא הוגדר משתנה סביבה, המערכת תשתמש בערך מ-`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=authorization_db;User=root;Password=your_password;Port=3306;CharSet=utf8mb4;"
  }
}
```

## מבנה מחרוזת החיבור

```
Server=HOST;Database=DATABASE_NAME;User=USERNAME;Password=PASSWORD;Port=PORT;CharSet=utf8mb4;
```

**פרמטרים:**
- `Server` - כתובת השרת (localhost או IP)
- `Database` - שם מסד הנתונים
- `User` - שם משתמש MySQL
- `Password` - סיסמת MySQL
- `Port` - פורט (ברירת מחדל: 3306)
- `CharSet` - קידוד תווים (מומלץ: utf8mb4)

**פרמטרים אופציונליים:**
- `SslMode` - מצב SSL (None, Preferred, Required)
- `ConnectionTimeout` - זמן המתנה (שניות)
- `AllowUserVariables` - אפשר משתני משתמש

## אבטחה

⚠️ **חשוב:**
- לעולם אל תכלול את קובץ `.env` ב-Git!
- הקובץ `.env` כבר מופיע ב-`.gitignore`
- השתמש ב-`.env.example` כדוגמה בלבד

## הרצת Migrations

לאחר הגדרת החיבור, הפעל migrations:

```bash
dotnet ef database update
```

או השתמש ב-`EnsureCreated()` שכבר מוגדר בקוד (יוצר את הטבלאות אוטומטית).

## פתרון בעיות

### שגיאת חיבור

אם אתה מקבל שגיאת חיבור:
1. ודא ש-MySQL Server פועל
2. בדוק את פרטי החיבור (שם משתמש, סיסמה, פורט)
3. ודא שהמסד נתונים קיים
4. בדוק את הגדרות Firewall

### שגיאת קידוד

אם יש בעיות עם עברית:
- ודא ש-`CharSet=utf8mb4` מוגדר
- ודא שהמסד נתונים נוצר עם `CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci`

### בדיקת חיבור

בדוק את החיבור באמצעות MySQL Command Line:

```bash
mysql -h localhost -u root -p
```

ואז:
```sql
USE authorization_db;
SHOW TABLES;
```

