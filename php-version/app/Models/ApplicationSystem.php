<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ApplicationSystem extends Model
{
    protected $table = 'systems';

    protected $fillable = [
        'name',
        'description',
        'category',
        'is_active',
    ];

    protected $casts = [
        'is_active' => 'boolean',
        'created_at' => 'datetime',
        'updated_at' => 'datetime',
    ];
}

