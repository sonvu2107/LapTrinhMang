using System;
using System.Windows.Forms;
using LanStream.Server.Forms;

namespace LanStream.Server;

internal static class Program
{
    /// <summary>
    /// Điểm khởi chạy chính của ứng dụng LAN Video Streaming Server.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ServerForm());
    }
}
