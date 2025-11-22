<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class RequestHistory extends Model
{
    protected $fillable = [
        'request_id',
        'previous_status',
        'new_status',
        'action_performed_by',
        'action_performed_by_id',
        'comments',
    ];

    protected $casts = [
        'previous_status' => 'integer',
        'new_status' => 'integer',
        'created_at' => 'datetime',
    ];

    /**
     * Get the request this history belongs to
     */
    public function request(): BelongsTo
    {
        return $this->belongsTo(AuthorizationRequest::class, 'request_id');
    }
}

