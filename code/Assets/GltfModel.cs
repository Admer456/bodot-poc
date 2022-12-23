
namespace Bodot.Assets
{
	public class GLTFModel
	{
		public static ArrayMesh Load( string path )
		{
			if ( !FileAccess.FileExists( path ) )
			{
				GD.PushWarning( $"Cannot find image '{path}', gonna search in local resources..." );
				path = $"res://{path}";
			}
			if ( !FileAccess.FileExists( path ) )
			{
				GD.PushError( $"Cannot find image '{path}', oops" );
				// Todo: default "error" model
				return null;
			}

			GLTFState state = new();
			GLTFDocument document = new();
			if ( document.AppendFromFile( path, state ) != Error.Ok )
			{
				GD.PrintErr( "Boohoo" );
				return null;
			}

			GD.Print( $"This thing has {state.Meshes.Count} meshes" );

			return state.Meshes[0].Mesh.GetMesh();
		}
	}
}
