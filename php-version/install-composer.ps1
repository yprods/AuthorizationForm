# Composer Installation Helper Script for Windows
# This script helps verify prerequisites and provides installation guidance

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Composer Installation Helper" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if PHP is installed
Write-Host "Checking PHP installation..." -ForegroundColor Yellow
try {
    $phpVersion = php --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ PHP is installed" -ForegroundColor Green
        Write-Host "  Version: $($phpVersion[0])" -ForegroundColor Gray
        
        # Check PHP version
        $versionMatch = $phpVersion[0] -match "PHP (\d+)\.(\d+)"
        if ($versionMatch) {
            $major = [int]$matches[1]
            $minor = [int]$matches[2]
            if ($major -ge 8 -and $minor -ge 1) {
                Write-Host "✓ PHP version is compatible (8.1+)" -ForegroundColor Green
            } else {
                Write-Host "⚠ PHP version should be 8.1 or higher" -ForegroundColor Yellow
            }
        }
    }
} catch {
    Write-Host "✗ PHP is not installed or not in PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install PHP first:" -ForegroundColor Yellow
    Write-Host "  1. Download from: https://windows.php.net/download/" -ForegroundColor White
    Write-Host "  2. Extract to C:\php" -ForegroundColor White
    Write-Host "  3. Add C:\php to your system PATH" -ForegroundColor White
    Write-Host "  4. Restart PowerShell and run this script again" -ForegroundColor White
    exit 1
}

Write-Host ""

# Check if Composer is already installed
Write-Host "Checking Composer installation..." -ForegroundColor Yellow
try {
    $composerVersion = composer --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Composer is already installed!" -ForegroundColor Green
        Write-Host "  $composerVersion" -ForegroundColor Gray
        Write-Host ""
        Write-Host "You can now run: composer install" -ForegroundColor Cyan
        exit 0
    }
} catch {
    Write-Host "✗ Composer is not installed" -ForegroundColor Yellow
}

Write-Host ""

# Provide installation instructions
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Installation Instructions" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Method 1: Using Composer Installer (Recommended)" -ForegroundColor Yellow
Write-Host "  1. Download Composer-Setup.exe from:" -ForegroundColor White
Write-Host "     https://getcomposer.org/download/" -ForegroundColor Cyan
Write-Host "  2. Run the installer" -ForegroundColor White
Write-Host "  3. Follow the installation wizard" -ForegroundColor White
Write-Host ""
Write-Host "Method 2: Manual Installation" -ForegroundColor Yellow
Write-Host "  1. Download composer.phar from:" -ForegroundColor White
Write-Host "     https://getcomposer.org/composer.phar" -ForegroundColor Cyan
Write-Host "  2. Save it to a folder in your PATH" -ForegroundColor White
Write-Host "  3. Create composer.bat with: @echo off`nphp `"%~dp0composer.phar`" %*" -ForegroundColor White
Write-Host ""

# Offer to open download page
$response = Read-Host "Would you like to open the Composer download page? (Y/N)"
if ($response -eq 'Y' -or $response -eq 'y') {
    Start-Process "https://getcomposer.org/download/"
    Write-Host ""
    Write-Host "After installation, restart PowerShell and run:" -ForegroundColor Yellow
    Write-Host "  composer install" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "For more information, see: INSTALL_COMPOSER.md" -ForegroundColor Gray

