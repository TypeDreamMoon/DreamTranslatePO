using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DreamTranslatePO.Classes.GithubTools;
using DreamTranslatePO.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DreamTranslatePO.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutViewModel ViewModel
        {
            get;
        }

        public AboutPage()
        {
            ViewModel = App.GetService<AboutViewModel>();
            InitializeComponent();
        }

        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
            if (VersionTextBlock != null)
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                VersionTextBlock.Text = $"Version : {version}";
            }

            UpdateGithubVersion();
        }

        public async void UpdateGithubVersion()
        {
            if (GitVersionTextBlock != null)
            {
                GitVersionTextBlock.Text = "Checking...";
                var release = await GitHubReleaseFetcher.GetLatestReleaseAsync("TypeDreamMoon", "DreamTranslatePO");
                if (release != null)
                {
                    GitVersionTextBlock.Text = $"Last Version : {release.TagName}";
                }
                else
                {
                    GitVersionTextBlock.Text = "Last Version : Failed to get latest release";
                }
            }
        }
    }
}
