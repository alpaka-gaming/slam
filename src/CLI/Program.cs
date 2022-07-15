using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core;
using Core.Models;
using Core.Services;
using GitHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CLI
{
    internal static class Program
    {
        internal static IConfiguration Configuration { get; private set; }
        internal static IServiceProvider Container { get; private set; }
        internal static HttpClient HttpClient { get; private set; }

        internal static Assembly Assembly = Assembly.GetExecutingAssembly();

        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        private static string Name => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product;

        private static void Initialize(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
#if DEBUG
                .AddJsonFile("appsettings.Development.json", true, true)
#endif
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            var services = new ServiceCollection();
            services.AddSingleton(Configuration);
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog();
            }).AddOptions();

            services.AddCore();

            Container = services.BuildServiceProvider();

            // Arguments
            _arguments = new Dictionary<string, string>();
            var assembly = Assembly.GetExecutingAssembly().Location;

            if (args == null || !args.Any())
                args = Environment.GetCommandLineArgs();

            foreach (var item in args.Where(m => m != assembly))
            {
                var regex = Regex.Match(item, @"^(?:\/|--|-)(.*):?(.+)?$", RegexOptions.Compiled);
                if (regex.Success)
                    _arguments.Add(regex.Groups[1].Value, regex.Groups[2].Value);
            }
        }

        private static Dictionary<string, string> _arguments;
        private static ResourceManager _resourceManager;

        public static bool ShowHelp => _arguments.ContainsKey("help");
        public static bool NoUpdate => _arguments.ContainsKey("no-update");

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            PrintHeader();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            GitHub.GitHubInfo.NewUpdate += NewVersion;

            _resourceManager = new ResourceManager("CLI.Resources.Strings", typeof(CLI.Resources.Strings).Assembly);

#if DEBUG
            System.Console.WriteLine(_resourceManager.GetString("PressAnyKey"));
            System.Console.ReadKey();
            System.Console.Clear();
#endif

            Initialize(args);

            // if (_arguments.Count < 3)
            // {
            //     throw new InvalidProgramException(_resourceManager.GetString("InvalidArguments"));
            // }

            var key = ConsoleKey.Enter;
            while (key != ConsoleKey.Escape)
            {
                try
                {
                    MainAsync(args).Start();
                }
                catch (Exception)
                {
                    //ignored
                }

                key = System.Console.ReadKey().Key;
            }
        }

        private static void PrintHeader()
        {
            Console.WriteLine($"{Name} CLI v{Version}");
        }

        private static void PrintHelp()
        {
            //TODO:
        }

        private static async Task MainAsync(string[] args)
        {
            if (ShowHelp)
            {
                PrintHelp();
                return;
            }

            if (!NoUpdate) await CheckForUpdatesAsync();

            var steamService = Container.GetService<SteamService>();
            var folders = steamService.GetLibraryFolders();
            var appIds = new List<long>();
            Configuration.Bind("AppIds", appIds);

            var appPaths = new Dictionary<long, Definition>();
            foreach (var libraryFolder in folders)
            {
                if (libraryFolder.Apps.Any(m => appIds.Contains(m.Key)))
                {
                    var apps = libraryFolder.Apps.Where(m => appIds.Contains(m.Key));
                    foreach (var app in apps)
                    {
                        var definition = steamService.GetAppDefinition(app.Key);
                        appPaths.Add(app.Key, definition);
                    }
                }
            }

        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;

            if (Log.Logger != null)
            {
                Log.Logger.Error(ex, ex.Message);
            }
            else
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static async Task CheckForUpdatesAsync()
        {
            await GitHub.GitHubInfo.CheckForUpdateAsync();
        }

        private static void NewVersion(object sender, EventArgs e)
        {
            var s_resourceManager = new ResourceManager("GitHub", typeof(GitHub.GitHubInfo).Assembly);
            var updateMessage = s_resourceManager.GetString("NewVersion");
            if (updateMessage != null)
            {
                updateMessage = updateMessage.Replace("{VERSION}", GitHub.GitHubInfo.LatestGitHubRelease.GetVersion().ToString());
                updateMessage = updateMessage.Replace("{CREATEDAT}", GitHub.GitHubInfo.LatestGitHubRelease.CreatedAt.UtcDateTime.ToShortDateString());

                Console.WriteLine(updateMessage);
                var response = Console.ReadKey();
                if (response.Key == ConsoleKey.Y)
                {
                    var assetName = GitHubInfo.AssetName;
                    if (string.IsNullOrEmpty(assetName)) assetName = $"{ApplicationInfo.Product}.zip";
                    var assetUrl = GitHub.GitHubInfo.LatestGitHubRelease.Assets.FirstOrDefault(m => m.Name == assetName);
                    var url = GitHub.GitHubInfo.LatestGitHubRelease.AssetsUrl;
                    if (assetUrl != null) url = assetUrl.BrowserDownloadUrl;
                    if (string.IsNullOrEmpty(url)) url = GitHubInfo.Repo;
                    if (!string.IsNullOrEmpty(url)) Process.Start(url);
                }
            }
        }
    }
}
