# מערכת תזכורות למנהלים

המערכת כוללת שירות רקע אוטומטי ששולח תזכורות למנהלים על בקשות הממתינות לאישורם.

## איך זה עובד

1. **שירות רקע** (`ReminderService`) רץ ברקע כל הזמן
2. כל כמה שעות (ברירת מחדל: 6 שעות) הוא בודק אם יש בקשות ממתינות לאישור מנהל
3. אם בקשה ממתינה יותר מ-24 שעות (ברירת מחדל), נשלחת תזכורת למנהל
4. התזכורת כוללת קישור ישיר לבקשה

## הגדרות

הגדרות התזכורות נמצאות ב-`appsettings.json`:

```json
"ReminderSettings": {
  "Enabled": true,
  "CheckIntervalHours": 6,
  "ReminderIntervalHours": 24
}
```

### פרמטרים:

- **Enabled** - האם להפעיל את מערכת התזכורות (true/false)
- **CheckIntervalHours** - כמה שעות להמתין בין בדיקות (ברירת מחדל: 6)
- **ReminderIntervalHours** - כמה שעות להמתין לפני שליחת תזכורת (ברירת מחדל: 24)

### דוגמאות:

```json
// בדיקה כל שעה, תזכורת אחרי 12 שעות
"ReminderSettings": {
  "Enabled": true,
  "CheckIntervalHours": 1,
  "ReminderIntervalHours": 12
}

// בדיקה כל יום, תזכורת אחרי 48 שעות
"ReminderSettings": {
  "Enabled": true,
  "CheckIntervalHours": 24,
  "ReminderIntervalHours": 48
}
```

## מעקב תזכורות

כל בקשה עוקבת אחר:
- **LastReminderSentAt** - מתי נשלחה התזכורת האחרונה
- **ReminderCount** - כמה תזכורות נשלחו עד כה

זה מונע שליחת תזכורות מרובות מדי.

## הגדרת URL בסיסי

בסביבת ייצור, הגדר משתנה סביבה `APP_BASE_URL` עם כתובת האתר שלך:

```bash
# Windows PowerShell
$env:APP_BASE_URL="https://your-domain.com"

# Linux/Mac
export APP_BASE_URL="https://your-domain.com"

# או בקובץ .env
APP_BASE_URL=https://your-domain.com
```

אם לא מוגדר, המערכת תשתמש ב-`http://localhost:5000` (לפיתוח בלבד).

## לוגים

השירות כותב לוגים מפורטים:
- מתי השירות מתחיל/נעצר
- כמה בקשות נמצאו
- כמה תזכורות נשלחו
- שגיאות (אם יש)

בדוק את הלוגים כדי לראות שהמערכת עובדת כראוי.

## השבתת התזכורות

כדי להשבית את התזכורות, שנה ב-`appsettings.json`:

```json
"ReminderSettings": {
  "Enabled": false
}
```

או הסר את השירות מ-`Program.cs`:

```csharp
// builder.Services.AddHostedService<ReminderService>();
```

## פתרון בעיות

### תזכורות לא נשלחות

1. בדוק שהשירות מופעל: `"Enabled": true`
2. בדוק שהאימייל של המנהל מוגדר
3. בדוק את הגדרות ה-SMTP ב-`EmailSettings`
4. בדוק את הלוגים לשגיאות

### תזכורות נשלחות יותר מדי

1. בדוק את `ReminderIntervalHours` - אולי הוא קטן מדי
2. בדוק את `CheckIntervalHours` - אולי הוא קטן מדי

### קישורים לא עובדים

1. הגדר את `APP_BASE_URL` עם הכתובת הנכונה
2. ודא שהכתובת נגישה מהאינטרנט (אם זה שרת מרוחק)

