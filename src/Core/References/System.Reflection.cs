using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Reflection
{
    public static class ApplicationInfo
    {
        public static Assembly Assembly => Assembly.GetCallingAssembly();

        public static Version Version => ApplicationInfo.Assembly.GetName().Version;
        public static string Title => ApplicationInfo.Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        public static string Product => ApplicationInfo.Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        public static string Description => ApplicationInfo.Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        public static string Copyright => ApplicationInfo.Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        public static string Company => ApplicationInfo.Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

        public static string Guid => ApplicationInfo.Assembly.GetCustomAttribute<GuidAttribute>()?.Value;

        internal static Dictionary<string, string> GetCommandLine()
        {
            var commandArgs = new Dictionary<string, string>();

            var assembly = $@"""{Assembly.GetExecutingAssembly().Location}"" ";
            var collection = Environment.CommandLine.Replace(assembly, "").Split(' ').Select(a => a.ToLower()).ToList();

            if (!collection.Any())
            {
                return commandArgs;
            }

            foreach (var item in collection.Where(m => m.StartsWith("/")))
                commandArgs.Add(item.ToLower(),
                    collection.Count - 1 > collection.IndexOf(item)
                        ? collection[collection.IndexOf(item) + 1].Replace(@"""", @"")
                        : null);

            return commandArgs;
        }
    }

}
