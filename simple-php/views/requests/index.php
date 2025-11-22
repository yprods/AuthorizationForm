<?php
$title = 'רשימת בקשות';
ob_start();
?>

<h1>רשימת בקשות</h1>

<table>
    <thead>
        <tr>
            <th>מספר</th>
            <th>משתמש</th>
            <th>סטטוס</th>
            <th>תאריך יצירה</th>
            <th>פעולות</th>
        </tr>
    </thead>
    <tbody>
        <?php foreach ($requests as $request): ?>
            <tr>
                <td><?= $request['id'] ?></td>
                <td><?= htmlspecialchars($request['user_name'] ?? 'אנונימי') ?></td>
                <td>
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
                </td>
                <td><?= date('d/m/Y H:i', strtotime($request['created_at'])) ?></td>
                <td>
                    <a href="/requests/<?= $request['id'] ?>" class="btn">צפה</a>
                </td>
            </tr>
        <?php endforeach; ?>
    </tbody>
</table>

<?php
$content = ob_get_clean();
include VIEWS_PATH . '/layout.php';
?>

