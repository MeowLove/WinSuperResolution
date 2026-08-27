using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace WinSuperResolution.Services
{
    internal sealed class JournalService
    {
        internal void Write<T>(string path, T value)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                serializer.WriteObject(stream, value);
            }
        }

        internal T Read<T>(string path)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (T)serializer.ReadObject(stream);
            }
        }

        internal string CreateTimestampedPath(string directory, string prefix)
        {
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, prefix + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N") + ".json");
        }

        internal void CopyAsLatest(string sourcePath, string latestPath)
        {
            string directory = Path.GetDirectoryName(latestPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.Copy(sourcePath, latestPath, true);
        }
    }
}
