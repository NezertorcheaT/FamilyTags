using System.Collections.Generic;
using System.Windows.Controls;
using Core;

namespace View
{
    public partial class SearchWindow
    {
        public SearchWindow()
        {
            InitializeComponent();
            foreach (var tag in Vault.Current.Storage)
            {
                var tagContent = new SearchTag();
                tagContent.Id.Content = tag.Id;
                tagContent.Name.Content = tag.Name;
                Tags.Children.Add(tagContent);
            }

            StartSearch.Click += (s, e) => Search();
        }

        private void Search()
        {
            var tags = new List<Tag>();
            IEnumerable<string> ret;
            foreach (SearchTag tagElement in Tags.Children)
            {
                if (tagElement.TagEnabled.IsChecked != null && tagElement.TagEnabled.IsChecked.Value)
                    tags.Add(Vault.Current.Storage[int.Parse(tagElement.Id.Content.ToString())]);
            }

            if (string.IsNullOrWhiteSpace(SearchText.Text))
                ret = Vault.Current.Search(tags);
            else if (tags.Count == 0)
                ret = Vault.Current.Search(SearchText.Text);
            else
                ret = Vault.Current.Search(tags, SearchText.Text);

            Files.Children.Clear();
            foreach (var file in ret)
            {
                var l = new Label();
                l.Content = file;
                l.MouseDoubleClick += (s, e) => App.OpenWithDefaultProgram(file);
                Files.Children.Add(l);
            }
        }
    }
}