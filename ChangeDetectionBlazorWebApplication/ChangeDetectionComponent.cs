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

        private readonly HashSet<object> _trackedObjects = new HashSet<object>();
        private readonly Dictionary<object, Dictionary<string, object>> _propertyChangedValues = new Dictionary<object, Dictionary<string, object>>();

        public ChangeDetectionComponent()
        {
            AttachChangeHandlers(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool AttachChangeHandlers(object obj)
        {
            if (obj == null || _trackedObjects.Contains(obj))
            {
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
                _trackedObjects.Add(obj);
            }

            // deep!
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (TryGetPropertyValue(obj, property, out var value))
                {
                    AttachChangeHandlers(value);
                }
            }

            if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    AttachChangeHandlers(item);
                }
            }

            return tracked;
        }

        protected bool DetachChangeHandlers(object obj)
        {
            if (obj == null || !_trackedObjects.Remove(obj))
            {
                return false;
            }

            if (obj is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
            }
            if (obj is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
            }

            if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    DetachChangeHandlers(item);
                }
            }

            // deep!
            foreach (var oldProperty in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (TryGetPropertyValue(obj, oldProperty, out var oldOldValue))
                {
                    DetachChangeHandlers(oldOldValue);
                }
            }

            return true;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var property = sender.GetType().GetProperty(args.PropertyName, BindingFlags.Public | BindingFlags.Instance);

            if (TryGetPropertyValue(sender, property, out var value))
            {
                if (_propertyChangedValues.TryGetValue(sender, out var properties) && properties.TryGetValue(args.PropertyName, out var oldValue))
                {
                    DetachChangeHandlers(oldValue);
                }
                else
                {
                    properties = _propertyChangedValues[sender] = new Dictionary<string, object>();
                }

                if (AttachChangeHandlers(value))
                {
                    properties[args.PropertyName] = value;
                }
            }

            StateHasChanged();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
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

            StateHasChanged();
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
            foreach (var obj in new List<object>(_trackedObjects))
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
