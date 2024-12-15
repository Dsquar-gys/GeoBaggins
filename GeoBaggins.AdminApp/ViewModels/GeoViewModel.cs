using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using GeoBaggins.AdminApp.Models;
using ReactiveUI;

namespace GeoBaggins.AdminApp.ViewModels;

public class GeoViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    
    private AnchorZone? _currentZone;
    private string? _address;
    private double? _latitude;
    private double? _longitude;
    private double? _radius;
    private string? _message;
    
    public ObservableCollection<AnchorZone> AnchorZones { get; } = new();
    
    public AnchorZone? CurrentZone
    {
        get => _currentZone;
        set => this.RaiseAndSetIfChanged(ref _currentZone, value);
    }
    
    public string? Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }
    
    public double? Latitude
    {
        get => _latitude;
        set => this.RaiseAndSetIfChanged(ref _latitude, value);
    }
    
    public double? Longitude
    {
        get => _longitude;
        set => this.RaiseAndSetIfChanged(ref _longitude, value);
    }
    
    public double? Radius
    {
        get => _radius;
        set => this.RaiseAndSetIfChanged(ref _radius, value);
    }
    
    public string? Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public GeoViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // TODO: Save command
            await Task.Delay(1000);
        });
        
        CreateCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // TODO: Create command
            await Task.Delay(1000);
        });
        
        ClearSelectionCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            // TODO: Clear selection command
            await Task.Delay(1000);
        });
    }
    
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearSelectionCommand { get; }
}