using System.Runtime.InteropServices;
using System.Windows;

namespace SpotifyNet.Cover.Model.Interop
{
    internal static class PInvoke
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        /// <summary>
        /// Mouse position relative to screen
        /// </summary>
        /// <returns></returns>
        internal static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
    }
}
