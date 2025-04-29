using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DreamTranslatePO.Classes
{
    namespace GithubTools
    {
        class GitHubReleaseFetcher
        {
            private static readonly HttpClient client = new HttpClient();

            public static async Task<GitHubRelease> GetLatestReleaseAsync(string owner, string repo)
            {
                try
                {
                    string url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
                    
                    Console.WriteLine($"url : {url}");
                    
                    client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    GitHubRelease release = JsonConvert.DeserializeObject<GitHubRelease>(jsonResponse);

                    Console.WriteLine($"Latest Release Info:");
                    Console.WriteLine($"Tag Name: {release.TagName}");
                    Console.WriteLine($"Name: {release.Name}");
                    Console.WriteLine($"Description: {release.Body}");
                    Console.WriteLine($"Published At: {release.PublishedAt}");

                    foreach (var asset in release.Assets)
                    {
                        Console.WriteLine($"Asset Name: {asset.Name}");
                        Console.WriteLine($"Download URL: {asset.BrowserDownloadUrl}");
                    }
                    
                    return release;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error: {e.Message}");
                    return null;
                }
            }
        }

        // 类表示 GitHub 发布版本的结构
        public class GitHubRelease
        {
            [JsonProperty("tag_name")]
            public string TagName
            {
                get;
                set;
            }

            [JsonProperty("name")]
            public string Name
            {
                get;
                set;
            }

            [JsonProperty("body")]
            public string Body
            {
                get;
                set;
            }

            [JsonProperty("published_at")]
            public DateTime PublishedAt
            {
                get;
                set;
            }

            [JsonProperty("assets")]
            public List<ReleaseAsset> Assets
            {
                get;
                set;
            }
        }

        // 类表示发布版本的附件资产
        public class ReleaseAsset
        {
            [JsonProperty("name")]
            public string Name
            {
                get;
                set;
            }

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl
            {
                get;
                set;
            }
        }
    }
}