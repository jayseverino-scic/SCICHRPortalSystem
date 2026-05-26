/*!
 * Sidebar Navigation System
 * Sea Clara HR Portal
 * Enhanced Version with Error Handling and Features
 */

(function ($) {
    'use strict';

    // Constants
    const CLICK_EVENT = 'click';
    const STORAGE_KEY = 'scichr_sidebar_collapsed';
    const ANIMATION_DURATION = 300;
    const DEBUG = false; // Set to true for console logging

    // State
    let isCollapsed = false;
    let isMobile = window.innerWidth <= 768;

    // Logging helper
    const log = (...args) => {
        if (DEBUG) {
            console.log('[Sidebar]', ...args);
        }
    };

    // Error handler
    const handleError = (context, error) => {
        console.error(`[Sidebar Error in ${context}]:`, error);
    };

    /* ====================================
       STATE MANAGEMENT
       ==================================== */

    // Load the sidebar state from local storage
    const loadState = () => {
        try {
            const stored = localStorage.getItem(STORAGE_KEY);
            return stored === 'true';
        } catch (e) {
            handleError('loadState', e);
            return false;
        }
    };

    // Save the sidebar state to local storage
    const saveState = (collapsed) => {
        try {
            localStorage.setItem(STORAGE_KEY, collapsed.toString());
            log('State saved:', collapsed);
        } catch (e) {
            handleError('saveState', e);
        }
    };

    // Apply the collapsed or expanded class to sidebar
    const applyState = (collapsed) => {
        const $sb = $('#admin-sidebar');
        const $body = $('body');

        if (!$sb.length) {
            console.warn('[Sidebar] #admin-sidebar element not found.');
            return;
        }

        try {
            if (collapsed) {
                $sb.addClass('sidebar-collapsed').removeClass('sidebar-expanded');
                $body.addClass('sidebar-collapsed');
                // Close all submenus when collapsing
                $('.sidebar-has-sub').removeClass('sub-open');
                $('.sidebar-sub-menu').hide();
                log('Sidebar collapsed');
            } else {
                $sb.removeClass('sidebar-collapsed').addClass('sidebar-expanded');
                $body.removeClass('sidebar-collapsed');
                log('Sidebar expanded');
            }
        } catch (e) {
            handleError('applyState', e);
        }
    };

    /* ====================================
       SIDEBAR TOGGLE
       ==================================== */

    // Toggle sidebar
    const toggleSidebar = () => {
        try {
            isCollapsed = !isCollapsed;
            applyState(isCollapsed);
            saveState(isCollapsed);
            log('Sidebar toggled:', isCollapsed ? 'collapsed' : 'expanded');
        } catch (e) {
            handleError('toggleSidebar', e);
        }
    };

    /* ====================================
       SUBMENU MANAGEMENT
       ==================================== */

    // Toggle submenus with smooth animation
    const toggleSubMenu = ($trigger) => {
        try {
            // If sidebar is collapsed, expand it first
            if (isCollapsed) {
                isCollapsed = false;
                applyState(false);
                saveState(false);

                // Wait for sidebar expansion, then open submenu
                setTimeout(() => {
                    openSubMenu($trigger);
                }, ANIMATION_DURATION);
                return;
            }

            openSubMenu($trigger);
        } catch (e) {
            handleError('toggleSubMenu', e);
        }
    };

    // Open/close submenu
    const openSubMenu = ($trigger) => {
        try {
            const $parentLi = $trigger.closest('.sidebar-has-sub');
            const $menu = $parentLi.find('.sidebar-sub-menu');

            if (!$menu.length) {
                log('Warning: Submenu not found for trigger:', $trigger);
                return;
            }

            const isCurrentlyOpen = $parentLi.hasClass('sub-open');

            // Close all other submenus
            $('.sidebar-has-sub').not($parentLi).removeClass('sub-open');
            $('.sidebar-sub-menu').not($menu).slideUp(ANIMATION_DURATION);

            // Toggle current submenu
            if (isCurrentlyOpen) {
                $parentLi.removeClass('sub-open');
                $menu.slideUp(ANIMATION_DURATION);
                log('Submenu closed');
            } else {
                $parentLi.addClass('sub-open');
                $menu.slideDown(ANIMATION_DURATION);
                log('Submenu opened');
            }
        } catch (e) {
            handleError('openSubMenu', e);
        }
    };

    /* ====================================
       ACTIVE STATE MANAGEMENT
       ==================================== */

    // Set active state for nav items
    const setActiveNavItem = ($item) => {
        try {
            // Remove active from all items
            $('.sidebar-nav-link').removeClass('active');
            $('.sidebar-has-sub').removeClass('active');

            // Add active to clicked item
            if ($item.closest('.sidebar-sub-menu').length) {
                // It's a submenu item
                $item.addClass('active');
                $item.closest('.sidebar-has-sub').addClass('active');
                log('Active state set: submenu item');
            } else if ($item.closest('.sidebar-has-sub').length) {
                // It's a parent with submenu
                $item.closest('.sidebar-has-sub').addClass('active');
                log('Active state set: parent item');
            } else {
                // Regular nav item
                $item.addClass('active');
                log('Active state set: regular item');
            }
        } catch (e) {
            handleError('setActiveNavItem', e);
        }
    };

    // Highlight active links based on current URL
    const highlightActive = () => {
        try {
            const currentPath = window.location.pathname.toLowerCase();
            const pathParts = currentPath.split('/').filter(p => p);
            const currentAction = pathParts[pathParts.length - 1] || '';

            log('Highlighting active link for path:', currentPath);

            let matched = false;

            // Check all navigation links
            $('.sidebar-nav-link[href]').each(function () {
                const $link = $(this);
                const href = ($link.attr('href') || '').toLowerCase();

                // Skip javascript:void(0) and # links
                if (!href || href === '#' || href.includes('javascript:')) {
                    return;
                }

                // Extract controller and action from href
                const hrefParts = href.split('/').filter(p => p);
                const hrefAction = hrefParts[hrefParts.length - 1] || '';

                // Check if current path matches
                if (currentAction && hrefAction && currentAction === hrefAction) {
                    setActiveNavItem($link);
                    matched = true;

                    // If it's in a submenu, open the parent
                    if ($link.closest('.sidebar-sub-menu').length) {
                        const $parent = $link.closest('.sidebar-has-sub');
                        $parent.addClass('sub-open');
                        $parent.find('.sidebar-sub-menu').show();
                        log('Parent submenu opened for active item');
                    }

                    return false; // Break the loop
                }
            });

            if (!matched) {
                log('No matching link found for current path');
            }
        } catch (e) {
            handleError('highlightActive', e);
        }
    };

    /* ====================================
       MOBILE HANDLING
       ==================================== */

    // Handle window resize
    const handleResize = () => {
        const wasMobile = isMobile;
        isMobile = window.innerWidth <= 768;

        if (wasMobile !== isMobile) {
            if (isMobile) {
                // Switched to mobile
                $('#admin-sidebar').removeClass('sidebar-collapsed sidebar-expanded');
                $('body').removeClass('sidebar-collapsed sidebar-mobile-open');
                log('Switched to mobile view');
            } else {
                // Switched to desktop
                applyState(isCollapsed);
                $('body').removeClass('sidebar-mobile-open');
                log('Switched to desktop view');
            }
        }
    };

    // Toggle mobile sidebar
    const toggleMobileSidebar = () => {
        if (!isMobile) return;

        const $sb = $('#admin-sidebar');
        const $body = $('body');

        if ($sb.hasClass('sidebar-expanded')) {
            $sb.removeClass('sidebar-expanded');
            $body.removeClass('sidebar-mobile-open');
        } else {
            $sb.addClass('sidebar-expanded');
            $body.addClass('sidebar-mobile-open');
        }
    };

    /* ====================================
       INITIALIZATION
       ==================================== */

    // Initialize the sidebar
    const init = () => {
        try {
            log('Initializing sidebar...');

            // Check for required elements
            if (!$('#admin-sidebar').length) {
                console.error('[Sidebar] Required element #admin-sidebar not found!');
                return;
            }

            // Load initial state (desktop only)
            if (!isMobile) {
                isCollapsed = loadState();
                applyState(isCollapsed);
            }

            // Highlight active links
            highlightActive();

            // Attach event handlers
            attachEventHandlers();

            // Window resize handler
            $(window).on('resize', debounce(handleResize, 250));

            log('Sidebar initialized successfully');
        } catch (e) {
            handleError('init', e);
        }
    };

    /* ====================================
       EVENT HANDLERS
       ==================================== */

    const attachEventHandlers = () => {
        // Sidebar toggle button
        $('#sidebar-toggle-btn').on(CLICK_EVENT, function (e) {
            e.preventDefault();
            e.stopPropagation();
            log('Toggle button clicked');

            if (isMobile) {
                toggleMobileSidebar();
            } else {
                toggleSidebar();
            }
        });

        // Submenu toggles - Attendance
        $('#v-pills-user-tab').on(CLICK_EVENT, function (e) {
            e.preventDefault();
            e.stopPropagation();
            log('Attendance menu clicked');
            toggleSubMenu($(this));
        });

        // Submenu toggles - Master Data
        $('#v-pills-master-data-tab').on(CLICK_EVENT, function (e) {
            e.preventDefault();
            e.stopPropagation();
            log('Master Data menu clicked');
            toggleSubMenu($(this));
        });

        // Handle clicks on regular nav items
        $('.sidebar-nav-link').on(CLICK_EVENT, function (e) {
            const $this = $(this);
            const href = $this.attr('href');

            // Only set active state if it's a real link (not submenu trigger)
            if (href && href !== '#' && !href.includes('javascript:')) {
                setActiveNavItem($this);

                // Close mobile sidebar after navigation
                if (isMobile) {
                    setTimeout(() => {
                        $('#admin-sidebar').removeClass('sidebar-expanded');
                        $('body').removeClass('sidebar-mobile-open');
                    }, 100);
                }
            }
        });

        // Home button
        $('#main-menu').on(CLICK_EVENT, function (e) {
            e.preventDefault();
            log('Home button clicked');
            window.location.href = '/Home/Index';
        });

        // Logout button
        $('#log-out').on(CLICK_EVENT, function (e) {
            e.preventDefault();
            log('Logout button clicked');

            // Show loading state
            $('#admin-sidebar').addClass('loading');

            try {
                // Clear cookies if CookieHelper exists
                if (typeof CookieHelper !== 'undefined') {
                    const c = new CookieHelper();
                    c.deleteAllContains('jsonWebToken');
                    c.delete('refreshToken');
                }
            } catch (error) {
                handleError('logout cookie cleanup', error);
            }

            // Redirect to login
            window.location.href = '/Login/Index';
        });

        // Close mobile sidebar when clicking overlay
        if (isMobile) {
            $('body').on(CLICK_EVENT, function (e) {
                if ($('body').hasClass('sidebar-mobile-open') &&
                    !$(e.target).closest('#admin-sidebar').length &&
                    !$(e.target).closest('#sidebar-toggle-btn').length) {
                    $('#admin-sidebar').removeClass('sidebar-expanded');
                    $('body').removeClass('sidebar-mobile-open');
                }
            });
        }

        log('Event handlers attached');
    };

    /* ====================================
       UTILITY FUNCTIONS
       ==================================== */

    // Debounce function
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    /* ====================================
       DOCUMENT READY
       ==================================== */

    $(document).ready(function () {
        log('Document ready, initializing sidebar system...');
        init();
    });

    // Public API (optional)
    window.SidebarAPI = {
        toggle: toggleSidebar,
        collapse: () => { isCollapsed = true; applyState(true); saveState(true); },
        expand: () => { isCollapsed = false; applyState(false); saveState(false); },
        isCollapsed: () => isCollapsed,
        refresh: highlightActive
    };

})(jQuery);