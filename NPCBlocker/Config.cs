using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NPCBlocker
{
    public class Config
    {
        public static Config Read(string savePath)
        {
            if (!File.Exists(savePath))
            {
                Config config = new Config();
                File.WriteAllText(savePath, JsonConvert.SerializeObject(config, Formatting.Indented));
                return config;
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(savePath));
        }

        public string NPCAddedMessageSilent = "[c/ff0000:Successfully banned {0} (ID: {1}) (Max Amount: {2}).] [c/ffff00:This message is only visible to you.]";

        public string NPCAddedMessage = "[c/ffff00:{0} has banned {1} from spawning (Max Amount: {2}).]";

        public string NPCRemovedMessageSilent = "[c/ff0000:Successfully unbanned {0} (ID: {1}).] [c/ffff00:This message is only visible to you.]";

        public string NPCRemovedMessage = "[c/ffff00:{0} has unbanned {1} from spawning.]";

        public bool BlockBanMessageToOthers = false;
    }
}
