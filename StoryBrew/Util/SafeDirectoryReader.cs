﻿using System;
using System.IO;

namespace StoryBrew.Util
{
    public class SafeDirectoryReader : IDisposable
    {
        public string Path { get; }

        public SafeDirectoryReader(string targetDirectory)
        {
            var backupDirectory = targetDirectory + ".bak";
            this.Path = Directory.Exists(targetDirectory) || !Directory.Exists(backupDirectory) ? targetDirectory : backupDirectory;
        }

        public string GetPath(string path)
            => System.IO.Path.Combine(Path, path);

        public void Dispose()
        {
        }
    }
}
