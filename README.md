# ChangeDetectionBlazorWebApplication

Demo application to demonstrate very simple change detection based on `INotifyPropertyChanged` and `INotifyCollectionChanged`.

Create a new [`ChangeDetector`](ChangeDetection/ChangeDetector.cs) with a state change handler and public property / obserable collection changes are automatically tracked.
Make sure to dispose the `ChangeDetector` to release all event handlers.

See [`Index.cshtml`](ChangeDetectionBlazorWebApplication/Pages/Index.cshtml) for a sample implementation.