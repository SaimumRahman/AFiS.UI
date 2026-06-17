// wwwroot/js/barcode-print.js
// JS interop helpers for barcode printer communication from Blazor WASM/Server

window.barcodePrint = {

    // ── Web Serial API (USB/COM) ──────────────────────────────────────────────
    // Requires: HTTPS + Chrome/Edge 89+, user must grant permission
    // Blazor call: await JS.InvokeVoidAsync("barcodePrint.sendSerial", port, baud, commands)

    sendSerial: async function (portName, baudRate, commands) {
        if (!('serial' in navigator)) {
            throw new Error(
                'Web Serial API not supported in this browser. ' +
                'Use Chrome or Edge on desktop, or switch to TCP/API send method.'
            );
        }

        // Request port (shows native port picker dialog)
        const port = await navigator.serial.requestPort();

        await port.open({ baudRate: parseInt(baudRate, 10) });

        const encoder = new TextEncoder();
        const writer = port.writable.getWriter();

        try {
            await writer.write(encoder.encode(commands));
        } finally {
            writer.releaseLock();
            await port.close();
        }
    },

    // ── TCP via fetch proxy ───────────────────────────────────────────────────
    // Use this if you have a local print-server proxy (e.g. LabelaryProxy / node)
    // Blazor call: await JS.InvokeVoidAsync("barcodePrint.sendTcpViaProxy", url, commands)

    sendTcpViaProxy: async function (proxyUrl, commands) {
        const resp = await fetch(proxyUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'text/plain' },
            body: commands
        });
        if (!resp.ok) {
            throw new Error(`Print proxy error: ${resp.status} ${resp.statusText}`);
        }
    },

    // ── Download as file ─────────────────────────────────────────────────────
    // Lets the user save the command file and print later / drag to print server

    downloadCommandFile: function (commands, filename) {
        const blob = new Blob([commands], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename || 'barcode-print.zpl';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },

    // ── Browser CSS print ─────────────────────────────────────────────────────
    // Used by Browser protocol — triggers the OS print dialog

    triggerPrint: function () {
        window.print();
    },

    // ── Labelary preview (ZPL → PNG via Labelary.com API) ─────────────────────
    // Renders ZPL as a PNG image for preview — great for testing without a printer
    // Returns a data-URL or throws on error.

    zplToPng: async function (zplCommands, dpi, widthInch, heightInch) {
        const url = `https://api.labelary.com/v1/printers/${dpi}dpmm/labels/${widthInch}x${heightInch}/0/`;
        const resp = await fetch(url, {
            method: 'POST',
            headers: { 'Accept': 'image/png', 'Content-Type': 'application/x-www-form-urlencoded' },
            body: zplCommands
        });
        if (!resp.ok) throw new Error('Labelary render failed: ' + resp.status);
        const blob = await resp.blob();
        return URL.createObjectURL(blob);
    }
};