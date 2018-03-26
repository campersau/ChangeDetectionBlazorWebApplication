# ChangeDetectionBlazorWebApplication

Demo application to demonstrate very simple change detection based on `INotifyPropertyChanged` and `INotifyCollectionChanged`.

Inherit from [`ChangeDetectionComponent`](ChangeDetectionBlazorWebApplication/ChangeDetectionComponent.cs) instead of `BlazorComponent` and public property changes are automatically beeing tracked.
Since `@inject` creates private properties you can call `AttachChangeHandlers(...)` manually to track changes for these objects.