namespace SymmetricalTelegramBot.Options;

public class TelegramOptions
{
    public const string Telegram = nameof(Telegram);

    public string Token { get; set; } = string.Empty;
}