using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class TagStorage : IEnumerable<Tag>
    {
        private Dictionary<int, Tag> _storage;
        private Random _random;

        public event Action<Tag> OnAdded;
        public event Action<Tag> OnRemoved;

        public TagStorage()
        {
            _random = new Random();
            _storage = new Dictionary<int, Tag>();
            _storage.Add((int) TagSpecialIds.Favorite, new Tag {Id = (int) TagSpecialIds.Favorite, Name = "Favorite"});
        }

        public void Add(Tag tag)
        {
            if (tag.Id == (int) TagSpecialIds.None)
            {
                var id = _random.Next(int.MinValue, int.MaxValue);
                while (_storage.ContainsKey(id) || id.ToBytes().Intersect(Vault.TagDivisor).Any() ||
                       id.ToBytes().Intersect(Vault.VaultNameEnded).Any())
                {
                    id = _random.Next(int.MinValue, int.MaxValue);
                }

                tag.Id = id;
            }

            _storage.Add(tag.Id, tag);
            OnAdded?.Invoke(tag);
        }

        public void Remove(int id)
        {
            if (id == (int) TagSpecialIds.Favorite) return;
            if (id == (int) TagSpecialIds.None) return;
            if (!_storage.ContainsKey(id)) return;
            OnRemoved?.Invoke(_storage[id]);
            _storage[id].Removed();
            _storage.Remove(id);
        }

        public bool Exists(int id) => _storage.ContainsKey(id);

        public IEnumerable<Tag> this[string criteria] => _storage.Values.Where(i => i.Name.Contains(criteria));
        public Tag this[int key] => _storage[key];

        public IEnumerator<Tag> GetEnumerator() => _storage.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}