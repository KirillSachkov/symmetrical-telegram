using Microsoft.Extensions.Options;
using SymmetricalTelegramBot;
using SymmetricalTelegramBot.Features;
using SymmetricalTelegramBot.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TelegramBotBackgroundService>();

builder.Services.AddTransient<ITelegramBotClient, TelegramBotClient>(serviceProvider =>
{
    var token = serviceProvider.GetRequiredService<IOptions<TelegramOptions>>().Value.Token;

    return new(token);
});

builder.Services.AddTransient<IHandler<Message>, MessageHandler>();

builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.Telegram));

var host = builder.Build();
host.Run();