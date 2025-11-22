<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;
use App\Models\User;
use App\Models\ApplicationSystem;
use App\Models\Employee;

class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        // Create default roles (handled by User model flags)
        
        // Create admin user
        User::firstOrCreate(
            ['email' => env('ADMIN_EMAIL', 'admin@example.com')],
            [
                'name' => env('ADMIN_USERNAME', 'admin'),
                'email' => env('ADMIN_EMAIL', 'admin@example.com'),
                'password' => Hash::make(env('ADMIN_PASSWORD', 'Qa123456')),
                'full_name' => env('ADMIN_FULL_NAME', 'מנהל מערכת'),
                'is_admin' => true,
                'is_manager' => false,
            ]
        );

        // Create default systems
        ApplicationSystem::firstOrCreate(
            ['id' => 1],
            [
                'name' => 'מערכת HR',
                'description' => 'מערכת ניהול משאבי אנוש',
                'category' => 'ניהול',
                'is_active' => true,
            ]
        );

        ApplicationSystem::firstOrCreate(
            ['id' => 2],
            [
                'name' => 'מערכת כספים',
                'description' => 'מערכת ניהול כספים',
                'category' => 'כספים',
                'is_active' => true,
            ]
        );

        ApplicationSystem::firstOrCreate(
            ['id' => 3],
            [
                'name' => 'מערכת מכירות',
                'description' => 'מערכת ניהול מכירות',
                'category' => 'מכירות',
                'is_active' => true,
            ]
        );

        ApplicationSystem::firstOrCreate(
            ['id' => 4],
            [
                'name' => 'מערכת לוגיסטיקה',
                'description' => 'מערכת ניהול לוגיסטיקה',
                'category' => 'לוגיסטיקה',
                'is_active' => true,
            ]
        );

        // Create sample employees
        Employee::firstOrCreate(
            ['employee_id' => 'EMP001'],
            [
                'first_name' => 'יוסי',
                'last_name' => 'כהן',
                'department' => 'IT',
                'email' => 'yossi@example.com',
                'is_active' => true,
            ]
        );

        Employee::firstOrCreate(
            ['employee_id' => 'EMP002'],
            [
                'first_name' => 'שרה',
                'last_name' => 'לוי',
                'department' => 'HR',
                'email' => 'sara@example.com',
                'is_active' => true,
            ]
        );

        Employee::firstOrCreate(
            ['employee_id' => 'EMP003'],
            [
                'first_name' => 'דוד',
                'last_name' => 'ישראלי',
                'department' => 'כספים',
                'email' => 'david@example.com',
                'is_active' => true,
            ]
        );
    }
}

