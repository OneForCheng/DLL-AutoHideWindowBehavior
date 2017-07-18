using System.Windows;
using ForCheng.Behaviors;

namespace TestWpfApplication
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private AutoHideWindowBehavior _autoHideBehavior;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _autoHideBehavior = new AutoHideWindowBehavior
            {
                AutoHideFactor = 5,//自动贴边隐藏的触发距离
                AutoHideDirection = AutoHideDirection.Top | AutoHideDirection.Right,//允许贴边上和右方的自动隐藏
            };
            _autoHideBehavior.Attach(this);
        }
    }
}
