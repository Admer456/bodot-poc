using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bodot.Entities
{
	public class FuncBreakable : Entity
	{
		public override void Spawn()
		{
			base.Spawn();
		}

		public override void PostSpawn()
		{
			base.PostSpawn();

			GD.Print( $"Spawned func_breakable at {mRootNode.GlobalPosition}" );
		}

		public override void KeyValue( Dictionary<string, string> pairs )
		{
			base.KeyValue( pairs );
		}
	}
}
