// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using System.Reflection;
using System.Runtime.Loader;

namespace Bodot
{
	internal sealed class BodotAssemblyLoadContext : AssemblyLoadContext
	{
		protected override Assembly Load( AssemblyName assemblyName )
		{
			switch ( assemblyName.Name )
			{
				case "BodotPoc1": return Assembly.GetExecutingAssembly();
				case "GodotSharp": return typeof( GD ).Assembly;
			}

			return null;
		}
	}
}
