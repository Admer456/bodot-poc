// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Bodot.Geometry;
using Bodot.Utilities;

namespace Bodot.Assets
{
	public class MapGeometry
	{
		private static void CreateBrushPolygons( ref List<MapFace> faces, float radius = 2048.0f, float scale = 1.0f / 39.37f )
		{
			for ( int i = 0; i < faces.Count; i++ )
			{
				Plane plane = faces[i].Plane;

				// Create a polygon in the centre of the world
				Polygon3D poly = new Polygon3D( plane, radius );

				// Then align its centre to the centre of this face... if we got any
				Vector3 shift = faces[i].Centre - poly.Origin;
				poly.Shift( shift );

				// Intersect current face with all other faces
				for ( int p = 0; p < faces.Count; p++ )
				{
					Plane intersector = faces[p].Plane;
					if ( i == p )
					{
						continue;
					}

					var splitResult = poly.Split( intersector );
					if ( splitResult.DidIntersect )
					{
						// Modify the polygon we started off from
						poly = splitResult.Back ?? poly;
					}
				}

				// Shift it back
				poly.Shift( -shift );

				// Axis:    Quake: Godot:
				// Forward  +X     -Z
				// Right    -Y     +X
				// Up       +Z     +Y
				poly.Points = poly.Points.Select( p => p = new Vector3( -p.y, p.z, -p.x ) * scale ).ToList();

				// Finally add the subdivided polygon
				faces[i].Polygon = poly;
			}
		}

		public static MeshInstance3D CreateMapNode( MapEntity brushEntity )
		{
			SurfaceTool builder = new SurfaceTool();
			builder.Begin( Mesh.PrimitiveType.Triangles );

			brushEntity.Brushes.ForEach( brush => CreateBrushPolygons( ref brush.Faces ) );

			List<int> vertexIndices = new();
			int vertexIndexOffset = 0;
			brushEntity.Brushes.ForEach( brush =>
			{
				brush.Faces.ForEach( face =>
				{
					Polygon3D polygon = face.Polygon;

					if ( !polygon.IsValid() )
					{
						return;
					}

					// Subdivide the polygon into triangles
					for ( int i = 2; i < polygon.Points.Count; i++ )
					{
						vertexIndices.Add( vertexIndexOffset );
						vertexIndices.Add( vertexIndexOffset + i - 1 );
						vertexIndices.Add( vertexIndexOffset + i );
					}
					vertexIndexOffset += polygon.Points.Count;

					Vector3 planeNormal = polygon.Plane.Normal;
					polygon.Points.ForEach( position =>
					{
						builder.SetUv( face.CalculateUV( position, face.Material.Width, face.Material.Height ) );
						builder.SetNormal( planeNormal );
						builder.AddVertex( position );
					} );
				} );
			} );

			builder.GenerateTangents();

			vertexIndices.ForEach( index => builder.AddIndex( index ) );

			try
			{
				// TODO: textures
				builder.SetMaterial( MapMaterial.Materials[0].EngineMaterial );
			}
			catch ( System.Exception ex )
			{
				GD.PushError( $"Exception: {ex.Message}\n{ex.StackTrace}" );
			}

			ArrayMesh mesh = builder.Commit();
			if ( mesh == null )
			{
				GD.PushWarning( $"Failed to extract geometry data from brush entity '{brushEntity.ClassName}' <{brushEntity.Centre}> (couldn't build mesh)" );
				return null;
			}
			
			MeshInstance3D meshInstance = Nodes.CreateNode<MeshInstance3D>();
			meshInstance.Mesh = mesh;
			meshInstance.Visible = true;
			meshInstance.GlobalPosition = Vector3.Zero;
			meshInstance.Name = "Boi";

			StaticBody3D staticBody = meshInstance.CreateChild<StaticBody3D>();
			staticBody.Name = "Boi_StaticBody";
			
			CollisionShape3D collisionShape = staticBody.CreateChild<CollisionShape3D>();
			collisionShape.Shape = Nodes.CreateCollisionShape( mesh );

			return meshInstance;
		}
	}
}
