# Simple PHP Authorization Form

Pure PHP application - no frameworks, no libraries, just core PHP.

## Requirements

- PHP 7.4 or higher
- SQLite extension (usually included)
- PDO extension (usually included)

## Installation

1. **Copy files to your web server directory** (e.g., `htdocs`, `www`, or use PHP built-in server)

2. **Create database directory:**
   ```bash
   mkdir database
   ```

3. **Initialize database:**
   ```bash
   sqlite3 database/authorization.db < database/init.sql
   ```
   
   Or use PHP:
   ```bash
   php -r "exec('sqlite3 database/authorization.db < database/init.sql');"
   ```

4. **Set permissions:**
   ```bash
   chmod 755 database
   chmod 666 database/authorization.db
   ```

5. **Configure (optional):**
   Edit `config/config.php` to change settings

## Running

### Option 1: PHP Built-in Server
```bash
php -S localhost:8000 -t .
```

Then open: http://localhost:8000

### Option 2: Apache/Nginx
Point your web server document root to this directory.

## Default Login

- **Email:** admin@example.com
- **Password:** Qa123456

## Project Structure

```
simple-php/
├── index.php              # Entry point
├── config/                # Configuration files
│   ├── config.php        # App configuration
│   ├── database.php      # Database config
│   └── routes.php        # Route definitions
├── app/
│   ├── core/             # Core classes
│   │   ├── Router.php    # Simple router
│   │   ├── Controller.php # Base controller
│   │   ├── Auth.php      # Authentication
│   │   └── Database.php  # Database wrapper
│   ├── models/           # Model classes
│   └── controllers/      # Controller classes
├── views/                # View templates
├── database/             # Database files
│   ├── authorization.db # SQLite database
│   └── init.sql         # Database schema
└── public/               # Public assets (CSS, JS, images)
```

## Features

- ✅ Pure PHP - no dependencies
- ✅ Simple routing system
- ✅ PDO database access
- ✅ Session-based authentication
- ✅ MVC structure
- ✅ SQLite database (can switch to MySQL)
- ✅ Role-based access control
- ✅ Authorization request management

## Database

The application uses SQLite by default. To use MySQL:

1. Edit `config/config.php`:
   ```php
   'db_type' => 'mysql',
   'db_host' => 'localhost',
   'db_name' => 'authorization_db',
   'db_user' => 'root',
   'db_pass' => '',
   ```

2. Create MySQL database and import schema from `database/init.sql` (adjust for MySQL syntax)

## Security Notes

- Passwords are hashed using `password_hash()` (bcrypt)
- SQL injection prevented by using PDO prepared statements
- XSS protection via `htmlspecialchars()` in views
- Session-based authentication

## Adding New Features

1. **Add route:** Edit `config/routes.php`
2. **Create controller:** Add file in `app/controllers/`
3. **Create view:** Add file in `views/`
4. **Add model:** Add file in `app/models/` if needed

## License

MIT License

