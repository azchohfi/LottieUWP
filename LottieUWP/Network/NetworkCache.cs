using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace LottieUWP.Network
{
    /// <summary>
    /// Helper class to save and restore animations fetched from an URL to the app disk cache.
    /// </summary>
    internal class NetworkCache
    {
        private readonly string _url;

        internal NetworkCache(string url)
        {
            _url = url;
        }

        /**
         * If the animation doesn't exist in the cache, null will be returned.
         *
         * Once the animation is successfully parsed, {@link #renameTempFile(FileExtension)} must be
         * called to move the file from a temporary location to its permanent cache location so it can
         * be used in the future.
         */
        internal async Task<KeyValuePair<FileExtension, Stream>?> FetchAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            StorageFile cachedFile = null;
            try
            {
                cachedFile = await GetCachedFileAsync(_url, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            if (cachedFile == null)
            {
                return null;
            }

            Stream inputStream;
            try
            {
                inputStream = await cachedFile.OpenStreamForReadAsync().AsAsyncOperation().AsTask(cancellationToken);
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            FileExtension extension;
            if (cachedFile.Path.EndsWith(".zip"))
            {
                extension = FileExtension.Zip;
            }
            else
            {
                extension = FileExtension.Json;
            }

            Debug.WriteLine("Cache hit for " + _url + " at " + cachedFile.Path, LottieLog.Tag);
            return new KeyValuePair<FileExtension, Stream>(extension, inputStream);
        }

        /// <summary>
        /// Writes an InputStream from a network response to a temporary file. If the file successfully parses
        /// to an composition, {@link #renameTempFile(FileExtension)} should be called to move the file
        /// to its final location for future cache hits.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        internal async Task<StorageFile> WriteTempCacheFileAsync(Stream stream, FileExtension extension, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fileName = FilenameForUrl(_url, extension, true);
            var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting).AsTask(cancellationToken);
            try
            {
                using (var output = await file.OpenStreamForWriteAsync().AsAsyncOperation().AsTask(cancellationToken))
                {
                    await stream.CopyToAsync(output).AsAsyncAction().AsTask(cancellationToken);
                }
            }
            finally
            {
                stream.Dispose();
            }
            return file;
        }

        /// <summary>
        /// If the file created by {@link #writeTempCacheFile(InputStream, FileExtension)} was successfully parsed,
        /// this should be called to remove the temporary part of its name which will allow it to be a cache hit in the future.
        /// </summary>
        /// <param name="extension"></param>
        internal async Task RenameTempFileAsync(FileExtension extension, CancellationToken cancellationToken = default(CancellationToken))
        {
            string fileName = FilenameForUrl(_url, extension, true);
            var file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(fileName).AsTask(cancellationToken);
            string newFileName = file.Name.Replace(".temp", "");
            string oldFilename = file.Name;
            try
            {
                await file.RenameAsync(newFileName).AsTask(cancellationToken);
                Debug.WriteLine($"Copying temp file to real file ({file.Name})", LottieLog.Tag);
            }
            catch
            {
                LottieLog.Warn($"Unable to rename cache file {oldFilename} to {newFileName}.");
            }
        }

        /// <summary>
        /// Returns the cache file for the given url if it exists. Checks for both json and zip.
        /// Returns null if neither exist.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<StorageFile> GetCachedFileAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            var jsonFile = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(FilenameForUrl(url, FileExtension.Json, false)).AsTask(cancellationToken);
            if (jsonFile != null)
            {
                return jsonFile as StorageFile;
            }
            var zipFile = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(FilenameForUrl(url, FileExtension.Zip, false)).AsTask(cancellationToken);
            if (zipFile != null)
            {
                return zipFile as StorageFile;
            }
            return null;
        }

        private static string FilenameForUrl(string url, FileExtension extension, bool isTemp)
        {
            return "lottie_cache_" + Regex.Replace(url, "\\W+", "") + (isTemp ? extension.TempExtension : extension.Extension);
        }
    }
}
