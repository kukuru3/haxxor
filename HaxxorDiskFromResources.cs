using System.Collections.Generic;

using UnityEngine;

namespace Haxxor {
    public interface IHaxxorDisk {
        DiskFile ReadDisk(string file);
    }

    public class DiskFile {
        public DiskFile(string text) {
            this.fullText = text;
            this.lines = text.Split('\n');
        }
        public string fullText;
        public string[] lines;
    }

    public class HaxxorDiskFromResources : IHaxxorDisk {
        readonly string prefix;
        public HaxxorDiskFromResources(string folder) {
            prefix = folder;
        }

        Dictionary<string, DiskFile> cachedFiles = new Dictionary<string, DiskFile>();

        public DiskFile ReadDisk(string file) {
            
            if (cachedFiles.TryGetValue(file, out var inCache)) return inCache;

            var textResourcePath = file;
            if (!string.IsNullOrWhiteSpace(prefix)) textResourcePath = $"{prefix}/{file}";
            var result = Resources.Load<TextAsset>(textResourcePath);

            if (result != null) {
                var diskFile = new DiskFile(result.text);
                cachedFiles.Add(file, diskFile);
                return diskFile;
            } else {
                cachedFiles.Add(file, null);
                return null;
            }
        }
    }
}