using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Core.Models;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace Core.Services
{
    public class SteamService
    {
        public bool IsSteamRuning()
        {
            return System.Diagnostics.Process.GetProcesses()
                .Any(m => m.ProcessName == "steam");
        }
        public bool IsSteamServiceRuning()
        {
            return System.Diagnostics.Process.GetProcesses()
                .Any(m => m.ProcessName == "steamservice");
        }
        
        private string _steamInstallPath;

        public string GetSteamInstallPath()
        {
            if (!string.IsNullOrWhiteSpace(_steamInstallPath)) return _steamInstallPath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var arch = string.Empty;
                if (Environment.Is64BitOperatingSystem) arch = @"WOW6432Node\";
                var regKey = Registry.GetValue(@$"HKEY_LOCAL_MACHINE\SOFTWARE\{arch}Valve\Steam", "InstallPath", null);
                _steamInstallPath = regKey.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
            }

            if (string.IsNullOrWhiteSpace(_steamInstallPath)) throw new FileNotFoundException("Steam is not installed");

            return _steamInstallPath;
        }

        public IEnumerable<LibraryFolder> GetLibraryFolders()
        {
            if (string.IsNullOrWhiteSpace(_steamInstallPath)) GetSteamInstallPath();

            var libraryFoldersFilePath = Path.Combine(_steamInstallPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersFilePath)) throw new FileNotFoundException("libraryfolders.vdf is not found");

            var content = File.ReadAllText(libraryFoldersFilePath);
            var vprop = VdfConvert.Deserialize(content);
            var jprop = vprop.ToJson();

            var items = jprop.First().Select(m => m.First().ToObject<LibraryFolder>());

            return items;
        }
    }
}
