using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Core.Persistency
{
    public abstract class Entity { }

    public abstract class LongEntity : Entity
    {
        public virtual long Id { get; set; }

        public override bool Equals(object? obj)
        {
            if (!(obj is LongEntity other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (Id == 0 || other.Id == 0)
                return false;

            return Id == other.Id;
        }

        public static bool operator ==(LongEntity a, LongEntity b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator != (LongEntity a, LongEntity b)
            => !(a == b);

        public override int GetHashCode()
            => (GetType().ToString() + Id).GetHashCode();
    }

    public abstract class StringEntity : Entity
    {
        public virtual string Id { get; set; }

        public override bool Equals(object? obj)
        {
            var other = obj as StringEntity;

            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(other.Id))
                return false;

            return Id == other.Id;
        }

        public static bool operator ==(StringEntity a, StringEntity b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(StringEntity a, StringEntity b)
            => !(a == b);

        public override int GetHashCode()
            => (GetType().ToString() + Id).GetHashCode();
    }

    public abstract class GuidEntity : Entity
    {
        public virtual Guid Id { get; set; }

        public override bool Equals(object? obj)
        {
            var other = obj as GuidEntity;

            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return Id == other.Id;
        }

        public static bool operator ==(GuidEntity a, GuidEntity b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(GuidEntity a, GuidEntity b)
            => !(a == b);

        public override int GetHashCode()
            => (GetType().ToString() + Id).GetHashCode();
    }
}
