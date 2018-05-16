using Capture.Extern;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Capture
{
    public class ScreenCapture
    {
        #region Member
        public IntPtr ImgBufPtr { get; private set; }
        private Graphics Graphics { get; set; }
        private byte[] ImgBuf { get; set; }
        private Bitmap Img { get; set; }
        private GCHandle BufHandle { get; set; }
        private IntPtr HandlingPtr { get; set; }
        private Rectangle WindowRtg => CaptureHelper.GetWindowCoordinate(HandlingPtr);
        private IntPtr FrontPtr => Win32Ext.GetForegroundWindow();

        private static object locker = new { };
        #endregion

        /// <summary>
        /// Contractor
        /// </summary>
        private ScreenCapture() { }

        /// <summary>
        /// ScreenCapture Content Setting Action
        /// </summary>
        private static Action<ScreenCapture, int, int> ContentSetting = (sc, w, h) =>
        {
            sc.ImgBuf = new byte[w * 4 * h];
            sc.BufHandle = GCHandle.Alloc(sc.ImgBuf, GCHandleType.Pinned);
            sc.ImgBufPtr = sc.BufHandle.AddrOfPinnedObject();
            sc.Img = new Bitmap(w, h, w * 4, PixelFormat.Format32bppArgb, sc.ImgBufPtr);
            sc.Graphics = Graphics.FromImage(sc.Img);
        };

        /// <summary>
        /// Create Screen Capture
        /// </summary>
        /// <param name="hWnd">Handling Windows Pointer</param>
        /// <returns></returns>
        public static ScreenCapture Create(IntPtr hWnd)
        {
            ScreenCapture sc = new ScreenCapture
            {
                HandlingPtr = hWnd,
            };

            // If hWnd is not Zero, just set HandlingPtr
            if (hWnd != IntPtr.Zero) return sc;

            // If hWnd is Zero, just consider as Full Screen Capture
            Rectangle windRtg = CaptureHelper.GetWindowCoordinate(hWnd);
            ContentSetting(sc, windRtg.Width, windRtg.Height);
            return sc;
        }

        /// <summary>
        /// Reset Handling Windows Pointer
        /// </summary>
        /// <param name="hWnd">current Handling Windows Pointer</param>
        public void ResetHandlingPointer(IntPtr hWnd)
        {
            HandlingPtr = hWnd;

            // If hWnd is not Zero, just set HandlingPtr
            if (hWnd != IntPtr.Zero) return;

            // If hWnd is Zero, just consider as Full Screen Capture
            Rectangle windRtg = CaptureHelper.GetWindowCoordinate(hWnd);
            ContentSetting(this, windRtg.Width, windRtg.Height);
        }

        /// <summary>
        /// Capture View
        /// </summary>
        /// <returns></returns>
        public Bitmap CaptureView()
        {
            try
            {
                lock (locker)
                {
                    Rectangle windRtg = WindowRtg;
                    Point sPoint = new Point(0, 0);

                    // if HandlingPtr is not Zero, Point will start from Specific Windows
                    if (HandlingPtr != IntPtr.Zero)
                    {
                        sPoint = new Point(windRtg.X, windRtg.Y);
                        ResetContent(windRtg.Width, windRtg.Height);
                    }

                    if (HandlingPtr == FrontPtr || HandlingPtr == IntPtr.Zero)
                        Graphics.CopyFromScreen(sPoint, new Point(0, 0), new Size(windRtg.Width, windRtg.Height));

                    return Img;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Capture Error:{ex.Message}");
            }
        }

        private int OldWidth;
        private int OldHeight;
        /// <summary>
        /// Reset Capture Setting
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void ResetContent(int width, int height)
        {
            if (OldWidth == width && OldHeight == height) return;
            OldWidth = width;
            OldHeight = height;
            ContentSetting(this, width, height);
        }

       
    }
}
