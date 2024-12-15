using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using AndroidX.Core.App;
using GeoBaggins.Models;
using Refit;

namespace GeoBaggins.Android.Services;

[Service(Exported = true, Name = "com.geobaggins.LocationService", ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LocationService : Service
    {
        private const int NotificationId = 1001;
        private Timer _timer;
        private LocationManager _locationManager;
        private IGeoBagginsApi _geoBagginsApi;

        public override void OnCreate()
        {
            base.OnCreate();
            _locationManager = (LocationManager)GetSystemService(LocationService);
            var notification = new NotificationCompat.Builder(this, "geo_channel")
                .SetContentTitle("Location Service")
                .SetContentText("Service is running in background")
                .SetSmallIcon(Resource.Drawable.Icon)
                .Build();
            
            try
            {
                var serverUrl = "http://192.168.0.106:5230/";
                _geoBagginsApi = RestService.For<IGeoBagginsApi>(serverUrl);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
            }

            StartForeground(NotificationId, notification);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _timer = new Timer(SendLocationToServer, null, 0, 5000);
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _timer?.Dispose();
        }

        private void SendLocationNotification(object state)
        {
            const string provider = LocationManager.GpsProvider;
            var location = _locationManager.GetLastKnownLocation(provider);

            if (location == null)
            {
                Notify("Невозможно получить геометку");
                return;
            }

            var lat = location.Latitude;
            var lng = location.Longitude;

            Notify($"Широта: {lat}, Долгота: {lng}");
        }

        private void Notify(string message)
        {
            var notification = CreateNotification(message);
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(NotificationId, notification);
        }

        private void SendLocationToServer(object state)
        {
            Task.Run(async () =>
            {
                const string provider = LocationManager.GpsProvider;
                var location = _locationManager.GetLastKnownLocation(provider);

                if (location == null)
                {
                    Notify("Невозможно получить геометку для отправки на сервер");
                    return;
                }

                var lat = location.Latitude;
                var lng = location.Longitude;

                var locationData = new LocationDto
                {
                    Latitude = lat,
                    Longitude = lng,
                    TimeStamp = DateTime.UtcNow
                };
            
                try
                {
                    var response = await _geoBagginsApi.CheckLocation(locationData);
                    Notify(response);
                }
                catch (ApiException apiEx)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"HTTP Error: {apiEx.StatusCode} - {apiEx.Content}");
                    Notify("Server Error");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Refit error: " + e);
                    Console.ResetColor();
                    Notify("Error");
                }
            });
        }

        private Notification CreateNotification(string message)
        {
            var builder = new NotificationCompat.Builder(this, "geo_channel")
                .SetContentTitle("Геолокация")
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetAutoCancel(false);

            return builder.Build();
        }

        public override bool StopService(Intent? name)
        {
            _timer?.Dispose();
            StopForeground(true);
            StopSelf();
            return true;
        }
    }