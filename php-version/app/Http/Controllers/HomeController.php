<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use App\Models\User;

class HomeController extends Controller
{
    public function index()
    {
        // Check if setup is needed (for authenticated users accessing admin/management areas)
        if (Auth::check()) {
            $isSetupNeeded = $this->isSetupNeeded();
            if ($isSetupNeeded) {
                return redirect()->route('setup.index');
            }
        }

        // If user is not authenticated, redirect to create form (allow anonymous access)
        if (!Auth::check()) {
            return redirect()->route('requests.create');
        }

        $user = Auth::user();
        if (!$user) {
            return redirect()->route('login');
        }

        // Redirect Admin users to Admin panel
        if ($user->isAdmin()) {
            return redirect()->route('admin.index');
        }

        // Redirect Manager users to Manager dashboard
        if ($user->isManager()) {
            return redirect()->route('manager.index');
        }

        // Regular users go to requests page
        return redirect()->route('requests.index');
    }

    /**
     * Check if setup is needed
     */
    private function isSetupNeeded(): bool
    {
        // Check if any admin users exist
        $adminUsers = User::where('is_admin', true)->count();
        return $adminUsers === 0;
    }
}

