using System;
using System.IO;
using System.Windows;

namespace Emly
{
	/// <summary>
	/// Interaction logic for PathWindow.xaml
	/// </summary>
	public partial class PathWindow : Window
	{
		public PathWindow(string currentPath = "")
		{
			InitializeComponent();
			PathInput.Text = currentPath;
		}

		private void DialogOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void DialogBrowse_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				if (!string.IsNullOrWhiteSpace(PathInput.Text) && Directory.Exists(PathInput.Text))
				{
					dialog.SelectedPath = PathInput.Text;
				}

				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					PathInput.Text = dialog.SelectedPath;
				}
			}
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			PathInput.SelectAll();
			PathInput.Focus();
		}

		public string Path
		{
			get { return PathInput.Text; }
		}
	}
}