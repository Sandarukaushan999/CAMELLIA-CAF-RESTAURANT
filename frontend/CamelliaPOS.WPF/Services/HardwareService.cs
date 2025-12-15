using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CamelliaPOS.WPF.Services;

public class HardwareService
{
    private SerialPort? _printerPort;
    private bool _isInitialized = false;

    public bool IsPrinterConnected => _isInitialized && _printerPort?.IsOpen == true;

    public void InitializePrinter(string portName = "COM1", int baudRate = 9600)
    {
        try
        {
            _printerPort = new SerialPort(portName, baudRate)
            {
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };
            _printerPort.Open();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Printer initialization failed: {ex.Message}", "Hardware Error");
            _isInitialized = false;
        }
    }

    public void PrintReceipt(string receiptText)
    {
        if (!IsPrinterConnected)
        {
            System.Windows.MessageBox.Show("Printer not connected", "Error");
            return;
        }

        try
        {
            // ESC/POS commands
            var esc = (char)0x1B;
            var reset = esc + "@";
            var cut = esc + "i";
            
            var printData = reset + receiptText + "\n\n\n" + cut;
            
            _printerPort?.Write(printData);
            
            // Open cash drawer (ESC/POS command)
            OpenCashDrawer();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Print error: {ex.Message}", "Error");
        }
    }

    public void OpenCashDrawer()
    {
        if (!IsPrinterConnected) return;

        try
        {
            // ESC/POS cash drawer command
            var esc = (char)0x1B;
            var openDrawer = esc + "p" + (char)0x00 + (char)0x19 + (char)0xFF;
            _printerPort?.Write(openDrawer);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Cash drawer error: {ex.Message}", "Error");
        }
    }

    public void Dispose()
    {
        _printerPort?.Close();
        _printerPort?.Dispose();
    }
}

// Barcode scanner integration (HID device)
public class BarcodeScannerService
{
    // USB HID barcode scanners typically act as keyboards
    // This would need to be implemented with low-level HID APIs
    // For now, this is a placeholder
    
    public event EventHandler<string>? BarcodeScanned;

    public void Initialize()
    {
        // TODO: Implement HID device detection and reading
        // This requires Windows API calls or a library like HIDSharp
    }

    public void Dispose()
    {
        // Cleanup
    }
}

