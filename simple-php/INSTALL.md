# Installation Guide - Simple PHP Version

## Quick Start

1. **Copy files to web directory** or use PHP built-in server

2. **Create database directory:**
   ```bash
   mkdir database
   ```

3. **Initialize database:**
   
   **Windows (PowerShell):**
   ```powershell
   sqlite3 database/authorization.db < database/init.sql
   ```
   
   **Linux/Mac:**
   ```bash
   sqlite3 database/authorization.db < database/init.sql
   ```
   
   **Or using PHP:**
   ```bash
   php -r "copy('database/init.sql', 'temp.sql'); exec('sqlite3 database/authorization.db < temp.sql'); unlink('temp.sql');"
   ```

4. **Set permissions (Linux/Mac):**
   ```bash
   chmod 755 database
   chmod 666 database/authorization.db
   ```

5. **Run the application:**
   ```bash
   php -S localhost:8000
   ```

6. **Open browser:**
   http://localhost:8000

## Default Login

- **Email:** admin@example.com
- **Password:** Qa123456

## Requirements

- PHP 7.4+ (PHP 8.0+ recommended)
- SQLite extension (usually enabled by default)
- PDO extension (usually enabled by default)

## Troubleshooting

### Database file not found
- Make sure `database/` directory exists
- Make sure `database/authorization.db` file exists
- Check file permissions

### SQLite not enabled
- Check `php -m | grep sqlite`
- Enable in `php.ini`: `extension=sqlite`

### Routes not working
- Make sure `.htaccess` is enabled (Apache)
- Or use PHP built-in server: `php -S localhost:8000`

### Session errors
- Check `session.save_path` in `php.ini`
- Make sure directory is writable

## Configuration

Edit `config/config.php` to change:
- Database settings
- Admin credentials
- Email settings
- Timezone
- Locale

## No Dependencies!

This version uses **zero external libraries** - just pure PHP:
- ✅ No Composer
- ✅ No frameworks
- ✅ No dependencies
- ✅ Just core PHP functions

Perfect for simple deployments!

