using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Cleo.Services
{
    public class SystemTrayManager : IDisposable
    {
        private NotifyIcon _notifyIcon = null!;
        private ContextMenuStrip _contextMenu = null!;

        public event Action? ShowWindowRequested;
        public event Action? ExitRequested;

        public SystemTrayManager()
        {
            InitializeNotifyIcon();
        }

        private void InitializeNotifyIcon()
        {
            _contextMenu = new ContextMenuStrip();

            var showItem = new ToolStripMenuItem("Show Cleo");
            showItem.Click += (s, e) => ShowWindowRequested?.Invoke();
            showItem.Font = new Font(showItem.Font, FontStyle.Bold);

            var separator = new ToolStripSeparator();

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => ExitRequested?.Invoke();

            _contextMenu.Items.AddRange(new ToolStripItem[] { showItem, separator, exitItem });

            _notifyIcon = new NotifyIcon
            {
                Icon = GetApplicationIcon(),
                Visible = true,
                Text = "Cleo - Your AI Assistant",
                ContextMenuStrip = _contextMenu
            };

            _notifyIcon.DoubleClick += (s, e) => ShowWindowRequested?.Invoke();

            // Show a balloon tip on first run
            _notifyIcon.ShowBalloonTip(
                3000,
                "Cleo is running",
                "Press Ctrl+Space to open Cleo anytime!",
                ToolTipIcon.Info
            );
        }

        private Icon GetApplicationIcon()
        {
            // Try to load custom icon, fall back to default
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "cleo.ico");
                if (File.Exists(iconPath))
                {
                    return new Icon(iconPath);
                }
            }
            catch { }

            // Create a simple default icon if custom icon not found
            return SystemIcons.Application;
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}
