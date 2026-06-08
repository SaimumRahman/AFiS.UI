// PrinterConfigDialog.razor.cs
// Full printer command generation for ZPL, TSPL, EPL2, CPCL, Browser, Raw
using JM.UI.Entities.Model.Barcodes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Sockets;
using System.Text;

namespace JM.UI.Client.Pages.Barcode;

// ── Enums ─────────────────────────────────────────────────────────────────────

public enum PrintProtocol { ZPL, TSPL, EPL, CPCL, Browser, RawText }

// ── Config DTO ────────────────────────────────────────────────────────────────

public class PrinterConfigModel
{
    public PrintProtocol Protocol { get; set; } = PrintProtocol.ZPL;

    // Connection
    public string SendMethod { get; set; } = "TCP";
    public string PrinterIp { get; set; } = "192.168.1.100";
    public int PrinterPort { get; set; } = 9100;
    public string PortName { get; set; } = "COM3";
    public string BaudRate { get; set; } = "9600";
    public string FilePath { get; set; } = @"C:\print\output.zpl";
    public string ApiEndpoint { get; set; } = "http://localhost:9100/print";

    // Label dimensions
    public decimal LabelWidthMm { get; set; } = 55;
    public decimal LabelHeightMm { get; set; } = 33;

    // Printer settings
    public int Dpi { get; set; } = 203;
    public string PrintSpeed { get; set; } = "4";
    public int Darkness { get; set; } = 15;

    // Barcode settings
    public string BarcodeSymbology { get; set; } = "QR";
    public string QrErrorCorrection { get; set; } = "M";
    public string Encoding { get; set; } = "UTF-8";

    // Helpers
    public double MmToDots(decimal mm) => (double)(mm * Dpi / 25.4m);
    public int WidthDots => (int)MmToDots(LabelWidthMm);
    public int HeightDots => (int)MmToDots(LabelHeightMm);
}

// ── Component ─────────────────────────────────────────────────────────────────

public partial class PrinterConfigDialog : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public List<BarcodePrintItemDTO> Items { get; set; } = new();
    [Parameter] public List<string> TemplateFields { get; set; } = new();
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> OnClose { get; set; }   // bool = print success
    [Parameter] public EventCallback<string> OnNotify { get; set; }

    protected PrinterConfigModel Config { get; set; } = new();
    protected bool IsSending { get; set; }

    protected int TotalLabels => Items.Sum(x => x.PrintQty);

    // ── Protocol help text ────────────────────────────────────────────────────

    protected string ProtocolHelpText => Config.Protocol switch
    {
        PrintProtocol.ZPL =>
            "ZPL (Zebra Programming Language) — supported by all Zebra printers (GK, GX, ZD, ZT series). " +
            "Default port 9100 over TCP. Industry standard for thermal label printing.",

        PrintProtocol.TSPL =>
            "TSPL (TSC Printer Language) — used by TSC, Bixolon, and many budget thermal printers. " +
            "Sends text commands via TCP/USB. Commonly used in South/Southeast Asian markets.",

        PrintProtocol.EPL =>
            "EPL2 (Eltron Programming Language 2) — older Zebra protocol (LP/TLP series). " +
            "Use ZPL for newer Zebra printers. Port 9100 over TCP.",

        PrintProtocol.CPCL =>
            "CPCL (Comtec Printer Control Language) — used by Honeywell/Intermec mobile printers. " +
            "Line-oriented command language. Also supported by some Zebra mobile printers (QLn, ZQ series).",

        PrintProtocol.Browser =>
            "Browser Print — opens the OS print dialog. Labels are rendered as HTML and printed via CSS. " +
            "No printer driver required. Works with Windows, macOS, Linux, and mobile. " +
            "Best for office printers or when a dedicated label printer is not available.",

        PrintProtocol.RawText =>
            "Raw Text — sends plain barcode values line-by-line to any configured endpoint. " +
            "Use this for custom middleware or when your print server accepts raw data.",

        _ => string.Empty
    };

    // ── Switch protocol ───────────────────────────────────────────────────────

    protected void SetProtocol(PrintProtocol p)
    {
        Config.Protocol = p;
        StateHasChanged();
    }

    // ── Generated command preview ─────────────────────────────────────────────

    protected string GeneratedCommandPreview
    {
        get
        {
            var first = Items.FirstOrDefault();
            if (first == null) return "— no items —";

            return Config.Protocol switch
            {
                PrintProtocol.ZPL => GenerateZplForItem(first, 1, previewOnly: true),
                PrintProtocol.TSPL => GenerateTsplForItem(first, 1, previewOnly: true),
                PrintProtocol.EPL => GenerateEplForItem(first, 1, previewOnly: true),
                PrintProtocol.CPCL => GenerateCpclForItem(first, 1, previewOnly: true),
                PrintProtocol.RawText => GenerateRawForItem(first),
                PrintProtocol.Browser => "(Browser print — no raw command. Click 'Send to Printer' to open OS dialog.)",
                _ => string.Empty
            };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ZPL GENERATOR
    // Spec: https://www.zebra.com/content/dam/zebra/manuals/printers/common/programming/zpl-zbi2-pm-en.pdf
    // ─────────────────────────────────────────────────────────────────────────

    private string GenerateZplForItem(BarcodePrintItemDTO item, int copyNum, bool previewOnly = false)
    {
        var sb = new StringBuilder();
        int w = Config.WidthDots;
        int h = Config.HeightDots;
        int dpi = Config.Dpi;

        // Helper: mm → dots
        int Dots(double mm) => (int)(mm * dpi / 25.4);

        int margin = Dots(2);
        int topY = Dots(2);

        sb.AppendLine("^XA");                                          // Start label
        sb.AppendLine($"^PW{w}");                                      // Print width
        sb.AppendLine($"^LL{h}");                                      // Label length
        sb.AppendLine($"^LH0,0");                                      // Label home
        sb.AppendLine($"^PR{Config.PrintSpeed}");                      // Print speed
        sb.AppendLine($"^MD{Config.Darkness}");                        // Darkness

        // ── Top colour stripe (graphic box) ──────────────────────
        string stripeColor = "B";                                      // B = black
        sb.AppendLine($"^FO0,0^GB{w},{Dots(1.5)},3,{stripeColor},0^FS");

        // ── Product name ─────────────────────────────────────────
        if (ShowField("ProductName"))
        {
            sb.AppendLine($"^FO{margin},{topY + Dots(1.5)}");
            sb.AppendLine($"^A0N,{Dots(3.5)},{Dots(3)}");             // Font 0, height, width
            sb.AppendLine($"^FD{Truncate(item.ProductName, 24)}^FS");
            topY += Dots(5);
        }

        // ── Brand ────────────────────────────────────────────────
        if (ShowField("Brand") && !string.IsNullOrWhiteSpace(item.Brand))
        {
            sb.AppendLine($"^FO{margin},{topY + Dots(1.5)}");
            sb.AppendLine($"^A0N,{Dots(2.5)},{Dots(2.5)}");
            sb.AppendLine($"^FD{Truncate(item.Brand, 28)}^FS");
            topY += Dots(4);
        }

        // ── QR code ──────────────────────────────────────────────
        int qrSize = Dots(16);
        int qrX = margin;
        int qrY = topY + Dots(1);

        if (Config.BarcodeSymbology == "QR")
        {
            sb.AppendLine($"^FO{qrX},{qrY}");
            sb.AppendLine($"^BQN,2,{Math.Max(2, (int)(qrSize / 21.0))}");   // QR, model 2, module size
            sb.AppendLine($"^FD{Config.QrErrorCorrection}A,{item.BarcodeValue}^FS");
        }
        else
        {
            // Linear barcode (Code128 example)
            sb.AppendLine($"^FO{qrX},{qrY}");
            sb.AppendLine($"^BY{Math.Max(1, Dots(0.6))},3,{Dots(8)}"); // bar width, ratio, height
            sb.AppendLine($"^B{ZplLinearSymbol()},N,Y,N,N");
            sb.AppendLine($"^FD{item.BarcodeValue}^FS");
        }

        // ── Right-side details ────────────────────────────────────
        int rightX = qrX + qrSize + Dots(2);
        int rightY = qrY;

        // Barcode text value
        sb.AppendLine($"^FO{rightX},{rightY}");
        sb.AppendLine($"^A0N,{Dots(2)},{Dots(1.8)}");
        sb.AppendLine($"^FD{item.BarcodeValue}^FS");
        rightY += Dots(3.5);

        // Price
        if (ShowField("Price") && item.Price.HasValue)
        {
            sb.AppendLine($"^FO{rightX},{rightY}");
            sb.AppendLine($"^A0N,{Dots(3)},{Dots(2.8)}");
            sb.AppendLine($"^FD৳ {item.Price:N2}^FS");
            rightY += Dots(4.5);
        }

        // UoM
        if (ShowField("UOM") || ShowField("UoM"))
        {
            string uom = item.UoM ?? "—";
            sb.AppendLine($"^FO{rightX},{rightY}");
            sb.AppendLine($"^A0N,{Dots(2)},{Dots(1.8)}");
            sb.AppendLine($"^FD{uom}^FS");
        }

        // ── Bottom footer line ────────────────────────────────────
        int footerY = h - Dots(4.5);
        sb.AppendLine($"^FO0,{footerY}^GB{w},1,1,B,0^FS");
        sb.AppendLine($"^FO{margin},{footerY + Dots(0.8)}");
        sb.AppendLine($"^A0N,{Dots(2)},{Dots(1.8)}");
        sb.AppendLine($"^FD{item.GroupName}^FS");

        // Copy indicator
        sb.AppendLine($"^FO{w - Dots(12)},{footerY + Dots(0.8)}");
        sb.AppendLine($"^A0N,{Dots(2)},{Dots(1.8)}");
        sb.AppendLine($"^FD{copyNum}/{item.PrintQty}^FS");

        sb.AppendLine("^XZ");                                          // End label

        return sb.ToString();
    }

    private string ZplLinearSymbol() => Config.BarcodeSymbology switch
    {
        "CODE128" => "C",
        "CODE39" => "3",
        "EAN13" => "E",
        "EAN8" => "8",
        "UPCA" => "U",
        _ => "C"   // default Code128
    };

    // ─────────────────────────────────────────────────────────────────────────
    // TSPL GENERATOR (TSC printers)
    // Spec: https://www.tscprinters.com/en/support/technical-documents
    // ─────────────────────────────────────────────────────────────────────────

    private string GenerateTsplForItem(BarcodePrintItemDTO item, int copyNum, bool previewOnly = false)
    {
        var sb = new StringBuilder();
        int wDots = Config.WidthDots;
        int hDots = Config.HeightDots;
        int dpi = Config.Dpi;
        int Dots(double mm) => (int)(mm * dpi / 25.4);

        sb.AppendLine($"SIZE {Config.LabelWidthMm} mm, {Config.LabelHeightMm} mm");
        sb.AppendLine("GAP 3 mm, 0 mm");
        sb.AppendLine($"SPEED {Config.PrintSpeed}");
        sb.AppendLine($"DENSITY {Config.Darkness}");
        sb.AppendLine($"DIRECTION 0,0");
        sb.AppendLine("CLS");

        // Product name
        if (ShowField("ProductName"))
        {
            sb.AppendLine($"TEXT {Dots(2)},{Dots(2)},\"3\",0,1,1,\"{Truncate(item.ProductName, 24)}\"");
        }

        // QR code or barcode
        if (Config.BarcodeSymbology == "QR")
        {
            int qrX = Dots(2);
            int qrY = ShowField("ProductName") ? Dots(8) : Dots(2);
            int qrCell = Math.Max(3, (int)(Dots(16) / 21.0));
            sb.AppendLine($"QRCODE {qrX},{qrY},{Config.QrErrorCorrection},4,A,0,M2,S7,\"{item.BarcodeValue}\"");
        }
        else
        {
            int bcX = Dots(2);
            int bcY = ShowField("ProductName") ? Dots(8) : Dots(2);
            string sym = Config.BarcodeSymbology switch
            {
                "CODE39" => "39",
                "EAN13" => "EAN13",
                "EAN8" => "EAN8",
                "UPCA" => "UPC-A",
                _ => "128"
            };
            sb.AppendLine($"BARCODE {bcX},{bcY},\"{sym}\",{Dots(8)},1,0,2,2,\"{item.BarcodeValue}\"");
        }

        // Price
        if (ShowField("Price") && item.Price.HasValue)
        {
            sb.AppendLine($"TEXT {Dots(22)},{Dots(12)},\"2\",0,1,1,\"BDT {item.Price:N2}\"");
        }

        // Brand
        if (ShowField("Brand") && !string.IsNullOrWhiteSpace(item.Brand))
        {
            sb.AppendLine($"TEXT {Dots(22)},{Dots(18)},\"1\",0,1,1,\"{item.Brand}\"");
        }

        sb.AppendLine("PRINT 1,1");
        sb.AppendLine("END");

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EPL2 GENERATOR
    // ─────────────────────────────────────────────────────────────────────────

    private string GenerateEplForItem(BarcodePrintItemDTO item, int copyNum, bool previewOnly = false)
    {
        var sb = new StringBuilder();
        int dpi = Config.Dpi;
        int Dots(double mm) => (int)(mm * dpi / 25.4);

        sb.AppendLine("\nN");                               // New label
        sb.AppendLine($"q{Config.WidthDots}");              // Set width
        sb.AppendLine($"Q{Config.HeightDots},0");           // Set height + gap

        // Product name
        if (ShowField("ProductName"))
        {
            sb.AppendLine($"A{Dots(2)},{Dots(2)},0,3,1,1,N,\"{Truncate(item.ProductName, 24)}\"");
        }

        // Barcode
        if (Config.BarcodeSymbology == "QR")
        {
            // EPL2 QR via b command
            sb.AppendLine($"b{Dots(2)},{Dots(8)},Q,s3,e{Config.QrErrorCorrection},r0,f0,w0,\"{item.BarcodeValue}\"");
        }
        else
        {
            string sym = Config.BarcodeSymbology switch
            {
                "CODE39" => "3",
                "EAN13" => "E30",
                "EAN8" => "E80",
                "UPCA" => "UA0",
                _ => "1"  // Code128
            };
            sb.AppendLine($"B{Dots(2)},{Dots(8)},0,{sym},2,5,{Dots(12)},B,\"{item.BarcodeValue}\"");
        }

        // Price
        if (ShowField("Price") && item.Price.HasValue)
        {
            sb.AppendLine($"A{Dots(22)},{Dots(10)},0,2,1,1,N,\"BDT {item.Price:N2}\"");
        }

        sb.AppendLine("P1");                                // Print 1 copy

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CPCL GENERATOR (Honeywell / Intermec)
    // ─────────────────────────────────────────────────────────────────────────

    private string GenerateCpclForItem(BarcodePrintItemDTO item, int copyNum, bool previewOnly = false)
    {
        var sb = new StringBuilder();
        int dpi = Config.Dpi;
        int Dots(double mm) => (int)(mm * dpi / 25.4);

        sb.AppendLine($"! 0 200 200 {Config.HeightDots} 1");   // offset, horiz-dpi, vert-dpi, height, qty

        if (ShowField("ProductName"))
        {
            sb.AppendLine($"TEXT 4 0 {Dots(2)} {Dots(2)} {Truncate(item.ProductName, 24)}");
        }

        if (Config.BarcodeSymbology == "QR")
        {
            sb.AppendLine($"EQR {Dots(2)} {Dots(8)} M 4 {item.BarcodeValue}");
        }
        else
        {
            string sym = Config.BarcodeSymbology switch
            {
                "CODE39" => "39",
                "EAN13" => "EAN13",
                _ => "128"
            };
            sb.AppendLine($"BARCODE {sym} 1 1 50 {Dots(2)} {Dots(8)} {item.BarcodeValue}");
        }

        if (ShowField("Price") && item.Price.HasValue)
        {
            sb.AppendLine($"TEXT 4 0 {Dots(22)} {Dots(10)} BDT {item.Price:N2}");
        }

        sb.AppendLine("FORM");
        sb.AppendLine("PRINT");

        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RAW TEXT GENERATOR
    // ─────────────────────────────────────────────────────────────────────────

    private string GenerateRawForItem(BarcodePrintItemDTO item)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"BARCODE:{item.BarcodeValue}");
        if (ShowField("ProductName")) sb.AppendLine($"PRODUCT:{item.ProductName}");
        if (ShowField("Brand")) sb.AppendLine($"BRAND:{item.Brand ?? ""}");
        if (ShowField("Price")) sb.AppendLine($"PRICE:{item.Price?.ToString("N2") ?? ""}");
        if (ShowField("UOM")) sb.AppendLine($"UOM:{item.UoM ?? ""}");
        sb.AppendLine($"GROUP:{item.GroupName}");
        sb.AppendLine("---");
        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FULL BATCH GENERATION
    // ─────────────────────────────────────────────────────────────────────────

    private string GenerateAllCommands()
    {
        var sb = new StringBuilder();
        foreach (var item in Items)
        {
            for (int copy = 1; copy <= item.PrintQty; copy++)
            {
                string cmd = Config.Protocol switch
                {
                    PrintProtocol.ZPL => GenerateZplForItem(item, copy),
                    PrintProtocol.TSPL => GenerateTsplForItem(item, copy),
                    PrintProtocol.EPL => GenerateEplForItem(item, copy),
                    PrintProtocol.CPCL => GenerateCpclForItem(item, copy),
                    PrintProtocol.RawText => GenerateRawForItem(item),
                    _ => string.Empty
                };
                sb.Append(cmd);
            }
        }
        return sb.ToString();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ACTIONS
    // ─────────────────────────────────────────────────────────────────────────

    protected async Task ExecutePrint()
    {
        if (!Items.Any()) return;

        IsSending = true;
        StateHasChanged();

        try
        {
            if (Config.Protocol == PrintProtocol.Browser)
            {
                // Trigger browser print dialog (CSS @media print handles rendering)
                await JS.InvokeVoidAsync("window.print");
                await OnClose.InvokeAsync(true);
                return;
            }

            string allCommands = GenerateAllCommands();

            switch (Config.SendMethod)
            {
                case "TCP":
                    await SendViaTcpAsync(allCommands);
                    break;

                case "USB":
                    // NOTE: Direct USB/serial requires a native bridge (desktop app, Electron, or
                    // Web Serial API). For Blazor Server, use a background service + SignalR.
                    // For Blazor WASM, use the Web Serial API via JS interop (see barcode-print.js).
                    await JS.InvokeVoidAsync("barcodePrint.sendSerial",
                        Config.PortName, Config.BaudRate, allCommands);
                    break;

                case "File":
                    // Server-side: write file for print server to pick up
                    await File.WriteAllTextAsync(Config.FilePath, allCommands,
                        Encoding.GetEncoding(Config.Encoding));
                    break;

                case "API":
                    using (var http = new System.Net.Http.HttpClient())
                    {
                        var content = new System.Net.Http.StringContent(
                            allCommands,
                            Encoding.GetEncoding(Config.Encoding),
                            "text/plain");
                        var resp = await http.PostAsync(Config.ApiEndpoint, content);
                        resp.EnsureSuccessStatusCode();
                    }
                    break;
            }

            await OnNotify.InvokeAsync($"✅ {TotalLabels} label(s) sent via {Config.Protocol}/{Config.SendMethod}.");
            await OnClose.InvokeAsync(true);
        }
        catch (Exception ex)
        {
            await OnNotify.InvokeAsync($"❌ Print failed: {ex.Message}");
        }
        finally
        {
            IsSending = false;
            StateHasChanged();
        }
    }

    private async Task SendViaTcpAsync(string commands)
    {
        // Raw TCP socket send — works for Zebra/TSC/Honeywell over network
        using var client = new TcpClient();
        await client.ConnectAsync(Config.PrinterIp, Config.PrinterPort);
        await using var stream = client.GetStream();
        byte[] data = Encoding.GetEncoding(Config.Encoding).GetBytes(commands);
        await stream.WriteAsync(data, 0, data.Length);
        await stream.FlushAsync();
    }

    protected async Task CopyCommandToClipboard()
    {
        string cmd = Config.Protocol == PrintProtocol.Browser
            ? "(Browser print has no raw command)"
            : GenerateAllCommands();
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", cmd);
        await OnNotify.InvokeAsync("Commands copied to clipboard.");
    }

    protected async Task Close()
    {
        await OnClose.InvokeAsync(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool ShowField(string name) =>
        TemplateFields.Contains(name, StringComparer.OrdinalIgnoreCase);

    private static string Truncate(string? s, int max) =>
        s == null ? "" : (s.Length > max ? s[..max] : s);
}