using System;

namespace Emly
{
	public class FileAddedEventArgs : EventArgs
	{
		public string Path { get; private set; }

		public FileAddedEventArgs(string path)
		{
			Path = path;
		}
	}
}
