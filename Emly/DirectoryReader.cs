using System.IO;

namespace Emly
{
	public class DirectoryReader
	{
		private readonly FileInputMonitor fileInputMonitor;
		private string path;
		private readonly MainWindow form;

		public DirectoryReader(string path, MainWindow form)
		{
			this.path = path;
			this.form = form;

			fileInputMonitor = new FileInputMonitor(path);
			fileInputMonitor.FileAdded += FileInputMonitor_FileAdded;
			fileInputMonitor.FileRemoved += FileInputMonitor_FileRemoved;
		}

		private void FileInputMonitor_FileAdded(object sender, FileAddedEventArgs e)
		{
			form.AddFile(e.Path);
		}

		private void FileInputMonitor_FileRemoved(object sender, FileAddedEventArgs e)
		{
			form.RemoveFile(e.Path);
		}

		public void InitFiles()
		{
			foreach (string file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
			{
				form.AddFile(file);
			}
		}

		public void SetPath(string path)
		{
			fileInputMonitor.StopWatching();
			form.ClearFiles();

			this.path = path;
			fileInputMonitor.SetPath(path);

			InitFiles();

			fileInputMonitor.StartWatching();
		}
	}
}
