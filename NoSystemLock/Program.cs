using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NoSystemLock
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SystemSleepManagement.PreventSleep(true);
            Task run = Task.Run(() =>
            {
                while (true)
                {
                    long lastInputTime = CheckComputerFreeState.GetLastInputTime();
                    Console.WriteLine($"电脑闲置：{lastInputTime}");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    if (lastInputTime > TimeSpan.FromMinutes(1).TotalSeconds)
                    {
                        //uint resetSleepTimer = SystemSleepManagement.ResetSleepTimer(true);
                        //Console.WriteLine(resetSleepTimer.ToString("X2"));
                        //break;
                        SystemSleepManagement.MoveMouse();
                    }
                }
            });
            Task.WaitAll(run);
            Console.WriteLine("Over");
            Console.ReadKey(true);
        }
    }
    class SystemSleepManagement
    {
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        //模拟鼠标滚轮滚动操作，必须配合dwData参数
        const int MOUSEEVENTF_WHEEL = 0x0800;


        public static void MoveMouse()
        {
            Console.WriteLine("模拟鼠标移动");
            //mouse_event(MOUSEEVENTF_MOVE, 50, 50, 0, 0);//相对当前鼠标位置x轴和y轴分别移动50像素
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 0, 0);//鼠标滚动，使界面向下滚动20的高度
            //mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 0, 0);//鼠标滚动，使界面向下滚动20的高度
        }
        //定义API函数
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);

        [Flags]
        enum ExecutionFlag : uint
        {
            System = 0x00000001,
            Display = 0x00000002,
            Continus = 0x80000000,
        }

        /// <summary>
        ///阻止系统休眠，直到线程结束恢复休眠策略
        /// </summary>
        /// <param name="includeDisplay">是否阻止关闭显示器</param>
        public static uint PreventSleep(bool includeDisplay = false)
        {
            if (includeDisplay)
                return SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display | ExecutionFlag.Continus);
            else
                return SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Continus);
        }

        /// <summary>
        ///恢复系统休眠策略
        /// </summary>
        public static uint ResotreSleep()
        {
            return SetThreadExecutionState(ExecutionFlag.Continus);
        }

        /// <summary>
        ///重置系统休眠计时器
        /// </summary>
        /// <param name="includeDisplay">是否阻止关闭显示器</param>
        public static uint ResetSleepTimer(bool includeDisplay = false)
        {
            if (includeDisplay)
                return SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display);
            else
                return SetThreadExecutionState(ExecutionFlag.System);
        }
    }

    public class CheckComputerFreeState
    {
        /// <summary>
        /// 创建结构体用于返回捕获时间
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            /// <summary>
            /// 设置结构体块容量
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;

            /// <summary>
            /// 抓获的时间
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        /// <summary>
        /// 获取键盘和鼠标没有操作的时间
        /// </summary>
        /// <returns>用户上次使用系统到现在的时间间隔，单位为秒</returns>
        public static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            if (!GetLastInputInfo(ref vLastInputInfo))
            {
                return 0;
            }
            else
            {
                var count = Environment.TickCount - (long)vLastInputInfo.dwTime;
                var icount = count / 1000;
                return icount;
            }
        }

    }
}
