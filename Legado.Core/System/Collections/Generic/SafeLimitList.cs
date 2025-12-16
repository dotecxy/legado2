using System;
using System.Collections; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public sealed class SafeLimitList<T>(int limit = 150) : IList<T>
    {

        private readonly List<T> items = new();

        public T this[int index]
        {
            get
            {
                lock (items)
                {
                    return items[index];
                }
            }
            set
            {
                lock (items)
                {
                    items[index] = value;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (items)
                {
                    return items.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            lock (items)
            {
                while (items.Count >= limit)
                {
                    items.RemoveAt(0);
                }
                items.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> data)
        {
            if (data == null) return;
            lock (items)
            {
                items.AddRange(data);
            }
        }

        public void Clear()
        {
            lock (items)
            {
                items.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (items)
            {
                return items.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (items)
            {
                items.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (items)
            {
                return items.GetEnumerator();
            }
        }

        public int IndexOf(T item)
        {
            lock (items)
            {
                return items.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (items)
            {
                while (items.Count >= limit)
                {
                    items.RemoveAt(0);
                }
                items.Insert(index, item);
            }
        }

        public bool Remove(T item)
        {
            lock (items)
            {
                return items.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (items)
            {
                items.RemoveAt(index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (items)
            {
                return items.GetEnumerator();
            }
        }

        public void ForEach(Action<T> act)
        {
            if (act == null) return;
            lock (items)
            {
                items.ForEach(act);
            }
        }


        public IReadOnlyList<T> GetReadOnlyList()
        {
            lock (items)
            {
                return items.AsReadOnly();
            }
        }
    }
}
