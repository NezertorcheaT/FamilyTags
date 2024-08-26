using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core;
using Microsoft.WindowsAPICodePack.Shell;

namespace View
{
    public partial class MainWindow
    {
        private string _currentFile;

        private MetaFile CurrentFile
        {
            get
            {
                if (Vault.Current is null)
                    return null;
                return Vault.Current.GetMetaFile(_currentFile);
            }
            set => _currentFile = value.FilePath;
        }

        private int ThumbnailSize
        {
            get => _size;
            set => _size = Math.Max(value, -7);
        }

        private int _size;
        private int _sizeMultiplier = 10;
        private int _pageCount = 50;
        private int _currentPage;

        private Dictionary<string, BitmapSource> _thumbnailCache;
        private static readonly int ThumbnailCacheCapacity = 100;

        public MainWindow()
        {
            InitializeComponent();

            _thumbnailCache = new Dictionary<string, BitmapSource>();
            Vault.Current.CacheCapacity = _pageCount;
            Vault.Current.CacheFiles(_pageCount);

            VaultClose.Click += (s, e) =>
            {
                Vault.Current.Dispose();
                var vaultsWindow = new VaultsWindow();
                App.Current.MainWindow = vaultsWindow;
                Close();
                vaultsWindow.Show();
            };
            VaultRename.Click += (s, e) =>
            {
                var ntw = new NewTagWindow();
                ntw.Title = "Название Хранилища";
                ntw.Name.Text = Vault.Current.Name;
                ntw.Show();
                ntw.Exit.Click += (s, e) => ntw.Close();
                ntw.Ok.Click += (s, e) =>
                {
                    Vault.Current.Name = ntw.Name.Text;
                    ntw.Close();
                    UpdateStatusBar();
                };
            };
            VaultFind.Click += (s, e) =>
            {
                var ntw = new SearchWindow();
                ntw.Show();
            };
            SizeUp.Click += (s, e) =>
            {
                ThumbnailSize++;
                RebuildImages();
                UpdateStatusBar();
            };
            SizeDown.Click += (s, e) =>
            {
                ThumbnailSize--;
                RebuildImages();
                UpdateStatusBar();
            };
            RebuildTags();
            RebuildImages();
            RebuildPages();
        }

        private void SetPage(int page)
        {
            _currentPage = page;
            Vault.Current.CacheFiles(_currentPage * _pageCount);
            RebuildImages();
            UpdateStatusBar();
            RebuildPages();
        }

        private void UpdateStatusBar()
        {
            TrackPage.Content = $"Страница: {_currentPage}/{Vault.Current.NotCachedPaths.Count() / _pageCount}";
            TrackFilesCount.Content = $"Кол-во файлов: {Vault.Current.NotCachedPaths.Count()}";
            TrackThumbnailCapacity.Content = $"Макс. кеш предпросмотра: {_thumbnailCache.Count}";
            TrackVaultCacheCapacity.Content = $"Макс. кеш убежища: {Vault.Current.CacheCapacity}";
            TrackVaultName.Content = $"Имя убежища: {Vault.Current.Name}";
            TrackSize.Content = $"Увеличение: {ThumbnailSize}";
        }

        private BitmapSource CheckThumbnail(string file)
        {
            if (_thumbnailCache.ContainsKey(file))
                return _thumbnailCache[file];
            if (_thumbnailCache.Count >= ThumbnailCacheCapacity)
                _thumbnailCache.Remove(_thumbnailCache.First().Key);

            _thumbnailCache.Add(file, ShellFile.FromFilePath(file).Thumbnail.ExtraLargeBitmapSource);
            return CheckThumbnail(file);
        }

        private void RebuildTags()
        {
            Tags.Children.Clear();

            foreach (var tag in Vault.Current.Storage)
            {
                Console.WriteLine(tag);
                var tagControl = new TagControl();
                tagControl.Id.Text = tag.Id.ToString();
                tagControl.Name.Text = tag.Name;
                tagControl.Name.KeyDown += (s, e) =>
                {
                    if (e.Key != Key.Enter) return;
                    tag.Name = tagControl.Name.Text;
                    Vault.Current.UpdateFile();
                    RebuildTags();
                    RebuildFileTags(CurrentFile);
                };
                tagControl.Remove.Click += (s, e) =>
                {
                    Vault.Current.Storage.Remove(tag.Id);
                    Vault.Current.UpdateFile();
                    RebuildTags();
                };
                tagControl.Plus.Click += (s, e) =>
                {
                    var f = CurrentFile;
                    if (f is null) return;
                    f.AddTag(tag);
                    f.UpdateTags();
                    RebuildFileTags(f);
                };
                Tags.Children.Add(tagControl);
            }

            var addTagButton = new Button();
            addTagButton.Click += (s, e) =>
            {
                var ntw = new NewTagWindow();
                ntw.Show();
                ntw.Exit.Click += (s, e) => { ntw.Close(); };
                ntw.Ok.Click += (s, e) =>
                {
                    Vault.Current.Storage.Add(new Tag {Name = ntw.Name.Text});
                    ntw.Close();
                    RebuildTags();
                };
            };
            addTagButton.Height = 25;
            addTagButton.Content = "Новый тег";
            addTagButton.Margin = new Thickness(10);
            Tags.Children.Add(addTagButton);


            UpdateStatusBar();
        }

        private void RebuildImages()
        {
            Images.Children.Clear();
            foreach (var metaFile in Vault.Current)
            {
                var fullFilePath = metaFile.FilePath;
                Console.WriteLine(fullFilePath);
                var filePreview = new FilePreview();
                filePreview.MaxWidth += ThumbnailSize * _sizeMultiplier;
                filePreview.MinWidth += ThumbnailSize * _sizeMultiplier;
                filePreview.Width += ThumbnailSize * _sizeMultiplier;
                filePreview.MaxHeight += ThumbnailSize * _sizeMultiplier;
                filePreview.MinHeight += ThumbnailSize * _sizeMultiplier;
                filePreview.Height += ThumbnailSize * _sizeMultiplier;
                filePreview.B.Click += (s, e) =>
                {
                    CurrentFile = metaFile;
                    RebuildFileTags(metaFile);
                    var color = new FilePreview().BackgroundRectangle.Fill;
                    foreach (FilePreview image in Images.Children)
                    {
                        image.BackgroundRectangle.Fill = color;
                    }

                    filePreview.BackgroundRectangle.Fill = Brushes.Cyan;
                };
                filePreview.B.MouseDoubleClick += (s, e) => App.OpenWithDefaultProgram(fullFilePath);
                var shellThumb = CheckThumbnail(fullFilePath);
                filePreview.Label.Content = Path.GetFileName(fullFilePath);
                filePreview.Image.Source = shellThumb;
                filePreview.Image.Height += ThumbnailSize * _sizeMultiplier;
                filePreview.Image.Width += ThumbnailSize * _sizeMultiplier;

                Images.Children.Add(filePreview);
            }
        }

        private void RebuildPages()
        {
            Pages.Children.Clear();
            foreach (var page in
                Enumerable
                    .Range(0, Vault.Current.NotCachedPaths.Count() / _pageCount)
                    .Where((file, i) => i < _currentPage + 3 && i > _currentPage - 3)
            )
            {
                if (page == _currentPage)
                {
                    var text = new TextBox();
                    text.Text = page.ToString();
                    text.Margin = new Thickness(0, 0, 0, 0);
                    text.KeyDown += (s, e) =>
                    {
                        if (e.Key == Key.Enter && int.TryParse(text.Text, out var newPage))
                            SetPage(newPage);
                    };
                    Pages.Children.Add(text);
                    continue;
                }

                var button = new Button();
                button.Margin = new Thickness(1, 1, 1, 1);
                button.Content = $"  {page}  ";
                button.Click += (s, e) => SetPage(page);
                Pages.Children.Add(button);
            }
        }

        private void RebuildFileTags(MetaFile file)
        {
            Console.WriteLine($"Rebuild {file.FilePath}");
            FileTags.Children.Clear();
            if (file is null) return;

            foreach (var tag in file.Tags)
            {
                Console.WriteLine(tag);
                var t = new FileTagControl();
                t.Id.Text = tag.Id.ToString();
                t.Id.KeyDown += (s, e) =>
                {
                    if (e.Key != Key.Enter) return;
                    var f = CurrentFile;
                    var key = int.Parse(t.Id.Text);
                    if (!Vault.Current.Storage.Exists(key)) return;
                    f.RemoveTag(tag);
                    f.AddTag(Vault.Current.Storage[key]);
                    f.UpdateTags();
                    RebuildFileTags(f);
                };
                t.Name.Content = tag.Name;
                t.Remove.Click += (s, e) =>
                {
                    var f = CurrentFile;
                    f.RemoveTag(tag);
                    f.UpdateTags();
                    RebuildFileTags(f);
                };
                FileTags.Children.Add(t);
            }

            UpdateStatusBar();
        }
    }
}