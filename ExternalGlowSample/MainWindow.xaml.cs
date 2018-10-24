using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using MetroRadiance.Chrome;
using MetroRadiance.Platform;

namespace ExternalGlowSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Dictionary<IntPtr, Tuple<WindowChrome, ExternalWindow>> _glowedWindows
            = new Dictionary<IntPtr, Tuple<WindowChrome, ExternalWindow>>();

        public MainWindow()
        {
            this.InitializeComponent();
        }
        
        private void Attach(object sender, RoutedEventArgs e)
        {
            var hWnd = this.GetWindowHandle(this.TextBox.Text);
            if (this._glowedWindows.ContainsKey(hWnd))
            {
                return;
            }
            this.AttachCore(hWnd);
        }

        private void Detach(object sender, RoutedEventArgs e)
        {
            var hWnd = this.GetWindowHandle(this.TextBox.Text);
            if (!this._glowedWindows.ContainsKey(hWnd))
            {
                return;
            }
            this.DetachCore(hWnd);
        }

        private void AttachCore(IntPtr hWnd)
        {
            var external = new ExternalWindow(hWnd);
            var chrome = new WindowChrome
            {
                BorderThickness = new Thickness(1.0),
            };
            chrome.Attach(external);
            external.Closed += (_, __) => this.DetachCore(hWnd);

            this._glowedWindows.Add(hWnd, new Tuple<WindowChrome, ExternalWindow>(chrome, external));

            external.Activated += (_, __) => Debug.WriteLine("Activated");
            external.Deactivated += (_, __) => Debug.WriteLine("Deactivated");
            external.LocationChanged += (_, __) => Debug.WriteLine("LocationChanged");
            external.SizeChanged += (_, __) => Debug.WriteLine("SizeChanged");
            external.StateChanged += (_, __) => Debug.WriteLine("StateChanged");
            external.Closed += (_, __) => Debug.WriteLine("Closed");
        }

        private void DetachCore(IntPtr hWnd)
        {
            this._glowedWindows[hWnd].Item1.Close();
            this._glowedWindows[hWnd].Item2.Dispose();
            this._glowedWindows.Remove(hWnd);
        }

        private IntPtr GetWindowHandle(string windowTitle)
        {
            var hWnd = IntPtr.Zero;
            foreach (var processList in Process.GetProcesses())
            {
                if (processList.MainWindowTitle.Contains(this.TextBox.Text))
                {
                    hWnd = processList.MainWindowHandle;
                }
            }
            return hWnd;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            foreach (var window in this._glowedWindows.Select(x => x.Value))
            {
                window.Item1.Close();
                window.Item2.Dispose();
            }
        }
    }
}
