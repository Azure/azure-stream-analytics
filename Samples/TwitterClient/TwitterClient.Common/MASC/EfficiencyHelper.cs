using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Mark Rowe - MASC - Mark.Rowe@Microsoft.com - www.AugmentedDeveloper.com
//Created side library for more efficient parsing of words. 
namespace TwitterClient.Common.MASC
{
	public static class EfficiencyHelper
	{
		public static bool StartsWithAndContains(this string toSearch, string startsWith, string contains)
		{
			return toSearch.StartsWithIgnoreCase(startsWith) && toSearch.ContainsIgnoreCase(contains);
		}

		public static bool EqualsIgnoreCase(this string toSearch, string equals)
		{
			return string.Equals(toSearch, equals, StringComparison.OrdinalIgnoreCase);
		}

		public static bool StartsWithIgnoreCase(this string toSearch, string startsWith)
		{
			return toSearch.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);
		}

		public static bool ContainsIgnoreCase(this string toSearch, string contains)
		{
			return (contains == string.Empty || toSearch.IndexOf(contains, StringComparison.OrdinalIgnoreCase) != -1);
		}
	}
}
