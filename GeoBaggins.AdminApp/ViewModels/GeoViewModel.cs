using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using DynamicData;
using GeoBaggins.AdminApp.Data;
using GeoBaggins.AdminApp.Models;
using Mapsui;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace GeoBaggins.AdminApp.ViewModels;

public sealed class GeoViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationManager _notificationManager;
    
    private AnchorZone? _currentZone;
    private string? _address;
    private double? _latitude;
    private double? _longitude;
    private double? _radius;
    private string? _message;
    
    private Map _map;
    
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

    public Map MapLayer
    {
        get => _map;
        set => this.RaiseAndSetIfChanged(ref _map, value);
    }

    public GeoViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _notificationManager = _serviceProvider.GetRequiredService<INotificationManager>();

        UpdateZoneList();

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var zone = await dbContext.GeoZones.FindAsync(CurrentZone!.Id);
            if (zone == null)
            {
                _notificationManager.Show(new Notification("Saving", "Area is not found in database.",
                    NotificationType.Warning));
                Console.WriteLine("Zone not found");
                return;
            }
            
            zone.Address = Address!;
            zone.Latitude = Latitude!.Value;
            zone.Longitude = Longitude!.Value;
            zone.Radius = Radius!.Value;
            zone.Message = Message!;
            
            CurrentZone = zone;
            
            await dbContext.SaveChangesAsync();

            _notificationManager.Show(new Notification("Saving", "Saving completed successfully.",
                NotificationType.Success));
            
            UpdateZoneList();
        }, this.WhenAnyValue(property1: x => x.CurrentZone,
            property2: x => x.Address,
            property3: x => x.Latitude,
            property4: x => x.Longitude,
            property5: x => x.Radius,
            (zone, addr, lat, lon, rad) =>
                zone != null && addr != null && lat != null && lon != null && rad != null));
        
        CreateCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var zone = new AnchorZone
            {
                Address = Address!,
                Latitude = Latitude!.Value,
                Longitude = Longitude!.Value,
                Radius = Radius!.Value,
                Message = Message!
            };
            
            dbContext.GeoZones.Add(zone);
            await dbContext.SaveChangesAsync();
            _notificationManager.Show(new Notification("Creating", "Area created successfully.",
                NotificationType.Success));
            
            UpdateZoneList();
        }, this.WhenAnyValue(property1: x => x.CurrentZone,
            property2: x => x.Address,
            property3: x => x.Latitude,
            property4: x => x.Longitude,
            property5: x => x.Radius,
            (zone, addr, lat, lon, rad) =>
                zone == null && addr != null && lat != null && lon != null && rad != null));
        
        ClearSelectionCommand = ReactiveCommand.Create(() =>
        {
            CurrentZone = null;
        });

        this.WhenAnyValue(x => x.CurrentZone)
            .Subscribe(zone =>
            {
                Address = zone?.Address ?? null;
                Latitude = zone?.Latitude ?? null;
                Longitude = zone?.Longitude ?? null;
                Radius = zone?.Radius ?? null;
                Message = zone?.Message ?? null;
            });
    }
    
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearSelectionCommand { get; }

    private void UpdateZoneList()
    {
        var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        AnchorZones.Clear();
        AnchorZones.AddRange(dbContext.GeoZones);
    }
}