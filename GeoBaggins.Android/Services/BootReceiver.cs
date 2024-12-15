using Android.App;
using Android.Content;

namespace GeoBaggins.Android.Services;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted })]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == Intent.ActionBootCompleted)
        {
            // Запускаем сервис после перезагрузки
            var serviceIntent = new Intent(context, typeof(LocationService));
            context.StartForegroundService(serviceIntent);
        }
    }
}