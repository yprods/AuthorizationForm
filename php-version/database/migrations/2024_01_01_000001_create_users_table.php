<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('users', function (Blueprint $table) {
            $table->id();
            $table->string('name');
            $table->string('email')->unique();
            $table->timestamp('email_verified_at')->nullable();
            $table->string('password');
            $table->string('full_name')->nullable();
            $table->string('department')->nullable();
            $table->boolean('is_manager')->default(false);
            $table->boolean('is_admin')->default(false);
            $table->unsignedBigInteger('manager_id')->nullable();
            $table->rememberToken();
            $table->timestamps();

            $table->foreign('manager_id')->references('id')->on('users')->onDelete('restrict');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('users');
    }
};

