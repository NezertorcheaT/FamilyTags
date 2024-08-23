using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
    public class Vault : IDisposable, IEnumerable<MetaFile>
    {
        public static readonly byte[] TagDivisor = {15, 202, 1, 23};
        public static readonly byte[] VaultNameEnded = {150, 20, 15, 1};

        public static readonly string VaultExcludeFolder = ".vaultExcludeFolder";
        public static readonly string VaultFileType = ".vaultFile";
        public static readonly string MetaFileType = ".vmeta";
        public static Vault? Current { get; private set; }

        private int _cacheCapacity = 10;

        public int CacheCapacity
        {
            get => _cacheCapacity;
            set
            {
                if (value == _cacheCapacity)
                    return;
                ClearCache();
                _cacheCapacity = Math.Min(value, NotCachedPaths.Count());
                _stackCache = new Stack<MetaFile>(_cacheCapacity);
            }
        }

        public IEnumerable<string> NotCachedPaths { get; private set; }
        public IEnumerable<string> ExcludedFolders => _excluded;

        private List<string> _excluded;
        private string _file;
        private FileStream _stream;
        private string _name = "Vault";
        private Stack<MetaFile> _stackCache;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                UpdateFile();
            }
        }

        public TagStorage Storage { get; private set; }

        public Vault(string vaultFile)
        {
            Storage = new TagStorage();
            _file = vaultFile;
            _stackCache = new Stack<MetaFile>(CacheCapacity);
            _excluded = new List<string>();
            _stream = new FileStream(_file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            var data = new byte[_stream.Length];
            _stream.Read(data);
            var buffer = new byte[4];
            UpdateNoneCached();

            var prev = 0;
            for (var i = 3; i < data.Length; i++)
            {
                buffer[3] = data[i];
                buffer[2] = data[i - 1];
                buffer[1] = data[i - 2];
                buffer[0] = data[i - 3];

                if (buffer.SequenceEqual(VaultNameEnded.Reverse()) || buffer.SequenceEqual(VaultNameEnded))
                {
                    prev = i;
                    _name = data[..(i - 3)].ToUTF8();
                    continue;
                }

                if (buffer.SequenceEqual(TagDivisor.Reverse()) || buffer.SequenceEqual(TagDivisor))
                {
                    var id = data[(prev + 1)..(prev + 5)].ToInt();
                    var name = data[(prev + 5)..(i - 3)].ToUTF8();
                    prev = i;

                    if (Storage.Exists(id)) continue;
                    Storage.Add(new Tag
                        {
                            Id = id,
                            Name = name
                        }
                    );
                }
            }

            Storage.OnAdded += i => UpdateFile();
            Storage.OnRemoved += i => UpdateFile();
        }

        public void UpdateNoneCached()
        {
            NotCachedPaths = Directory.EnumerateFiles(_file.Replace(VaultFileType, ""), "*.*",
                new EnumerationOptions {RecurseSubdirectories = true}).Select(file =>
            {
                if (file.Contains(VaultExcludeFolder))
                    _excluded.Add(file.Replace('\\' + VaultExcludeFolder, ""));
                return file;
            }).Where(file =>
                !file.Contains(VaultFileType) && !file.Contains(MetaFileType) &&
                !ExcludedFolders.Any(file.Contains)).ToArray();
        }

        public void UpdateFile()
        {
            _stream.Position = 0;
            _stream.SetLength(0);

            _stream.Write(Name.ToBytes());
            _stream.Write(VaultNameEnded);
            foreach (var tag in Storage)
            {
                _stream.Write(tag.Id.ToBytes());
                _stream.Write(tag.Name.ToBytes());
                _stream.Write(TagDivisor);
            }

            _stream.Flush();
            Console.WriteLine("Files updated");
        }

        public static string NewVaultFile(string path)
        {
            var storage = new TagStorage();
            var p = Path.Combine(path, VaultFileType);
            var sw = new StreamWriter(p);
            var sb = sw.BaseStream;
            sb.Write("Vault".ToBytes());
            sb.Write(VaultNameEnded);

            foreach (var tag in storage)
            {
                sb.Write(tag.Id.ToBytes());
                sb.Write(tag.Name.ToBytes());
                sb.Write(TagDivisor);
            }

            sw.Flush();
            sw.Close();

            return p;
        }

        public static void OpenVault(string file)
        {
            if (!File.Exists(file) && file.Contains(VaultFileType))
                throw new FileNotFoundException($"Vault file \"{file}\" does not exist, initialize new vault first");
            if (Current is not null)
                Current.Dispose();
            Current = new Vault(file);
        }

        public static void CreateNewVault(string path)
        {
            var v = new Vault(NewVaultFile(path));
            Current = v;
        }

        private bool CheckVault() => File.Exists(_file);

        public void Dispose()
        {
            _stream.Close();
            _stream.Dispose();
            ClearCache();

            Current = null;
        }

        public string MetaPath(string file) => $"{file}{MetaFileType}";

        public bool TryGetMetaFileSilent(string file, out MetaFile? metaFile)
        {
            var m = GetMetaFileSilent(file);
            metaFile = m;
            return m is not null;
        }

        public MetaFile? GetMetaFileSilent(string file)
        {
            if (_stackCache.Select(f => f.FilePath).Contains(file))
                return _stackCache.First(f => f.FilePath == file);
            if (!File.Exists(MetaPath(file)))
                return null;
            return new MetaFile(file);
        }

        public MetaFile GetMetaFile(string file)
        {
            while (true)
            {
                if (_stackCache.Select(f => f.FilePath).Contains(file))
                    return _stackCache.First(f => f.FilePath == file);
                Check(file);
            }
        }

        public void Check(string file)
        {
            if (!File.Exists(MetaPath(file)))
                new StreamWriter(MetaPath(file)).Dispose();
            if (!_stackCache.Select(f => f.FilePath).Contains(file))
                _stackCache.Push(new MetaFile(file));
        }

        public void ClearCache()
        {
            foreach (var file in _stackCache)
            {
                file.Dispose();
            }

            _stackCache.Clear();
        }

        public void CacheFiles(int offset)
        {
            if (offset < 0) return;

            ClearCache();
            UpdateNoneCached();

            foreach (var file in NotCachedPaths.Skip(offset).Take(CacheCapacity))
            {
                if (file.Contains(VaultExcludeFolder))
                    _excluded.Add(file.Replace('\\' + VaultExcludeFolder, ""));
                if (_excluded.Any(file.Contains)) continue;
                if (file.Contains(VaultFileType) || file.Contains(MetaFileType)) continue;
                Check(file);
            }
        }

        public void UntagFile(Tag tag, string file)
        {
            var meta = GetMetaFile(file);
            meta.RemoveTag(tag);
        }

        public void TagFile(Tag tag, string file)
        {
            var meta = GetMetaFile(file);
            meta.AddTag(tag);
        }

        public IEnumerator<MetaFile> GetEnumerator() => _stackCache.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<string> Search(IEnumerable<Tag> tags)
        {
            foreach (var file in NotCachedPaths)
            {
                MetaFile meta;
                if (!TryGetMetaFileSilent(file, out meta))
                    continue;
                
                if (tags.All(tag => meta.Tags.Contains(tag)))
                    yield return file;
                
                if (!_stackCache.Contains(meta))
                    meta.Dispose();
            }
        }

        public IEnumerable<string> Search(string criteria) => NotCachedPaths.Where(file => file.Contains(criteria));

        public IEnumerable<string> Search(IEnumerable<Tag> tags, string criteria)
        {
            foreach (var file in Search(criteria))
            {
                MetaFile meta;
                if (!TryGetMetaFileSilent(file, out meta))
                    continue;

                if (tags.All(tag => meta.Tags.Contains(tag)))
                    yield return file;

                if (!_stackCache.Contains(meta))
                    meta.Dispose();
            }
        }

        public IEnumerable<string> Search(Tag tag) => Search(new[] {tag});
        public IEnumerable<string> Search(Tag tag, string criteria) => Search(new[] {tag}, criteria);
    }
}