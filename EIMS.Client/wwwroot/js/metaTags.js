/**
 * Sets the Content Security Policy dynamically at runtime.
 * This can help overcome browser caching issues with CSP headers.
 */
export function setContentSecurityPolicy(cspValue) {
    // Check if a CSP meta tag already exists
    let cspMeta = document.querySelector('meta[http-equiv="Content-Security-Policy"]');
    
    if (cspMeta) {
        // If it exists, update its content
        console.log("Updating existing CSP meta tag");
        console.log("Previous CSP:", cspMeta.content);
        cspMeta.content = cspValue;
        console.log("New CSP:", cspMeta.content);
    } else {
        // If it doesn't exist, create a new one
        console.log("Creating new CSP meta tag");
        cspMeta = document.createElement('meta');
        cspMeta.httpEquiv = 'Content-Security-Policy';
        cspMeta.content = cspValue;
        document.head.appendChild(cspMeta);
        console.log("CSP meta tag added:", cspMeta.content);
    }
    return true;
}

/**
 * Logs all meta tags to the console for debugging
 */
export function logMetaTags() {
    const metaTags = document.querySelectorAll('meta');
    console.log("All meta tags:");
    metaTags.forEach((tag, index) => {
        console.log(`[${index}] ${tag.outerHTML}`);
    });
    return true;
}

/**
 * Helper function to verify if a specific URL is allowed by the CSP
 */
export function checkCspAllowsUrl(url) {
    const cspMeta = document.querySelector('meta[http-equiv="Content-Security-Policy"]');
    if (!cspMeta) {
        console.log("No CSP meta tag found");
        return false;
    }
    
    console.log(`Checking if CSP allows URL: ${url}`);
    
    // Simple check if the URL or domain is mentioned in the CSP
    // This is not a complete CSP validation
    const cspContent = cspMeta.content;
    const domain = new URL(url).hostname;
    
    const result = cspContent.includes(url) || cspContent.includes(domain);
    console.log(`CSP ${result ? 'allows' : 'does not allow'} ${url}`);
    return result;
} 