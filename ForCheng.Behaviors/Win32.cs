using System.Runtime.InteropServices;

namespace ForCheng.Behaviors
{
    public static class Win32
    {
        #region 获取鼠标坐标

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out Point pt);

        //记录坐标的数据结构
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        #endregion
    }
}
