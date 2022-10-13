using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ScriptCord.Bot;
using ScriptCord.Core.DiscordExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptyCord.Bot.Commands
{
    public class UtilitiesModule : ScriptyCordCommandModule
    {
        private readonly ILoggerFacade<UtilitiesModule> _logger;

        public UtilitiesModule(ILoggerFacade<UtilitiesModule> logger, DiscordSocketClient client, IConfiguration configuration)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "playback");
        }

        [SlashCommand("roll", "Roll virtual dice to generate a random number")]
        public async Task Roll([Summary(description: "Number of dice that will be rolled")] int numberOfDice, 
            [Summary(description: "Number of the dice sides")] int sides, 
            [Summary(description: "Seed used for generating random numbers. Leave 0 for Bot to select")] int seed = 0
        )
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Rolling a dice");
            if (numberOfDice < 1)
            {
                await RespondAsync(embed: new EmbedBuilder().WithTitle("Invalid dice amount").WithDescription("You need at least one dice to roll!").Build());
                return;
            }
            else if (numberOfDice > 100)
            {
                await RespondAsync(embed: new EmbedBuilder().WithTitle("Invalid dice amount").WithDescription("You can't throw more than 100 dice!").Build());
                return;
            }
            else if (sides < 2)
            {
                await RespondAsync(embed: new EmbedBuilder().WithTitle("Invalid sides").WithDescription("You need at least two sides on a dice!").Build());
                return;
            }
            else if (sides > 100)
            {
                await RespondAsync(embed: new EmbedBuilder().WithTitle("Invalid sides").WithDescription("Maximum side size is 100!").Build());
                return;
            }

            Random random = seed == 0 ? new Random() : new Random(seed);
            StringBuilder sb = new StringBuilder($"You roll result{ (numberOfDice > 1 ? "s" : "") }: ");
            List<int> numbers = new List<int>();
            for (int i = 0; i < numberOfDice; i++)
            {
                int next = random.Next(sides) + 1;
                numbers.Add(next);
                sb.Append($"{numbers[i]}, ");
            }
            sb.Length -= 2;
            sb.Append($"\r\nFinal Result: {numbers.Sum()}");

            await RespondAsync(embed: new EmbedBuilder().WithTitle("Roll result").WithDescription(sb.ToString()).Build());
        }
    }
}
