<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('request_histories', function (Blueprint $table) {
            $table->id();
            $table->foreignId('request_id')->constrained('authorization_requests')->onDelete('cascade');
            $table->integer('previous_status');
            $table->integer('new_status');
            $table->string('action_performed_by')->nullable();
            $table->string('action_performed_by_id')->nullable();
            $table->text('comments')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('request_histories');
    }
};

