using Windows.System;
using DreamTranslatePO.Classes;
using DreamTranslatePO.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace DreamTranslatePO.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public List<ModelPreset> ModelPresets = new List<ModelPreset>();
    public ModelPreset CurrentSelectedModelPreset = ModelPreset.Deepseek();
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        ModelPresets.AddRange( new []
        {
            ModelPreset.SiliconFlow(),
            ModelPreset.Deepseek(),
            ModelPreset.Hunyuan(),
            ModelPreset.Kimi(),
            ModelPreset.OpenAI(),
            ModelPreset.Custom()
        });
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        LoadSettings();
    }


    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        AppSettingsManager.SaveSettings(new AppSettings
        {
            ModelPreset = CurrentSelectedModelPreset.DisplayName,
            URL = UrlTextBox.Text,
            Model = ModelTextBox.Text,
            APIKey = ApiKeyPasswordBox.Password,
            MaxTokens = (int)MaxTokensSlider.Value,
            PromptForReplacementWord = PromptRepWordTextBox.Text,
            PromptForReplacementWordContext = PromptRepWordContextTextBox.Text,
            AiPrompt = AiPromptBox.Text,
            BackgroundOpacity = BackgroundOpacitySlider.Value,
            BackgroundMode = BackgroundModeComboBox.SelectedItem.ToString()
        });
    }


    private void MaxTokensSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        // 将值转换为整数并更新 TextBox 的文本
        if (MaxTokensTextBox != null)
        {
            MaxTokensTextBox.Text = MaxTokensSlider.Value.ToString();
        }
    }

    private void LoadSettings()
    {
        var settings = AppSettingsManager.GetSettings();

        UrlTextBox.Text = settings.URL;
        
        PromptRepWordTextBox.Text = settings.PromptForReplacementWord;
        PromptRepWordContextTextBox.Text = settings.PromptForReplacementWordContext;
        
        CurrentSelectedModelPreset = FindModelPreset(settings.ModelPreset);
        ModelPresetSelector.SelectedItem = CurrentSelectedModelPreset;
        ModelSelector.SelectedItem = settings.Model;
        ModelTextBox.Text = settings.Model;
        
        ApiKeyPasswordBox.Password = settings.APIKey;
        
        MaxTokensSlider.Value = settings.MaxTokens;
        MaxTokensTextBox.Text = settings.MaxTokens.ToString();
        
        BackgroundModeComboBox.SelectedItem = settings.BackgroundMode;
        BackgroundOpacitySlider.Value = settings.BackgroundOpacity;

        AiPromptBox.Text = settings.AiPrompt;

        BackgroundModeComboBox.SelectedItem = settings.BackgroundMode;
    }

    private void MaxTokensTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            if (int.TryParse(MaxTokensTextBox.Text, out int newValue))
            {
                // 确保新值在 Slider 的范围内
                newValue = Math.Clamp(newValue, (int)MaxTokensSlider.Minimum, (int)MaxTokensSlider.Maximum);
                MaxTokensSlider.Value = newValue;
            }
            else
            {
                // 输入无效，设置为默认值
                MaxTokensSlider.Value = 512;
            }
        }
    }

    private void BackgroundModeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AppSettings settings = AppSettingsManager.GetSettings();
        settings.BackgroundMode = BackgroundModeComboBox.SelectedItem.ToString();
        AppSettingsManager.SaveSettings(settings);
    }

    private void BackgroundOpacitySlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        ShellPageGlobal.Get().AppBackgroundBrush.Opacity = BackgroundOpacitySlider.Value / 100;
    }

    private void ModelPresetSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Console.WriteLine($"Current Selected Preset : {GetCurrentSelectedModelPreset().DisplayName} Is Custom : {CurrentIsCustomModel()}");
        
        CurrentSelectedModelPreset = GetCurrentSelectedModelPreset();
        
        UrlTextBox.Text = CurrentSelectedModelPreset.BaseUrl;
        if (!CurrentIsCustomModel())
        {
            ModelSelector.ItemsSource = CurrentSelectedModelPreset.Models;
            ModelSelector.SelectedItem = AppSettingsManager.GetSettings().Model;
        }
        ModelSelector.Visibility = CurrentIsCustomModel() ? Visibility.Collapsed : Visibility.Visible;
        ModelCustomStack.Visibility = CurrentIsCustomModel() ? Visibility.Visible : Visibility.Collapsed;
    }

    private ModelPreset GetCurrentSelectedModelPreset()
    {
        return ModelPresetSelector.SelectedItem as ModelPreset;
    }

    private bool ModelPresetIsEqual(ModelPreset a, ModelPreset b)
    {
        return a.DisplayName == b.DisplayName;
    }

    private ModelPreset FindModelPreset(string DisplayName)
    {
        foreach (var Elem in ModelPresets)
        {
            if (Elem.DisplayName == DisplayName)
            {
                return Elem;
            }
        }

        return null;
    }

    private bool CurrentIsCustomModel()
    {
        return CurrentSelectedModelPreset.IsEqual(ModelPreset.Custom());
    }

    private void ModelSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ModelTextBox.Text = ModelSelector.SelectedItem as string;
        Console.WriteLine($"Current Selected Model : {ModelTextBox.Text}");
    }
}   