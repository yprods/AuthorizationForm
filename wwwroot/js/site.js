// Modern JavaScript for Authorization Form
// Auto-complete and Search Functionality

$(document).ready(function() {
    // Employee search with highlighting
    if ($('#employeeSearch').length) {
        $('#employeeSearch').on('keyup', function() {
            var value = $(this).val().toLowerCase();
            var items = $('.employee-item');
            var visibleCount = 0;
            
            items.each(function() {
                var name = $(this).data('name') || '';
                var isVisible = name.toLowerCase().indexOf(value) > -1 || value === '';
                $(this).toggle(isVisible);
                if (isVisible) visibleCount++;
            });
            
            // Show message if no results
            var listContainer = $('.employee-list');
            if (visibleCount === 0 && value !== '') {
                if ($('.no-results-message').length === 0) {
                    listContainer.append('<div class="no-results-message alert alert-info">לא נמצאו תוצאות</div>');
                }
            } else {
                $('.no-results-message').remove();
            }
        });
    }

    // Manager search with Active Directory auto-complete
    if ($('#managerSearch').length) {
        let searchTimeout;
        let currentResults = [];
        
        $('#managerSearch').on('input', function() {
            clearTimeout(searchTimeout);
            var value = $(this).val().trim();
            var dropdown = $('.autocomplete-dropdown');
            
            if (value.length < 2) {
                dropdown.hide();
                currentResults = [];
                return;
            }
            
            // Debounce search
            searchTimeout = setTimeout(function() {
                searchAdUsers(value);
            }, 300);
        });
        
        function searchAdUsers(term) {
            var input = $('#managerSearch');
            var dropdown = $('.autocomplete-dropdown');
            
            if (!dropdown.length) {
                // Create dropdown if it doesn't exist
                input.parent().append('<div class="autocomplete-dropdown"></div>');
                dropdown = $('.autocomplete-dropdown');
            }
            
            // Show loading
            dropdown.html('<div class="autocomplete-item text-center p-3"><div class="spinner-border spinner-border-sm" role="status"></div> טוען...</div>').show();
            
            console.log('Searching AD users for term:', term);
            
            $.ajax({
                url: '/Requests/SearchAdUsers',
                method: 'GET',
                data: { term: term, maxResults: 15 },
                timeout: 10000,
                success: function(users) {
                    console.log('AD search results:', users);
                    
                    // Filter out error objects
                    if (users && Array.isArray(users)) {
                        currentResults = users.filter(function(u) {
                            return !u.error; // Remove error objects
                        });
                    } else {
                        currentResults = [];
                    }
                    
                    // If all results are errors, show error message
                    if (currentResults.length === 0 && users && users.length > 0 && users[0].error) {
                        dropdown.html('<div class="autocomplete-item text-danger"><i class="bi bi-exclamation-triangle"></i> ' + (users[0].message || 'שגיאה בחיפוש') + '</div>').show();
                    } else {
                        renderAutocomplete(currentResults);
                    }
                },
                error: function(xhr, status, error) {
                    console.error('AD search error:', status, error, xhr);
                    var errorMsg = 'שגיאה בחיפוש משתמשים';
                    if (xhr.responseJSON && xhr.responseJSON.error) {
                        errorMsg = xhr.responseJSON.error;
                    } else if (xhr.status === 404) {
                        errorMsg = 'Endpoint לא נמצא. בדוק שהקוד מוגדר נכון.';
                    } else if (xhr.status === 500) {
                        errorMsg = 'שגיאת שרת. בדוק את הלוגים.';
                    }
                    dropdown.html('<div class="autocomplete-item text-danger"><i class="bi bi-exclamation-triangle"></i> ' + errorMsg + '</div>');
                }
            });
        }
        
        function renderAutocomplete(users) {
            var dropdown = $('.autocomplete-dropdown');
            
            if (users.length === 0) {
                dropdown.html('<div class="autocomplete-item text-muted"><i class="bi bi-info-circle"></i> לא נמצאו תוצאות</div>').show();
                return;
            }
            
            var html = users.map(function(user, index) {
                var isLocal = user.isLocal === true;
                var badge = isLocal ? '<span class="badge bg-success ms-2">מקומי</span>' : '<span class="badge bg-primary ms-2">AD</span>';
                var display = `${user.fullName}${user.email ? ' - ' + user.email : ''}${user.department ? ' (' + user.department + ')' : ''}`;
                return `<div class="autocomplete-item" data-index="${index}" data-username="${user.username}" data-user-id="${user.userId || ''}" data-is-local="${isLocal}">
                    <strong>${user.fullName}</strong> ${badge}
                    ${user.email ? '<br><small class="text-muted"><i class="bi bi-envelope"></i> ' + user.email + '</small>' : ''}
                    ${user.department ? '<br><small class="text-muted"><i class="bi bi-building"></i> ' + user.department + '</small>' : ''}
                </div>`;
            }).join('');
            
            dropdown.html(html).show();
        }
        
        // Handle autocomplete selection
        $(document).on('click', '.autocomplete-item', function(e) {
            e.stopPropagation();
            var index = $(this).data('index');
            var user = currentResults[index];
            
            if (user && !$(this).hasClass('text-danger') && !$(this).hasClass('text-muted')) {
                // Set the selected user display
                $('#managerSearch').val(user.fullName + (user.email ? ' - ' + user.email : ''));
                $('.autocomplete-dropdown').hide();
                
                // Find matching option in select
                var select = $('#managerSelect');
                
                // If user has userId (from local DB), use it directly
                if (user.userId && user.isLocal) {
                    var option = select.find(`option[value="${user.userId}"]`);
                    if (option.length > 0) {
                        select.val(user.userId);
                        select.show(); // Show select if hidden
                        $('#selectedManagerId').val(user.userId);
                        $('.import-user-message').remove(); // Remove any previous messages
                        return;
                    }
                }
                
                // Try to find by username in existing options
                var option = select.find(`option[data-username="${user.username}"]`);
                
                if (option.length > 0) {
                    // User exists in dropdown - select it
                    select.val(option.val());
                    select.show();
                    $('#selectedManagerId').val(option.val());
                    $('.import-user-message').remove();
                } else {
                    // User not in dropdown - add it
                    console.log('Adding user to dropdown:', user);
                    
                    var valueToUse = user.userId || user.username;
                    
                    // Add option to select
                    var newOption = $('<option></option>')
                        .attr('value', valueToUse)
                        .attr('data-username', user.username)
                        .attr('data-temp', user.isLocal ? 'false' : 'true')
                        .text(user.fullName + (user.email ? ' - ' + user.email : ''));
                    
                    select.append(newOption);
                    select.val(valueToUse);
                    select.show();
                    $('#selectedManagerId').val(valueToUse);
                    
                    // Show info message only for AD users (not local)
                    if (!user.isLocal) {
                        $('.import-user-message').remove();
                        select.after('<div class="alert alert-info import-user-message mt-2"><i class="bi bi-info-circle"></i> משתמש זה מ-Active Directory. אם הוא לא במערכת, הוא יתווסף בעת שליחת הבקשה.</div>');
                    } else {
                        $('.import-user-message').remove();
                    }
                }
            }
        });
        
        // Hide dropdown when clicking outside
        $(document).on('click', function(e) {
            if (!$(e.target).closest('#managerSearch, .autocomplete-dropdown').length) {
                $('.autocomplete-dropdown').hide();
            }
        });
        
        // Create autocomplete dropdown container
        if ($('.autocomplete-dropdown').length === 0) {
            $('#managerSearch').parent().append('<div class="autocomplete-dropdown"></div>');
        }
    }

    // Theme toggle with smooth transition
    if ($('#themeToggle').length) {
        $('#themeToggle').on('click', function() {
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            
            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            
            // Update icon
            const icon = $('#themeIcon');
            if (newTheme === 'dark') {
                icon.removeClass('bi-moon-stars-fill').addClass('bi-sun-fill');
            } else {
                icon.removeClass('bi-sun-fill').addClass('bi-moon-stars-fill');
            }
        });
        
        // Initialize icon on load
        const savedTheme = localStorage.getItem('theme') || 'light';
        const icon = $('#themeIcon');
        if (savedTheme === 'dark') {
            icon.removeClass('bi-moon-stars-fill').addClass('bi-sun-fill');
        }
    }

    // Add loading overlay for form submissions
    $('form').on('submit', function() {
        var form = $(this);
        if (form.find('button[type="submit"]').length) {
            var submitBtn = form.find('button[type="submit"]').first();
            var originalText = submitBtn.html();
            
            submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>טוען...');
            
            // Re-enable after 30 seconds as fallback
            setTimeout(function() {
                submitBtn.prop('disabled', false).html(originalText);
            }, 30000);
        }
    });

    // Add fade-in animation to cards on page load
    $('.card').each(function(index) {
        $(this).css({
            'opacity': '0',
            'transform': 'translateY(20px)'
        }).delay(index * 50).animate({
            'opacity': '1',
            'transform': 'translateY(0)'
        }, 300);
    });

    // Add smooth scroll to top button
    if ($(window).scrollTop() > 100) {
        $('<button id="scrollToTop" class="btn btn-primary position-fixed" style="bottom: 20px; left: 20px; z-index: 1000; border-radius: 50%; width: 50px; height: 50px; display: flex; align-items: center; justify-content: center;"><i class="bi bi-arrow-up"></i></button>').appendTo('body');
    }
    
    $(window).on('scroll', function() {
        if ($(this).scrollTop() > 100) {
            if ($('#scrollToTop').length === 0) {
                $('<button id="scrollToTop" class="btn btn-primary position-fixed" style="bottom: 20px; left: 20px; z-index: 1000; border-radius: 50%; width: 50px; height: 50px; display: flex; align-items: center; justify-content: center;"><i class="bi bi-arrow-up"></i></button>').appendTo('body').fadeIn();
            } else {
                $('#scrollToTop').fadeIn();
            }
        } else {
            $('#scrollToTop').fadeOut();
        }
    });
    
    $(document).on('click', '#scrollToTop', function() {
        $('html, body').animate({ scrollTop: 0 }, 500);
        return false;
    });

    // Add confirmation dialogs for delete actions
    $('form[data-confirm]').on('submit', function(e) {
        var message = $(this).data('confirm');
        if (!confirm(message || 'האם אתה בטוח?')) {
            e.preventDefault();
            return false;
        }
    });
});

// Performance: Lazy load images
if ('IntersectionObserver' in window) {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                observer.unobserve(img);
            }
        });
    });

    document.querySelectorAll('img.lazy').forEach(img => imageObserver.observe(img));
}
