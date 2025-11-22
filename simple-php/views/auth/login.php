<?php
$title = 'התחברות';
ob_start();
?>

<h1>התחברות</h1>

<?php if (isset($error)): ?>
    <div class="error"><?= htmlspecialchars($error) ?></div>
<?php endif; ?>

<form method="POST" action="/login">
    <div class="form-group">
        <label>אימייל:</label>
        <input type="email" name="email" required>
    </div>
    
    <div class="form-group">
        <label>סיסמה:</label>
        <input type="password" name="password" required>
    </div>
    
    <button type="submit" class="btn">התחבר</button>
</form>

<?php
$content = ob_get_clean();
include VIEWS_PATH . '/layout.php';
?>

