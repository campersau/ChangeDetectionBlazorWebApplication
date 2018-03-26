using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace ChangeDetectionBlazorWebApplication.Tests
{
    public class ChangeDetectionEnumerableTests : IDisposable
    {
        private readonly ChangeDetectionComponent ChangeDetectionComponent;
        private int StateHasChanged;

        public ChangeDetectionEnumerableTests()
        {
            ChangeDetectionComponent = new ChangeDetectionComponent(() => StateHasChanged++);
        }

        [Fact]
        public void TracksItself()
        {
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(ChangeDetectionComponent));
            Assert.Equal(0, ChangeDetectionComponent.TrackedObjects);

            Assert.Equal(0, StateHasChanged);
        }

        [Fact]
        public void TracksEmptyObservableCollection()
        {
            var collection = new ObservableCollection<object>();

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(collection));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(collection));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);

            Assert.Equal(0, StateHasChanged);
        }

        [Fact]
        public void TracksObservableCollection()
        {
            var collection = new ObservableCollection<object>();

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(collection));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            collection.Add(new object());
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            collection.RemoveAt(0);
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            collection.Add(new ObservableCollection<object>());
            Assert.Equal(3, ChangeDetectionComponent.TrackedObjects);

            collection.RemoveAt(0);
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(collection));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);

            Assert.Equal(4 * 3, StateHasChanged); // observablecollection fires 2 properties and 1 collection change
        }

        [Fact]
        public void TracksEmptyObservableCollectionMultipleTimes()
        {
            var collection = new ObservableCollection<object>();

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(collection));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.False(ChangeDetectionComponent.AttachChangeHandlers(collection));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(collection));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(collection));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);

            Assert.Equal(0, StateHasChanged);
        }

        [Fact]
        public void TrackCycleReference()
        {
            var collection = new ObservableCollection<object>(new[] { ChangeDetectionComponent });

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(collection));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(collection));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);

            Assert.Equal(0, StateHasChanged);
        }

        [Fact]
        public void DoesNotTrackList()
        {
            var list = new List<object>();

            Assert.False(ChangeDetectionComponent.AttachChangeHandlers(list));

            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);

            Assert.Equal(0, StateHasChanged);
        }

        [Fact]
        public void DoesTrackListWithObservableCollection()
        {
            var list = new List<object>();
            list.Add(new ObservableCollection<object>());

            Assert.False(ChangeDetectionComponent.AttachChangeHandlers(list));

            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.False(ChangeDetectionComponent.DetachChangeHandlers(list));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);

            Assert.Equal(0, StateHasChanged);
        }

        public void Dispose()
        {
            ChangeDetectionComponent.Dispose();
            Assert.Equal(0, ChangeDetectionComponent.TrackedObjects);
        }
    }
}
