using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        internal static ServiceCollection Services { get; private set; }
        internal static ServiceProvider Container { get; private set; }

        internal static HttpClient HttpClient { get; private set; }

        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        private static string Name => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Configuration = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
#if DEBUG
                .AddJsonFile("appsettings.Development.json", true, true)
#endif
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            HttpClient = new HttpClient();

            // Initialize Logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            Services = new ServiceCollection();

            Services.AddSingleton(Configuration);
            Services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog();
            }).AddOptions();

            Container = Services.BuildServiceProvider();

            try
            {
                Log.Information("Application Starting");
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                if (IsNewInstance)
                    Application.Run(new FormMain());
                else
                    NewInstanceHandler(null, EventArgs.Empty);
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

        private static void NewInstanceHandler(object sender, EventArgs e)
        {
            NewInstance?.Invoke(sender, e);
        }

        public static event EventHandler NewInstance;

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
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
    }
}
