using Capture.Extern;
using Capture.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Capture
{
    public class CaptureHelper
    {
        /// <summary>
        /// Get Handling Window Coordinate(x,y start point and width,height)
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static Rectangle GetWindowCoordinate(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return Screen.PrimaryScreen.Bounds;

            WindowInfo wInfo = new WindowInfo()
            {
                cbSize = WindowInfo.GetSize()
            };
            Win32Ext.GetWindowInfo(hWnd, ref wInfo);
            return wInfo.rcWindow;
        }

        /// <summary>
        /// Get Handling Window 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<IntPtr, string>> GetHandlingWindowList()
        {
            List<KeyValuePair<IntPtr, string>> list = new List<KeyValuePair<IntPtr, string>>();
            Win32Ext.EnumWindows((hWnd, lParam) =>
            {
                if (hWnd == IntPtr.Zero) return false;
                if (!Win32Ext.IsWindowVisible(hWnd)) return true;
                string name = GetWindowName(hWnd);
                if (!string.IsNullOrEmpty(name)) list.Add(new KeyValuePair<IntPtr, string>(hWnd, name));
                return true;
            }, IntPtr.Zero);

            return list;
        }

        /// <summary>
        /// Get Window Name
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static string GetWindowName(IntPtr hWnd)
        {
            int length = Win32Ext.GetWindowTextLength(hWnd) + 1;
            StringBuilder name = new StringBuilder(length);
            Win32Ext.GetWindowText(hWnd, name, length);
            return name.ToString();
        }

        public static bool IsWindowVisible(IntPtr hWnd)
        {
            return Win32Ext.IsWindowVisible(hWnd);
        }

        public static IntPtr GetTopWindow(IntPtr hWnd)
        {
            return Win32Ext.GetTopWindow(hWnd);
        }

        public static IntPtr GetForegroundWindow()
        {
            return Win32Ext.GetForegroundWindow();
        }

        /// <summary>
        /// Trans Bitmap To BitmapSource
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr hObject);
        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            BitmapSource result =
                Imaging.CreateBitmapSourceFromHBitmap(ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //release resource
            DeleteObject(ptr);
            return result;
        }

    }
}
