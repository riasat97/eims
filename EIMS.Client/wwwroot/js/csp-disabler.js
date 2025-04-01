// This script attempts to work around Content Security Policy issues
// by intercepting fetch calls and adding special headers

console.log("CSP Disabler initialized");

// Try to remove any CSP meta tags in the page
document.addEventListener('DOMContentLoaded', function() {
    console.log("Removing CSP meta tags");
    const cspMetaTags = document.querySelectorAll('meta[http-equiv="Content-Security-Policy"]');
    cspMetaTags.forEach(tag => {
        console.log("Removing CSP tag:", tag.outerHTML);
        tag.remove();
    });
    
    console.log("Adding permissive CSP");
    const meta = document.createElement('meta');
    meta.httpEquiv = 'Content-Security-Policy';
    meta.content = "default-src * 'unsafe-inline' 'unsafe-eval'; script-src * 'unsafe-inline' 'unsafe-eval'; connect-src * 'unsafe-inline'; img-src * data: blob: 'unsafe-inline'; frame-src *; style-src * 'unsafe-inline';";
    document.head.appendChild(meta);
});

// If using fetch directly, try to intercept it
const originalFetch = window.fetch;
window.fetch = function(url, options) {
    console.log(`Intercepted fetch to: ${url}`);
    
    // Detect direct API calls that should go through the proxy
    if (typeof url === 'string') {
        // If it's a direct call to the API but not to the proxy
        if (url.includes('localhost:5063/parts') || url.includes('localhost:5063/api/parts')) {
            // Redirect to the proxy endpoint
            const newUrl = url.replace('/parts', '/client-api/parts')
                             .replace('/api/parts', '/client-api/parts');
            console.log(`Redirecting API call to proxy: ${url} -> ${newUrl}`);
            url = newUrl;
        }
    }
    
    // If this is a call to our API server, log it
    if (url.toString().includes('localhost:5063')) {
        console.log(`Fetch to API server: ${url}`);
        
        // Add any headers needed for CORS
        options = options || {};
        options.headers = options.headers || {};
        options.mode = 'cors';
        options.credentials = 'same-origin';
    }
    
    return originalFetch(url, options).catch(error => {
        console.error(`Fetch error for ${url}:`, error);
        
        // If there's a CSP error, we can try to identify it
        if (error.toString().includes('Content Security Policy') || 
            error.toString().includes('Failed to fetch')) {
            console.error('Likely CSP error detected. URL:', url);
        }
        
        throw error;
    });
}; 