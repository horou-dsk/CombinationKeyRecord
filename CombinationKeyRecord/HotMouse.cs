using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotMouse
{
    public class Win32Api
    {
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public long dwExtraInfo;
        }
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        //安装钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        //调用下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
    }

    public class MouseHook
    {
        private Point point;
        private Point Point
        {
            get { return point; }
            set
            {
                if (point != value)
                {
                    point = value;
                    if (MouseMoveEvent != null)
                    {
                        var e = new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0);
                        MouseMoveEvent(this, e);
                    }
                }
            }
        }
        private int hHook;
        private static int hMouseHook = 0;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        private const int WM_WHEEL = 0x20a;

        public const int WH_MOUSE_LL = 14;
        public Win32Api.HookProc hProc;
        public MouseHook()
        {
            this.Point = new Point();
        }
        public int SetHook()
        {
            hProc = new Win32Api.HookProc(MouseHookProc);
            hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
            return hHook;
        }
        public void UnHook()
        {
            Win32Api.UnhookWindowsHookEx(hHook);
        }
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || (Int32)wParam == WM_MOUSEMOVE)
            {
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            Win32Api.MouseHookStruct MyMouseHookStruct = (Win32Api.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32Api.MouseHookStruct));
            MouseButtons button = MouseButtons.None;
            int clickCount = 0;
            switch ((Int32)wParam)
            {
                case WM_LBUTTONDOWN:
                    button = MouseButtons.Left;
                    clickCount = 1;
                    MouseDownEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                    break;
                case WM_RBUTTONDOWN:
                    button = MouseButtons.Right;
                    clickCount = 1;
                    MouseDownEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                    break;
                case WM_MBUTTONDOWN:
                    button = MouseButtons.Middle;
                    clickCount = 1;
                    MouseDownEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                    break;
                //case WM_LBUTTONUP:
                //    button = MouseButtons.Left;
                //    clickCount = 1;
                //    MouseUpEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                //    break;
                //case WM_RBUTTONUP:
                //    button = MouseButtons.Right;
                //    clickCount = 1;
                //    MouseUpEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                //    break;
                //case WM_MBUTTONUP:
                //    button = MouseButtons.Middle;
                //    clickCount = 1;
                //    MouseUpEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                //    break;
                case WM_WHEEL:
                    button = MouseButtons.Middle;
                    clickCount = 1;
                    if (MouseWheelEvent != null)
                    {
                        MouseWheelEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, MyMouseHookStruct.mouseData >> 16));
                    }
                    break;
            }

            this.Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            
        }

        public delegate void MouseMoveHandler(object sender, MouseEventArgs e);
        public event MouseMoveHandler MouseMoveEvent;

        public delegate void MouseClickHandler(object sender, MouseEventArgs e);
        public event MouseClickHandler MouseClickEvent;

        public delegate void MouseDownHandler(object sender, MouseEventArgs e);
        public event MouseDownHandler MouseDownEvent;

        public delegate void MouseUpHandler(object sender, MouseEventArgs e);
        public event MouseUpHandler MouseUpEvent;

        public delegate void MouseWheelHandler(object sender, MouseEventArgs e);
        public event MouseWheelHandler MouseWheelEvent;
    }

}
