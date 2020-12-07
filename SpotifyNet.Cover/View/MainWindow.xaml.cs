using GalaSoft.MvvmLight;
using SpotifyNet.Cover.Model.Interop;
using SpotifyNet.Cover.ViewModel;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SpotifyNet.Cover.View
{
    public partial class MainWindow : Window
    {
        // https://stackoverflow.com/questions/2471867/resize-a-wpf-window-but-maintain-proportions
        private double _aspectRatio;
        private bool? _adjustingHeight = null;

        private MainViewModel mainViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;
            mainViewModel.PropertyChanged += MainWindow_PropertyChanged;
        }

        private void MainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CoverHeight))
                Height = mainViewModel.CoverHeight;
            else if (e.PropertyName == nameof(MainViewModel.CoverWidth))
                Width = mainViewModel.CoverWidth;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);

            _aspectRatio = this.Width / this.Height;
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                    if ((pos.flags & (int)SWP.NOMOVE) != 0)
                        return IntPtr.Zero;

                    Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                    if (wnd == null)
                        return IntPtr.Zero;

                    // determine what dimension is changed by detecting the mouse position relative to the 
                    // window bounds. if gripped in the corner, either will work.
                    if (!_adjustingHeight.HasValue)
                    {
                        Point p = PInvoke.GetMousePosition();

                        double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
                        double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));

                        _adjustingHeight = diffHeight > diffWidth;
                    }

                    if (_adjustingHeight.Value)
                        pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
                    else
                        pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change

                    Marshal.StructureToPtr(pos, lParam, true);
                    handled = true;
                    break;

                case WM.EXITSIZEMOVE:
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
            }

            return IntPtr.Zero;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CanvasLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
                return;

            mainViewModel.PreviousCommand.Execute(e);
        }

        private void CanvasMid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
                return;

            mainViewModel.StartResumeCommand.Execute(e);
        }

        private void CanvasRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
                return;

            mainViewModel.PreviousCommand.Execute(e);
        }
    }
}
