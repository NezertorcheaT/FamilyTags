using System;
using System.Collections.Generic;
using System.IO;

namespace Core
{
    public class MetaFile : IDisposable
    {
        public string FilePath { get; }
        public string MetaPath { get; }
        public IEnumerable<Tag> Tags => tags;

        private HashSet<Tag> tags;
        private FileStream _stream;

        public MetaFile(string file)
        {
            FilePath = file;
            MetaPath = Vault.Current.MetaPath(file);

            _stream = new FileStream(MetaPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var data = new byte[_stream.Length];
            _stream.Read(data);
            var currentTags = new HashSet<Tag>((int) (_stream.Length / 4));

            for (var i = 3; i < data.Length; i += 4)
            {
                var id = data[(i - 3)..(i + 1)].ToInt();
                currentTags.Add(Vault.Current.Storage[id]);
            }

            tags = currentTags;
        }

        public void UpdateTags()
        {
            _stream.Position = 0;
            _stream.SetLength(0);
            foreach (var tag in Tags)
            {
                _stream.Write(tag.Id.ToBytes());
            }
            _stream.Flush();
        }

        public void AddTag(Tag tag)
        {
            tags.Add(tag);
            UpdateTags();
        }

        public void RemoveTag(Tag tag)
        {
            tags.Remove(tag);
            UpdateTags();
        }

        public void Dispose()
        {
            _stream.Close();
            _stream.Dispose();
        }
    }
}