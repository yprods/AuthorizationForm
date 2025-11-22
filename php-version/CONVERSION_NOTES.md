# Conversion Notes: ASP.NET Core to Laravel/PHP

This document outlines the key differences and conversion patterns used when converting the ASP.NET Core application to Laravel/PHP.

## Architecture Differences

### Framework
- **ASP.NET Core MVC** → **Laravel MVC**
- Both follow MVC pattern, so structure is similar

### Dependency Injection
- **ASP.NET Core**: Constructor injection via `IServiceCollection`
- **Laravel**: Constructor injection via Service Container (automatic)

### Routing
- **ASP.NET Core**: `app.MapControllerRoute()` in `Program.cs`
- **Laravel**: `routes/web.php` with `Route::` facade

### Views
- **ASP.NET Core**: Razor syntax (`@model`, `@Html.*`)
- **Laravel**: Blade syntax (`@extends`, `{{ }}`, `@if`)

### Database
- **ASP.NET Core**: Entity Framework Core with `DbContext`
- **Laravel**: Eloquent ORM with Models

### Authentication
- **ASP.NET Core**: ASP.NET Identity with `UserManager<T>`
- **Laravel**: Built-in Authentication with `Auth` facade

## Key Conversions

### Models

**C#:**
```csharp
public class User : IdentityUser
{
    public string? FullName { get; set; }
    public bool IsAdmin { get; set; }
}
```

**PHP:**
```php
class User extends Authenticatable
{
    protected $fillable = ['full_name', 'is_admin'];
    
    protected $casts = [
        'is_admin' => 'boolean',
    ];
}
```

### Controllers

**C#:**
```csharp
public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    
    public HomeController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        return View();
    }
}
```

**PHP:**
```php
class HomeController extends Controller
{
    public function index()
    {
        $user = Auth::user();
        return view('home.index');
    }
}
```

### Views

**Razor:**
```razor
@model AuthorizationRequest
<h1>@Model.User.FullName</h1>
@if (User.IsInRole("Admin"))
{
    <p>Admin content</p>
}
```

**Blade:**
```blade
<h1>{{ $request->user->full_name }}</h1>
@if (Auth::user()->isAdmin())
    <p>Admin content</p>
@endif
```

### Database Queries

**C# EF Core:**
```csharp
var requests = await _context.AuthorizationRequests
    .Where(r => r.Status == RequestStatus.Pending)
    .ToListAsync();
```

**PHP Eloquent:**
```php
$requests = AuthorizationRequest::where('status', AuthorizationRequest::STATUS_PENDING)
    ->get();
```

### Middleware

**C#:**
```csharp
public class AdminMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.User.IsInRole("Admin"))
        {
            context.Response.StatusCode = 403;
            return;
        }
        await next(context);
    }
}
```

**PHP:**
```php
class AdminMiddleware
{
    public function handle(Request $request, Closure $next)
    {
        if (!Auth::user()->isAdmin()) {
            abort(403);
        }
        return $next($request);
    }
}
```

## Remaining Work

The following components still need to be converted:

1. **Controllers**: 
   - AccountController
   - RequestsController
   - AdminController
   - ManagerController
   - SetupController
   - SearchController

2. **Services**:
   - EmailService
   - PdfService
   - AuthorizationService
   - ActiveDirectoryService
   - ReminderService

3. **Views**: All Razor views need to be converted to Blade templates

4. **Middleware**:
   - AutoLoginMiddleware
   - SetupCheckMiddleware

5. **Configuration**: Additional config files may be needed

## Testing

After conversion, test:
- User authentication and authorization
- CRUD operations for all entities
- Email sending functionality
- PDF generation
- Active Directory integration (if used)
- Reminder system

## Notes

- Windows Authentication is not directly available in PHP. Consider alternatives like LDAP authentication.
- Some .NET libraries don't have direct PHP equivalents (e.g., System.DirectoryServices → Adldap2)
- PDF generation uses DomPDF instead of iTextSharp
- Email uses PHPMailer instead of MailKit

