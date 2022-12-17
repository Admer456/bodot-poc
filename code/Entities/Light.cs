
using Bodot.Utilities;

namespace Bodot.Entities
{
	public class Light : Entity
	{
		public override void Spawn()
		{
			base.Spawn();

			mLight = Nodes.CreateNode<OmniLight3D>();
			mRootNode = mLight;
		}

		public override void KeyValue( Dictionary<string, string> pairs )
		{
			base.KeyValue( pairs );

			if ( pairs.TryGetValue( "targetname", out string targetname ) )
			{
				mLight.Name = targetname;
			}

			mLight.OmniAttenuation = 1.0f;
			mLight.OmniShadowMode = OmniLight3D.ShadowMode.Cube;
			mLight.ShadowEnabled = true;
			mLight.ShadowBias = 0.02f;
			mLight.ShadowNormalBias = 1.2f;

			if ( pairs.ContainsKey( "_light" ) )
			{
				Vector4 lightValues = pairs["_light"].ToVector4();
				if ( lightValues.w == 0.0f )
				{
					lightValues.w = 300.0f;
				}

				string lightString = pairs["_light"];
				GD.Print( $"Light values: <{lightValues}> (string: '{lightString}')" );

				// Convert from [0-255] to [0-1]
				lightValues = lightValues * (1.0f / 255.0f);

				mLight.LightColor = new Color( lightValues.x, lightValues.y, lightValues.z, 1.0f );
				mLight.LightEnergy = lightValues.w;
				mLight.OmniRange = Mathf.Sqrt( lightValues.w ) * 10.0f;
			}
			else
			{
				mLight.LightColor = Color.Color8( 255, 255, 255 );
				mLight.LightEnergy = 1.0f;
				mLight.OmniRange = 6.5f;
			}

			mLight.LightIndirectEnergy = 1.5f;
			mLight.LightVolumetricFogEnergy = 1.5f;
			mLight.LightSize = 1.5f;
			mLight.LightAngularDistance = 0.1f;
		}

		OmniLight3D mLight;
	}
}
