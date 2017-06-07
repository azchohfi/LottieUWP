using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace LottieUWP.Sample
{
    static class AssetUtils
    {
        public static async Task<IList<StorageFile>> GetJsonAssets()
        {
            var localizationDirectory = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets");

            return await GetJsonAssetsFromFolder(localizationDirectory);
        }

        private static async Task<IList<StorageFile>> GetJsonAssetsFromFolder(StorageFolder folder)
        {
            var files = new List<StorageFile>();
            foreach (var asset in await folder.GetItemsAsync())
            {
                if (asset is StorageFile file && file.Name.ToLower().EndsWith(".json", StringComparison.Ordinal))
                {
                    files.Add(file);
                }
                else if (asset is StorageFolder storageFolder)
                {
                    files.AddRange(await GetJsonAssetsFromFolder(storageFolder));
                }
            }
            return files;
        }
    }
}