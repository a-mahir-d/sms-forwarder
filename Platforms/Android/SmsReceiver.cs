using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using sms_forwarder.Models;
using sms_forwarder.Services;
using SmsMessage = Android.Telephony.SmsMessage;

namespace sms_forwarder;

[BroadcastReceiver(Enabled = true, Exported = true, Name = "com.amd.sms_forwarder.SmsReceiver")]
[IntentFilter(["android.provider.Telephony.SMS_RECEIVED"], Priority = 999)]
public class SmsReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent == null || !string.Equals(intent.Action, "android.provider.Telephony.SMS_RECEIVED", StringComparison.OrdinalIgnoreCase)) return;

        try
        {
            var bundle = intent.Extras;
            if (bundle == null) return;

            var pdus = (Java.Lang.Object[])bundle.Get("pdus");
            if (pdus == null) return;

            var format = bundle.GetString("format");

            foreach (var pdu in pdus)
            {
                var bytes = (byte[])((Java.Lang.Object)pdu);

#pragma warning disable CA1416
#pragma warning disable CA1422
                var message = Build.VERSION.SdkInt >= BuildVersionCodes.M ? SmsMessage.CreateFromPdu(bytes, format) : SmsMessage.CreateFromPdu(bytes);
#pragma warning restore CA1422
#pragma warning restore CA1416

                if (message == null) continue;
                var sender = message.OriginatingAddress;
                var body = message.DisplayMessageBody;
                var settings = SettingsService.Get();
                
                if (settings.SpecialSenders.Contains(sender))
                {
                    if (body != null && body.StartsWith("Yeni alıcı ", StringComparison.OrdinalIgnoreCase))
                    {
                        var target = body.Replace("Yeni alıcı ", "", StringComparison.OrdinalIgnoreCase).Trim().ToUpper();
                        if (target is "A" or "B" or "C")
                        {
                            settings.ActiveReceiver = target;
                            SettingsService.Update(settings);
                            return;
                        }
                    }
                }
            
                if (settings.WhiteList.Contains(sender))
                {
                    ForwardSms(sender, body);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SmsReceiver Hatası: {ex.Message}");
        }
    }

    private static void ForwardSms(string senderName, string messageContent)
    {
        try
        {
            var targetNumber = AppSettings.Owner; 
            var finalMessage = $"Alıcı: {AppSettings.Owner}\n" +
                               $"Gönderen: {senderName}\n" +
                               $"Mesaj: {messageContent}";

            SmsManager smsManager;
            var context = Android.App.Application.Context;
            
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
#pragma warning disable CA1416
                smsManager = (SmsManager)context.GetSystemService(Java.Lang.Class.FromType(typeof(SmsManager)));
#pragma warning restore CA1416
            }
            else
            {
#pragma warning disable CA1422
                smsManager = SmsManager.Default;
#pragma warning restore CS0618
            }
            
            if (smsManager == null || string.IsNullOrEmpty(targetNumber)) return;
            var parts = smsManager.DivideMessage(finalMessage);
            smsManager.SendMultipartTextMessage(targetNumber, null, parts, null, null);

            if (parts != null) System.Diagnostics.Debug.WriteLine($"SMS Multipart olarak gönderildi. Parça sayısı: {parts.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SMS gönderilemedi: {ex.Message}");
        }
    }
}