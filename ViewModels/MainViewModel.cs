using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using 网易云音乐下载.Commands;
using 网易云音乐下载.Models;
using 网易云音乐下载.Services;
using Win32OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Win32SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using FormsFolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using FormsDialogResult = System.Windows.Forms.DialogResult;
using InputMode = 网易云音乐下载.Models.InputMode;

namespace 网易云音乐下载.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AudioConverterService _converterService;
        private readonly NeteaseDownloadService _downloadService;
        private CancellationTokenSource _conversionCts;
        private CancellationTokenSource _downloadCts;

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { SetProperty(ref _selectedTabIndex, value); }
        }

        private InputMode _inputMode = InputMode.SingleFile;
        public InputMode InputMode
        {
            get { return _inputMode; }
            set
            {
                if (SetProperty(ref _inputMode, value))
                {
                    InputPath = string.Empty;
                    NcmFiles.Clear();
                    OnPropertyChanged("IsSingleFileMode");
                    OnPropertyChanged("IsBatchMode");
                    OnPropertyChanged("IsMultipleFilesMode");
                }
            }
        }

        public bool IsSingleFileMode
        {
            get { return InputMode == InputMode.SingleFile; }
        }

        public bool IsBatchMode
        {
            get { return InputMode == InputMode.BatchFolder; }
        }

        public bool IsMultipleFilesMode
        {
            get { return InputMode == InputMode.MultipleFiles; }
        }

        private string _inputPath;
        public string InputPath
        {
            get { return _inputPath; }
            set { SetProperty(ref _inputPath, value); }
        }

        private string _outputPath;
        public string OutputPath
        {
            get { return _outputPath; }
            set { SetProperty(ref _outputPath, value); }
        }

        private string _outputFormat = "mp3";
        public string OutputFormat
        {
            get { return _outputFormat; }
            set { SetProperty(ref _outputFormat, value); }
        }

        public ObservableCollection<NcmFileInfo> NcmFiles { get; private set; }

        private int _conversionProgress;
        public int ConversionProgress
        {
            get { return _conversionProgress; }
            set { SetProperty(ref _conversionProgress, value); }
        }

        private string _currentConvertingFile;
        public string CurrentConvertingFile
        {
            get { return _currentConvertingFile; }
            set { SetProperty(ref _currentConvertingFile, value); }
        }

        private string _conversionStatusText = "准备就绪";
        public string ConversionStatusText
        {
            get { return _conversionStatusText; }
            set { SetProperty(ref _conversionStatusText, value); }
        }

        private bool _isConverting;
        public bool IsConverting
        {
            get { return _isConverting; }
            set
            {
                if (SetProperty(ref _isConverting, value))
                {
                    ((RelayCommand)StartConversionCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CancelConversionCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private bool _showConversionComplete;
        public bool ShowConversionComplete
        {
            get { return _showConversionComplete; }
            set { SetProperty(ref _showConversionComplete, value); }
        }

        private string _songId;
        public string SongId
        {
            get { return _songId; }
            set
            {
                if (SetProperty(ref _songId, value))
                {
                    ((RelayCommand)StartDownloadCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private string _downloadOutputPath;
        public string DownloadOutputPath
        {
            get { return _downloadOutputPath; }
            set { SetProperty(ref _downloadOutputPath, value); }
        }

        private bool _useSameOutputPath = true;
        public bool UseSameOutputPath
        {
            get { return _useSameOutputPath; }
            set
            {
                if (SetProperty(ref _useSameOutputPath, value))
                {
                    if (value && !string.IsNullOrEmpty(OutputPath))
                    {
                        DownloadOutputPath = OutputPath;
                    }
                }
            }
        }

        public ObservableCollection<DownloadTaskInfo> DownloadTasks { get; private set; }

        private int _downloadProgress;
        public int DownloadProgress
        {
            get { return _downloadProgress; }
            set { SetProperty(ref _downloadProgress, value); }
        }

        private string _downloadStatusText = "准备就绪";
        public string DownloadStatusText
        {
            get { return _downloadStatusText; }
            set { SetProperty(ref _downloadStatusText, value); }
        }

        private bool _isDownloading;
        public bool IsDownloading
        {
            get { return _isDownloading; }
            set
            {
                if (SetProperty(ref _isDownloading, value))
                {
                    ((RelayCommand)StartDownloadCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CancelDownloadCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private bool _showDownloadComplete;
        public bool ShowDownloadComplete
        {
            get { return _showDownloadComplete; }
            set { SetProperty(ref _showDownloadComplete, value); }
        }

        public ICommand BrowseInputCommand { get; private set; }
        public ICommand BrowseOutputCommand { get; private set; }
        public ICommand BrowseDownloadOutputCommand { get; private set; }
        public ICommand RemoveFileCommand { get; private set; }
        public ICommand ClearFilesCommand { get; private set; }
        public ICommand StartConversionCommand { get; private set; }
        public ICommand CancelConversionCommand { get; private set; }
        public ICommand StartDownloadCommand { get; private set; }
        public ICommand CancelDownloadCommand { get; private set; }

        public MainViewModel()
        {
            _converterService = new AudioConverterService();
            _downloadService = new NeteaseDownloadService();
            NcmFiles = new ObservableCollection<NcmFileInfo>();
            DownloadTasks = new ObservableCollection<DownloadTaskInfo>();

            BrowseInputCommand = new RelayCommand(_ => BrowseInput());
            BrowseOutputCommand = new RelayCommand(_ => BrowseOutput());
            BrowseDownloadOutputCommand = new RelayCommand(_ => BrowseDownloadOutput());
            RemoveFileCommand = new RelayCommand(param => RemoveFile(param));
            ClearFilesCommand = new RelayCommand(_ => ClearFiles(), _ => NcmFiles.Count > 0);
            StartConversionCommand = new RelayCommand(_ => StartConversionAsync(), _ => CanStartConversion());
            CancelConversionCommand = new RelayCommand(_ => CancelConversion(), _ => IsConverting);
            StartDownloadCommand = new RelayCommand(_ => StartDownloadAsync(), _ => CanStartDownload());
            CancelDownloadCommand = new RelayCommand(_ => CancelDownload(), _ => IsDownloading);

            OutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Converted");
            DownloadOutputPath = OutputPath;

            EnsureDirectoryExists(OutputPath);
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch { }
            }
        }

        private void BrowseInput()
        {
            switch (InputMode)
            {
                case InputMode.SingleFile:
                    BrowseSingleFile();
                    break;
                case InputMode.BatchFolder:
                    BrowseFolder();
                    break;
                case InputMode.MultipleFiles:
                    BrowseMultipleFiles();
                    break;
            }
        }

        private void BrowseSingleFile()
        {
            Win32OpenFileDialog dialog = new Win32OpenFileDialog
            {
                Filter = "NCM 文件|*.ncm|所有文件|*.*",
                Title = "选择 NCM 文件"
            };

            if (dialog.ShowDialog() == true)
            {
                InputPath = dialog.FileName;
                NcmFiles.Clear();

                if (_converterService.IsValidNcmFile(dialog.FileName))
                {
                    FileInfo fileInfo = new FileInfo(dialog.FileName);
                    NcmFileInfo ncmFile = new NcmFileInfo();
                    ncmFile.FileName = fileInfo.Name;
                    ncmFile.FullPath = fileInfo.FullName;
                    ncmFile.FileSize = fileInfo.Length;
                    NcmFiles.Add(ncmFile);
                }
                else
                {
                    MessageBox.Show("选择的文件不是有效的 NCM 文件。", "无效文件", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BrowseFolder()
        {
            using (FormsFolderBrowserDialog dialog = new FormsFolderBrowserDialog())
            {
                dialog.Description = "选择包含 NCM 文件的文件夹";
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == FormsDialogResult.OK)
                {
                    InputPath = dialog.SelectedPath;
                    LoadNcmFilesFromFolder(dialog.SelectedPath);
                }
            }
        }

        private void BrowseMultipleFiles()
        {
            Win32OpenFileDialog dialog = new Win32OpenFileDialog
            {
                Filter = "NCM 文件|*.ncm|所有文件|*.*",
                Title = "选择 NCM 文件",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                NcmFiles.Clear();
                List<string> validFiles = new List<string>();

                foreach (string fileName in dialog.FileNames)
                {
                    if (_converterService.IsValidNcmFile(fileName))
                    {
                        validFiles.Add(fileName);
                        FileInfo fileInfo = new FileInfo(fileName);
                        NcmFileInfo ncmFile = new NcmFileInfo();
                        ncmFile.FileName = fileInfo.Name;
                        ncmFile.FullPath = fileInfo.FullName;
                        ncmFile.FileSize = fileInfo.Length;
                        NcmFiles.Add(ncmFile);
                    }
                }

                if (validFiles.Count > 0)
                {
                    InputPath = string.Format("已选择 {0} 个文件", validFiles.Count);
                }
                else
                {
                    MessageBox.Show("未选择有效的 NCM 文件。", "无效文件", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void LoadNcmFilesFromFolder(string folderPath)
        {
            NcmFiles.Clear();
            List<NcmFileInfo> files = _converterService.ScanNcmFiles(folderPath, SearchOption.TopDirectoryOnly);

            foreach (NcmFileInfo file in files)
            {
                NcmFiles.Add(file);
            }

            ConversionStatusText = string.Format("找到 {0} 个 NCM 文件", NcmFiles.Count);
        }

        private void BrowseOutput()
        {
            using (FormsFolderBrowserDialog dialog = new FormsFolderBrowserDialog())
            {
                dialog.Description = "选择输出文件夹";
                dialog.ShowNewFolderButton = true;

                if (!string.IsNullOrEmpty(OutputPath) && Directory.Exists(OutputPath))
                {
                    dialog.SelectedPath = OutputPath;
                }

                if (dialog.ShowDialog() == FormsDialogResult.OK)
                {
                    OutputPath = dialog.SelectedPath;
                    if (UseSameOutputPath)
                    {
                        DownloadOutputPath = OutputPath;
                    }
                }
            }
        }

        private void BrowseDownloadOutput()
        {
            using (FormsFolderBrowserDialog dialog = new FormsFolderBrowserDialog())
            {
                dialog.Description = "选择下载保存文件夹";
                dialog.ShowNewFolderButton = true;

                if (!string.IsNullOrEmpty(DownloadOutputPath) && Directory.Exists(DownloadOutputPath))
                {
                    dialog.SelectedPath = DownloadOutputPath;
                }

                if (dialog.ShowDialog() == FormsDialogResult.OK)
                {
                    DownloadOutputPath = dialog.SelectedPath;
                }
            }
        }

        private void RemoveFile(object param)
        {
            if (param is NcmFileInfo fileInfo)
            {
                NcmFiles.Remove(fileInfo);
                ((RelayCommand)ClearFilesCommand).RaiseCanExecuteChanged();
            }
        }

        private void ClearFiles()
        {
            NcmFiles.Clear();
            InputPath = string.Empty;
            ((RelayCommand)ClearFilesCommand).RaiseCanExecuteChanged();
        }

        private bool CanStartConversion()
        {
            return !IsConverting && NcmFiles.Count > 0 && !string.IsNullOrEmpty(OutputPath);
        }

        private async void StartConversionAsync()
        {
            if (NcmFiles.Count == 0) return;

            EnsureDirectoryExists(OutputPath);
            _conversionCts = new CancellationTokenSource();

            IsConverting = true;
            ShowConversionComplete = false;
            ConversionProgress = 0;

            Progress<BatchConversionProgress> progress = new Progress<BatchConversionProgress>(p =>
            {
                CurrentConvertingFile = p.CurrentFile;
                ConversionProgress = p.OverallProgress;
                ConversionStatusText = string.Format("正在转换: {0} ({1}%)", p.CurrentFile, p.CurrentFileProgress);
            });

            try
            {
                List<NcmFileInfo> files = NcmFiles.ToList();
                BatchConversionResult result = await _converterService.ConvertBatchAsync(
                    files, OutputFormat, OutputPath, progress, _conversionCts.Token);

                ConversionStatusText = string.Format("转换完成: {0} 成功, {1} 失败, {2} 取消", 
                    result.CompletedFiles, result.FailedFiles, result.CancelledFiles);
                ShowConversionComplete = true;

                await Task.Delay(3000);
                ShowConversionComplete = false;
            }
            catch (OperationCanceledException)
            {
                ConversionStatusText = "转换已取消";
            }
            catch (Exception ex)
            {
                ConversionStatusText = string.Format("转换出错: {0}", ex.Message);
                MessageBox.Show(string.Format("转换过程中发生错误:\n{0}", ex.Message), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsConverting = false;
                if (_conversionCts != null)
                {
                    _conversionCts.Dispose();
                    _conversionCts = null;
                }
            }
        }

        private void CancelConversion()
        {
            if (_conversionCts != null)
            {
                _conversionCts.Cancel();
            }
            ConversionStatusText = "正在取消...";
        }

        private bool CanStartDownload()
        {
            return !IsDownloading && _downloadService.IsValidSongId(SongId) && !string.IsNullOrEmpty(DownloadOutputPath);
        }

        private async void StartDownloadAsync()
        {
            if (!_downloadService.IsValidSongId(SongId)) return;

            EnsureDirectoryExists(DownloadOutputPath);
            _downloadCts = new CancellationTokenSource();

            IsDownloading = true;
            ShowDownloadComplete = false;
            DownloadProgress = 0;

            DownloadTaskInfo taskInfo = new DownloadTaskInfo { SongId = SongId };
            DownloadTasks.Add(taskInfo);

            Progress<DownloadProgress> progress = new Progress<DownloadProgress>(p =>
            {
                DownloadProgress = p.Progress;
                DownloadStatusText = string.Format("{0} ({1}%)", p.Phase, p.Progress);
            });

            try
            {
                DownloadResult result = await _downloadService.DownloadAsync(
                    taskInfo, DownloadOutputPath, progress, _downloadCts.Token);

                if (result.Success)
                {
                    DownloadStatusText = string.Format("下载完成: {0}", result.FileName);
                    ShowDownloadComplete = true;

                    await Task.Delay(3000);
                    ShowDownloadComplete = false;
                }
                else if (result.Cancelled)
                {
                    DownloadStatusText = "下载已取消";
                }
                else
                {
                    DownloadStatusText = string.Format("下载失败: {0}", result.ErrorMessage);
                    MessageBox.Show(string.Format("下载失败:\n{0}", result.ErrorMessage), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                DownloadStatusText = string.Format("下载出错: {0}", ex.Message);
                MessageBox.Show(string.Format("下载过程中发生错误:\n{0}", ex.Message), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsDownloading = false;
                if (_downloadCts != null)
                {
                    _downloadCts.Dispose();
                    _downloadCts = null;
                }
            }
        }

        private void CancelDownload()
        {
            if (_downloadCts != null)
            {
                _downloadCts.Cancel();
            }
            DownloadStatusText = "正在取消...";
        }
    }
}
