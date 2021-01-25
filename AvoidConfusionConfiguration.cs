#region .NET
using System;
using System.Text;
using System.Text.Json;
using System.Data;
#endregion
#region D#+
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
#endregion
#region NewtonSoft
using Newtonsoft.Json.Serialization;
#endregion
namespace AvoidConfusion
{
    public enum ApplicationState:byte
    {
        Release = 4,
        Indev = 1,
        Infdev = 2,
        Alpha = 1,
        Beta = 2,
        ReleaseCandidate = 3,
        PreRelease = 3,
        Snapshot = 0
    }
    public class AvoidConfusionConfiguration
    {
        public AvoidConfusionConfiguration(Version version = default, ApplicationState applicationState = default)
        {
            Version = version;
            ApplicationState = applicationState;
        }
        public string Token { get; init; }

        public string DatabaseUsername { get; init; }

        public string DatabasePassword { get; init; }



        public TokenType TokenType { get; init; } = TokenType.Bot;

        public ApplicationState ApplicationState { get; init; }
        
        public Version Version { get; init; }

        public static implicit operator DiscordConfiguration(AvoidConfusionConfiguration configuration) => 
        new()
        {
            Token = configuration.Token,
            TokenType = configuration.TokenType
        };

        

    }
}