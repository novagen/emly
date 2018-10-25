using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Emly
{
	public class FileInputMonitor
	{
		private readonly FileSystemWatcher fileSystemWatcher;

		public event EventHandler<FileAddedEventArgs> FileAdded;
		public event EventHandler<FileAddedEventArgs> FileRemoved;

		protected virtual void OnFileAdded(FileAddedEventArgs e)
		{
			FileAdded?.Invoke(this, e);
		}

		protected virtual void OnFileRemoved(FileAddedEventArgs e)
		{
			FileRemoved?.Invoke(this, e);
		}

		public FileInputMonitor(string path, bool start = true)
		{
			fileSystemWatcher = new FileSystemWatcher(path)
			{
				EnableRaisingEvents = start,
				IncludeSubdirectories = true,
				//NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
				Filter = "*.eml"
			};

			fileSystemWatcher.Created += Created;
			fileSystemWatcher.Deleted += Deleted;
			fileSystemWatcher.Renamed += Renamed;
			fileSystemWatcher.Changed += Changed;
		}

		public void StartWatching()
		{
			fileSystemWatcher.EnableRaisingEvents = true;
		}

		public void StopWatching()
		{
			fileSystemWatcher.EnableRaisingEvents = false;
		}

		public void SetPath(string path)
		{
			fileSystemWatcher.Path = path;
		}

		private void Renamed(object sender, RenamedEventArgs e)
		{
			Debug.WriteLine("Renamed", e.Name);
		}

		private void Changed(object sender, FileSystemEventArgs e)
		{
			Debug.WriteLine("Changed", e.Name);
		}

		private void Created(object sender, FileSystemEventArgs e)
		{
			Debug.WriteLine("Created", e.FullPath);
			ProcessFile(e.FullPath, true);
		}

		private void Deleted(object sender, FileSystemEventArgs e)
		{
			Debug.WriteLine("Deleted", e.FullPath);
			ProcessFile(e.FullPath, false);
		}

		public static bool IsFileReady(string filename)
		{
			try
			{
				using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					return inputStream.Length > 0;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void ProcessFile(string fileName, bool added)
		{
			var args = new FileAddedEventArgs(fileName);

			if (added)
			{
				while (!IsFileReady(fileName)) {
					Thread.Sleep(500);
				}

				OnFileAdded(args);
			}
			else
			{
				OnFileRemoved(args);
			}
		}
	}
}