using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace ChangeDetection
{
    public class ChangeDetector : IDisposable
    {

        private readonly Dictionary<object, int> _trackedObjects = new Dictionary<object, int>();
        private readonly Dictionary<object, Dictionary<string, object>> _propertyChangedValues = new Dictionary<object, Dictionary<string, object>>();

        internal int TrackedObjects => _trackedObjects.Count;

        private readonly Action _stateChanged;

        public static ChangeDetector Create(object obj, Action stateChanged)
        {
            return new ChangeDetector(obj, stateChanged);
        }

        private ChangeDetector(object obj, Action stateChanged)
        {
            _stateChanged = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));

            AttachChangeHandlers(obj);
        }

        protected internal bool AttachChangeHandlers(object obj) => AttachChangeHandlersInternal(obj);

        private bool AttachChangeHandlersInternal(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (_trackedObjects.TryGetValue(obj, out var refCount))
            {
                refCount++;

                _trackedObjects[obj] = refCount;
                return false;
            }

            if (obj is INotifyPropertyChanged notifyPropertyChanged)
            {
                _trackedObjects[obj] = 1;

                foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (TryGetPropertyValue(obj, property, out var value))
                    {
                        if (!_propertyChangedValues.TryGetValue(obj, out var properties))
                        {
                            properties = _propertyChangedValues[obj] = new Dictionary<string, object>();
                        }
                        properties[property.Name] = value;

                        AttachChangeHandlersInternal(value);
                    }
                }

                notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
            }
            if (obj is INotifyCollectionChanged notifyCollectionChanged)
            {
                _trackedObjects[obj] = 1;

                if (obj is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        AttachChangeHandlersInternal(item);
                    }
                }

                notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
            }

            return _trackedObjects.ContainsKey(obj);
        }

        protected internal bool DetachChangeHandlers(object obj) => DetachChangeHandlersInternal(obj);

        protected bool DetachChangeHandlersInternal(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            bool tracked = false, finished = false;
            if (_trackedObjects.TryGetValue(obj, out var refCount))
            {
                tracked = true;

                refCount--;

                if (refCount == 0)
                {
                    _trackedObjects.Remove(obj);
                    finished = true;
                }
                else
                {
                    _trackedObjects[obj] = refCount;
                }
            }

            if (finished)
            {
                if (obj is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;

                    if (_propertyChangedValues.TryGetValue(obj, out var properties))
                    {
                        foreach (var property in properties)
                        {
                            DetachChangeHandlersInternal(property.Value);
                        }
                        properties.Clear();
                        _propertyChangedValues.Remove(obj);
                    }
                }
                if (obj is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;

                    if (obj is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            DetachChangeHandlersInternal(item);
                        }
                    }
                }
            }

            return tracked;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var property = sender.GetType().GetProperty(args.PropertyName, BindingFlags.Public | BindingFlags.Instance);

            if (TryGetPropertyValue(sender, property, out var value))
            {
                if (_propertyChangedValues.TryGetValue(sender, out var properties) && properties.TryGetValue(args.PropertyName, out var oldValue))
                {
                    if (Equals(value, oldValue))
                    {
                        return; // value has not changed
                    }

                    DetachChangeHandlers(oldValue);
                }
                else
                {
                    properties = _propertyChangedValues[sender] = new Dictionary<string, object>();
                }

                AttachChangeHandlers(value);

                properties[args.PropertyName] = value;
            }

            _stateChanged();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                // not supported...
            }

            if (args.NewItems != null)
            {
                foreach (var newItem in args.NewItems)
                {
                    AttachChangeHandlers(newItem);
                }
            }
            if (args.OldItems != null)
            {
                foreach (var oldItem in args.OldItems)
                {
                    DetachChangeHandlers(oldItem);
                }
            }

            _stateChanged();
        }

        private static bool TryGetPropertyValue(object sender, PropertyInfo property, out object value)
        {
            if (property != null && property.CanRead && property.GetIndexParameters().Length == 0)
            {
                try
                {
                    value = property.GetValue(sender);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to get property value: {sender.GetType().FullName} => {property.Name}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
            value = null;
            return false;
        }

        public virtual void Dispose()
        {
            foreach (var obj in new List<object>(_trackedObjects.Keys))
            {
                DetachChangeHandlers(obj);
            }

            foreach (var properties in _propertyChangedValues)
            {
                foreach (var property in properties.Value)
                {
                    DetachChangeHandlers(property.Value);
                }
                properties.Value.Clear();
            }
            _propertyChangedValues.Clear();
        }

    }

}
