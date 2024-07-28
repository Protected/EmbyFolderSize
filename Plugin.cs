using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Common.Plugins;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbyFolderSize
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
        }

        public override string Name => "Folder Size";
        public override string Description => "Adds content disk space usage to the 'Size' field of folders.";

        public override Guid Id => new Guid("A01EED72-7696-49A1-80C0-228EE5603247");
    }
}
