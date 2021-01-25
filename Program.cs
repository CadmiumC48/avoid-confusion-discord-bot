#define _INDEV_

using System;
using System.Json;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.NetworkInformation;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using Newtonsoft.Json;

using LiteDB;


namespace AvoidConfusion
{
    class Program
    {
        static int Main
        (string[] args)
        {
            //MainAsync'i çağıralım.
           return MainAsync().GetAwaiter().GetResult();
        }
        static async Task<int> MainAsync
        (                               )
        {
            try
            {
                //Get the configuration.
                //Ayarlarımızı alalım.
                StreamReader configFile =
                    new
                    (
                        Path.Combine
                        (
                            Environment.GetEnvironmentVariable("UserProfile"),
                            @"config\AvoidConfusion\config.json"
                        )
                    );

                AvoidConfusionConfiguration configuration =
                    JsonConvert.DeserializeObject<AvoidConfusionConfiguration>
                    (
                        //Read the config file to the end.
                        //Ayarlar dosyasını sonuna kadar okuyalım.
                        configFile.ReadToEnd()
                    );

                //Create the Discord Client.
                //Discord İstemcimiz'i yaratalım.
                var discord = new DiscordClient(configuration);
                var commands= discord.UseCommandsNext(new() { StringPrefixes = new[] { "&", "@AvoidConfusion#4395" } });

                //Add some actions for the bot.
                //Bota biraz işlevsellik ekleyelim.
                commands.RegisterCommands<AvoidConfusionCommands>();

                //Set the status of the bot.
                //Botumuzun durumunu ayarlayalım.
                discord.Ready += SetClientStatusWhenReady;

                //Wait for the Discord Client to connect (WTF).
                //Discord İstemcimiz'in bağlanmasını bekleyelim (AMK).
                await discord.ConnectAsync();




                //Delay for an indefinite span of time.
                //Sonsuza kadar bekleyelim.
                await Task.Delay(-1);
            }
            catch(Exception excp)
            {
                Debug.WriteLine($"{excp}\n");
                Trace.WriteLine($"{excp}\n");
                #if DEBUG
                Console.Out.WriteLine($"{excp}\n");
                #endif
                return excp.GetHashCode();
            }
            
            return 0;
        }

        ///<summary>
        /// Updates the status when the client is ready.
        ///</summary>
        private static async Task SetClientStatusWhenReady(DiscordClient sender, ReadyEventArgs e)
        {
            await sender.UpdateStatusAsync
            (new DiscordActivity
                (
                    "Karmaşalarınızı",
                    ActivityType.Watching
                )
           );
        }
    }
}
