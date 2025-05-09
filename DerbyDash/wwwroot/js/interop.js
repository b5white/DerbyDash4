window.toggleClass = (element, className) => {
    if (element) {
        element.classList.toggle(className);
    }
};

// Initialize Bootstrap accordion
window.initializeAccordion = () => {
    // Check if Bootstrap is available
    if (typeof bootstrap !== 'undefined') {
        // Get all accordion items
        const accordionItems = document.querySelectorAll('.accordion-item');
        
        // Initialize each accordion item
        accordionItems.forEach(item => {
            const button = item.querySelector('.accordion-button');
            const collapseId = button?.getAttribute('data-bs-target')?.substring(1);
            
            if (collapseId) {
                const collapseElement = document.getElementById(collapseId);
                if (collapseElement) {
                    // Create a new collapse instance
                    new bootstrap.Collapse(collapseElement, {
                        toggle: false
                    });
                    
                    // Add click event listener to toggle the collapse
                    button.addEventListener('click', function() {
                        const isCollapsed = button.classList.contains('collapsed');
                        if (isCollapsed) {
                            button.classList.remove('collapsed');
                            button.setAttribute('aria-expanded', 'true');
                            collapseElement.classList.add('show');
                        } else {
                            button.classList.add('collapsed');
                            button.setAttribute('aria-expanded', 'false');
                            collapseElement.classList.remove('show');
                        }
                    });
                }
            }
        });
    } else {
        console.warn('Bootstrap is not available. Accordion functionality may be limited.');
    }
};

// Set a cookie with a specified expiration time
window.setCookie = (name, value, days) => {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + encodeURIComponent(value) + expires + "; path=/";
};

// Get a cookie by name
window.getCookie = (name) => {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return decodeURIComponent(c.substring(nameEQ.length, c.length));
    }
    return null;
};
