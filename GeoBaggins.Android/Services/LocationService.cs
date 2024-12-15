using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using GeoBaggins.Models;
using Refit;

namespace GeoBaggins.Android.Services;

[Service(Exported = true, Name = "com.geobaggins.LocationService", ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LocationService : Service
    {
        private const int NotificationId = 1001;
        
        // Для проверки изменения области
        // private int _counter;
        // private LocationDto _first = new LocationDto{DeviceId = "111", Latitude = 30, Longitude = 60, TimeStamp = DateTime.Now};
        // private LocationDto _second = new LocationDto{DeviceId = "111", Latitude = 60, Longitude = 30, TimeStamp = DateTime.Now};
        
        private Timer _timer;
        private LocationManager _locationManager;
        private IGeoBagginsApi _geoBagginsApi;

        public override void OnCreate()
        {
            base.OnCreate();
            _locationManager = (LocationManager)GetSystemService(LocationService);
            var notification = CreateNotification("GeoBaggins.Android");
            
            try
            {
                var serverUrl = "http://192.168.0.106:5230/";
                _geoBagginsApi = RestService.For<IGeoBagginsApi>(serverUrl);
            }
            catch (Exception e)
            {
                Notify("Ошибка подключения к серверу");
            }

            StartForeground(NotificationId, notification);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Notify("Сервис запущен");
            _timer = new Timer(SendLocationToServer, null, 0, 10000);
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

                var deviceId =
                    Settings.Secure.GetString(Application.Context.ContentResolver, Settings.Secure.AndroidId);

                var lat = location.Latitude;
                var lng = location.Longitude;

                var locationData = new LocationDto
                {
                    Latitude = lat,
                    Longitude = lng,
                    TimeStamp = DateTime.UtcNow,
                    DeviceId = deviceId!
                };
            
                try
                {
                    var response = await _geoBagginsApi.CheckLocation(locationData);
                    // string response;
                    // if (_counter++ % 2 == 0)
                    // {
                    //     response = await _geoBagginsApi.CheckLocation(_first);
                    // }
                    // else
                    // {
                    //     response = await _geoBagginsApi.CheckLocation(_second);
                    // }
                    Notify(response);
                }
                catch (ApiException apiEx)
                {
                    Notify("Server Error");
                }
                catch (Exception e)
                {
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
    }