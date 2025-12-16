using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections.ObjectModel
{
    [ComVisible(false)]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class QImmutableCollection<T> :
      IList<T>,
      ICollection<T>,
      IEnumerable<T>,
      IEnumerable,
      IList,
      ICollection,
      IReadOnlyList<T>,
      IReadOnlyCollection<T>
    {
        private ImmutableList<T> items;
        [NonSerialized]
        private object _syncRoot;


        public QImmutableCollection()
        {
            this.items = ImmutableList.Create<T>();
        }

        public int Count
        {

            get => this.items.Count;
        }


        protected IList<T> Items
        {

            get => this.items;
        }


        public T this[int index]
        {

            get => this.items[index];

            set
            {
                if (index < 0 || index >= this.items.Count)
                    throw new ArgumentOutOfRangeException($"index:{index}");
                this.SetItem(index, value);
            }
        }


        public virtual void Add(T item)
        {
            this.InsertItem(this.items.Count, item);
        }


        public void Clear()
        {
            this.ClearItems();
        }


        public void CopyTo(T[] array, int index) => this.items.CopyTo(array, index);


        public bool Contains(T item) => this.items.Contains(item);


        public IEnumerator<T> GetEnumerator() => this.items.GetEnumerator();


        public int IndexOf(T item) => this.items.IndexOf(item);


        public void Insert(int index, T item)
        {
            if (index < 0 || index > this.items.Count)
                throw new ArgumentOutOfRangeException($"index:{index}");
            this.InsertItem(index, item);
        }


        public bool Remove(T item)
        {
            int index = this.items.IndexOf(item);
            if (index < 0)
                return false;
            this.RemoveItem(index);
            return true;
        }


        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.items.Count)
                throw new ArgumentOutOfRangeException($"index:{index}");
            this.RemoveItem(index);
        }


        protected virtual void ClearItems()
        {
            ImmutableInterlocked.Update(ref items, list => list.Clear());
            //this.items = this.items.Clear();
        }


        protected virtual void InsertItem(int index, T item)
        {

            ImmutableInterlocked.Update(ref items, list =>
            {
                if (index < 0)
                {
                    return list.Insert(0, item);
                }
                else if (index > list.Count)
                {
                    return list.Insert(items.Count, item);
                }
                else
                {
                    return list.Insert(index, item);
                }
            });

            //    if (index >= 0 && index < Count)
            //    {
            //        this.items = this.items.Insert(index, item);
            //    }
            //    else if (index < 0)
            //    {
            //        Trace.Write($"type:{typeof(T).Name}, index:{index}, items.Count:{items.Count}");
            //        this.items = this.items.Insert(0, item);

            //    }
            //    else
            //    {
            //        Trace.Write($"type:{typeof(T).Name}, index:{index}, items.Count:{items.Count}");
            //        this.items = this.items.Insert(items.Count, item);
            //    }
        }


        protected virtual void RemoveItem(int index)
        {
            ImmutableInterlocked.Update(ref items, (list) =>
            {
                if (index >= 0 && index < list.Count)
                {
                    return list.RemoveAt(index);
                }
                return list;
            });
            //this.items = items.RemoveAt(index);
        }


        protected virtual void SetItem(int index, T item)
        {
            ImmutableInterlocked.Update(ref items, (list) =>
            {
                return list.SetItem(index, item);
            });
            //this.items = this.items.SetItem(index, item);
        }


        bool ICollection<T>.IsReadOnly
        {

            get => false;
        }


        IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();


        bool ICollection.IsSynchronized
        {

            get => false;
        }


        object ICollection.SyncRoot
        {

            get
            {
                if (this._syncRoot == null)
                {
                    if (this.items is ICollection items)
                        this._syncRoot = items.SyncRoot;
                    else
                        Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), (object)null);
                }
                return this._syncRoot;
            }
        }


        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                return;
            if (array.Rank != 1)
                return;
            if (array.GetLowerBound(0) != 0)
                return;
            if (index < 0)
                return;
            if (array.Length - index < this.Count)
                return;
            if (array is T[] array1)
            {
                this.items.CopyTo(array1, index);
            }
            else
            {
                Type elementType = array.GetType().GetElementType();
                Type c = typeof(T);
                if (!elementType.IsAssignableFrom(c) && !c.IsAssignableFrom(elementType))
                    return;
                if (!(array is object[] objArray))
                    return;
                int count = this.items.Count;
                try
                {
                    for (int index1 = 0; index1 < count; ++index1)
                    {
                        int index2 = index++;
                        // ISSUE: variable of a boxed type
                        var local = (object)this.items[index1];
                        objArray[index2] = (object)local;
                    }
                }
                catch (ArrayTypeMismatchException ex)
                {
                    throw ex;
                }
            }
        }


        object IList.this[int index]
        {

            get
            {
                object obj = this.items[index];
                return obj;
            }
            set
            {
                CheckThrow(value);
                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException ex)
                {
                    throw ex;
                }
            }
        }

        void CheckThrow(object value)
        {
            if (value != null || (object)default(T) == null)
                return;
            throw new ArgumentNullException("value");
        }

        bool IList.IsReadOnly
        {

            get => false;
        }


        bool IList.IsFixedSize
        {

            get => true;
        }


        int IList.Add(object value)
        {
            try
            {
                this.Add((T)value);
            }
            catch (InvalidCastException ex)
            {
                throw ex;
            }
            return this.Count - 1;
        }


        bool IList.Contains(object value) => QImmutableCollection<T>.IsCompatibleObject(value) && this.Contains((T)value);


        int IList.IndexOf(object value) => QImmutableCollection<T>.IsCompatibleObject(value) ? this.IndexOf((T)value) : -1;


        void IList.Insert(int index, object value)
        {
            try
            {
                this.Insert(index, (T)value);
            }
            catch (InvalidCastException ex)
            {
                throw ex;
            }
        }


        void IList.Remove(object value)
        {
            if (!QImmutableCollection<T>.IsCompatibleObject(value))
                return;
            this.Remove((T)value);
        }

        private static bool IsCompatibleObject(object value)
        {
            if (value is T)
                return true;
            return value == null && (object)default(T) == null;
        }


        public void ForEach(Action<T> action)
        {
            items.ForEach(action);
        }

    }


}
