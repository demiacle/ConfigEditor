using Demiacle.OptionPageCreator.OptionPage;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demiacle.OptionPageCreator {
    public class ModConfig {
        public string configName;
        public List<ModOption> options;
        public JObject json;
        public IModHelper helper;

        public ModConfig( string configName, List<ModOption> options, JObject json, IModHelper helper ) {
            this.configName = configName;
            this.options = options;
            this.json = json;
            this.helper = helper;
        }

    }
}
