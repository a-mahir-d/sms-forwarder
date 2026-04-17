using System.Runtime.Versioning;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;

namespace sms_forwarder;

[Service(Name = "com.amd.sms_forwarder.SmsForegroundService", ForegroundServiceType = ForegroundService.TypeSpecialUse)]
public class SmsForegroundService : Service
{
    private const string ChannelId = "sms_forwarder_channel";
    private const int NotificationId = 1001;
    private SmsReceiver _receiver;

    public override IBinder OnBind(Intent intent) => null;

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
#pragma warning disable CA1416
        CreateNotificationChannel();
#pragma warning restore CA1416

        var notificationCompatBuilder = new NotificationCompat.Builder(this, ChannelId);
        notificationCompatBuilder.SetContentTitle("SMS Forwarder Aktif");
        notificationCompatBuilder.SetContentText("Gelen mesajlar kontrol ediliyor...");
        notificationCompatBuilder.SetSmallIcon(Resource.Drawable.sms);
        notificationCompatBuilder.SetOngoing(true);
        var notification = notificationCompatBuilder.Build();
        
        _receiver = new SmsReceiver();
        var filter = new IntentFilter("android.provider.Telephony.SMS_RECEIVED") 
        { 
            Priority = (int)IntentFilterPriority.HighPriority 
        };

        if (Build.VERSION.SdkInt >= (BuildVersionCodes)34)
        {
#pragma warning disable CA1416
            if (notification != null) StartForeground(NotificationId, notification, ForegroundService.TypeSpecialUse);
            RegisterReceiver(_receiver, filter, ReceiverFlags.Exported);
#pragma warning restore CA1416
        }
        else
        {
            StartForeground(NotificationId, notification);
            RegisterReceiver(_receiver, filter);
        }

        return StartCommandResult.Sticky;
    }
    
    public override void OnDestroy()
    {
        if (_receiver != null) UnregisterReceiver(_receiver);
        base.OnDestroy();
    }

    [SupportedOSPlatform("android26.0")]
    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
        var channel = new NotificationChannel(ChannelId, "SMS Forwarding Service", NotificationImportance.Low)
        {
            Description = "SMS yönlendirme servisi bildirim kanalı"
        };
        
        var manager = (NotificationManager)GetSystemService(NotificationService);
        manager?.CreateNotificationChannel(channel);
    }
}