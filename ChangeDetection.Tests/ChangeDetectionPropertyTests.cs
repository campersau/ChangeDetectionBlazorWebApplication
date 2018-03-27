using ChangeDetection;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit;

namespace ChangeDetectionBlazorWebApplication.Tests
{
    public class ChangeDetectionPropertyTests
    {
        private int StateHasChanged;

        private ChangeDetector CreateChangeDetector(object obj)
        {
            return ChangeDetector.Create(obj, () => StateHasChanged++);
        }

        [Fact]
        public void TrackNotifyPropertyChanged()
        {
            var dummy = new Dummy();

            using (var changeDetector = CreateChangeDetector(dummy))
            {
                Assert.Equal(1, changeDetector.TrackedObjects);

                Assert.True(changeDetector.DetachChangeHandlers(dummy));
                Assert.Equal(0, changeDetector.TrackedObjects);

                Assert.Equal(0, StateHasChanged);
            }
        }

        [Fact]
        public void TrackNotifyPropertyChangedSimpleValue()
        {
            var dummy = new Dummy();

            using (var changeDetector = CreateChangeDetector(dummy))
            {
                Assert.Equal(1, changeDetector.TrackedObjects);

                dummy.IntProperty = 1;
                Assert.Equal(1, StateHasChanged);

                dummy.IntProperty = 1;
                Assert.Equal(1, StateHasChanged);

                dummy.IntProperty = 2;
                Assert.Equal(2, StateHasChanged);

                Assert.True(changeDetector.DetachChangeHandlers(dummy));
                Assert.Equal(0, changeDetector.TrackedObjects);
            }
        }

        [Fact]
        public void TrackNotifyPropertyChangedInitialSimpleValue()
        {
            var dummy = new Dummy() { IntProperty = 1 };

            using (var changeDetector = CreateChangeDetector(dummy))
            {
                Assert.Equal(1, changeDetector.TrackedObjects);

                dummy.IntProperty = 1;
                Assert.Equal(0, StateHasChanged);

                dummy.IntProperty = 2;
                Assert.Equal(1, StateHasChanged);

                Assert.True(changeDetector.DetachChangeHandlers(dummy));
                Assert.Equal(0, changeDetector.TrackedObjects);
            }
        }

        [Fact]
        public void TrackNotifyPropertyChangedNestedValue()
        {
            var dummy = new Dummy();

            using (var changeDetector = CreateChangeDetector(dummy))
            {
                Assert.Equal(1, changeDetector.TrackedObjects);

                dummy.DummyProperty = new Dummy();
                Assert.Equal(1, StateHasChanged);
                Assert.Equal(2, changeDetector.TrackedObjects);

                Assert.True(changeDetector.DetachChangeHandlers(dummy));
                Assert.Equal(0, changeDetector.TrackedObjects);
            }
        }

        [Fact]
        public void TrackNotifyPropertyChangedInitialNestedValue()
        {
            var dummy = new Dummy() { DummyProperty = new Dummy() };

            using (var changeDetector = CreateChangeDetector(dummy))
            {
                Assert.Equal(2, changeDetector.TrackedObjects);

                dummy.DummyProperty = null;
                Assert.Equal(1, StateHasChanged);
                Assert.Equal(1, changeDetector.TrackedObjects);

                Assert.True(changeDetector.DetachChangeHandlers(dummy));
                Assert.Equal(0, changeDetector.TrackedObjects);
            }
        }

        [Fact]
        public void TrackNotifyPropertyChangedNestedValueChange()
        {
            var dummy = new Dummy();

            using (var changeDetector = CreateChangeDetector(dummy))
            {
                Assert.Equal(1, changeDetector.TrackedObjects);

                dummy.DummyProperty = new Dummy();
                Assert.Equal(1, StateHasChanged);
                Assert.Equal(2, changeDetector.TrackedObjects);

                dummy.DummyProperty.IntProperty = 1;
                Assert.Equal(2, StateHasChanged);
                Assert.Equal(2, changeDetector.TrackedObjects);

                dummy.DummyProperty = null;
                Assert.Equal(3, StateHasChanged);
                Assert.Equal(1, changeDetector.TrackedObjects);

                Assert.True(changeDetector.DetachChangeHandlers(dummy));
                Assert.Equal(0, changeDetector.TrackedObjects);
            }
        }

        [Fact]
        public void TrackNotifyPropertyChangedCycle()
        {
            var dummy = new Dummy();
            dummy.DummyProperty = dummy;

            using (var changeDetector = CreateChangeDetector(dummy))
            {
                Assert.Equal(1, changeDetector.TrackedObjects);

                dummy.IntProperty = 1;
                Assert.Equal(1, StateHasChanged);
                Assert.Equal(1, changeDetector.TrackedObjects);

                dummy.DummyProperty = null;
                Assert.Equal(2, StateHasChanged);
                Assert.Equal(1, changeDetector.TrackedObjects);

                dummy.DummyProperty = dummy;
                Assert.Equal(3, StateHasChanged);
                Assert.Equal(1, changeDetector.TrackedObjects);

                Assert.True(changeDetector.DetachChangeHandlers(dummy));
                Assert.Equal(0, changeDetector.TrackedObjects);
            }
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
