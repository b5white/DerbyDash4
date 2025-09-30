// Offline detection functionality
window.setupOfflineDetection = (dotNetRef) => {
    // Store reference for cleanup
    window.offlineDetectionRef = dotNetRef;
    
    // Event handlers
    const onlineHandler = () => {
        console.log('Network status: Online');
        dotNetRef.invokeMethodAsync('OnOfflineStatusChange', false);
    };
    
    const offlineHandler = () => {
        console.log('Network status: Offline');
        dotNetRef.invokeMethodAsync('OnOfflineStatusChange', true);
    };
    
    // Store handlers for cleanup
    window.onlineHandler = onlineHandler;
    window.offlineHandler = offlineHandler;
    
    // Add event listeners
    window.addEventListener('online', onlineHandler);
    window.addEventListener('offline', offlineHandler);
    
    console.log('Offline detection initialized');
};

window.cleanupOfflineDetection = () => {
    if (window.onlineHandler) {
        window.removeEventListener('online', window.onlineHandler);
        delete window.onlineHandler;
    }
    
    if (window.offlineHandler) {
        window.removeEventListener('offline', window.offlineHandler);
        delete window.offlineHandler;
    }
    
    if (window.offlineDetectionRef) {
        delete window.offlineDetectionRef;
    }
    
    console.log('Offline detection cleaned up');
};

// Cookie management functions (already exist but ensuring they're available)
window.setCookie = (name, value, days, secure, sameSite, path) => {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    
    const secureFlag = secure ? "; Secure" : "";
    const sameSiteFlag = sameSite ? `; SameSite=${sameSite}` : "";
    const pathFlag = path ? `; Path=${path}` : "";
    
    document.cookie = name + "=" + (value || "") + expires + pathFlag + secureFlag + sameSiteFlag;
};

window.getCookie = (name) => {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
};

// Service Worker registration for PWA
window.registerServiceWorker = () => {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/sw.js')
            .then((registration) => {
                console.log('Service Worker registered successfully:', registration);
            })
            .catch((error) => {
                console.log('Service Worker registration failed:', error);
            });
    }
};

// Initialize PWA on load
document.addEventListener('DOMContentLoaded', () => {
    window.registerServiceWorker();
});
