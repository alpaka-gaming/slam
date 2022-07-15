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

        private IEnumerable<Library> _libraries;

        public IEnumerable<Library> GetLibraryFolders(bool force = false)
        {
            if (string.IsNullOrWhiteSpace(_steamInstallPath)) GetSteamInstallPath();
            if (_libraries != null && !force) return _libraries;

            var libraryFoldersFilePath = Path.Combine(_steamInstallPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersFilePath)) throw new FileNotFoundException("libraryfolders.vdf is not found");

            var content = File.ReadAllText(libraryFoldersFilePath);
            var vprop = VdfConvert.Deserialize(content);
            var jprop = vprop.ToJson();

            _libraries = jprop.First().Select(m => m.First().ToObject<Library>());

            return _libraries;
        }

        public Definition GetAppDefinition(long appId)
        {
            if (string.IsNullOrWhiteSpace(_steamInstallPath)) GetSteamInstallPath();
            if (_libraries == null) GetLibraryFolders();

            var library = _libraries.FirstOrDefault(m => m.Apps.Any(m => m.Key == appId));
            if (library == null) throw new FileNotFoundException($"library was not found");

            var appManifestFilePath = Path.Combine(library.Path, "steamapps", $"appmanifest_{appId}.acf");
            if (!File.Exists(appManifestFilePath)) throw new FileNotFoundException($"appmanifest_{appId}.acf is not found");

            var content = File.ReadAllText(appManifestFilePath);
            var vprop = VdfConvert.Deserialize(content);
            var jprop = vprop.ToJson();

            var definition = jprop.First().ToObject<Definition>();

            definition.InstallDir = Path.Combine(library.Path, "steamapps", "common", definition.InstallDir);

            if (Directory.Exists(definition.InstallDir))
                return definition;
            else
                return null;
        }
    }
}
