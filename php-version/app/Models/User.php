<?php

namespace App\Models;

use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;
use Laravel\Sanctum\HasApiTokens;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class User extends Authenticatable
{
    use HasApiTokens, Notifiable;

    protected $fillable = [
        'name',
        'email',
        'password',
        'full_name',
        'department',
        'is_manager',
        'is_admin',
        'manager_id',
        'created_at',
    ];

    protected $hidden = [
        'password',
        'remember_token',
    ];

    protected $casts = [
        'email_verified_at' => 'datetime',
        'password' => 'hashed',
        'is_manager' => 'boolean',
        'is_admin' => 'boolean',
        'created_at' => 'datetime',
    ];

    /**
     * Get the manager that this user reports to
     */
    public function manager(): BelongsTo
    {
        return $this->belongsTo(User::class, 'manager_id');
    }

    /**
     * Get users that report to this user
     */
    public function subordinates()
    {
        return $this->hasMany(User::class, 'manager_id');
    }

    /**
     * Get authorization requests created by this user
     */
    public function authorizationRequests()
    {
        return $this->hasMany(AuthorizationRequest::class, 'user_id');
    }

    /**
     * Get requests where this user is the manager
     */
    public function managedRequests()
    {
        return $this->hasMany(AuthorizationRequest::class, 'manager_id');
    }

    /**
     * Get requests where this user is the final approver
     */
    public function finalApprovedRequests()
    {
        return $this->hasMany(AuthorizationRequest::class, 'final_approver_id');
    }

    /**
     * Check if user has a specific role
     */
    public function hasRole(string $role): bool
    {
        if ($role === 'Admin') {
            return $this->is_admin;
        }
        if ($role === 'Manager') {
            return $this->is_manager;
        }
        return true; // All users have 'User' role
    }

    /**
     * Check if user is admin
     */
    public function isAdmin(): bool
    {
        return $this->is_admin;
    }

    /**
     * Check if user is manager
     */
    public function isManager(): bool
    {
        return $this->is_manager;
    }
}

