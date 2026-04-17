namespace sms_forwarder.Models;

public class AppSettings
{
    public List<string> WhiteList { get; set; } = [];
    public int DayOfPackageRenewal { get; set; } = 10;
    public int SmsLimitPerMonth { get; set; } = 200;
    public static string ReceiverA => "+905*********";
    public static string ReceiverB => "+905*********";
    public static string ReceiverC => "+905*********";
    public static string Owner => "*****";
    public string ActiveReceiver { get; set; } = "A";
    public List<string> SpecialSenders = ["*****"];

    public string ActivePhoneNumber => ActiveReceiver switch
    {
        "B" => ReceiverB,
        "C" => ReceiverC,
        _ => ReceiverA
    };
}