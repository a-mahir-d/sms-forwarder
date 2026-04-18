using Android.App;
using Android.Content;
using Android.OS;

namespace sms_forwarder;

[IntentFilter([Intent.ActionBootCompleted])]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent != null && intent.Action != Intent.ActionBootCompleted) return;
        if (context == null) return;
        
        var serviceIntent = new Intent(context, typeof(SmsForegroundService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
#pragma warning disable CA1416
            context.StartForegroundService(serviceIntent);
#pragma warning restore CA1416
        }
        else
        {
            context.StartService(serviceIntent);
        }
    }
}