<?php
if (!isset($title)) $title = 'צור בקשה חדשה';
ob_start();
?>

<h1>צור בקשה חדשה</h1>

<form method="POST" action="/requests/store">
    <div class="form-group">
        <label>רמת שירות:</label>
        <select name="service_level" required>
            <option value="1">רמת משתמש</option>
            <option value="2">רמת משתמש אחר</option>
            <option value="3">מספר משתמשים</option>
        </select>
    </div>
    
    <div class="form-group">
        <label>עובדים נבחרים:</label>
        <select name="selected_employees[]" multiple size="5">
            <?php foreach ($employees as $employee): ?>
                <option value="<?= $employee['id'] ?>">
                    <?= htmlspecialchars($employee['first_name'] . ' ' . $employee['last_name']) ?>
                </option>
            <?php endforeach; ?>
        </select>
    </div>
    
    <div class="form-group">
        <label>מערכות נבחרות:</label>
        <?php foreach ($systems as $system): ?>
            <label>
                <input type="checkbox" name="selected_systems[]" value="<?= $system['id'] ?>">
                <?= htmlspecialchars($system['name']) ?>
            </label><br>
        <?php endforeach; ?>
    </div>
    
    <div class="form-group">
        <label>הערות:</label>
        <textarea name="comments" rows="4"></textarea>
    </div>
    
    <div class="form-group">
        <label>
            <input type="checkbox" name="disclosure_acknowledged" required>
            אני מאשר שקראתי והבנתי את תנאי הגילוי
        </label>
    </div>
    
    <button type="submit" class="btn btn-success">שלח בקשה</button>
</form>

<script>
document.querySelector('form').addEventListener('submit', function(e) {
    const employees = Array.from(document.querySelectorAll('select[name="selected_employees[]"] option:checked'))
        .map(opt => opt.value);
    const systems = Array.from(document.querySelectorAll('input[name="selected_systems[]"]:checked'))
        .map(cb => cb.value);
    
    const hiddenEmployees = document.createElement('input');
    hiddenEmployees.type = 'hidden';
    hiddenEmployees.name = 'selected_employees';
    hiddenEmployees.value = JSON.stringify(employees);
    this.appendChild(hiddenEmployees);
    
    const hiddenSystems = document.createElement('input');
    hiddenSystems.type = 'hidden';
    hiddenSystems.name = 'selected_systems';
    hiddenSystems.value = JSON.stringify(systems);
    this.appendChild(hiddenSystems);
});
</script>

<?php
$content = ob_get_clean();
include VIEWS_PATH . '/layout.php';
?>

