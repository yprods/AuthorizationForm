# How to Install Composer on Windows

Composer is a dependency manager for PHP. Follow these steps to install it on Windows.

## Method 1: Using Composer Installer (Recommended)

1. **Download Composer Installer:**
   - Go to: https://getcomposer.org/download/
   - Click on "Composer-Setup.exe" to download the Windows installer

2. **Run the Installer:**
   - Double-click `Composer-Setup.exe`
   - The installer will:
     - Check for PHP installation
     - Download the latest Composer
     - Add Composer to your PATH
     - Set up the Composer environment

3. **Verify Installation:**
   - Open PowerShell or Command Prompt
   - Run: `composer --version`
   - You should see the Composer version number

## Method 2: Manual Installation

1. **Download Composer:**
   - Go to: https://getcomposer.org/Composer-Setup.exe
   - Download `composer.phar` file

2. **Place composer.phar:**
   - Move `composer.phar` to a directory in your PATH (e.g., `C:\ProgramData\ComposerSetup\bin\`)

3. **Create composer.bat:**
   - Create a file named `composer.bat` in the same directory with this content:
   ```batch
   @echo off
   php "%~dp0composer.phar" %*
   ```

## Prerequisites

Before installing Composer, make sure you have:

1. **PHP installed** (PHP 8.1 or higher)
   - Download from: https://windows.php.net/download/
   - Make sure PHP is in your system PATH
   - Verify by running: `php --version`

2. **OpenSSL extension enabled** (usually enabled by default)

## Quick Installation Script

You can also use the PowerShell script provided (`install-composer.ps1`) to help with installation.

## After Installation

Once Composer is installed, navigate to the `php-version` directory and run:

```bash
cd php-version
composer install
```

This will install all Laravel dependencies.

## Troubleshooting

### "composer is not recognized"
- Make sure Composer is added to your system PATH
- Restart your terminal/PowerShell after installation
- Verify PATH: `echo $env:PATH` (PowerShell) or `echo %PATH%` (CMD)

### "PHP not found"
- Install PHP first
- Add PHP to your system PATH
- Restart terminal after adding PHP to PATH

### SSL Certificate Issues
- Download `cacert.pem` from: https://curl.se/ca/cacert.pem
- Add to `php.ini`: `openssl.cafile="C:\path\to\cacert.pem"`

## Verify Everything Works

Run these commands to verify:

```bash
php --version        # Should show PHP 8.1+
composer --version   # Should show Composer version
```

