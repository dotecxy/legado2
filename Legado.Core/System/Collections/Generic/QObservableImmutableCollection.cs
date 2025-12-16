using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace System.Collections.ObjectModel
{

    [Serializable]
    public class QObservableImmutableCollection<T> : QImmutableCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";
        private SimpleMonitor _monitor = new SimpleMonitor();

        public QObservableImmutableCollection()
        {
        }


        private void CopyFrom(IEnumerable<T> collection)
        {
            IList<T> items = this.Items;
            if (collection == null || items == null)
                return;
            foreach (T obj in collection)
                items.Add(obj);
        }


        public void Move(int oldIndex, int newIndex) => this.MoveItem(oldIndex, newIndex);


        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {

            add => this.PropertyChanged += value;

            remove => this.PropertyChanged -= value;
        }

        [field: NonSerialized]

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;


        protected override void ClearItems()
        {
            this.CheckReentrancy();
            base.ClearItems();
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }


        protected override void RemoveItem(int index)
        {
            this.CheckReentrancy();
            T obj = this.ElementAtOrDefault<T>(index);
            if (obj == null) return;
            base.RemoveItem(index);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, (object)obj, index);
        }


        protected override void InsertItem(int index, T item)
        {
            this.CheckReentrancy();
            base.InsertItem(index, item);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, (object)item, index);
        }


        protected override void SetItem(int index, T item)
        {
            this.CheckReentrancy();
            T oldItem = this.ElementAtOrDefault<T>(index);
            if (oldItem == null) return;
            base.SetItem(index, item);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, (object)oldItem, (object)item, index);
        }


        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            this.CheckReentrancy();
            T obj = this.ElementAtOrDefault<T>(oldIndex);
            if (obj == null) return;
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, obj);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Move, (object)obj, newIndex, oldIndex);
        }


        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, e);
        }

        [field: NonSerialized]

        protected virtual event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged == null)
                return;
            using (this.BlockReentrancy())
                this.CollectionChanged((object)this, e);
        }


        protected IDisposable BlockReentrancy()
        {
            this._monitor.Enter();
            return (IDisposable)this._monitor;
        }


        protected void CheckReentrancy()
        {
            if (this._monitor.Busy && this.CollectionChanged != null && this.CollectionChanged.GetInvocationList().Length > 1)
                throw new InvalidOperationException("ObservableImmutableCollectionReentrancyNotAllowed");
        }

        private void OnPropertyChanged(string propertyName) => this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index) => this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));

        private void OnCollectionChanged(
          NotifyCollectionChangedAction action,
          object item,
          int index,
          int oldIndex)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(
          NotifyCollectionChangedAction action,
          object oldItem,
          object newItem,
          int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset() => this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        [Serializable]
        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;

            public void Enter() => ++this._busyCount;

            public void Dispose() => --this._busyCount;

            public bool Busy => this._busyCount > 0;
        }
    }



    public static class ICollectionLockExtensions
    {
        public static IDisposable Lock<T>(this ICollection<T> collection, int ms = 5000)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            return Lock(collection, ms, true);
        }

        public static IDisposable Lock<T>(this ICollection<T> collection, int ms, bool throwException)
        {
            if (collection == null)
            {
                return new EmptyCollectionLockDisposable();
            }
            if (ms < 0)
            {
                ms = 500;
            }

            if (ms > 999999)
            {
                ms = 999999;
            }

            bool entered = false;
            Exception lastEception = null;
            try
            {
                entered = Monitor.TryEnter((object)collection, ms);
            }
            catch (Exception ex)
            {
                lastEception = ex;
            }
            finally
            {
            }

            if (!entered)
            {
                if (throwException)
                {
                    throw new TimeoutException("ICollectionLockExtensions.Lock");
                }
                else
                {
                    return new EmptyCollectionLockDisposable();
                }
            }
            return (IDisposable)new ICollectionLockExtensions.CollectionLock<T>(collection, entered);
        }

        public static bool SafeAdd<T>(this ICollection<T> collection, T model)
        {
            if (collection == null) return false;
            if (model == null) return false;

            return SafeRun(collection, (c) =>
            {
                c.Add(model);
            });
        }

        public static bool SafeRun<T>(this ICollection<T> collection, Action<ICollection<T>> act)
        {
            if (collection == null)
            {
                return false;
            }

            if (act == null)
            {
                return false;
            }

            try
            {
                using (var _ = Lock(collection, 5000, true))
                {
                    act.Invoke(collection);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("SafeRun ex:" + ex.Message);
                return false;
            }
        }


        public static void SafeForeach<T>(this ICollection<T> collection, Action<ICollection<T>, T> act)
        {
            if (collection == null)
            {
                return;
            }

            if (act == null)
            {
                return;
            }
            SafeForeach(collection, TimeSpan.FromMilliseconds(5000), act);
        }

        public static void SafeForeach<T>(this ICollection<T> collection, TimeSpan ts, Action<ICollection<T>, T> act)
        {
            if (collection == null)
            {
                return;
            }

            if (act == null)
            {
                return;
            }

            try
            {
                using (var _ = collection.Lock((int)(ts.TotalMilliseconds)))
                {
                    foreach (var item in collection)
                    {
                        act?.Invoke(collection, item);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("SafeForeach ex:" + ex.Message);
            }
        }


        public static T SafeSelect<T>(this ICollection<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
            {
                return default;
            }
            if (predicate == null)
            {
                return default;
            }
            var list = SafeFind<T>(collection, predicate);
            list = list ?? new List<T>();
            return list.SingleOrDefault();
        }

        public static List<T> SafeFind<T>(this ICollection<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
            {
                return new List<T>();
            }
            if (predicate == null)
            {
                return new List<T>();
            }
            return SafeFind(collection, TimeSpan.FromMilliseconds(5000), predicate);
        }

        public static List<T> SafeFind<T>(this ICollection<T> collection, TimeSpan ts, Predicate<T> predicate)
        {
            if (collection == null)
            {
                return new List<T>();
            }

            if (predicate == null)
            {
                return new List<T>();
            }
            List<T> list = new List<T>();
            try
            {
                using (var _ = collection.Lock((int)(ts.TotalMilliseconds)))
                {
                    if (collection == null || collection.Count == 0)
                    {
                        return list;
                    }
                    foreach (var item in collection)
                    {
                        if (predicate?.Invoke(item) == true)
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("SafeFind ex:" + ex.Message);
            }
            return list;
        }

        private sealed class EmptyCollectionLockDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private sealed class CollectionLock<T> : IDisposable
        {
            private ICollection<T> _collection;
            private bool _entered;
            public CollectionLock(ICollection<T> collection, bool entered)
            {
                this._collection = collection;
                this._entered = entered;
            }

            public void Dispose()
            {
                if (_entered)
                {
                    try
                    {
                        Monitor.Exit((object)this._collection);
                    }
                    catch { }
                }
                this._collection = (ICollection<T>)null;
            }
        }
    }
}
