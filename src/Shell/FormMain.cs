using System;
using System.Windows.Forms;
using Core.Interfaces;
using Core.Services;

namespace Shell
{
    public partial class FormMain : Form
    {
        private readonly SteamService _steamService;

        public FormMain(SteamService steamService)
        {
            InitializeComponent();
            _steamService = steamService;
        }

        protected override void OnShown(EventArgs e)
        {
            //var gamelist = _steamService.GetSteamInstallPath();
            var running = _steamService.IsSteamRuning();
            var folders = _steamService.GetLibraryFolders();
            base.OnShown(e);
        }
    }

}
