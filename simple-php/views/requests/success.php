<?php
$title = 'בקשה נשלחה בהצלחה';
ob_start();
?>

<h1>בקשה נשלחה בהצלחה!</h1>

<div class="success">
    <p>מספר בקשה: <strong><?= $requestId ?></strong></p>
    <p>הבקשה נשלחה בהצלחה ותטופל בהקדם.</p>
</div>

<a href="/requests/create" class="btn">צור בקשה נוספת</a>

<?php
$content = ob_get_clean();
include VIEWS_PATH . '/layout.php';
?>

