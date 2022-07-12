using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace GitHub
{

    [AttributeUsage(AttributeTargets.Assembly)]
    public class GitHubAttribute : Attribute
    {
        public GitHubAttribute()
        {
        }

        public GitHubAttribute(string owner, string repo, string assetName = "") : base()
        {
            Owner = owner;
            Repo = repo;
            AssetName = assetName;
        }

        public string Owner { get; private set; }
        public string Repo { get; private set; }
        public string AssetName { get; private set; }

        public override string ToString()
        {
            return $"https://github.com/{Owner}/{Repo}";
        }
    }

    internal static class GitHubInfo
    {

        private static readonly ResourceManager s_resourceManager = new("GitHub", typeof(GitHubInfo).Assembly);

        public static string Repo => ApplicationInfo.Assembly.GetCustomAttribute<GitHubAttribute>()?.ToString();
        public static string Owner => ApplicationInfo.Assembly.GetCustomAttribute<GitHubAttribute>()?.Owner;
        public static string Name => ApplicationInfo.Assembly.GetCustomAttribute<GitHubAttribute>()?.Repo;
        public static string AssetName => ApplicationInfo.Assembly.GetCustomAttribute<GitHubAttribute>()?.AssetName;

        public static GitHubRelease LatestGitHubRelease { get; set; }

        public static async Task<GitHubRelease> GetLatestReleaseAsync()
        {
            try
            {
                using var client = new HttpClient();
                var url = new Uri($"https://api.github.com/repos/{GitHubInfo.Owner}/{GitHubInfo.Name}/releases/latest");
                client.DefaultRequestHeaders.Add("User-Agent", ApplicationInfo.Title);
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<GitHubRelease>(await response.Content.ReadAsStringAsync()); ;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        public static async Task CheckForUpdateAsync()
        {
            try
            {
                LatestGitHubRelease = await GetLatestReleaseAsync();
                if (ApplicationInfo.Version < LatestGitHubRelease.GetVersion())
                {
                    var updateMessage = s_resourceManager.GetString("NewVersion");
                    if (updateMessage != null)
                    {
                        updateMessage = updateMessage.Replace("{VERSION}", LatestGitHubRelease.GetVersion().ToString());
                        updateMessage = updateMessage.Replace("{CREATEDAT}",
                            LatestGitHubRelease.CreatedAt.UtcDateTime.ToShortDateString());
                        if (MessageBox.Show(updateMessage, ApplicationInfo.Title, MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            var assetName = GitHubInfo.AssetName;
                            if (string.IsNullOrEmpty(assetName)) assetName = $"{ApplicationInfo.Product}.zip";
                            var assetUrl = LatestGitHubRelease.Assets.FirstOrDefault(m => m.Name == assetName);
                            var url = LatestGitHubRelease.AssetsUrl;
                            if (assetUrl != null) url = assetUrl.BrowserDownloadUrl;
                            if (string.IsNullOrEmpty(url)) url = GitHubInfo.Repo;
                            if (!string.IsNullOrEmpty(url)) Process.Start(url);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static Version GetVersion(this GitHubRelease gitHubRelease)
        {
            Version.TryParse(gitHubRelease.TagName.Replace("v", ""), out var result);
            return result;
        }
    }

    internal class GitHubRelease
    {
        public GitHubRelease()
        {
            Assets = new HashSet<GitHubAsset>();
        }

        [JsonProperty("tarball_url")]
        public string TarballUrl { get; set; }

        //[JsonProperty("author")]
        //public Author Author { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset? PublishedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("prerelease")]
        public bool Prerelease { get; set; }

        [JsonProperty("draft")]
        public bool Draft { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("target_commitish")]
        public string TargetCommitish { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }

        [JsonProperty("assets_url")]
        public string AssetsUrl { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("zipball_url")]
        public string ZipballUrl { get; set; }

        [JsonProperty("assets")]
        public ICollection<GitHubAsset> Assets { get; set; }
    }

    internal class GitHubAsset
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }

        //[JsonProperty("uploader")]
        //public Author Uploader { get; set; }
    }
}
