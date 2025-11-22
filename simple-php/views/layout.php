<!DOCTYPE html>
<html lang="he" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><?= $title ?? 'Authorization Form' ?></title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: Arial, sans-serif; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; padding: 20px; }
        header { background: #2c3e50; color: white; padding: 15px 0; }
        nav { display: flex; justify-content: space-between; align-items: center; }
        nav a { color: white; text-decoration: none; margin: 0 10px; }
        nav a:hover { text-decoration: underline; }
        .content { background: white; padding: 20px; margin-top: 20px; border-radius: 5px; }
        .btn { padding: 10px 20px; background: #3498db; color: white; border: none; border-radius: 3px; cursor: pointer; }
        .btn:hover { background: #2980b9; }
        .btn-danger { background: #e74c3c; }
        .btn-danger:hover { background: #c0392b; }
        .btn-success { background: #27ae60; }
        .btn-success:hover { background: #229954; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        table th, table td { padding: 10px; border: 1px solid #ddd; text-align: right; }
        table th { background: #ecf0f1; }
        .form-group { margin-bottom: 15px; }
        .form-group label { display: block; margin-bottom: 5px; font-weight: bold; }
        .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 3px; }
        .error { color: #e74c3c; margin-top: 5px; }
        .success { color: #27ae60; margin-top: 5px; }
    </style>
</head>
<body>
    <header>
        <div class="container">
            <nav>
                <div>
                    <a href="/">דף הבית</a>
                    <?php if (Auth::getInstance()->isLoggedIn()): ?>
                        <a href="/requests">בקשות</a>
                        <?php if (Auth::getInstance()->isManager()): ?>
                            <a href="/manager">מנהל</a>
                        <?php endif; ?>
                        <?php if (Auth::getInstance()->isAdmin()): ?>
                            <a href="/admin">ניהול</a>
                        <?php endif; ?>
                    <?php else: ?>
                        <a href="/requests/create">צור בקשה</a>
                    <?php endif; ?>
                </div>
                <div>
                    <?php if (Auth::getInstance()->isLoggedIn()): ?>
                        <span><?= htmlspecialchars(Auth::getInstance()->getUser()['name']) ?></span>
                        <form method="POST" action="/logout" style="display: inline;">
                            <button type="submit" class="btn btn-danger">התנתק</button>
                        </form>
                    <?php else: ?>
                        <a href="/login">התחבר</a>
                    <?php endif; ?>
                </div>
            </nav>
        </div>
    </header>
    
    <div class="container">
        <div class="content">
            <?php if (isset($content)): ?>
                <?= $content ?>
            <?php else: ?>
                <p>No content</p>
            <?php endif; ?>
        </div>
    </div>
</body>
</html>

