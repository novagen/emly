using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Emly
{
	public class SortableObservableCollection<T> : ObservableCollection<T>
	{
		private readonly Comparer<T> comparer;

		public SortableObservableCollection(Comparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public void Sort()
		{
			Sort(comparer);
		}

		public void Sort(IComparer<T> comparer)
		{
			int i, j;

			T index;

			for (i = 1; i < Count; i++)

			{
				index = this[i];

				j = i;

				while ((j > 0) && (comparer.Compare(this[j - 1], index) == 1))
				{
					this[j] = this[j - 1];
					j = j - 1;
				}

				this[j] = index;
			}
		}
	}
}