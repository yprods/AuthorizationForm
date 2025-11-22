<?php
$title = 'פרטי בקשה';
ob_start();
?>

<h1>פרטי בקשה #<?= $request['id'] ?></h1>

<div class="form-group">
    <strong>משתמש:</strong> <?= htmlspecialchars($request['user_full_name'] ?? $request['user_name'] ?? 'אנונימי') ?>
</div>

<div class="form-group">
    <strong>מנהל:</strong> <?= htmlspecialchars($request['manager_full_name'] ?? $request['manager_name'] ?? 'לא הוגדר') ?>
</div>

<div class="form-group">
    <strong>סטטוס:</strong>
    <?php
    $statuses = [
        0 => 'טיוטה',
        1 => 'ממתין לאישור מנהל',
        2 => 'ממתין לאישור סופי',
        3 => 'אושר',
        4 => 'נדחה',
        5 => 'בוטל על ידי משתמש',
        6 => 'בוטל על ידי מנהל',
        7 => 'מנהל שונה',
    ];
    echo $statuses[$request['status']] ?? 'לא ידוע';
    ?>
</div>

<?php if ($request['comments']): ?>
    <div class="form-group">
        <strong>הערות:</strong>
        <p><?= nl2br(htmlspecialchars($request['comments'])) ?></p>
    </div>
<?php endif; ?>

<?php if (!empty($history)): ?>
    <h2>היסטוריה</h2>
    <table>
        <thead>
            <tr>
                <th>תאריך</th>
                <th>סטטוס קודם</th>
                <th>סטטוס חדש</th>
                <th>הערות</th>
            </tr>
        </thead>
        <tbody>
            <?php foreach ($history as $h): ?>
                <tr>
                    <td><?= date('d/m/Y H:i', strtotime($h['created_at'])) ?></td>
                    <td><?= $statuses[$h['previous_status']] ?? $h['previous_status'] ?></td>
                    <td><?= $statuses[$h['new_status']] ?? $h['new_status'] ?></td>
                    <td><?= htmlspecialchars($h['comments'] ?? '') ?></td>
                </tr>
            <?php endforeach; ?>
        </tbody>
    </table>
<?php endif; ?>

<a href="/requests" class="btn">חזור לרשימה</a>

<?php
$content = ob_get_clean();
include VIEWS_PATH . '/layout.php';
?>

