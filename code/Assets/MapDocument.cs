// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Bodot.Geometry;
using Bodot.Text;
using Bodot.Utilities;
using System;

namespace Bodot.Assets
{
	public class MapMaterial
	{
		public string Name = string.Empty;
		public Material EngineMaterial = null;
		public ImageTexture DiffuseTexture = null;

		public int Width => DiffuseTexture?.GetWidth() ?? 128;
		public int Height => DiffuseTexture?.GetHeight() ?? 128;

		public static List<MapMaterial> Materials { get; private set; } = new();

		public readonly static MapMaterial Default = new()
		{
			Name = "Default",
			EngineMaterial = new StandardMaterial3D()
			{
				AlbedoColor = new Color( 1.0f, 0.5f, 0.7f ),
				Roughness = 0.5f,
				Metallic = 0.5f
			}
		};

		public static MapMaterial Load( string materialName )
		{
			Image image = Image.LoadFromFile( $"res://textures/{materialName}.png" );
			if ( image == null )
			{
				GD.PushError( $"Cannot load image 'textures/{materialName}.png'" );
				return Default;
			}

			MapMaterial existingMaterial = Materials.Find( material => material.Name == materialName );
			if ( existingMaterial != null )
			{
				return existingMaterial;
			}

			ImageTexture texture = ImageTexture.CreateFromImage( image );

			StandardMaterial3D material = new();
			material.ResourceName = $"{materialName}";
			material.AlbedoTexture = texture;
			material.Roughness = 1.0f;
			material.Metallic = 0.0f;

			Materials.Add( new()
			{
				Name = materialName,
				EngineMaterial = material,
				DiffuseTexture = texture
			} );
			return Materials.Last();
		}
	}

	public class MapFace
	{
		public Vector3[] PlaneDefinition = new Vector3[3];
		public Plane Plane = new();

		public string MaterialName = string.Empty;
		// XYZ -> axis; W -> offset along axis
		public Vector4[] ProjectionUVS = new Vector4[2];
		public float Rotation = 0.0f;
		public Vector2 Scale = Vector2.One;

		public Vector3 Centre => (PlaneDefinition[0] + PlaneDefinition[1] + PlaneDefinition[2]) / 3.0f;

		public Vector2 CalculateUV( Vector3 point, int imageWidth, int imageHeight, float scale = 39.37f )
		{
			Vector3 axisU = ProjectionUVS[0].ToVector3().ToGodot() * (1.0f / Scale.x) * scale * scale;
			Vector3 axisV = ProjectionUVS[1].ToVector3().ToGodot() * (1.0f / Scale.y) * scale * scale;

			return new()
			{
				x = (point.Dot( axisU ) + ProjectionUVS[0].w) / imageWidth,
				y = (point.Dot( axisV ) + ProjectionUVS[1].w) / imageHeight
			};
		}

		// Filled in later
		public Polygon3D Polygon = new();
		public MapMaterial Material = null;
	}

	public class MapBrush
	{
		public Vector3 Centre = Vector3.Zero;
		//public AABB BoundingBox = new();
		public List<MapFace> Faces = new();
	}

	public class MapEntity
	{
		public Vector3 Centre = Vector3.Zero;
		//public AABB BoundingBox = new();
		public List<MapBrush> Brushes = new();

		public string ClassName = string.Empty;
		public Dictionary<string, string> Pairs = new();
	}

	public class MapDocument
	{
		// ( x1 y1 z1 ) ( x2 y2 z2 ) ( x3 y3 z3 ) texture_name [ ux uy uz offsetX ] [ vx vy vz offsetY ] rotation scaleX scaleY
		private static MapFace ParseFace( Lexer lex )
		{
			MapFace face = new();

			for ( int i = 0; i < 3; i++ )
			{
				// Eat the (
				if ( !lex.Expect( "(", true ) )
				{
					throw new Exception( $"Expected '(' {lex.GetLineInfo()}" );
				}

				face.PlaneDefinition[i].x = StringUtils.ToFloat( lex.Next() );
				face.PlaneDefinition[i].y = StringUtils.ToFloat( lex.Next() );
				face.PlaneDefinition[i].z = StringUtils.ToFloat( lex.Next() );
				
				// Eat the )
				if ( !lex.Expect( ")", true ) )
				{
					throw new Exception( $"Expected ')' {lex.GetLineInfo()}" );
				}
			}

			face.Plane = new Plane( face.PlaneDefinition[0], face.PlaneDefinition[1], face.PlaneDefinition[2] );

			// We could potentially have slashes in here and all kinds of wacky characters
			lex.IgnoreDelimiters = true;
			face.MaterialName = lex.Next();
			lex.IgnoreDelimiters = false;

			if ( face.MaterialName == string.Empty )
			{
				throw new Exception( $"Texture or material is empty {lex.GetLineInfo()}" );
			}

			for ( int i = 0; i < 2; i++ )
			{
				if ( !lex.Expect( "[", true ) )
				{
					throw new Exception( $"Expected '[' {lex.GetLineInfo()}" );
				}

				string token = lex.Next();
				face.ProjectionUVS[i].x = StringUtils.ToFloat( token );
				token = lex.Next();
				face.ProjectionUVS[i].y = StringUtils.ToFloat( token );
				token = lex.Next();
				face.ProjectionUVS[i].z = StringUtils.ToFloat( token );
				token = lex.Next();
				face.ProjectionUVS[i].w = StringUtils.ToFloat( token );

				if ( !lex.Expect( "]", true ) )
				{
					throw new Exception( $"Expected ']' {lex.GetLineInfo()}" );
				}
			}

			face.Rotation = float.Parse( lex.Next() );
			face.Scale.x = float.Parse( lex.Next() );
			face.Scale.y = float.Parse( lex.Next() );

			return face;
		}

		private static MapBrush ParseBrush( Lexer lex )
		{
			MapBrush brush = new();

			// Eat the {
			lex.Next();

			while ( true )
			{
				if ( lex.IsEnd() )
				{
					throw new Exception( $"Unexpected EOF {lex.GetLineInfo()}" );
				}

				// Eat the }
				if ( lex.Expect( "}", true ) )
				{
					break;
				}
				// It's a map face
				else if ( lex.Expect( "(" ) )
				{
					brush.Faces.Add( ParseFace( lex ) );
				}
				// Forgor to add this
				else
				{
					throw new Exception( $"Unexpected token '{lex.Next()}' {lex.GetLineInfo()}" );
				}
			}

			return brush;
		}

		private static MapEntity ParseEntity( Lexer lex )
		{
			MapEntity entity = new();

			while ( true )
			{
				if ( lex.IsEnd() )
				{
					throw new Exception( $"Unexpected EOF {lex.GetLineInfo()}" );
				}

				// Closure of this entity
				if ( lex.Expect( "}", true ) )
				{
					break;
				}
				// New brush
				else if ( lex.Expect( "{" ) )
				{
					entity.Brushes.Add( ParseBrush( lex ) );
				}
				// Key-value pair
				else
				{
					string key = lex.Next();

					lex.IgnoreDelimiters = true;
					string value = lex.Next();
					lex.IgnoreDelimiters = false;

					entity.Pairs.Add( key, value );
				}
			}

			entity.Pairs.TryGetValue( "classname", out entity.ClassName );
			if ( entity.Pairs.TryGetValue( "origin", out string? originString ) )
			{
				var components = originString.Split( ' ' );
				entity.Centre.x = float.Parse( components[0] );
				entity.Centre.y = float.Parse( components[1] );
				entity.Centre.z = float.Parse( components[2] );
			}

			return entity;
		}

		public static MapDocument FromValve220MapFile( string path )
		{
			using FileAccess fileAccess = FileAccess.Open( path, FileAccess.ModeFlags.Read );
			
			MapDocument map = new();
			try
			{
				Lexer lex = new( fileAccess.GetAsText() );
				while ( !lex.IsEnd() )
				{
					string token = lex.Next();

					if ( token == "{" )
					{
						map.MapEntities.Add( ParseEntity( lex ) );
					}
					else if ( token == "}" || token == string.Empty )
					{
						break;
					}
					else
					{
						throw new Exception( $"Unknown token '{token}' {lex.GetLineInfo()}" );
					}
				}
			}
			catch ( Exception exception )
			{
				Console.WriteLine( $"Error while parsing TB map: {exception.Message}" );
				Console.WriteLine( $"Stack trace: {exception.StackTrace}" );
				return null;
			}

			try
			{
				map.MapEntities.ForEach( entity =>
				{
					entity.Brushes.ForEach( brush =>
					{
						brush.Faces.ForEach( face =>
						{
							face.Material = MapMaterial.Load( face.MaterialName );
						} );
					} );
				} );
			}
			catch ( Exception ex )
			{
				GD.PushError( $"Exception: {ex.Message}\n{ex.StackTrace}" );
			}

			return map;
		}

		public string Title = "unknown";
		public string Description = "unknown";
		public List<MapEntity> MapEntities = new();
	}
}
