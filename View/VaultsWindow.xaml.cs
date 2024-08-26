using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Core;
using Microsoft.Win32;

namespace View
{
    public partial class VaultsWindow
    {
        private string _currentVault;
        private string _selectedVault;

        private void Open(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!File.Exists(path))
            {
                Config.Instance.ForgetPath(path);
                return;
            }

            Paths.Children.Clear();
            _currentVault = path;
            Config.Instance.LastPath = _currentVault;
            Config.Instance.RememberPath(_currentVault);
            Vault.OpenVault(_currentVault);
            var mw = new MainWindow();
            App.Current.MainWindow = mw;
            Close();
            mw.Show();
        }

        public VaultsWindow()
        {
            InitializeComponent();
            _selectedVault = Config.Instance.LastPath;
            _currentVault = "";

            OpenVault.Click += (s, e) =>
            {
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.DefaultExt = Vault.VaultFileType;
                dialog.Filter = $"Vault Files (*{Vault.VaultFileType})|*{Vault.VaultFileType}";
                dialog.Title = "Открыть убежище";

                var result = dialog.ShowDialog();
                if (result is null || result is false) return;

                Open(dialog.FileName);
            };
            NewVault.Click += (s, e) =>
            {
                OpenFolderDialog dialog = new OpenFolderDialog();
                dialog.Title = "Новое убежище";

                var result = dialog.ShowDialog();
                if (result is null || result is false) return;

                Vault.NewVaultFile(dialog.FolderName);
                Open(Path.Combine(dialog.FolderName, Vault.VaultFileType));
            };
            Forget.Click += (s, e) =>
            {
                Config.Instance.ForgetPath(_selectedVault);
                Rebuild();
            };
            RecentVault.Click += (s, e) =>
            {
                if (!Config.Instance.Paths.Contains(Config.Instance.LastPath))
                    Config.Instance.RememberPath(Config.Instance.LastPath);
                Open(Config.Instance.LastPath);
            };
            Rebuild();
        }

        private void Rebuild()
        {
            Paths.Children.Clear();
            foreach (var path in Config.Instance.Paths.Reverse())
            {
                var label = new Label();
                label.Content = path;
                label.MouseLeftButtonDown += (s, e) =>
                {
                    _selectedVault = path;
                    Rebuild();
                };
                label.MouseDoubleClick += (s, e) => Open(path);
                if (path == _selectedVault)
                    label.Background = Brushes.Cyan;
                Paths.Children.Add(label);
            }
        }
    }
}