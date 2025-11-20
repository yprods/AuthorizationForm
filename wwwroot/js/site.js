// Write your JavaScript code here

$(document).ready(function() {
    // Enable tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Enable popovers
    $('[data-bs-toggle="popover"]').popover();
    
    // Confirm delete actions
    $('form[action*="Delete"]').on('submit', function(e) {
        if (!confirm('האם אתה בטוח שברצונך למחוק?')) {
            e.preventDefault();
            return false;
        }
    });
    
    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert').fadeOut('slow');
    }, 5000);
});

