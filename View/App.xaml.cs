using System.Diagnostics;
using System.Windows;
using Core;

namespace View
{
    public partial class App
    {
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            new Config();
            var wnd = new VaultsWindow();
            wnd.Show();
        }

        public static void OpenWithDefaultProgram(string file)
        {
            using Process filerOpener = new Process();

            filerOpener.StartInfo.FileName = "explorer";
            filerOpener.StartInfo.Arguments = file;
            filerOpener.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Vault.Current?.Dispose();
        }
    }
}