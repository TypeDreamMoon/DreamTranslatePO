using Newtonsoft.Json;

namespace DreamTranslatePO.Classes;

public class AppSettings
{
    public double BackgroundOpacity
    {
        get;
        set;
    }
    public string BackgroundMode
    {
        get;
        set;
    }
    public string? AiPrompt
    {
        get;
        set;
    }
    public string? URL
    {
        get;
        set;
    }

    public string? Model
    {
        get;
        set;
    }

    public string? APIKey
    {
        get;
        set;
    }

    public bool Stream
    {
        get;
        set;
    }

    public int MaxTokens
    {
        get;
        set;
    }

    public string PromptForReplacementWord
    {
        get;
        set;
    }

    public string PromptForReplacementWordContext
    {
        get;
        set;
    }

    public AppSettings()
    {
        BackgroundMode = "Mica";
        BackgroundOpacity = 0.5f;
        PromptForReplacementWord = "[PROMPT]";
        PromptForReplacementWordContext = "[CONTEXT]";
    }
}

public static class AppSettingsManager
{
    private static string _settingsFilePath;

    static AppSettingsManager()
    {
        _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "App", "settings.json");
    }


    public static AppSettings GetSettings()
    {
        if (File.Exists(_settingsFilePath))
        {
            var json = File.ReadAllText(_settingsFilePath);
            return JsonConvert.DeserializeObject<AppSettings>(json);
        }
        else
        {
            // 如果文件不存在，返回默认设置
            return new AppSettings
            {
                URL = "https://default.url",
                Model = "defaultModel",
                APIKey = "",
                Stream = true,
                MaxTokens = 512,
                PromptForReplacementWord = "[PROMPT]"
            };
        }
    }

    public static void SaveSettings(AppSettings newSettings)
    {
        var json = JsonConvert.SerializeObject(newSettings, Formatting.Indented);
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)); // Ensure the folder exists
        File.WriteAllText(_settingsFilePath, json);
    }
}