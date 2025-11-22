<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class AuthorizationRequest extends Model
{
    protected $fillable = [
        'user_id',
        'service_level',
        'selected_employees',
        'selected_systems',
        'comments',
        'manager_id',
        'final_approver_id',
        'status',
        'manager_approved_at',
        'manager_approval_signature',
        'final_approved_at',
        'final_approval_decision',
        'final_approval_comments',
        'disclosure_acknowledged',
        'disclosure_acknowledged_at',
        'rejection_reason',
        'changed_by_admin_id',
        'previous_manager_id',
        'manager_changed_at',
        'pdf_path',
        'last_reminder_sent_at',
        'reminder_count',
    ];

    protected $casts = [
        'service_level' => 'integer',
        'status' => 'integer',
        'disclosure_acknowledged' => 'boolean',
        'manager_approved_at' => 'datetime',
        'disclosure_acknowledged_at' => 'datetime',
        'final_approved_at' => 'datetime',
        'manager_changed_at' => 'datetime',
        'last_reminder_sent_at' => 'datetime',
        'created_at' => 'datetime',
        'updated_at' => 'datetime',
        'reminder_count' => 'integer',
    ];

    // Service Level Constants
    const SERVICE_LEVEL_USER = 1;
    const SERVICE_LEVEL_OTHER_USER = 2;
    const SERVICE_LEVEL_MULTIPLE_USERS = 3;

    // Status Constants
    const STATUS_DRAFT = 0;
    const STATUS_PENDING_MANAGER_APPROVAL = 1;
    const STATUS_PENDING_FINAL_APPROVAL = 2;
    const STATUS_APPROVED = 3;
    const STATUS_REJECTED = 4;
    const STATUS_CANCELLED_BY_USER = 5;
    const STATUS_CANCELLED_BY_MANAGER = 6;
    const STATUS_MANAGER_CHANGED = 7;

    /**
     * Get the user who created this request
     */
    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class, 'user_id');
    }

    /**
     * Get the manager for this request
     */
    public function manager(): BelongsTo
    {
        return $this->belongsTo(User::class, 'manager_id');
    }

    /**
     * Get the final approver for this request
     */
    public function finalApprover(): BelongsTo
    {
        return $this->belongsTo(User::class, 'final_approver_id');
    }

    /**
     * Get the admin who changed this request
     */
    public function changedByAdmin(): BelongsTo
    {
        return $this->belongsTo(User::class, 'changed_by_admin_id');
    }

    /**
     * Get the history entries for this request
     */
    public function history(): HasMany
    {
        return $this->hasMany(RequestHistory::class, 'request_id');
    }

    /**
     * Get selected employees as array
     */
    public function getSelectedEmployeesAttribute($value)
    {
        return json_decode($value, true) ?? [];
    }

    /**
     * Set selected employees as JSON
     */
    public function setSelectedEmployeesAttribute($value)
    {
        $this->attributes['selected_employees'] = is_string($value) ? $value : json_encode($value);
    }

    /**
     * Get selected systems as array
     */
    public function getSelectedSystemsAttribute($value)
    {
        return json_decode($value, true) ?? [];
    }

    /**
     * Set selected systems as JSON
     */
    public function setSelectedSystemsAttribute($value)
    {
        $this->attributes['selected_systems'] = is_string($value) ? $value : json_encode($value);
    }

    /**
     * Check if request is pending manager approval
     */
    public function isPendingManagerApproval(): bool
    {
        return $this->status === self::STATUS_PENDING_MANAGER_APPROVAL;
    }

    /**
     * Check if request is pending final approval
     */
    public function isPendingFinalApproval(): bool
    {
        return $this->status === self::STATUS_PENDING_FINAL_APPROVAL;
    }

    /**
     * Check if request is approved
     */
    public function isApproved(): bool
    {
        return $this->status === self::STATUS_APPROVED;
    }

    /**
     * Check if request is rejected
     */
    public function isRejected(): bool
    {
        return $this->status === self::STATUS_REJECTED;
    }
}

