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
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Tools = DreamTranslatePO.Classes.Tools;
using ATools = Dream.AI.Tools;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace DreamTranslatePO.Views;

public sealed partial class POTranslatePage : Page
{
    private readonly DispatcherQueue _dispatcher;
    private PoFile _poFile;
    private static readonly AiClient _aiClient = new AiClient();

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
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // Hidden Progress Bar
        StateProgressBar.Visibility = Visibility.Collapsed;
        AiPromptBox.PlaceholderText = $"在此输入提示词... \"{AppSettingsManager.GetSettings().PromptForReplacementWord}\"为要替换的文本.";
        AiPromptBox.Text = AppSettingsManager.GetSettings().AiPrompt;

        if (AppGlobalCacheData.Get()._data.cachedPoFile != null)
        {
            OpenFile(AppGlobalCacheData.Get()._data.cachedPoFile);
        }
    }

    private async void OpenPoButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));
        picker.FileTypeFilter.Add(".po");

        var file = await picker.PickSingleFileAsync();
        if (file == null) return;

        OpenFile(file);
    }

    private async void OpenFile(StorageFile inFile)
    {
        var content = await FileIO.ReadTextAsync(inFile);
        OpenFile(PoFileReader.ParseContent(content));
    }

    private void OpenFile(PoFile inFile)
    {
        _poFile = inFile;

        foreach (var entry in _poFile.Entries)
            PoEntries.Add(entry);

        var sb = new StringBuilder();

        foreach (var elem in _poFile.Comments)
        {
            sb.AppendLine($"{elem}");
        }

        sb.AppendLine($"Project-Id-Version: {_poFile.Header.ProjectIdVersion}");
        sb.AppendLine($"POT-Creation-Date: {_poFile.Header.PotCreationDate}");
        sb.AppendLine($"PO-Revision-Date: {_poFile.Header.PoRevisionDate}");
        sb.AppendLine($"Language-Team: {_poFile.Header.LanguageTeam}");
        sb.AppendLine($"Language: {_poFile.Header.Language}");
        sb.AppendLine($"MIME-Version: {_poFile.Header.MimeVersion}");
        sb.AppendLine($"Content-Type: {_poFile.Header.ContentType}");
        sb.AppendLine($"Content-Transfer-Encoding: {_poFile.Header.ContentTransferEncoding}");
        sb.AppendLine($"Plural-Forms: {_poFile.Header.PluralForms}");

        _dispatcher.TryEnqueue(() =>
        {
            InformationTextBlock.Text = sb.ToString();
            PoEntries.Clear();
            foreach (var entry in _poFile.Entries)
                PoEntries.Add(entry);
            ResultText.Text = $"打开成功. 全部 {PoEntries.Count} 条记录";
        });
    }

    private async void ExportPoButton_Click(object sender, RoutedEventArgs e)
    {
        if (PoEntries.Count == 0)
        {
            ResultText.Text = "没有可导出的 PO 文件内容";
            return;
        }

        // 选择保存位置
        var picker = new FileSavePicker();
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));
        picker.FileTypeChoices.Add("PO Files", new List<string> { ".po" });
        picker.SuggestedFileName = "translated.po";

        // 保存文件
        var file = await picker.PickSaveFileAsync();
        if (file == null) return;

        try
        {
            var poContent = Tools.BuildPoContent(_poFile);
            await FileIO.WriteTextAsync(file, poContent, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            ResultText.Text = $"成功导出至: {file.Path}";
        }
        catch (Exception ex)
        {
            ResultText.Text = $"导出失败: {ex.Message}";
        }
    }

    private async void TranslatePoButton_OnClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine($"=================== Start Translate - Time : {DateTime.Now.ToString()} ======================");

        StateProgressBar.Visibility = Visibility.Visible;
        ValueProgressBar.Visibility = Visibility.Visible;
        ValueProgressBar.Value = 0;

        for (int i = 0; i < PoEntries.Count; i++)
        {
            PoEntry entry = PoEntries[i];
            string str = AiPromptBox.Text.Replace(AppSettingsManager.GetSettings().PromptForReplacementWord, entry.MsgId);
            Console.WriteLine($">>>>> Start Translate : {entry.MsgStr} Send Str : {str}");

            // 异步调用翻译并获取结果
            var translatedText = await Translate(str);

            // 更新 PoEntry 中的 MsgStr
            PoEntries[i].MsgStr = Tools.RemoveBlankLines(translatedText);

            ValueProgressBar.Value = (i + 1) * 100 / _poFile.Entries.Count;

            Console.WriteLine($"<<<<<< End Translate : {entry.MsgStr}");
        }

        StateProgressBar.Visibility = Visibility.Collapsed;
        ValueProgressBar.Visibility = Visibility.Collapsed;

        Console.WriteLine($"=================== End Translate - Time : {DateTime.Now.ToString()} ======================");
    }

    public async Task<string> Translate(string InStr)
    {
        _aiClient.InitializeRequestBody()
            .SetModel(AppSettingsManager.GetSettings().Model)
            .AddMessage(new RequestMessage(ERole.User, InStr))
            .SetStream(AppSettingsManager.GetSettings().Stream)
            .SetMaxTokens(AppSettingsManager.GetSettings().MaxTokens)
            .SetTemperature(0.7)
            .SetTopP(0.7)
            .SetTopK(50)
            .SetFrequencyPenalty(0.5)
            .SetN(1)
            .SetResponseFormat(new RequestFormat());

        return ATools.GetContentFromJsonString(await _aiClient.Send());
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
        PoEntries[EntriesListView.SelectedIndex].MsgStr = Tools.RemoveBlankLines(translatedText);


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
}