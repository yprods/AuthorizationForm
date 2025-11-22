<?php

use Illuminate\Support\Facades\Route;
use App\Http\Controllers\HomeController;
use App\Http\Controllers\Auth\LoginController;
use App\Http\Controllers\RequestsController;
use App\Http\Controllers\AdminController;
use App\Http\Controllers\ManagerController;

/*
|--------------------------------------------------------------------------
| Web Routes
|--------------------------------------------------------------------------
*/

Route::get('/', [HomeController::class, 'index'])->name('home');

// Authentication Routes
Route::get('/login', [LoginController::class, 'showLoginForm'])->name('login');
Route::post('/login', [LoginController::class, 'login']);
Route::post('/logout', [LoginController::class, 'logout'])->name('logout');

// Public Routes - Allow anonymous access to create requests
Route::prefix('requests')->name('requests.')->group(function () {
    Route::get('/create', [RequestsController::class, 'create'])->name('create');
    Route::post('/store', [RequestsController::class, 'store'])->name('store');
});

// Authenticated Routes
Route::middleware(['auth'])->group(function () {
    
    // Requests Routes
    Route::prefix('requests')->name('requests.')->group(function () {
        Route::get('/', [RequestsController::class, 'index'])->name('index');
        Route::get('/{id}', [RequestsController::class, 'show'])->name('show');
        Route::get('/{id}/manager-approve', [RequestsController::class, 'managerApprove'])->name('manager-approve');
        Route::post('/{id}/manager-approve', [RequestsController::class, 'managerApproveStore'])->name('manager-approve.store');
        Route::get('/{id}/final-approve', [RequestsController::class, 'finalApprove'])->name('final-approve');
        Route::post('/{id}/final-approve', [RequestsController::class, 'finalApproveStore'])->name('final-approve.store');
        Route::get('/{id}/change-manager', [RequestsController::class, 'changeManager'])->name('change-manager');
        Route::post('/{id}/change-manager', [RequestsController::class, 'changeManagerStore'])->name('change-manager.store');
    });

    // Manager Routes
    Route::prefix('manager')->name('manager.')->middleware(['manager'])->group(function () {
        Route::get('/', [ManagerController::class, 'index'])->name('index');
    });

    // Admin Routes
    Route::prefix('admin')->name('admin.')->middleware(['admin'])->group(function () {
        Route::get('/', [AdminController::class, 'index'])->name('index');
        
        // Users Management
        Route::get('/users', [AdminController::class, 'users'])->name('users');
        Route::get('/users/create', [AdminController::class, 'createUser'])->name('users.create');
        Route::post('/users', [AdminController::class, 'storeUser'])->name('users.store');
        Route::get('/users/{id}/edit', [AdminController::class, 'editUser'])->name('users.edit');
        Route::put('/users/{id}', [AdminController::class, 'updateUser'])->name('users.update');
        
        // Employees Management
        Route::get('/employees', [AdminController::class, 'employees'])->name('employees');
        Route::get('/employees/create', [AdminController::class, 'createEmployee'])->name('employees.create');
        Route::post('/employees', [AdminController::class, 'storeEmployee'])->name('employees.store');
        Route::get('/employees/{id}/edit', [AdminController::class, 'editEmployee'])->name('employees.edit');
        Route::put('/employees/{id}', [AdminController::class, 'updateEmployee'])->name('employees.update');
        
        // Systems Management
        Route::get('/systems', [AdminController::class, 'systems'])->name('systems');
        Route::get('/systems/create', [AdminController::class, 'createSystem'])->name('systems.create');
        Route::post('/systems', [AdminController::class, 'storeSystem'])->name('systems.store');
        Route::get('/systems/{id}/edit', [AdminController::class, 'editSystem'])->name('systems.edit');
        Route::put('/systems/{id}', [AdminController::class, 'updateSystem'])->name('systems.update');
        
        // Email Templates Management
        Route::get('/email-templates', [AdminController::class, 'emailTemplates'])->name('email-templates');
        Route::get('/email-templates/create', [AdminController::class, 'createEmailTemplate'])->name('email-templates.create');
        Route::post('/email-templates', [AdminController::class, 'storeEmailTemplate'])->name('email-templates.store');
        Route::get('/email-templates/{id}/edit', [AdminController::class, 'editEmailTemplate'])->name('email-templates.edit');
        Route::put('/email-templates/{id}', [AdminController::class, 'updateEmailTemplate'])->name('email-templates.update');
        
        // Form Templates Management
        Route::get('/form-templates', [AdminController::class, 'formTemplates'])->name('form-templates');
        Route::get('/form-templates/create', [AdminController::class, 'createFormTemplate'])->name('form-templates.create');
        Route::post('/form-templates', [AdminController::class, 'storeFormTemplate'])->name('form-templates.store');
        Route::get('/form-templates/{id}/edit', [AdminController::class, 'editFormTemplate'])->name('form-templates.edit');
        Route::put('/form-templates/{id}', [AdminController::class, 'updateFormTemplate'])->name('form-templates.update');
    });

    // Setup Route
    Route::prefix('setup')->name('setup.')->group(function () {
        Route::get('/', [\App\Http\Controllers\SetupController::class, 'index'])->name('index');
        Route::post('/', [\App\Http\Controllers\SetupController::class, 'store'])->name('store');
    });
});

