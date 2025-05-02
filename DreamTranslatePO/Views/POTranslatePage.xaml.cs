using System.Text;
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.System;
using Dream.AI;
using WinRT.Interop;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DreamTranslatePO.ViewModels;
using DreamTranslatePO.Classes.PoParser;
using DreamTranslatePO.Classes;
using DreamTranslatePO.Contracts.Services;
using DreamTranslatePO.ControlPages;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications.Builder;
using AiTools = Dream.AI.Tools;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace DreamTranslatePO.Views;

public sealed partial class POTranslatePage : Page
{
    private readonly DispatcherQueue _dispatcher;
    private PoFile _poFile;
    private static readonly AiClient _aiClient = new AiClient();
    private bool isPausedTranslate = false;
    private bool isStopTranslate = false;
    private readonly object pauseLock = new object();

    private void TogglePause()
    {
        lock (pauseLock)
        {
            isPausedTranslate = !isPausedTranslate;
        }

        PauseTranslatePoButton.Content = isPausedTranslate ? "继续" : "暂停";
        ValueProgressBar.ShowPaused = isPausedTranslate;
        StateProgressBar.ShowPaused = isPausedTranslate;
    }

    public ObservableCollection<PoEntry> PoEntries
    {
        get;
    } = new();

    public POTranslateViewModel ViewModel
    {
        get;
    }

    public POTranslatePage()
    {
        ViewModel = App.GetService<POTranslateViewModel>();
        InitializeComponent();
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _poFile = new();
        _aiClient.InitializeClient(AppSettingsManager.GetSettings().APIKey, AppSettingsManager.GetSettings().URL);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _poFile.Entries.Clear();
        foreach (var elem in PoEntries)
        {
            _poFile.Entries.Add(elem);
        }

        if (_poFile.Entries.Count != 0)
        {
            AppGlobalCacheData.Get()._data.cachedPoFile = _poFile;
            Console.WriteLine($"缓存 {_poFile.Entries.Count} 条记录");
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // Hidden Progress Bar
        StateProgressBar.Visibility = Visibility.Collapsed;
        ValueProgressBar.Visibility = Visibility.Collapsed;
        ValueProgressTextBox.Visibility = Visibility.Collapsed;
        StopTranslatePoButton.Visibility = Visibility.Collapsed;
        StopTranslatePoButton.IsEnabled = false;
        PauseTranslatePoButton.Visibility = Visibility.Collapsed;
        PauseTranslatePoButton.IsEnabled = false;
        TranslatePoButton.IsEnabled = false;

        AiPromptBox.PlaceholderText = $"在此输入提示词... \"{AppSettingsManager.GetSettings().PromptForReplacementWord}\"为要替换的文本.";
        AiPromptBox.Text = AppSettingsManager.GetSettings().AiPrompt;

        if (AppGlobalCacheData.Get()._data.cachedPoFile != null && AppGlobalCacheData.Get()._data.cachedPoFile.IsValid())
        {
            Console.WriteLine("从缓存中加载 PO 文件");
            _poFile.OpenFile(AppGlobalCacheData.Get()._data.cachedPoFile);
            Console.WriteLine($"{_poFile.Entries.Count} 条记录");
            UpdateListView();
        }
        
        // Check Model Is Valid
        if (AppSettingsManager.GetSettings().APIKey.Length == 0)
        {
            TranslatePoButton.IsEnabled = false;
            ButtonTranslate.IsEnabled = false;
            TranslatePoButton.Content = "请先输入 API Key 才能进行翻译";
            ButtonTranslate.Content = "请先输入 API Key 才能进行翻译";
        }
    }

    private async void OpenPoButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));
        picker.FileTypeFilter.Add(".po");

        var file = await picker.PickSingleFileAsync();
        if (file == null) return;

        await _poFile.OpenFile(file);

        UpdateListView();
    }

    private void UpdateListView()
    {
        _dispatcher.TryEnqueue(() =>
        {
            InformationTextBlock.Text = _poFile.BuildPoFileInformationString();
            PoEntries.Clear();
            foreach (var entry in _poFile.Entries)
                PoEntries.Add(entry);
            ResultText.Text = $"打开成功. 全部 {PoEntries.Count} 条记录";
            if (PoEntries.Count != 0)
            {
                TranslatePoButton.IsEnabled = true;
            }
        });
    }


    private async void ExportPoButton_Click(object sender, RoutedEventArgs e)
    {
        if (PoEntries.Count == 0)
        {
            ResultText.Text = "没有可导出的 PO 文件内容";
            return;
        }

        var file = await _poFile.ExportFile();
        if (file == null)
        {
            ResultText.Text = "导出失败";
            return;
        }

        ResultText.Text = $"导出成功. {file.Name}";
    }

    private async Task WaitForResume()
    {
        while (isPausedTranslate)
        {
            if (isStopTranslate)
            {
                break;
            }

            await Task.Delay(100);
        }
    }

    private string GetPoEntryTranslateString(PoEntry inEntry)
    {
        var repStr = AppSettingsManager.GetSettings().PromptForReplacementWord;
        var str = AiPromptBox.Text;
        if (repStr.Length != 0)
        {
            str = str.Replace(AppSettingsManager.GetSettings().PromptForReplacementWord, inEntry.MsgId);
        }

        var repStrContext = AppSettingsManager.GetSettings().PromptForReplacementWordContext;
        if (repStrContext.Length != 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Tips PO FILE INFORMATION :");
            sb.Append(_poFile.BuildPoFileInformationString());
            sb.AppendLine($"Context = {inEntry.MsgCtxt}");
            sb.AppendLine($"MsgId = {inEntry.MsgId}");
            sb.AppendLine($"MsgStr = {inEntry.MsgStr}");
            sb.AppendLine($"Key = {inEntry.Key}");
            sb.AppendLine($"SoruceLocation = {ArrayToString(inEntry.SourceLocations)}");
            sb.AppendLine($"Source = {ArrayToString(inEntry.Source)}");

            str = str.Replace(repStrContext, sb.ToString());
        }

        return str;
    }

    private async void TranslatePoButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Console.WriteLine($"=================== Start Translate - Time : {DateTime.Now} ======================");

            int startEntryIndex = 0;
            int endEntryIndex = PoEntries.Count;

            ContentDialog dialog = new ContentDialog();
            EntrySelect entrySelect = new EntrySelect();

            entrySelect.SetSliderRange(startEntryIndex, endEntryIndex);

            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "选择范围";
            dialog.PrimaryButtonText = "确定";
            dialog.CloseButtonText = "取消";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = entrySelect;

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            startEntryIndex = entrySelect.GetSliderRangeMinimum();
            endEntryIndex = entrySelect.GetSliderRangeMaximum();

            SetTranslateWidgetState(true);
            isStopTranslate = false;

            for (int i = startEntryIndex; i < endEntryIndex; i++)
            {
                if (isStopTranslate) break;

                await WaitForResume(); // 等待恢复

                PoEntry entry = PoEntries[i];
                var translateStr = GetPoEntryTranslateString(entry);
                Console.WriteLine($"==> Start Translate : {entry.MsgStr} Send Str : \n[ {translateStr} ]");

                // 添加异常处理到具体的异步操作
                try
                {
                    var translatedText = await Translate(translateStr);
                    PoEntries[i].MsgStr = PoTools.RemoveBlankLines(translatedText);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"翻译失败: {ex.Message}");
                    // 可选：标记条目为未翻译或记录错误
                }

                // 更新 UI 状态
                ValueProgressBar.Value = (int)((i + 1 - startEntryIndex) * 100 / (endEntryIndex - startEntryIndex));
                ValueProgressTextBox.Text = $"{i + 1 - startEntryIndex} / {(endEntryIndex - startEntryIndex)}";

                Console.WriteLine($"<== End Translate   : {entry.MsgStr}");
            }

            SetTranslateWidgetState(false);

            var notification = isStopTranslate
                ? "翻译任务取消"
                : "翻译任务完成";
            AppNotificationBuilder builder = new AppNotificationBuilder();
            builder.AddText(notification);
            App.GetService<IAppNotificationService>().Show(builder);

            Console.WriteLine($"=================== {(isStopTranslate ? "Cancel" : "End")} Translate - Time : {DateTime.Now} ======================");
        }
        catch (Exception ex)
        {
            // 记录全局异常
            Console.WriteLine($"全局异常: {ex}");
            AppNotificationBuilder builder = new AppNotificationBuilder();
            builder.AddText($"翻译任务失败: {ex.Message}");
            App.GetService<IAppNotificationService>().Show(builder);
            SetTranslateWidgetState(false);
        }
    }

    private void SetTranslateWidgetState(bool isEnable)
    {
        _dispatcher.TryEnqueue(() =>
        {
            PauseTranslatePoButton.IsEnabled = isEnable;
            PauseTranslatePoButton.Visibility = isEnable ? Visibility.Visible : Visibility.Collapsed;
            StopTranslatePoButton.IsEnabled = isEnable;
            StopTranslatePoButton.Visibility = isEnable ? Visibility.Visible : Visibility.Collapsed;

            ValueProgressBar.Visibility = isEnable ? Visibility.Visible : Visibility.Collapsed;
            ValueProgressTextBox.Visibility = isEnable ? Visibility.Visible : Visibility.Collapsed;
            StateProgressBar.Visibility = isEnable ? Visibility.Visible : Visibility.Collapsed;
        });
    }

    public async Task<string> Translate(string InStr)
    {
        _aiClient.InitializeRequestBody()
            .SetModel(AppSettingsManager.GetSettings().Model)
            .AddMessage(new RequestMessage(ERole.User, InStr))
            .SetStream(false)
            .SetMaxTokens(AppSettingsManager.GetSettings().MaxTokens)
            .SetTemperature(0.7)
            .SetTopP(0.7)
            .SetTopK(50)
            .SetFrequencyPenalty(0.5)
            .SetN(1)
            .SetResponseFormat(new RequestFormat());

        return AiTools.GetContentFromJsonString(await _aiClient.Send());
    }

    public string ArrayToString(List<string> strList)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var elem in strList)
        {
            sb.AppendLine(elem);
        }

        return sb.ToString();
    }

    private void EntriesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var entry = EntriesListView.SelectedItem as PoEntry;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Context : {entry.MsgCtxt}");
        sb.AppendLine($"Msgid : {entry.MsgId}");
        sb.AppendLine($"MsgStr : {entry.MsgStr}");
        sb.AppendLine($"键 : {ArrayToString(entry.Key)}");
        sb.AppendLine($"源位置 : {ArrayToString(entry.SourceLocations)}");
        sb.AppendLine($"源 : {ArrayToString(entry.Source)}");
        EntryInformation.Text = sb.ToString();
        Console.WriteLine(sb.ToString());
    }

    private async void ButtonTranslate_OnClick(object sender, RoutedEventArgs e)
    {
        StateProgressBar.Visibility = Visibility.Visible;
        var entry = EntriesListView.SelectedItem as PoEntry;

        string prompt = AiPromptBox.Text;
        prompt = prompt.Replace(AppSettingsManager.GetSettings().PromptForReplacementWord, entry.MsgStr);
        Console.WriteLine($"Start Translate Line : {prompt}");

        // 等待翻译完成
        var translatedText = await Translate(prompt);

        // 更新 UI 线程上的 PoEntries
        PoEntries[EntriesListView.SelectedIndex].MsgStr = PoTools.RemoveBlankLines(translatedText);


        StateProgressBar.Visibility = Visibility.Collapsed;
        Console.WriteLine("Done.");
    }


    private void AiPromptBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (AiPromptBox != null && AiPromptBox.Text.Length != 0)
        {
            AppSettings Settings = AppSettingsManager.GetSettings();
            Settings.AiPrompt = AiPromptBox.Text;
            AppSettingsManager.SaveSettings(Settings);
        }
    }

    private void PauseTranslatePoButton_OnClick(object sender, RoutedEventArgs e)
    {
        TogglePause();
    }

    private async void StopTranslatePoButton_OnClick(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "提示",
            Content = "确定要取消翻译吗？",
            CloseButtonText = "取消",
            PrimaryButtonText = "确定",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Set the flag to true for cancellation
            isStopTranslate = true;
        }
    }
}