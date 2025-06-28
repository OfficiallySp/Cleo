using System.Windows;
using System.Windows.Forms;
using Cleo.Services;
using Cleo.Helpers;
using Application = System.Windows.Application;

namespace Cleo
{
    public partial class App : Application
    {
        private SystemTrayManager? _trayManager;
        private HotKeyManager? _hotKeyManager;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize services
            InitializeSystemTray();
            InitializeHotKeys();

            // Create but don't show the main window
            _mainWindow = new MainWindow();
            _mainWindow.Hide();
        }

        private void InitializeSystemTray()
        {
            _trayManager = new SystemTrayManager();
            _trayManager.ShowWindowRequested += OnShowWindowRequested;
            _trayManager.ExitRequested += OnExitRequested;
        }

        private void InitializeHotKeys()
        {
            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.RegisterHotKey(Helpers.ModifierKeys.Control, () =>
            {
                OnShowWindowRequested();
            });
        }

        private void OnShowWindowRequested()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
            }

            _mainWindow.ShowAndActivate();
        }

        private void OnExitRequested()
        {
            _hotKeyManager?.Dispose();
            _trayManager?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotKeyManager?.Dispose();
            _trayManager?.Dispose();
            base.OnExit(e);
        }
    }
}
