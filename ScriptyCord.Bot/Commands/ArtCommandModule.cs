using BooruSharp.Booru;
using Discord;
using Discord.Interactions;
using ScriptCord.Bot;
using ScriptCord.Core.DiscordExtensions;

namespace ScriptyCord.Bot.Commands
{
    [Group("art", "Searches for art on selected websites.")]
    public class ArtCommandModule : ScriptyCordCommandModule
    {
        private readonly ILoggerFacade<ArtCommandModule> _logger;
        private readonly IDictionary<Gallery, Func<ABooru>> _boorus;

        public ArtCommandModule(ILoggerFacade<ArtCommandModule> logger)
        {
            _logger = logger;
            _boorus = new Dictionary<Gallery, Func<ABooru>>
            {
                { Gallery.Atfbooru, () => new BooruSharp.Booru.Atfbooru() },
                { Gallery.DanbooruDonmai, () => new BooruSharp.Booru.DanbooruDonmai() },
                { Gallery.Derpibooru, () => new BooruSharp.Booru.Derpibooru() },
                { Gallery.E621, () => new BooruSharp.Booru.E621() },
                { Gallery.E926, () => new BooruSharp.Booru.E926() },
                { Gallery.Gelbooru, () => new BooruSharp.Booru.Gelbooru() },
                { Gallery.Konachan, () => new BooruSharp.Booru.Konachan() },
                { Gallery.Lolibooru, () => new BooruSharp.Booru.Lolibooru() },
                { Gallery.Ponybooru, () => new BooruSharp.Booru.Ponybooru() },
                { Gallery.Sakugabooru, () => new BooruSharp.Booru.Sakugabooru() },
                { Gallery.SankakuComplex, () => new BooruSharp.Booru.SankakuComplex() },
                { Gallery.Twibooru, () => new BooruSharp.Booru.Twibooru() },
                { Gallery.Yandere, () => new BooruSharp.Booru.Yandere() }
            };
        }

        public enum Gallery
        {
            Atfbooru,
            DanbooruDonmai,
            Derpibooru,
            E621,
            E926,
            Gelbooru,
            Konachan,
            Lolibooru,
            Ponybooru,
            Sakugabooru,
            [ChoiceDisplay("Sankaku Complex")]
            SankakuComplex,
            Twibooru,
            Yandere
        }

        [SlashCommand("by-tags", "Searches for random art on selected website with given tags")]
        public async Task FindRandomByTags([Summary(description: "Name of the gallery")] Gallery gallery, [Summary(description: "Tags used for search divided by space")] string tags, [Summary(description: "Amount of returned arts (max 5)")]  int count = 1)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Looking for art with selected tags");

            string[] tagsSeparated = tags.Split(" ");
            if (tagsSeparated.Length == 0 || string.IsNullOrEmpty(tagsSeparated[0]))
            {
                await RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("Invalid tags")
                    .WithDescription("You must provide at least one tag for this command!")
                    .WithColor(Discord.Color.Red)
                    .Build()
                );
                return;
            }
            else if (count < 1)
            {
                await RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("Invalid count")
                    .WithDescription("You can't look for less than one art!")
                    .WithColor(Discord.Color.Red)
                    .Build()
                );
                return;
            }
            else if (count > 5)
            {
                await RespondAsync(embed: new EmbedBuilder()
                    .WithTitle("Invalid count")
                    .WithDescription("You can't look for more than five arts!")
                    .WithColor(Discord.Color.Red)
                    .Build()
                );
                return;
            }

            ABooru booru = _boorus[gallery]();
            BooruSharp.Search.Post.SearchResult[] result = null;
            try
            {
                result = await booru.GetRandomPostsAsync(count, tagsSeparated);
                if (result.Count() == 0)
                {
                    await RespondAsync(
                        embed: new EmbedBuilder()
                            .WithColor(Discord.Color.Blue)
                            .WithTitle($"'{tags.Replace("_", "\\_")}' result from {gallery}")
                            .WithDescription("No results have been found!")
                            .Build()
                    );
                    return;
                }
            }
            catch(Exception e)
            {
                await RespondAsync(
                        embed: new EmbedBuilder()
                            .WithColor(Discord.Color.Blue)
                            .WithTitle($"'{tags.Replace("_", "\\_")}' result from {gallery}")
                            .WithDescription($"Error: {e.Message}")
                            .Build()
                    );
                return;
            }

            IList<Embed> embeds = new List<Embed>();
            foreach (var art in result)
            {
                string description = $"**Tags:** {String.Join(" ", art.Tags).Replace("_", "\\_")}\n\n**Source:** {(art.Source != null ? art.Source : "<not provided>")}\n**{gallery} Link:** {art.PostUrl}";
                EmbedBuilder eb = new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
                        .WithTitle($"'{tags.Replace("_", "\\_")}' result from {gallery}")
                        .WithDescription(description);
                if (art.FileUrl.ToString().Contains("mp4") || art.FileUrl.ToString().Contains("mp4"))
                    eb.WithImageUrl(art.PreviewUrl.ToString());
                else
                    eb.WithImageUrl(art.FileUrl.ToString());

                embeds.Add(eb.Build());
            }

            if(embeds.Count() > 1)
                await RespondAsync(embeds: embeds.ToArray());
            else
                await RespondAsync(embed: embeds.First());
        }
    }
}
