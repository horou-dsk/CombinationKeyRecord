using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeyBoardHook;
using HotMouse;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Microsoft.Win32;

namespace CombinationKeyRecord
{

    public partial class Form1 : Form
    {
        KeyboardHook k_hook = new KeyboardHook();
        MouseHook m_hook = new MouseHook();
        Boolean ctrlbool = false;
        Boolean isContinuity = false;
        List<String> logList = new List<string>();

        int ScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;
        int ScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;
        Point point;

        public Form1()
        {
            Visible = false;
            TopLevel = false;
            InitializeComponent();
            KeyEventHandler myKeyEventHandler = new KeyEventHandler(hook_KeyDown);
            k_hook.KeyDownEvent += myKeyEventHandler;//钩住键按下 
            k_hook.KeyUpEvent += new KeyEventHandler(hook_KeyUp);
            k_hook.Start();//安装键盘钩子
            point = new Point(ScreenWidth - Width - 10, ScreenHeight - Height - 10);
            RegisterAppBar(false);
            m_hook.SetHook();//安装钩子
            m_hook.MouseDownEvent += mh_MouseDownEvent;//添加鼠标事件
            //m_hook.MouseUpEvent += mh_MouseUpEvent;
            m_hook.MouseWheelEvent += mh_MouseWheelEvent;
            //程序退出事件
            Application.ApplicationExit += new EventHandler(AppExitEvent);
            SystemEvents.SessionEnded += new SessionEndedEventHandler(AppOnExit);
            // 给托盘右键菜单添加事件
            退出应用ToolStripMenuItem.Click += new EventHandler(AppOnExit);
            写入LogToolStripMenuItem.Click += new EventHandler(WriteLog);
        }

        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.KeyData.ToString());
            logList.Add("[" + DateTime.Now.ToString() + " -> KeyDown -> " + e.KeyData.ToString() +"]\t");
            //if(e.KeyCode == Keys.LControlKey && !ctrlbool  && !RunningFullScreenApp)
            //{
            //    ctrlbool = true;
            //    Visible = true;
            //    TopLevel = true;
            //    TopMost = true;
            //    SetPenetrate();
            //    Opacity = 0.5;
            //    //SetBitmap(new Bitmap(BackgroundImage), 120);
            //    Location = point;
            //}
        }

        private void hook_KeyUp(object sender, KeyEventArgs e)
        {
            logList.Add("[" + DateTime.Now.ToString() + " -> KeyUp -> " + e.KeyData.ToString() + "]\t");
            if(logList.Count > 500)
            {
                WriteLog();
            }
            //Console.WriteLine(Control.ModifierKeys == Keys.Control);
            //Keys k = Control.ModifierKeys;
            //if(ctrlbool && (e.KeyCode == Keys.LControlKey || k == Keys.Shift || k == Keys.Alt))
            //{
            //    ctrlbool = false;
            //    Visible = false;
            //    TopLevel = false;
            //    isContinuity = false;
            //    if(logList.Count > 15)
            //    {
            //        WriteLog();
            //    }
            //}
        }

        private void mh_MouseDownEvent(object sender, MouseEventArgs e)
        {
            //if (ctrlbool)
            //{
            //    if(e.Button == MouseButtons.Left)
            //    {
            //        logList.Add("ctrl + 鼠标左键 -- 时间：" + DateTime.Now.ToString() + "  "+isContinuity);
            //    }
            //    if(e.Button == MouseButtons.Right)
            //    {
            //        logList.Add("ctrl + 鼠标右键 -- 时间：" + DateTime.Now.ToString() + "  "+isContinuity);
            //    }
            //    if (!isContinuity) isContinuity = true;
            //}
        }

        private void mh_MouseUpEvent(object sender, MouseEventArgs e) { }

        private void mh_MouseWheelEvent(object sender, MouseEventArgs e)
        {
            //if (ctrlbool)
            //{
            //    if (e.Delta > 0)
            //    {
            //        logList.Add("ctrl + 鼠标滚轮上 -- 时间：" + DateTime.Now.ToString() + "  " + isContinuity);
            //    }
            //    else
            //    {
            //        logList.Add("ctrl + 鼠标滚轮下 -- 时间：" + DateTime.Now.ToString() + "  " + isContinuity);
            //    }
            //    if (!isContinuity) isContinuity = true;
            //}
        }

        private void WriteLog()
        {
            FileStream logFile = new FileStream("D:\\Backup\\Log\\CombinationKeyRecord\\log.txt", FileMode.Append);

            StreamWriter sw = new StreamWriter(logFile);

            int index = 0; String row = "";

            foreach(String log in logList)
            {
                index++;
                row += log;
                if(index == 3)
                {
                    sw.WriteLine(row);
                    index = 0;
                    row = "";
                }
                // 写入换行符
                // sw.WriteLine(sw.NewLine);
            }
            if (index != 0) sw.WriteLine(row);
            sw.Flush();
            logFile.Flush();
            sw.Close();
            logFile.Close();
            logList.Clear();

        }

        private void WriteLog(object sender, EventArgs e)
        {
            WriteLog();
        }

        private void AppExitEvent(object sender, EventArgs e)
        {
            //Console.WriteLine("程序退出！！");
            RegisterAppBar(true);
            SystemEvents.SessionEnded -= new SessionEndedEventHandler(AppOnExit);
            k_hook.Stop();
            m_hook.UnHook();
            WriteLog();
            notifyIcon1.Dispose();
        }

        private void AppOnExit(object sender, EventArgs e) {
            AppExitEvent(sender, e);
            this.Close();
        }

        //声明常量：(释义可参见windows API)  
        const int WS_EX_NOACTIVATE = 0x08000000;
        //重载Form的CreateParams属性，添加不获取焦点属性值。  
        protected override CreateParams CreateParams

        {

            get

            {

                CreateParams cp = base.CreateParams;

                cp.ExStyle |= WS_EX_NOACTIVATE;

                return cp;

            }

        }

        private const uint WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_STYLE = (-16);
        private const int GWL_EXSTYLE = (-20);
        private const int LWA_ALPHA = 0;

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(
        IntPtr hwnd,
        int nIndex,
        uint dwNewLong
        );

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(
        IntPtr hwnd,
        int nIndex
        );

        [DllImport("user32", EntryPoint = "SetLayeredWindowAttributes")]
        private static extern int SetLayeredWindowAttributes(
        IntPtr hwnd,
        int crKey,
        int bAlpha,
        int dwFlags
        );

        /// <summary> 
        /// 设置窗体具有鼠标穿透效果 
        /// </summary> 
        public void SetPenetrate()
        {
            this.TopMost = true;
            GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED);
            SetLayeredWindowAttributes(this.Handle, 0, 100, LWA_ALPHA);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();
            base.OnHandleCreated(e);
        }

        public void SetBitmap(Bitmap bitmap, byte opacity)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new ApplicationException("位图必须是32位包含alpha 通道");

            IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
            IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));   // 创建GDI位图句柄，效率较低
                oldBitmap = Win32.SelectObject(memDc, hBitmap);

                Win32.Size size = new Win32.Size(bitmap.Width, bitmap.Height);
                Win32.Point pointSource = new Win32.Point(0, 0);
                Win32.Point topPos = new Win32.Point(Left, Top);
                Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION();
                blend.BlendOp = Win32.AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = opacity;
                blend.AlphaFormat = Win32.AC_SRC_ALPHA;

                Win32.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);
            }
            finally
            {
                Win32.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBitmap);

                    Win32.DeleteObject(hBitmap);
                }
                Win32.DeleteDC(memDc);
            }
        }

        Boolean RunningFullScreenApp = false;
        private IntPtr desktopHandle;
        private IntPtr shellHandle;
        int uCallBackMsg;

        // 注册AppBar 这样系统会在有全屏应用运行时会向我们发送消息
        private void RegisterAppBar(bool registered)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = this.Handle;

            desktopHandle = APIWrapper.GetDesktopWindow();
            shellHandle = APIWrapper.GetShellWindow();
            if (!registered)
            {
                //register
                uCallBackMsg = APIWrapper.RegisterWindowMessage("APPBARMSG_CSDN_HELPER");
                abd.uCallbackMessage = uCallBackMsg;
                uint ret = APIWrapper.SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
            }
            else
            {
                APIWrapper.SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
            }
        }

        
        //重载窗口消息处理函数
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == uCallBackMsg)
            {
                switch (m.WParam.ToInt32())
                {
                    case (int)ABNotify.ABN_FULLSCREENAPP:
                        {
                            IntPtr hWnd = APIWrapper.GetForegroundWindow();
                            //判断当前全屏的应用是否是桌面
                            if(hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle))
                            {
                                RunningFullScreenApp = false;
                                break;
                            }
                            //判断是否全屏
                            if ((int)m.LParam == 1)
                                this.RunningFullScreenApp = true;
                            else
                                this.RunningFullScreenApp = false;
                            break;
                        }
                    default:
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            Console.WriteLine("打开右键菜单");
        }
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cParms = base.CreateParams;
        //        cParms.ExStyle |= 0x00080000; // WS_EX_LAYERED
        //        return cParms;
        //    }
        //}
    }
}
