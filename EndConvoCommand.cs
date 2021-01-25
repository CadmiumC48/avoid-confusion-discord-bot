#region Using Statements - Using Deyimleri
#region .NET
using System;
using System.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
#endregion

#region D#+
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Builders;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using Newtonsoft.Json;
using DSharpPlus.CommandsNext.Entities;
using AvoidConfusion.Entities;
#endregion
#endregion

namespace AvoidConfusion
{
    public partial class AvoidConfusionCommands
    {
        [
            Command("end-discussion"),
            Description("Konuşmayı sonlandırır.")
        ]
        public async Task EndConversation(CommandContext context,[Description("Kapatılacak kanalın adı.")] DiscordChannel channelHash)
        {
            try
            {
                
            var convoChannelCategory = (from entry in context.Guild.Channels
                                         where entry.Value.IsCategory && entry.Value.Name == "AvoidConfusion Kanalları"
                                         select entry.Value).SingleOrDefault();
           var convoChannel= (channelHash.Parent == convoChannelCategory) ? channelHash : null;
            
            await convoChannel?.DeleteAsync(reason:"Konuşma sona erdi.");

            var recordOfConversation = AvoidConfusionDatabase.Database.RegisteredGuilds.Query()
                                                .Include(g => g.Guild)
                                                .Where(g => g.Guild.Id == context.Guild.Id)
                                                .Select(g => g)
                                                .Single()
                                                .Conversations
                                                .Where(c => c.Channel.Id == channelHash.Id)
                                                .Select(c => c)
                                                .Single();
                                                
            recordOfConversation.EndConversation();
            await recordOfConversation.Channel.DeleteAsync(reason:"Konuşma sona erdi.");
            await recordOfConversation.ConversatingUserRole.DeleteAsync(reason:"Konuşma sona erdi.");
            await recordOfConversation.ConversationSupervisior.DeleteAsync(reason: "Konuşma sona erdi.");
            }
            catch(Exception excp)
            {
                var resultEmbed = new DiscordEmbedBuilder();
                resultEmbed.Title = "Beklenmeyen Özel Durum";
                resultEmbed.Color = new(new DiscordColor(255, 10, 10));
                resultEmbed.Description = $"{excp.GetType().FullName} türünden istisna";
                resultEmbed.AddField("Hata Mesajı", excp.Message);
                resultEmbed.AddField("Hata Kaynağı", excp.Source ?? "Bilinmiyor");
                resultEmbed.AddField("Tam Hata Kaynağı", excp.StackTrace ?? "Bilinmiyor");
                resultEmbed.AddField("Yardım linki", excp.HelpLink ?? "(Bulunamadı)");

                await context.RespondAsync(isTTS: true, embed: resultEmbed.Build());
            }
        }
    }
}