using SymmetricalTelegramBot.Features;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace SymmetricalTelegramBot;

public class TelegramBotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramBotBackgroundService> _logger;

    public TelegramBotBackgroundService(
        ITelegramBotClient botClient,
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramBotBackgroundService> logger)
    {
        _botClient = botClient;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = []
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            await _botClient.ReceiveAsync(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken);
        }
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        var scope = _scopeFactory.CreateScope();

        var messageHandler = scope.ServiceProvider.GetRequiredService<IHandler<Message>>();

        var handler = update switch
        {
            { Message: { } message } => messageHandler.Handle(message, cancellationToken),
            { CallbackQuery: { } query } => CallbackQueryHandler(query, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown type message");
        return Task.CompletedTask;
    }

    private async Task CallbackQueryHandler(CallbackQuery query, CancellationToken cancellationToken)
    {
        if (query.Message is not { } message)
            return;

        switch (query.Data)
        {
            case "lessons-info":
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Вот тебе информация о занятиях, дорогой друг",
                    cancellationToken: cancellationToken);
                return;
        }
    }

    private Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ApiRequestException apiRequestException:
                _logger.LogError(
                    apiRequestException,
                    "Telegram API Error:\n[{errorCode}]\n{message}",
                    apiRequestException.ErrorCode,
                    apiRequestException.Message);
                return Task.CompletedTask;

            default:
                _logger.LogError(exception, "Error while processing message in telegram bot");
                return Task.CompletedTask;
        }
    }
}