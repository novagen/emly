using System.Collections;

namespace Emly
{
	public class EmlSortComparer
	{
		public static IComparer sortByDate()
		{
			return new EmlSortComparerHelper();
		}
	}
}
