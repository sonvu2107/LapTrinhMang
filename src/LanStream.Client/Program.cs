using System;
using System.Windows.Forms;
using LanStream.Client.Forms;

namespace LanStream.Client;

internal static class Program
{
    /// <summary>
    /// Điểm khởi chạy chính của ứng dụng LAN Video Streaming Client.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ClientForm());
    }
}
