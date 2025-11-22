# Authorization Form - PHP/Laravel Version

This is the PHP/Laravel conversion of the ASP.NET Core Authorization Form application.

## Requirements

- PHP >= 8.1
- Composer
- SQLite (or MySQL)
- Laravel 10.x

## Installation

1. **Install dependencies:**
   ```bash
   cd php-version
   composer install
   ```

2. **Copy environment file:**
   ```bash
   cp .env.example .env
   ```

3. **Generate application key:**
   ```bash
   php artisan key:generate
   ```

4. **Configure database:**
   Edit `.env` file and set your database configuration:
   ```env
   DB_CONNECTION=sqlite
   DB_DATABASE=database/authorization.db
   ```

5. **Run migrations:**
   ```bash
   php artisan migrate
   ```

6. **Seed initial data:**
   ```bash
   php artisan db:seed
   ```

7. **Start development server:**
   ```bash
   php artisan serve
   ```

## Project Structure

```
php-version/
├── app/
│   ├── Http/
│   │   └── Controllers/     # Controllers (converted from C#)
│   ├── Models/              # Eloquent Models (converted from C#)
│   ├── Services/            # Service classes (converted from C#)
│   └── Middleware/         # Middleware (converted from C#)
├── database/
│   ├── migrations/          # Database migrations
│   └── seeders/            # Database seeders
├── resources/
│   └── views/              # Blade templates (converted from Razor)
├── routes/
│   └── web.php             # Web routes
└── config/                 # Configuration files
```

## Key Conversions

### Models
- `ApplicationUser` → `User` (extends Laravel's Authenticatable)
- `AuthorizationRequest` → `AuthorizationRequest`
- `Employee` → `Employee`
- `ApplicationSystem` → `ApplicationSystem`
- `RequestHistory` → `RequestHistory`
- `EmailTemplate` → `EmailTemplate`
- `FormTemplate` → `FormTemplate`

### Controllers
- All C# controllers converted to Laravel controllers
- Dependency injection handled by Laravel's service container
- Authorization using Laravel's middleware

### Views
- Razor (.cshtml) → Blade (.blade.php)
- Same structure and functionality

### Services
- C# services converted to PHP classes
- Email service using PHPMailer
- PDF service using DomPDF
- Active Directory service using Adldap2

### Authentication
- ASP.NET Identity → Laravel Authentication
- Custom user model with admin/manager flags
- Role-based authorization

## Features

- ✅ User authentication and authorization
- ✅ Role-based access control (Admin, Manager, User)
- ✅ Authorization request management
- ✅ Manager approval workflow
- ✅ Final approval workflow
- ✅ Email notifications
- ✅ PDF generation
- ✅ Active Directory integration (optional)
- ✅ Reminder system
- ✅ Admin panel for managing users, employees, systems, templates

## Configuration

### Email Settings
Edit `.env` file:
```env
MAIL_MAILER=smtp
MAIL_HOST=smtp.gmail.com
MAIL_PORT=587
MAIL_USERNAME=your-email@gmail.com
MAIL_PASSWORD=your-password
MAIL_ENCRYPTION=tls
```

### Admin Settings
Edit `.env` file:
```env
ADMIN_USERNAME=admin
ADMIN_EMAIL=admin@example.com
ADMIN_PASSWORD=Qa123456
ADMIN_FULL_NAME="מנהל מערכת"
```

### Active Directory (Optional)
```env
AD_ENABLED=false
AD_DOMAIN=yourdomain.com
AD_LDAP_PATH=LDAP://yourdomain.com
```

## Migration from ASP.NET Core

The PHP version maintains the same functionality as the original C# application:

1. **Database Schema**: Same structure, converted to Laravel migrations
2. **Business Logic**: Same logic, converted to PHP syntax
3. **User Interface**: Same views, converted to Blade templates
4. **API Endpoints**: Same routes and functionality

## Notes

- This is a complete conversion but may require testing and adjustments
- Some Windows-specific features (like Windows Authentication) may need alternative implementations
- Active Directory integration uses Adldap2 library instead of System.DirectoryServices

## License

MIT License

