using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace 网易云音乐下载.Converters
{
    /// <summary>
    /// 根据按钮索引返回渐变起始颜色
    /// </summary>
    public class GradientStartConverter : IValueConverter
    {
        // 每个按钮的渐变起始颜色
        private static readonly Color[] StartColors = new[]
        {
            Color.FromRgb(66, 133, 244),   // 蓝色 - NCM 转换
            Color.FromRgb(234, 67, 53),    // 红色 - ID 下载
            Color.FromRgb(52, 168, 83),    // 绿色 - 我的歌单
            Color.FromRgb(156, 39, 176)    // 紫色 - 更新日志
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string tag)
            {
                int index = tag switch
                {
                    "ncm" => 0,
                    "download" => 1,
                    "playlist" => 2,
                    "updateLog" => 3,
                    _ => 0
                };
                return index < StartColors.Length ? StartColors[index] : StartColors[0];
            }
            return StartColors[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 根据按钮索引返回渐变结束颜色
    /// </summary>
    public class GradientEndConverter : IValueConverter
    {
        // 每个按钮的渐变结束颜色
        private static readonly Color[] EndColors = new[]
        {
            Color.FromRgb(100, 181, 246),  // 浅蓝色 - NCM 转换
            Color.FromRgb(239, 154, 154),  // 浅红色 - ID 下载
            Color.FromRgb(129, 199, 132),  // 浅绿色 - 我的歌单
            Color.FromRgb(186, 104, 200)   // 浅紫色 - 更新日志
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string tag)
            {
                int index = tag switch
                {
                    "ncm" => 0,
                    "download" => 1,
                    "playlist" => 2,
                    "updateLog" => 3,
                    _ => 0
                };
                return index < EndColors.Length ? EndColors[index] : EndColors[0];
            }
            return EndColors[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 根据按钮索引返回图标 Unicode 字符（不使用 FontAwesome，使用普通 Unicode 符号）
    /// </summary>
    public class IconTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string tag)
            {
                return tag switch
                {
                    "ncm" => "⟳",      // 循环/刷新符号
                    "download" => "↓",  // 下载箭头
                    "playlist" => "♫",  // 音乐符号
                    "updateLog" => "≡", // 日志/文本
                    _ => "♫"
                };
            }
            return "♫";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
