using System.ComponentModel;
using System.Windows;

namespace 网易云音乐下载
{
    /// <summary>
    /// 简单的输入对话框
    /// </summary>
    public partial class InputDialog : Window, INotifyPropertyChanged
    {
        private string _windowTitle;
        private string _message;
        private string _inputText;

        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string InputText
        {
            get { return _inputText; }
            set
            {
                _inputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        public InputDialog(string title, string message, string defaultInput)
        {
            InitializeComponent();
            DataContext = this;
            WindowTitle = title;
            Message = message;
            InputText = defaultInput ?? string.Empty;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
