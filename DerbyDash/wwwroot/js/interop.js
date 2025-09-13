window.toggleClass = (element, className) => {
    if (element) {
        element.classList.toggle(className);
    }
};

window.removeClass = (element, className) => {
    if (element) {
        element.classList.remove(className);
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

// Get browser information for feedback form
window.getBrowserInfo = () => {
    const userAgent = navigator.userAgent;
    const browserInfo = {
        browser: '',
        version: '',
        os: '',
        device: 'Desktop'
    };
    
    // Detect browser and version
    if (userAgent.indexOf("Firefox") > -1) {
        browserInfo.browser = "Firefox";
        browserInfo.version = userAgent.match(/Firefox\/([0-9.]+)/)[1];
    } else if (userAgent.indexOf("Edge") > -1 || userAgent.indexOf("Edg/") > -1) {
        browserInfo.browser = "Edge";
        const edgeMatch = userAgent.match(/Edge\/([0-9.]+)/) || userAgent.match(/Edg\/([0-9.]+)/);
        browserInfo.version = edgeMatch ? edgeMatch[1] : "";
    } else if (userAgent.indexOf("Chrome") > -1) {
        browserInfo.browser = "Chrome";
        browserInfo.version = userAgent.match(/Chrome\/([0-9.]+)/)[1];
    } else if (userAgent.indexOf("Safari") > -1 && userAgent.indexOf("Chrome") === -1) {
        browserInfo.browser = "Safari";
        browserInfo.version = userAgent.match(/Version\/([0-9.]+)/)[1];
    } else if (userAgent.indexOf("MSIE") > -1 || userAgent.indexOf("Trident/") > -1) {
        browserInfo.browser = "Internet Explorer";
        const ieMatch = userAgent.match(/MSIE ([0-9.]+)/) || userAgent.match(/rv:([0-9.]+)/);
        browserInfo.version = ieMatch ? ieMatch[1] : "";
    } else {
        browserInfo.browser = "Unknown";
    }
    
    // Detect OS
    if (userAgent.indexOf("Windows") > -1) {
        browserInfo.os = "Windows";
    } else if (userAgent.indexOf("Mac") > -1) {
        browserInfo.os = "MacOS";
    } else if (userAgent.indexOf("Linux") > -1) {
        browserInfo.os = "Linux";
    } else if (userAgent.indexOf("Android") > -1) {
        browserInfo.os = "Android";
        browserInfo.device = "Mobile";
    } else if (userAgent.indexOf("iPhone") > -1 || userAgent.indexOf("iPad") > -1) {
        browserInfo.os = "iOS";
        browserInfo.device = userAgent.indexOf("iPad") > -1 ? "Tablet" : "Mobile";
    } else {
        browserInfo.os = "Unknown";
    }
    
    return `${browserInfo.browser} ${browserInfo.version} on ${browserInfo.os} (${browserInfo.device})`;
};

// Toggle password visibility
window.togglePasswordVisibility = (inputId) => {
    const input = document.getElementById(inputId);
    const button = document.querySelector(`button[onclick="togglePasswordVisibility('${inputId}')"]`);
    
    if (input && button) {
        const icon = button.querySelector('i');
        const srText = button.querySelector('.sr-only');
        
        if (input.type === 'password') {
            input.type = 'text';
            icon.className = 'fas fa-eye-slash';
            srText.textContent = 'Hide password';
        } else {
            input.type = 'password';
            icon.className = 'fas fa-eye';
            srText.textContent = 'Show password';
        }
    }
};

// Register escape key handler for closing modals
window.registerEscapeKey = (dotNetObjectReference) => {
    const handler = function(event) {
        if (event.key === 'Escape') {
            dotNetObjectReference.invokeMethodAsync('HandleEscapeKey');
        }
    };
    
    document.addEventListener('keydown', handler);
    
    return {
        dispose: function() {
            document.removeEventListener('keydown', handler);
        }
    };
};
