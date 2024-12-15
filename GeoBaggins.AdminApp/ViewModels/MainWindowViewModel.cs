using System;

namespace GeoBaggins.AdminApp.ViewModels;

public class MainWindowViewModel(IServiceProvider provider) : ViewModelBase
{
    public ViewModelBase CurrentPage { get; set; } = new GeoViewModel(provider);
}