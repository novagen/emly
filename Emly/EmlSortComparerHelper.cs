using System.Collections;

namespace Emly
{
	public class EmlSortComparerHelper : IComparer
	{
		public int Compare(object x, object y)
		{
			var a = (EmlFile)x;
			var b = (EmlFile)y;

			return a.Created.CompareTo(b.Created);
		}
	}
}
