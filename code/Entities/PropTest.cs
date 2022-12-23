
using Bodot.Utilities;
using System;

namespace Bodot.Entities
{
	public class PropTest : Entity
	{
		public override void Spawn()
		{
			base.Spawn();

			mMesh = Nodes.CreateNode<MeshInstance3D>();
			mRootNode = mMesh;
		}

		public override void KeyValue( Dictionary<string, string> pairs )
		{
			base.KeyValue( pairs );

			try
			{
				if ( pairs.TryGetValue( "model", out string modelPath ) )
				{
					mMesh.Mesh = Assets.GLTFModel.Load( modelPath );
				}
			}
			catch( Exception ex )
			{
				GD.PrintErr( $"Exception \"handled\": {ex.Message}" );
				GD.PrintErr( $"Stack trace: {ex.StackTrace}" );
			}
		}

		private MeshInstance3D mMesh;
	}
}
