using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using System;

namespace EmbyFolderSize
{
    public class FolderSizeMetadataProvider : ICustomMetadataProvider<Folder>, IForcedProvider
    {
        public string Name => "Folder Sizes";

        private ILogger _logger;
        private ILibraryManager _libraryManager;
        private Dictionary<long,Task> _pendingUpdates = new Dictionary<long,Task>();
        private InternalItemsQuery _query = new InternalItemsQuery();

        public FolderSizeMetadataProvider(ILogManager logManager, ILibraryManager libraryManager)
        {
            _logger = logManager.GetLogger("FolderSize");
            _libraryManager = libraryManager;

            _libraryManager.ItemAdded += new EventHandler<ItemChangeEventArgs>(AncestorsNeedUpdate);
            _libraryManager.ItemRemoved += new EventHandler<ItemChangeEventArgs>(AncestorsNeedUpdate);
        }

        private void Log(string message)
        {
            _logger.Info(message);
        }

        public async Task<ItemUpdateType> FetchAsync(MetadataResult<Folder> itemResult, MetadataRefreshOptions options, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            Folder folder = itemResult.Item;
            Log("Fetching size of folder " + folder.Name);
            await RequestSizeForFolder(folder, false, true);
            return ItemUpdateType.MetadataImport;
        }

        private async void AncestorsNeedUpdate(object sender, ItemChangeEventArgs e)
        {
            Log("Updating ancestors of item " + e.Item.Name);
            await UpdateAncestors(e.Item);
        }

        private async Task UpdateAncestors(BaseItem item)
        {
            Folder folder = item.Parent;
            if (folder == null) return;
            await RequestSizeForFolder(folder, true, false);
            await UpdateAncestors(folder);
        }

        private async Task RequestSizeForFolder(Folder folder, bool save, bool propagate)
        {
            if (_pendingUpdates.ContainsKey(folder.InternalId))
            {
                await _pendingUpdates[folder.InternalId];
            }
            else
            {
                Task update = CalculateSizeForFolder(folder, save, propagate);
                _pendingUpdates.Add(folder.InternalId, update);
                await update;
            }
        }

        private async Task CalculateSizeForFolder(Folder folder, bool save, bool propagate)
        {
            BaseItem[] children = folder.GetChildren(_query);

            long size = 0;
            foreach (BaseItem child in children)
            {
                if (child is Folder && propagate)
                {
                    await RequestSizeForFolder(child as Folder, true, true);
                }

                size += child.Size;
            }
            if (folder.Size != size)
            {
                folder.Size = size;
                if (save)
                {
                    folder.UpdateToRepository(ItemUpdateType.MetadataImport);
                }
                Log("Set size of folder " + folder.Name + ": " + folder.Size);
            }

            _pendingUpdates.Remove(folder.InternalId);
        }

    }
}
