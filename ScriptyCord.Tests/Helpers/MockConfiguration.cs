using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptyCord.Tests.Helpers
{
    public class MockConfiguration
    {
        public static readonly Dictionary<string, string> DefaultConfiguration = new Dictionary<string, string>
        {
            { "ConnectionStrings:DefaultConnection", "" },
            { "discord:token", "" },
            { "store:audioPath", "" },
            { "discord:loggingChannels:guildId", "0" },
            { "discord:loggingChannels:generalId", "0" },
            { "discord:loggingChannels:userManagementId", "0" },
            { "discord:loggingChannels:playbackId", "0" },
            { "discord:logToChannels", "false" }
        };
    }
}
