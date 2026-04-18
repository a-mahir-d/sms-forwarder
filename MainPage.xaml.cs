using sms_forwarder.Models;
using sms_forwarder.Services;

namespace sms_forwarder;

public partial class MainPage : ContentPage
{
    public AppSettings Settings { get; set; }

    public MainPage()
    {
        InitializeComponent();
        Settings = SettingsService.Get();
        _ = CheckPermissions();
        BindingContext = Settings;
    }

    private async Task CheckPermissions()
    {
        var smsPermGranted = await CheckSmsPermission();
        if(smsPermGranted) await CheckNotificationPermission();
    }

    private async Task<bool> CheckSmsPermission()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Sms>();
            if (status == PermissionStatus.Granted) return true;
            
            await DisplayAlertAsync("Hata", "SMS izni verilmedi, uygulama çalışmayacaktır.", "Tamam");
            return false;

        }
        catch
        {
            return false;
        }
    }
    
    private static async Task CheckNotificationPermission()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
            
            if (status == PermissionStatus.Granted)
            {
                var activity = Platform.CurrentActivity as MainActivity;
                activity?.TryStartSmsService();
            }
        }
        catch
        {
            // ignored
        }
    }
    
    private async void OnDayOfPackageRenewalTapped(object sender, EventArgs e)
    {
        try
        {
            var result = await DisplayPromptAsync("Paket Yenilenme Günü", "Paket yenileme gününü giriniz:", "Tamam", "İptal", "", -1, Keyboard.Numeric);

            if (result == "İptal" || !int.TryParse(result, out var newDay)) return;
            Settings.DayOfPackageRenewal = newDay;
            SettingsService.Update(Settings);

            RefreshUi();
        }
        catch
        {
            // ignored
        }
    }

    private async void OnSmsLimitTapped(object sender, EventArgs e)
    {
        try
        {
            var result = await DisplayPromptAsync("SMS Limiti", "Aylık limiti giriniz:", "Tamam", "İptal", "", -1, Keyboard.Numeric);

            if (!int.TryParse(result, out var newLimit)) return;
            Settings.SmsLimitPerMonth = newLimit;
            SettingsService.Update(Settings);
            
            RefreshUi();
        }
        catch
        {
            // ignored
        }
    }
    
    private async void OnActiveReceiverTapped(object sender, EventArgs e)
    {
        try
        {
            var result = await DisplayActionSheetAsync("Aktif Alıcıyı Değiştir", "İptal", null, $"A ({AppSettings.ReceiverA})", $"B ({AppSettings.ReceiverB})", $"C ({AppSettings.ReceiverC})");

            if (result == "İptal" || string.IsNullOrEmpty(result)) return;

            var selectedLetter = result.Substring(0, 1);
            Settings.ActiveReceiver = selectedLetter;
            SettingsService.Update(Settings);
        
            RefreshUi();
        }
        catch
        {
            // ignored
        }
    }
    
    private async void OnAddWhitelistClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await DisplayPromptAsync("Yeni Numara", "Beyaz listeye eklenecek numara veya gönderen adı:", "Ekle", "İptal");

            if (string.IsNullOrWhiteSpace(result)) return;
            if (Settings.WhiteList.Contains(result))
            {
                await DisplayAlertAsync("Hata", "Bu kayıt zaten listede mevcut.", "Tamam");
                return;
            }

            Settings.WhiteList.Add(result);
            SettingsService.Update(Settings);
            RefreshUi();
        }
        catch
        {
            // ignored
        }
    }

    private async void OnWhitelistItemTapped(object sender, EventArgs e)
    {
        try
        {
            var gesture = (TapGestureRecognizer)((Border)sender).GestureRecognizers[0];
            var selectedItem = (string)gesture.CommandParameter;
            var answer = await DisplayAlertAsync("Kaydı Sil", $"{selectedItem} listesinden silinsin mi?", "Evet", "Hayır");
            if (!answer) return;
        
            Settings.WhiteList.Remove(selectedItem);
            SettingsService.Update(Settings);
            RefreshUi();
        }
        catch
        {
            // ignored
        }
    }

    private void RefreshUi()
    {
        OnPropertyChanged(nameof(Settings));
        BindingContext = null;
        BindingContext = Settings;
    }
}