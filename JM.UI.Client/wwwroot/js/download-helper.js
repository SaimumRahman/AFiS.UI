// Add this to your wwwroot/js/app.js (or any JS file registered in index.html / _Host.cshtml)

/**
 * Downloads a file in the browser from a base64 or byte array passed via Blazor JS Interop.
 * @param {string} fileName - The desired file name (e.g. "report.pdf")
 * @param {string} mimeType - The MIME type (e.g. "application/pdf")
 * @param {Uint8Array|number[]} byteArray - The file content as bytes
 */
window.downloadFileFromBytes = function (fileName, mimeType, byteArray) {
    const blob = new Blob([new Uint8Array(byteArray)], { type: mimeType });
    const url = URL.createObjectURL(blob);

    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    // Release the object URL after a short delay
    setTimeout(() => URL.revokeObjectURL(url), 1000);
};
