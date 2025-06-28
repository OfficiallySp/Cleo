using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Cleo.Helpers
{
    public class HotKeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly Dictionary<int, Action> _hotKeyActions = new();
        private readonly HotKeyWindow _window;
        private int _currentId = 1;

        public HotKeyManager()
        {
            _window = new HotKeyWindow();
            _window.HotKeyPressed += OnHotKeyPressed;
        }

        public bool RegisterHotKey(ModifierKeys modifiers, Action action)
        {
            // Default to Space key for Ctrl+Space
            return RegisterHotKey(modifiers, Keys.Space, action);
        }

        public bool RegisterHotKey(ModifierKeys modifiers, Keys key, Action action)
        {
            var id = _currentId++;
            _hotKeyActions[id] = action;

            return RegisterHotKey(_window.Handle, id, (uint)modifiers, (uint)key);
        }

        private void OnHotKeyPressed(int id)
        {
            if (_hotKeyActions.TryGetValue(id, out var action))
            {
                action?.Invoke();
            }
        }

        public void Dispose()
        {
            foreach (var id in _hotKeyActions.Keys)
            {
                UnregisterHotKey(_window.Handle, id);
            }
            _window?.Dispose();
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private class HotKeyWindow : NativeWindow, IDisposable
        {
            public event Action<int>? HotKeyPressed;

            public HotKeyWindow()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    var id = m.WParam.ToInt32();
                    HotKeyPressed?.Invoke(id);
                }
                base.WndProc(ref m);
            }

            public void Dispose()
            {
                DestroyHandle();
            }
        }
    }

    [Flags]
    public enum ModifierKeys : uint
    {
        None = 0x0000,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Win = 0x0008
    }
}
