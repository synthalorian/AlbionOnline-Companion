using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace StatisticsAnalysisTool.Network;

/// <summary>
/// Handles privilege escalation for packet capture.
/// On Linux: uses pkexec (PolicyKit) for graphical sudo prompt.
/// On Windows: checks for admin rights.
/// </summary>
public static class PrivilegeEscalation
{
    /// <summary>
    /// Check if we currently have the privileges needed for raw socket capture.
    /// </summary>
    public static bool HasCapturePrivileges()
    {
        if (OperatingSystem.IsWindows())
        {
            return IsRunningAsAdminWindows();
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            return IsRunningAsRootUnix();
        }

        return false;
    }

    /// <summary>
    /// Check if we CAN capture (either already privileged, or can escalate).
    /// </summary>
    public static bool CanEscalate()
    {
        if (HasCapturePrivileges()) return true;

        if (OperatingSystem.IsLinux())
        {
            // Check if pkexec exists
            return File.Exists("/usr/bin/pkexec") || File.Exists("/usr/local/bin/pkexec");
        }

        return false;
    }

    /// <summary>
    /// Try to test raw socket creation to see if we have privileges.
    /// </summary>
    public static bool TestRawSocketAccess()
    {
        try
        {
            using var socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Raw,
                System.Net.Sockets.ProtocolType.Udp);
            return true;
        }
        catch (System.Net.Sockets.SocketException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Get a user-friendly message about the privilege status.
    /// </summary>
    public static string GetStatusMessage()
    {
        if (TestRawSocketAccess())
            return "✅ Packet capture ready";

        if (OperatingSystem.IsLinux())
        {
            if (CanEscalate())
                return "⚠️ Root required — click Start Tracking to authenticate";
            return "❌ Root required — run with sudo or set capabilities";
        }

        if (OperatingSystem.IsWindows())
        {
            return "⚠️ Administrator required — restart as admin";
        }

        return "❌ Unknown platform";
    }

    /// <summary>
    /// Get instructions for enabling capture without restarting.
    /// </summary>
    public static string GetSetupInstructions()
    {
        if (OperatingSystem.IsLinux())
        {
            return """
                To enable packet capture without sudo:
                
                Option 1 (one-time setup):
                  sudo setcap cap_net_raw+ep $(readlink -f $(which dotnet))
                
                Option 2 (run with sudo):
                  sudo dotnet AlbionOnlineCompanion.dll
                
                Option 3 (pkexec — graphical prompt):
                  Click Start Tracking and enter your password
                """;
        }

        if (OperatingSystem.IsWindows())
        {
            return "Right-click → Run as Administrator, or install Npcap.";
        }

        return "Unknown platform";
    }

    private static bool IsRunningAsRootUnix()
    {
        return Environment.UserName == "root" || GetUid() == 0;
    }

    private static bool IsRunningAsAdminWindows()
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    [DllImport("libc", SetLastError = true)]
    private static extern uint getuid();

    private static uint GetUid()
    {
        try
        {
            return getuid();
        }
        catch
        {
            return 1000; // Non-root fallback
        }
    }
}
