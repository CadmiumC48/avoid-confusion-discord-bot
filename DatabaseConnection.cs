#define _INDEV_

using System;
using System.Json;
using System.Collections.Generic;
using System.Linq;
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

using AvoidConfusion.Entities;

namespace AvoidConfusion.Entities
{
    public struct RegisteredGuild
    {
        public ObjectId ID {get; set;}
        public DiscordGuild Guild{get; set;}
        public DiscordRole GeneralSupervisior {get; set;}
        public List<Conversation> Conversations{get; set;}

        public RegisteredGuild(DiscordGuild guild, DiscordRole supervisior)
        {
            Guild = guild;
            GeneralSupervisior = supervisior;
            ID = ObjectId.NewObjectId();
            Conversations = new();
        }

        public void AddConversation(DiscordChannel channel, DiscordRole conversationSupervisior, DiscordRole conversatingUserRole, string topic, string description)
        {
            Conversations.Add(new(channel,conversationSupervisior,conversatingUserRole,topic,description));
        }
    }
    public struct Conversation
    {
        public ObjectId ID {get; set;}
        public DiscordChannel Channel {get; set;}
        public DiscordRole ConversationSupervisior {get; set;}
        public DiscordRole ConversatingUserRole {get; set;}
        public DateTime FiredOn {get; set;}
        public DateTime? EndedOn {get; set;}
        public bool IsActive {get; set;}
        public string Topic {get; set;}
        public string Description {get; set;} 

        public Conversation(DiscordChannel channel, DiscordRole conversationSupervisior, DiscordRole conversatingUserRole, string topic, string description)
        {
            ID = ObjectId.NewObjectId();
            Channel = channel;
            ConversationSupervisior = conversationSupervisior;
            ConversatingUserRole = conversatingUserRole;
            FiredOn = ID.CreationTime;
            EndedOn = null;
            IsActive = true;
            Topic = topic;
            Description = description;
        }

        public void EndConversation()
        {
            EndedOn = (DateTime?) DateTime.Now;
            IsActive = false;
        }
    }
}
namespace AvoidConfusion
{
    public class AvoidConfusionDatabase
    {
        private static AvoidConfusionDatabase databaseObject;
        private static LiteDatabase database;
        public ILiteCollection<AvoidConfusion.Entities.RegisteredGuild> RegisteredGuilds{get; set;}
        public AvoidConfusionDatabase()
        {
            RegisteredGuilds =(ILiteCollection<AvoidConfusion.Entities.RegisteredGuild>) AvoidConfusionDatabase.database.GetCollection<RegisteredGuild>("RegisteredGuilds");
        }
        static AvoidConfusionDatabase()
        {
            database = new LiteDatabase(
                Path.Combine
                (
                    Environment.GetEnvironmentVariable("UserProfile"),
                    
                    @"config\AvoidConfusion\AvoidConfusion.db"
                )
            );
        }
        ~AvoidConfusionDatabase()
        {
            RegisteredGuilds = null;
            database.Dispose();
        }
        public static AvoidConfusionDatabase Database 
        {
            get
            {
                if(databaseObject == null)
                    databaseObject = new();
                
                return databaseObject;
            }
        }
        public void RegisterGuild(DiscordGuild guild, DiscordRole supervisior)
        {
            RegisteredGuilds.Insert(new RegisteredGuild(guild, supervisior));
        }
        public bool GuildRegistered(DiscordGuild guild)
        {
            var results = RegisteredGuilds.Query()
                            .Include(g => g.Guild)
                            .Where(g => g.Guild == guild)
                            .Select(g => g);

            return (results is not null);
        }
    }
}

