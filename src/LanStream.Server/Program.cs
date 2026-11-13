using System;
using System.Windows.Forms;
using LanStream.Server.Forms;

namespace LanStream.Server;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, args) =>
        {
            MessageBox.Show(
                $"An unexpected UI error occurred:\n{args.Exception.Message}",
                "LAN Video Server - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"A critical background error occurred:\n{ex.Message}",
                    "LAN Video Server - Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        };

        ApplicationConfiguration.Initialize();
        Application.Run(new ServerForm());
    }
}
