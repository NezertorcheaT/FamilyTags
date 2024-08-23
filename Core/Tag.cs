using System;

namespace Core
{
    public class Tag
    {
        public int Id = (int) TagSpecialIds.None;
        public string Name;

        public void Removed()
        {
        }

        public override string ToString() => $"{Name}({Id})";

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Tag) obj);
        }

        protected bool Equals(Tag other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }

    internal enum TagSpecialIds
    {
        None = 0,
        Favorite = 1,
    }
}