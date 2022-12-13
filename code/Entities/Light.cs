
using Bodot.Utilities;

namespace Bodot.Entities
{
	public class Light : Entity
	{
		public override void Spawn()
		{
			base.Spawn();

			mLight = Nodes.CreateNode<OmniLight3D>();
		}

		public override void KeyValue( Dictionary<string, string> pairs )
		{
			base.KeyValue( pairs );

			if ( pairs.TryGetValue( "targetname", out string targetname ) )
			{
				mLight.Name = targetname;
			}

			mLight.GlobalPosition = pairs["origin"].ToVector3().ToGodot();
			mLight.OmniAttenuation = 1.0f;
			mLight.OmniShadowMode = OmniLight3D.ShadowMode.DualParaboloid;
			mLight.ShadowEnabled = true;

			if ( pairs.ContainsKey( "_light" ) )
			{
				Vector4 lightValues = pairs["_light"].ToVector4();
				if ( lightValues.w == 0.0f )
				{
					lightValues.w = 200.0f;
				}

				string lightString = pairs["_light"];
				GD.Print( $"Light values: <{lightValues}> (string: '{lightString}')" );

				// Convert from [0-255] to [0-1]
				lightValues = lightValues * (1.0f / 255.0f);

				mLight.LightColor = new Color( lightValues.x, lightValues.y, lightValues.z, 1.0f );
				mLight.LightEnergy = lightValues.w;
				mLight.OmniRange = lightValues.w * 10.0f;
			}
		}

		OmniLight3D mLight;
	}
}
