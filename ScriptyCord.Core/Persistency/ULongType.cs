using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Core.Persistency
{
    public class ULongType : IUserType
    {
        public SqlType[] SqlTypes
        {
            get { return new[] { new SqlType(System.Data.DbType.String) }; }
        }

        public Type ReturnedType 
        {
            get { return typeof(ulong); }
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public object Assemble(object cached, object owner)
            => cached;

        public object DeepCopy(object value)
        {
            if (value == null)
                return null;
            if (value is not string)
                return ((ulong)value).ToString();
            else
                return value;
        }

        public object Disassemble(object value)
            => value;

        public new bool Equals(object x, object y)
            => x == null ? false : x.Equals(y);

        public int GetHashCode(object x)
        {
            if (x == null)
                return 0;
            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            object obj = NHibernateUtil.String.NullSafeGet(rs, names, session);
                //.UInt32.NullSafeGet(rs, names0);

            if (obj == null)
                return null;

            return UInt64.Parse((string)obj);
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (value == null)
            {
                NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
                return;
            }

            if (value is not string)
                NHibernateUtil.String.NullSafeSet(cmd, ((ulong)value).ToString(), index, session);
            else
                NHibernateUtil.String.NullSafeSet(cmd, value, index, session);
        }

        public object Replace(object original, object target, object owner)
            => original;
    }
}
