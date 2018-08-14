using System.Diagnostics;

namespace LottieUWP.Network
{
    /// <summary>
    /// Helpers for known Lottie file types.
    /// </summary>
    public class FileExtension
    {
        public static FileExtension Json = new FileExtension(".json");
        public static FileExtension Zip = new FileExtension(".zip");

        public string Extension { get; }

        private FileExtension(string extension)
        {
            Extension = extension;
        }

        public string TempExtension => ".temp" + Extension;

        public override string ToString()
        {
            return Extension;
        }

        public static FileExtension ForFile(string filename)
        {
            foreach (FileExtension e in new[] { Json, Zip })
            {
                if (filename.EndsWith(e.Extension))
                {
                    return e;
                }
            }
            // Default to Json.
            Debug.WriteLine("Unable to find correct extension for " + filename, LottieLog.Tag);
            return Json;
        }
    }
}
