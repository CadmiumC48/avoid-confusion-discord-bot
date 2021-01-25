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
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
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

        //Command for starting a conversation.
        //Konuşma başlatmak için komut.
        [
             Command("begin-discussion"),
             Description("Bir konuşma başlatır.")
        ]
        public async Task InitConversation
            (
                CommandContext context,
                [Description("Konuşma başlığı.")]
                string conversationTitle,
                [Description("Konuşma açıklaması.")]
                string conversationDescription
            )
        {
            //The startup variables.
            //Başlangıç değişkenleri.
            DiscordEmbedBuilder resultEmbed = new();
            resultEmbed.Author = new() { Name = "AvoidConfusion - Karmaşaları çözen Discord botu." };
            if ((conversationTitle is "") || (conversationTitle.Length is < 1 or > 250))
            {
                //We need valid values.
                //Geçerli değerlere ihtiyacımız var.
                resultEmbed.Title = "Hata";
                resultEmbed.Color = new(new DiscordColor(255, 0, 0));
                resultEmbed.Description = "Konuşma başlığı boş, 250 karakterden uzun veya 1 karakterden kısa.";
                await context.RespondAsync(embed: resultEmbed.Build());
            }
            else if (conversationDescription is "")
            {
                //An invalid value is given.
                //Yine geçersiz bir değer girildi.
                resultEmbed.Title = "Hata";
                resultEmbed.Color = new(new DiscordColor(255, 0, 0));
                resultEmbed.Description = "Konuşma açıklaması boş";
                await context.RespondAsync(embed: resultEmbed.Build());
            }
            else
            {
                try
                {
                    //Create a category for the conversation channels, if any doesn't exist.
                    //Eğer yoksa, konuşma kanalları için bir kategori açalım.
                    var convoChannelCategory = (from entry in context.Guild.Channels
                                                where entry.Value.IsCategory && entry.Value.Name == "AvoidConfusion Kanalları"
                                                select entry.Value).SingleOrDefault();
                    
                    if(convoChannelCategory == null)
                    {
                        convoChannelCategory = await context.Guild.CreateChannelCategoryAsync
                        (
                            name: "AvoidConfusion Kanalları"
                        );
                    }

                    //Init a conversation channel.
                    //Bir konuşma kanalı açalım.
                    var convoChannel = await context.Guild.CreateChannelAsync
                        (
                            name:$"{conversationTitle.ToLower().Replace(' ','-')}{DateTime.Now.GetHashCode():X8}",
                            parent: convoChannelCategory,
                            type:ChannelType.Text,
                            topic: new Optional<string>(conversationDescription)
                        );
                    //Create a role for the conversation.
                    //Konuşma için bir rol yaratalım.
                    var conversatingRole = await context.Guild.CreateRoleAsync(name:"Konuşmacı",mentionable:(bool?) false,permissions:Permissions.None);
                    
                    await convoChannel.AddOverwriteAsync(conversatingRole,allow:
                    (Permissions.SendMessages|Permissions.SendTtsMessages|Permissions.AttachFiles|Permissions.AccessChannels));

                    await convoChannel.AddOverwriteAsync(context.Guild.EveryoneRole, deny:(Permissions.SendMessages|Permissions.SendTtsMessages|Permissions.AttachFiles|Permissions.AccessChannels));

                    await ((DiscordMember)context.Message.Author).GrantRoleAsync(conversatingRole);
                    
                    var conversationSubSupervisiorRole = await context.Guild.CreateRoleAsync(name:"Konuşma Sorumlusu", mentionable:(bool?)false, permissions:Permissions.None);
                    
                    await convoChannel.AddOverwriteAsync(conversationSubSupervisiorRole,allow:
                    (Permissions.SendMessages|Permissions.SendTtsMessages|Permissions.AttachFiles|Permissions.AccessChannels));
                    await ((DiscordMember)context.Message.Author).GrantRoleAsync(conversationSubSupervisiorRole);

                    //Save the conversation to the database.
                    //Konuşmayı veritabanına kaydedelim.
                    if(!AvoidConfusionDatabase.Database.GuildRegistered(context.Guild))
                    {
                        //Save the guild to the database if it was not registered before.
                        //Eğer sunucu daha önce kaydedilmemişse onu veritabanına kaydedelim.

                        //If a supervisior role wasn't created before, create one.
                        //Eğer bir konuşma sorumlusu rolü daha önce açılmamışsa, bir tane açalım.
                           var conversationSupervisiorRole = await context.Guild.CreateRoleAsync(name: "Konuşma Genel Sorumlusu", mentionable: false, permissions:Permissions.None);
                           await convoChannel.AddOverwriteAsync(conversationSupervisiorRole, allow: (Permissions.SendMessages|Permissions.SendTtsMessages|Permissions.AttachFiles|Permissions.AccessChannels));
                           await context.Guild.Owner.GrantRoleAsync(conversationSupervisiorRole);
                           AvoidConfusionDatabase.Database.RegisterGuild(context.Guild, conversationSupervisiorRole);
                    }
                    var registeredContextGuild = AvoidConfusionDatabase.Database.RegisteredGuilds.Query()
                                                .Include(g => g.Guild)
                                                .Where(g => g.Guild.Id == context.Guild.Id)
                                                .Select(g => g)
                                                .Single();
                    
                    registeredContextGuild.AddConversation(convoChannel, conversationSubSupervisiorRole, conversatingRole, conversationTitle, conversationDescription);

                    resultEmbed
                    .WithTitle("Konuşma başarıyla açıldı.")
                    .WithColor(DiscordColor.SapGreen)
                    .AddField("Konuşma Başlığı",conversationTitle)
                    .AddField("Konuşma Açıklaması", conversationDescription);

                    await context.RespondAsync(embed:resultEmbed.Build());
                }
                catch (Exception excp)
                {
                    //Bir hata oluştu.
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
}
