<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class EmailTemplate extends Model
{
    protected $fillable = [
        'name',
        'description',
        'trigger_type',
        'subject',
        'body',
        'is_active',
        'created_by_id',
        'recipient_type',
        'custom_recipients',
    ];

    protected $casts = [
        'trigger_type' => 'integer',
        'is_active' => 'boolean',
        'created_at' => 'datetime',
        'updated_at' => 'datetime',
    ];

    // Email Trigger Type Constants
    const TRIGGER_REQUEST_CREATED = 1;
    const TRIGGER_MANAGER_APPROVAL_REQUEST = 2;
    const TRIGGER_MANAGER_APPROVED = 3;
    const TRIGGER_MANAGER_REJECTED = 4;
    const TRIGGER_FINAL_APPROVAL_REQUEST = 5;
    const TRIGGER_FINAL_APPROVED = 6;
    const TRIGGER_FINAL_REJECTED = 7;
    const TRIGGER_REQUEST_CANCELLED_BY_USER = 8;
    const TRIGGER_REQUEST_CANCELLED_BY_MANAGER = 9;
    const TRIGGER_STATUS_CHANGED = 10;

    /**
     * Get the user who created this template
     */
    public function createdBy(): BelongsTo
    {
        return $this->belongsTo(User::class, 'created_by_id');
    }
}

