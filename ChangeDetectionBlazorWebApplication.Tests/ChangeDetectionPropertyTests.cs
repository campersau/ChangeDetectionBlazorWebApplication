using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit;

namespace ChangeDetectionBlazorWebApplication.Tests
{
    public class ChangeDetectionPropertyTests : IDisposable
    {
        private readonly ChangeDetectionComponent ChangeDetectionComponent;
        private int StateHasChanged;

        public ChangeDetectionPropertyTests()
        {
            ChangeDetectionComponent = new ChangeDetectionComponent(() => StateHasChanged++);
        }

        [Fact]
        public void TrackNotifyPropertyChanged()
        {
            var dummy = new Dummy();

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(dummy));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(dummy));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);

            Assert.Equal(0, StateHasChanged);
        }

        [Fact]
        public void TrackNotifyPropertyChangedSimpleValue()
        {
            var dummy = new Dummy();

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(dummy));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            dummy.IntProperty = 1;
            Assert.Equal(1, StateHasChanged);

            dummy.IntProperty = 1;
            Assert.Equal(1, StateHasChanged);

            dummy.IntProperty = 2;
            Assert.Equal(2, StateHasChanged);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(dummy));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);
        }

        [Fact]
        public void TrackNotifyPropertyChangedInitialSimpleValue()
        {
            var dummy = new Dummy() { IntProperty = 1 };
             
            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(dummy));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            dummy.IntProperty = 1;
            Assert.Equal(0, StateHasChanged);

            dummy.IntProperty = 2;
            Assert.Equal(1, StateHasChanged);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(dummy));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);
        }

        [Fact]
        public void TrackNotifyPropertyChangedNestedValue()
        {
            var dummy = new Dummy();

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(dummy));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            dummy.DummyProperty = new Dummy();
            Assert.Equal(1, StateHasChanged);
            Assert.Equal(3, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(dummy));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);
        }

        [Fact]
        public void TrackNotifyPropertyChangedInitialNestedValue()
        {
            var dummy = new Dummy() { DummyProperty = new Dummy() };

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(dummy));
            Assert.Equal(3, ChangeDetectionComponent.TrackedObjects);

            dummy.DummyProperty = null;
            Assert.Equal(1, StateHasChanged);
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(dummy));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);
        }

        [Fact]
        public void TrackNotifyPropertyChangedNestedValueChange()
        {
            var dummy = new Dummy();

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(dummy));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            dummy.DummyProperty = new Dummy();
            Assert.Equal(1, StateHasChanged);
            Assert.Equal(3, ChangeDetectionComponent.TrackedObjects);

            dummy.DummyProperty.IntProperty = 1;
            Assert.Equal(2, StateHasChanged);
            Assert.Equal(3, ChangeDetectionComponent.TrackedObjects);

            dummy.DummyProperty = null;
            Assert.Equal(3, StateHasChanged);
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(dummy));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);
        }

        [Fact]
        public void TrackNotifyPropertyChangedCycle()
        {
            var dummy = new Dummy();
            dummy.DummyProperty = dummy;

            Assert.True(ChangeDetectionComponent.AttachChangeHandlers(dummy));
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            dummy.IntProperty = 1;
            Assert.Equal(1, StateHasChanged);
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            dummy.DummyProperty = null;
            Assert.Equal(2, StateHasChanged);
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            dummy.DummyProperty = dummy;
            Assert.Equal(3, StateHasChanged);
            Assert.Equal(2, ChangeDetectionComponent.TrackedObjects);

            Assert.True(ChangeDetectionComponent.DetachChangeHandlers(dummy));
            Assert.Equal(1, ChangeDetectionComponent.TrackedObjects);
        }

        public void Dispose()
        {
            ChangeDetectionComponent.Dispose();
            Assert.Equal(0, ChangeDetectionComponent.TrackedObjects);
        }

        private class Dummy : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private int _intProperty;
            public int IntProperty
            {
                get => _intProperty;
                set { _intProperty = value; OnPropertyChanged(); }
            }

            private Dummy _dummyProperty;
            public Dummy DummyProperty
            {
                get => _dummyProperty;
                set { _dummyProperty = value; OnPropertyChanged(); }
            }

        }
    }
}
