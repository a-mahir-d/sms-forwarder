using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace sms_forwarder;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        TryStartSmsService();
    }
    
    public async void TryStartSmsService()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted) return;
        
            var intent = new Intent(this, typeof(SmsForegroundService));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
#pragma warning disable CA1416
                StartForegroundService(intent);
#pragma warning restore CA1416
            }
            else
            {
                StartService(intent);
            }
        }
        catch
        {
            // ignored
        }
    }
}