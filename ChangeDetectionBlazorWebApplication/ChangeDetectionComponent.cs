using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChangeDetectionBlazorWebApplication
{
    public class ChangeDetectionComponent : BlazorComponent, INotifyPropertyChanged, IDisposable
    {

        private readonly Dictionary<object, int> _trackedObjects = new Dictionary<object, int>();
        private readonly Dictionary<object, Dictionary<string, object>> _propertyChangedValues = new Dictionary<object, Dictionary<string, object>>();

        internal int TrackedObjects => _trackedObjects.Count;

        private readonly Action _stateChanged;

        public ChangeDetectionComponent()
        {
            _stateChanged = StateHasChanged;

            AttachChangeHandlers(this);
        }

        internal ChangeDetectionComponent(Action stateChanged)
        {
            _stateChanged = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));

            AttachChangeHandlers(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected internal bool AttachChangeHandlers(object obj) => AttachChangeHandlersInternal(obj, false);

        private bool AttachChangeHandlersInternal(object obj, bool deep)
        {
            if (obj == null)
            {
                return false;
            }

            if (_trackedObjects.TryGetValue(obj, out var refCount))
            {
                //if (deep)
                //{
                //    return false;
                //}

                refCount++;

                _trackedObjects[obj] = refCount;
                return false;
            }

            var tracked = false;
            if (obj is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
                tracked = true;
            }
            if (obj is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
                tracked = true;
            }

            if (tracked)
            {
                _trackedObjects.Add(obj, 1);
            }

            // deep!
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (TryGetPropertyValue(obj, property, out var value))
                {
                    if (obj is INotifyPropertyChanged) // initial values
                    {
                        if (!_propertyChangedValues.TryGetValue(obj, out var properties))
                        {
                            properties = _propertyChangedValues[obj] = new Dictionary<string, object>();
                        }
                        properties[property.Name] = value;
                    }

                    AttachChangeHandlersInternal(value, true);
                }
            }

            if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    AttachChangeHandlersInternal(item, true);
                }
            }

            return tracked;
        }

        protected internal bool DetachChangeHandlers(object obj) => DetachChangeHandlersInternal(obj, false);

        protected bool DetachChangeHandlersInternal(object obj, bool deep)
        {
            if (obj == null)
            {
                return false;
            }

            bool tracked = false, finished = false;
            if (_trackedObjects.TryGetValue(obj, out var refCount))
            {
                tracked = true;

                //if (deep)
                //{
                //    return false;
                //}

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
                            if (property.Value != null && _trackedObjects.ContainsKey(property.Value))
                            {
                                DetachChangeHandlersInternal(property.Value, true);
                            }
                        }
                        properties.Clear();
                        _propertyChangedValues.Remove(obj);
                    }
                }
                if (obj is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
                }
            }

            // deep!
            foreach (var oldProperty in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (TryGetPropertyValue(obj, oldProperty, out var oldOldValue))
                {
                    if (oldOldValue != null && _trackedObjects.ContainsKey(oldOldValue))
                    {
                        DetachChangeHandlersInternal(oldOldValue, true);
                    }
                }
            }

            if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    DetachChangeHandlersInternal(item, true);
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
