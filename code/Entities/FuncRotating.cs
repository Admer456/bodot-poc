using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bodot.Entities
{
	public class FuncRotating : Entity
	{
		public override void Spawn()
		{
			base.Spawn();
		}

		public override void KeyValue( Dictionary<string, string> pairs )
		{
			base.KeyValue( pairs );
		}

		public override void PostSpawn()
		{
			base.PostSpawn();

			GD.Print( $"FuncRotating: root node pos <{mRootNode.GlobalPosition}>" );
		}

		public override void PhysicsUpdate( float delta )
		{
			base.PhysicsUpdate( delta );
		
			mRootNode.RotateY( delta * 0.5f );
		}
	}
}
