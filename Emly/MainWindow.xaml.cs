using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;

namespace Emly
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly ObservableCollection<EmlFile> files;
		private DirectoryReader directoryReader;

		public MainWindow()
		{
			InitializeComponent();
			RestoreWindowState();

			webBrowser.Navigating += WebBrowser_Navigating;

			files = new ObservableCollection<EmlFile>();
			mailList.ItemsSource = files;

			var view = (CollectionView)CollectionViewSource.GetDefaultView(files);
			view.SortDescriptions.Add(new SortDescription("Created", ListSortDirection.Descending));
		}

		private void LoadDirectory()
		{
			if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Directory))
			{
				if (Directory.Exists(Properties.Settings.Default.Directory))
				{
					InitializeFiles();
					return;
				}
			}

			SetPath();
		}

		private void SetPath()
		{
			var pathWindow = new PathWindow(Properties.Settings.Default.Directory)
			{
				Owner = this
			};

			if (pathWindow.ShowDialog() == true)
			{
				if (!string.IsNullOrWhiteSpace(pathWindow.Path) && Directory.Exists(pathWindow.Path))
				{
					Properties.Settings.Default.Directory = pathWindow.Path;
					Properties.Settings.Default.Save();

					InitializeFiles();
				}
			}
		}

		private void InitializeFiles()
		{
			ClearFiles();

			if (directoryReader == null)
			{
				directoryReader = new DirectoryReader(Properties.Settings.Default.Directory, this);
				directoryReader.InitFiles();
			}
			else
			{
				directoryReader.SetPath(Properties.Settings.Default.Directory);
			}
		}

		public void ClearFiles()
		{
			mailList.Dispatcher.Invoke(new Action(() =>
			{
				files.Clear();
			}));
		}

		public void AddFile(string path)
		{
			var file = new EmlFile(path);

			mailList.Dispatcher.Invoke(new Action(() =>
			{
				files.Add(file);
			}));
		}

		public void RemoveFile(string path)
		{
			var file = files.FirstOrDefault(c => c.FilePath == path);

			if (file != null)
			{
				mailList.Dispatcher.Invoke(new Action(() =>
				{
					files.Remove(file);
				}));
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			LoadDirectory();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			webBrowser.Navigating -= WebBrowser_Navigating;
			SaveWindowState();
		}

		private static void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
		{
			if (e.Uri != null)
			{
				e.Cancel = true;
				Process.Start(e.Uri.ToString());
			}
		}

		private void SaveWindowState()
		{
			if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
			{
				Properties.Settings.Default.WindowTop = Application.Current.MainWindow.RestoreBounds.Top;
				Properties.Settings.Default.WindowLeft = Application.Current.MainWindow.RestoreBounds.Left;
				Properties.Settings.Default.WindowHeight = Application.Current.MainWindow.RestoreBounds.Height;
				Properties.Settings.Default.WindowWidth = Application.Current.MainWindow.RestoreBounds.Width;
				Properties.Settings.Default.WindowMaximized = true;
			}
			else
			{
				Properties.Settings.Default.WindowTop = Application.Current.MainWindow.Top;
				Properties.Settings.Default.WindowLeft = Application.Current.MainWindow.Left;
				Properties.Settings.Default.WindowHeight = Application.Current.MainWindow.Height;
				Properties.Settings.Default.WindowWidth = Application.Current.MainWindow.Width;
				Properties.Settings.Default.WindowMaximized = false;
			}

			Properties.Settings.Default.TreeWidth = MainGrid.ColumnDefinitions[0].Width.Value;
			Properties.Settings.Default.EditorWidth = MainGrid.ColumnDefinitions[2].Width.Value;

			Properties.Settings.Default.Save();
		}

		private void RestoreWindowState()
		{
			if (Properties.Settings.Default.WindowTop > 0 || Properties.Settings.Default.WindowLeft > 0)
			{
				Application.Current.MainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
				Application.Current.MainWindow.Top = Properties.Settings.Default.WindowTop;
				Application.Current.MainWindow.Left = Properties.Settings.Default.WindowLeft;
			}

			Application.Current.MainWindow.Height = Properties.Settings.Default.WindowHeight;
			Application.Current.MainWindow.Width = Properties.Settings.Default.WindowWidth;
			Application.Current.MainWindow.WindowState = Properties.Settings.Default.WindowMaximized ? WindowState.Maximized : WindowState.Normal;

			MainGrid.ColumnDefinitions[0].Width = new GridLength(Properties.Settings.Default.TreeWidth, GridUnitType.Star);
			MainGrid.ColumnDefinitions[2].Width = new GridLength(Properties.Settings.Default.EditorWidth, GridUnitType.Star);

			SizeToFit();
			MoveIntoView();
		}

		private static void SizeToFit()
		{
			if (Application.Current.MainWindow.Height > SystemParameters.VirtualScreenHeight)
			{
				Application.Current.MainWindow.Height = SystemParameters.VirtualScreenHeight;
			}

			if (Application.Current.MainWindow.Width > SystemParameters.VirtualScreenWidth)
			{
				Application.Current.MainWindow.Width = SystemParameters.VirtualScreenWidth;
			}
		}

		private static void MoveIntoView()
		{
			if (Application.Current.MainWindow.Top + Application.Current.MainWindow.Height / 2 > SystemParameters.VirtualScreenHeight)
			{
				Application.Current.MainWindow.Top = SystemParameters.VirtualScreenHeight - Application.Current.MainWindow.Height;
			}

			if (Application.Current.MainWindow.Left + Application.Current.MainWindow.Width / 2 > SystemParameters.VirtualScreenWidth)
			{
				Application.Current.MainWindow.Left = SystemParameters.VirtualScreenWidth - Application.Current.MainWindow.Width;
			}

			if (Application.Current.MainWindow.Top < 0)
			{
				Application.Current.MainWindow.Top = 0;
			}

			if (Application.Current.MainWindow.Left < 0)
			{
				Application.Current.MainWindow.Left = 0;
			}
		}

		private void SelectedMailChanged(object sender, SelectionChangedEventArgs e)
		{
			EmlFile item = null;
			var list = (ListView)sender;

			if (list.SelectedItem != null)
			{
				item = (EmlFile)list.SelectedItem;
			}

			if (item != null)
			{
				TextSender.Text = item.Sender;
				TextSubject.Text = item.FormattedSubject;
				TextCreated.Text = $"{item.Created.ToShortDateString()} {item.Created.ToShortTimeString()}";

				var html = item.GetHtml().Replace("<title>", "<meta http-equiv=\"X-UA-Compatible\" content=\"IE edge\" /><title>");
				webBrowser.NavigateToString(html);
			}
			else
			{
				TextSender.Text = string.Empty;
				TextSubject.Text = string.Empty;
				TextCreated.Text = string.Empty;
				webBrowser.NavigateToString(string.Empty);
			}
		}

		private void MenuPath_Click(object sender, RoutedEventArgs e)
		{
			SetPath();
		}

		private void MenuExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
