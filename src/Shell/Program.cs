using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Shell
{
    internal static class Program
    {
        internal static bool IsNewInstance;
        internal static Mutex Mutex = new(true, ApplicationInfo.Guid, out IsNewInstance);
        internal static Assembly Assembly = Assembly.GetExecutingAssembly();

        internal static IConfiguration Configuration { get; private set; }
        internal static IServiceProvider Container { get; private set; }
        internal static HttpClient HttpClient { get; private set; }

        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        private static string Name => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            NewInstance += OnNewInstance;
            
            if (IsNewInstance)
            {
                // Configurations
                Configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
#if DEBUG
                    .AddJsonFile("appsettings.Development.json", true, true)
#endif
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

                // HttpClient
                HttpClient = new HttpClient();

                // Initialize Logger
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(Configuration)
                    .CreateLogger();

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Container = CreateHostBuilder().Build().Services;

                try
                {
                    Log.Information("Application Starting");
                    // To customize application configuration such as set high DPI settings or default font,
                    // see https://aka.ms/applicationconfiguration.
                    ApplicationConfiguration.Initialize();
                    Application.Run(Container.GetRequiredService<FormMain>());
                }
                catch (Exception e)
                {
                    Log.Fatal(e, "The Application failed to start");
                    throw;
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
            else
            {
                NewInstanceHandler(null, EventArgs.Empty);
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
            {
                services.AddSingleton(HttpClient);
                services.AddSingleton(Configuration);
                services.AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddSerilog();
                }).AddOptions();

                services.AddCore();
                services.AddTransient<FormMain>();
            });

            return builder;
        }

        private static void NewInstanceHandler(object sender, EventArgs e)
        {
            NewInstance?.Invoke(sender, e);
        }

        public static event EventHandler NewInstance;

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

        private static void OnNewInstance(object sender, EventArgs e)
        {
            //TODO:
        }
    }
}
