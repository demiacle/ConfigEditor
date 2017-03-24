using Demiacle.OptionPageCreator.OptionPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demiacle.OptionPageCreator {
    public class ModConfig {
        public string configName;
        public List<ModOption> options;

        public ModConfig( string configName, List<ModOption> options ) {
            this.configName = configName;
            this.options = options;
        }

    }
}
