using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmbyFolderSize
{
    public class FolderSizePostScanTask : ILibraryPostScanTask {

        private ILibraryManager _libraryManager;
        private ILogger _logger;

        public FolderSizePostScanTask(ILibraryManager libraryManager, ILogManager logManager)
        {
            _libraryManager = libraryManager;
            _logger = logManager.GetLogger("FolderSize");
        }

        private void log(string message)
        {
            _logger.Info(message);
        }

        public async Task Run(IProgress<double> progress, CancellationToken cancellationToken)
        {

            await CalculateItemSize(_libraryManager.GetUserRootFolder());

        }

        private async Task CalculateItemSize(BaseItem item)
        {

            long size = 0;

            BaseItem[] children = item.GetChildrenForValidationSorted();
            foreach (BaseItem child in children)
            {
                
                if (child.Size > 0 && !child.IsFolder)
                {
                    size += child.Size;
                    continue;
                }

                await CalculateItemSize(child);

                if (!item.IsFolder)
                {
                    continue;
                }

                if (child.Size > 0)
                {
                    size += child.Size;
                }

            }

            if (item.IsFolder && size > 0 && size != item.Size)
            {
                item.Size = size;
                item.UpdateToRepository(ItemUpdateType.None);
                log("Updated size for " + item.Name + ": " + item.Size);
            }

        }

    }
}
