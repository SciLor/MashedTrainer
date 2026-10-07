param(
    [string]$ExePath = "bin/Release/SciLors Mashed Trainer.exe",
    [string]$OutputDir = "screenshots"
)

$ErrorActionPreference = "Continue"

# 1. Adjust resolution so the 1330x940 window fits completely on screen
try {
    Set-DisplayResolution -Width 1920 -Height 1080 -Force
    Start-Sleep -Seconds 1
} catch {
    Write-Host "Set-DisplayResolution not supported or failed: $_"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win32 {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left; public int Top; public int Right; public int Bottom;
        public int Width { get { return Right - Left; } }
        public int Height { get { return Bottom - Top; } }
    }
}
"@

function Save-Bitmap([System.Drawing.Bitmap]$bmp, [string]$outFile) {
    $bmp.Save($outFile, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Saved: $outFile"
}

function Capture-FullScreen([string]$outFile) {
    $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap($bounds.Width, $bounds.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
    $g.Dispose()
    Save-Bitmap $bmp $outFile
}

function Capture-Window([IntPtr]$hWnd, [string]$outFile) {
    [Win32]::ShowWindow($hWnd, 9) # SW_RESTORE
    [Win32]::SetForegroundWindow($hWnd)
    Start-Sleep -Milliseconds 600
    
    $rect = New-Object Win32+RECT
    [Win32]::GetWindowRect($hWnd, [ref]$rect)
    
    $w = [Math]::Max(100, $rect.Width)
    $h = [Math]::Max(100, $rect.Height)
    
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size($w, $h)))
    $g.Dispose()
    Save-Bitmap $bmp $outFile
}

Write-Host "Launching $ExePath..."
$proc = Start-Process -FilePath $ExePath -PassThru
Start-Sleep -Seconds 6

$hWnd = $proc.MainWindowHandle
$retry = 0
while ($hWnd -eq [IntPtr]::Zero -and $retry -lt 10) {
    Start-Sleep -Seconds 1
    $proc.Refresh()
    $hWnd = $proc.MainWindowHandle
    $retry++
}

Write-Host "Window Handle: $hWnd"
Capture-FullScreen "$OutputDir/fullscreen.png"

if ($hWnd -ne [IntPtr]::Zero) {
    Capture-Window $hWnd "$OutputDir/mainwindow.png"
    
    # Enumerate and capture all TabItems via UI Automation
    try {
        Add-Type -AssemblyName UIAutomationClient
        Add-Type -AssemblyName UIAutomationTypes
        
        $root = [System.Windows.Automation.AutomationElement]::FromHandle($hWnd)
        $tabCondition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::Tab
        )
        $tabControls = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCondition)
        Write-Host "Found $($tabControls.Count) TabControl(s)"
        
        foreach ($tabCtrl in $tabControls) {
            $itemCondition = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::TabItem
            )
            $tabItems = $tabCtrl.FindAll([System.Windows.Automation.TreeScope]::Children, $itemCondition)
            Write-Host "Found $($tabItems.Count) TabItem(s)"
            
            foreach ($tabItem in $tabItems) {
                $tabName = $tabItem.Current.Name
                Write-Host "Selecting Tab: $tabName"
                try {
                    $selectPattern = $tabItem.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
                    if ($selectPattern) {
                        $selectPattern.Select()
                        Start-Sleep -Milliseconds 600
                        $safeName = $tabName -replace '[^a-zA-Z0-9_-]', '_'
                        Capture-Window $hWnd "$OutputDir/tab_$safeName.png"
                    }
                } catch {
                    Write-Host "Failed to select tab ${tabName}: $_"
                }
            }
        }
    } catch {
        Write-Host "UI Automation inspection notice: $_"
    }
}

# Cleanly close application
try {
    $proc.CloseMainWindow()
    Start-Sleep -Seconds 1
    if (-not $proc.HasExited) {
        Stop-Process -Id $proc.Id -Force
    }
} catch {}

Write-Host "Screenshot capture complete!"
exit 0

