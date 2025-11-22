<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('authorization_requests', function (Blueprint $table) {
            $table->id();
            $table->foreignId('user_id')->constrained('users')->onDelete('restrict');
            $table->integer('service_level');
            $table->text('selected_employees'); // JSON
            $table->text('selected_systems'); // JSON
            $table->text('comments')->nullable();
            $table->foreignId('manager_id')->constrained('users')->onDelete('restrict');
            $table->foreignId('final_approver_id')->nullable()->constrained('users')->onDelete('restrict');
            $table->integer('status')->default(0);
            $table->timestamp('manager_approved_at')->nullable();
            $table->string('manager_approval_signature')->nullable();
            $table->timestamp('final_approved_at')->nullable();
            $table->string('final_approval_decision')->nullable();
            $table->text('final_approval_comments')->nullable();
            $table->boolean('disclosure_acknowledged')->default(false);
            $table->timestamp('disclosure_acknowledged_at')->nullable();
            $table->text('rejection_reason')->nullable();
            $table->foreignId('changed_by_admin_id')->nullable()->constrained('users')->onDelete('restrict');
            $table->foreignId('previous_manager_id')->nullable()->constrained('users')->onDelete('restrict');
            $table->timestamp('manager_changed_at')->nullable();
            $table->string('pdf_path')->nullable();
            $table->timestamp('last_reminder_sent_at')->nullable();
            $table->integer('reminder_count')->default(0);
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('authorization_requests');
    }
};

