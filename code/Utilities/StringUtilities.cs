// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Bodot.Utilities
{
	public static class StringUtils
	{
		public static bool IsNumeric( this char c )
		{
			return c >= '0' && c <= '9';
		}

		public static float ToFloat( string token )
		{
			return float.Parse( token, System.Globalization.CultureInfo.InvariantCulture );
		}
	}
}
