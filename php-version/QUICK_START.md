# Quick Start Guide

## Step 1: Install PHP

If you don't have PHP installed:

1. Download PHP 8.1+ from: https://windows.php.net/download/
2. Extract to `C:\php`
3. Add `C:\php` to your system PATH
4. Verify: Open PowerShell and run `php --version`

## Step 2: Install Composer

**Option A: Use the installer (Easiest)**
1. Download: https://getcomposer.org/Composer-Setup.exe
2. Run the installer
3. It will automatically detect PHP and set everything up

**Option B: Use the helper script**
1. Open PowerShell in this directory
2. Run: `.\install-composer.ps1`
3. Follow the instructions

**Option C: Manual installation**
1. Download `composer.phar` from: https://getcomposer.org/composer.phar
2. Place it in a folder in your PATH
3. Create `composer.bat` with: `@echo off` and `php "%~dp0composer.phar" %*`

## Step 3: Install Dependencies

Open PowerShell in the `php-version` directory and run:

```powershell
composer install
```

This will download all Laravel dependencies.

## Step 4: Configure Environment

1. Copy `.env.example` to `.env`:
   ```powershell
   copy .env.example .env
   ```

2. Generate application key:
   ```powershell
   php artisan key:generate
   ```

3. Edit `.env` file and configure:
   - Database connection (SQLite is default)
   - Email settings
   - Admin credentials

## Step 5: Setup Database

```powershell
php artisan migrate --seed
```

This creates all tables and seeds initial data.

## Step 6: Start Development Server

```powershell
php artisan serve
```

Open your browser to: http://localhost:8000

## Default Login

- **Email:** admin@example.com (or as configured in .env)
- **Password:** Qa123456 (or as configured in .env)

## Troubleshooting

### Composer not found
- Make sure Composer is in your PATH
- Restart PowerShell after installation
- Run: `composer --version` to verify

### PHP not found
- Make sure PHP is installed and in PATH
- Restart PowerShell after adding PHP to PATH
- Run: `php --version` to verify

### Database errors
- Make sure SQLite is enabled in PHP
- Check `database/authorization.db` file permissions
- Try: `php artisan migrate:fresh --seed`

## Next Steps

After installation, you may need to:
1. Convert remaining controllers from C# to PHP
2. Convert views from Razor to Blade templates
3. Set up email service configuration
4. Configure Active Directory (if needed)

See `CONVERSION_NOTES.md` for details on what still needs to be converted.

