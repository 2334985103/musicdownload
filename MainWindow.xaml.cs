using System.Windows;
using System.Windows.Media.Animation;
using 网易云音乐下载.ViewModels;

namespace 网易云音乐下载
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // 设置数据上下文
            DataContext = new MainViewModel();
            
            // 订阅完成动画事件
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 窗口加载完成后的初始化
            // 可以在这里添加额外的动画或初始化逻辑
        }

        /// <summary>
        /// 播放完成动画
        /// </summary>
        private void PlayCompleteAnimation(UIElement element)
        {
            if (Resources["CompleteAnimation"] is Storyboard storyboard)
            {
                Storyboard.SetTarget(storyboard, element);
                storyboard.Begin();
            }
        }

        /// <summary>
        /// 播放按钮点击动画
        /// </summary>
        private void PlayButtonClickAnimation(UIElement element)
        {
            if (Resources["ButtonClickStoryboard"] is Storyboard storyboard)
            {
                Storyboard.SetTarget(storyboard, element);
                storyboard.Begin();
            }
        }
    }
}
