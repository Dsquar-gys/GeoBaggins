using System;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using GeoBaggins.Android.Services;
using GeoBaggins.Models;
using GeoBaggins.Views;
using Refit;

namespace GeoBaggins.Android;

[Activity(
    Label = "GB.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI();
    }
    
    // IGeoBagginsApi _geoBagginsApi;
    LocationManager _locationManager;
    NotificationManagerCompat _notificationManager;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _locationManager = (LocationManager)GetSystemService(LocationService);
        _notificationManager = NotificationManagerCompat.From(this);

        CreateNotificationChannel();
        
        var mainView = (MainView?)Content;
        var locSwitch = mainView?.FindControl<ToggleSwitch>("LocationServiceToggleSwitch");
        
        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, 0);
        }

        if (locSwitch != null)
        {
            locSwitch.IsCheckedChanged += async (s, e) =>
            {
                if (((ToggleSwitch)s!).IsChecked!.Value)
                {
                    StartLocationService();
                }
                else
                {
                    StopLocationService();
                }
            };
        }
    }
    
    public async Task ShowLocationNotification()
    {
        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, 0);
            return;
        }

        const string provider = LocationManager.GpsProvider;
        var location = _locationManager.GetLastKnownLocation(provider);

        if (location == null)
        {
            Console.WriteLine("Location is null");
            Toast.MakeText(this, "Невозможно получить геометку", ToastLength.Short).Show();
            return;
        }

        var lat = location.Latitude;
        var lng = location.Longitude;

        Console.WriteLine($"Location received: Lat={lat}, Lng={lng}");

        var builder = new NotificationCompat.Builder(this, "geo_channel")
            .SetContentTitle("Текущая геометка")
            .SetContentText($"Широта: {lat}, Долгота: {lng}")
            .SetSmallIcon(Resource.Drawable.Icon)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetAutoCancel(true);

        _notificationManager.Notify(1, builder.Build());
        Console.WriteLine("Notification sent.");
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel("geo_channel", "Geo Notifications", NotificationImportance.High)
            {
                Description = "Уведомления о геометке"
            };
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }

            
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == 0 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
        {
            StartLocationService();
        }
        else
        {
            Toast.MakeText(this, "Разрешение необходимо для работы приложения", ToastLength.Long).Show();
        }
    }
    
    private void StartLocationService()
    {
        if (IsServiceRunning(typeof(LocationService))) return;
        
        var serviceIntent = new Intent(this, typeof(LocationService));
        ContextCompat.StartForegroundService(this, serviceIntent);
    }

    private void StopLocationService()
    {
        if (!IsServiceRunning(typeof(LocationService))) return;
        
        var serviceIntent = new Intent(this, typeof(LocationService));
        StopService(serviceIntent);
    }
    
    private bool IsServiceRunning(Type serviceClass)
    {
        var manager = (ActivityManager)GetSystemService(Context.ActivityService);
        return manager.GetRunningServices(int.MaxValue)
            .Any(service => service.Service.ClassName.EndsWith(serviceClass.Name));
    }

    protected override void OnDestroy()
    {
        StopLocationService();
        base.OnDestroy();
    }
}