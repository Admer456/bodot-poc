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

		private class MapRenderSurface
		{
			public List<Vector3> Vertices = new();
			public List<Vector3> Normals = new();
			public List<Vector2> Uvs = new();
			public List<int> VertexIndices = new();
			public int VertexCount = 0;
		}

		private static Node3D CreateMapRenderSurfaceNode( Node3D parent, MapMaterial material, MapRenderSurface surface )
		{
			SurfaceTool builder = new();
			builder.Begin( Mesh.PrimitiveType.Triangles );

			for ( int vertexId = 0; vertexId < surface.Vertices.Count; vertexId++ )
			{
				builder.SetUv( surface.Uvs[vertexId] );
				builder.SetNormal( surface.Normals[vertexId] );
				builder.AddVertex( surface.Vertices[vertexId] );
			}

			surface.VertexIndices.ForEach( index => builder.AddIndex( index ) );
			builder.GenerateTangents();

			builder.SetMaterial( material.EngineMaterial );
			
			ArrayMesh mesh = builder.Commit();

			MeshInstance3D meshInstance = parent.CreateChild<MeshInstance3D>();
			meshInstance.Mesh = mesh;
			meshInstance.Visible = true;
			meshInstance.GlobalPosition = Vector3.Zero;
			meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.DoubleSided;

			StaticBody3D staticBody = meshInstance.CreateChild<StaticBody3D>();
			CollisionShape3D collisionShape = staticBody.CreateChild<CollisionShape3D>();
			collisionShape.Shape = Nodes.CreateCollisionShape( mesh );

			return meshInstance;
		}

		public static Node3D CreateMapNode( MapEntity brushEntity )
		{
			brushEntity.Brushes.ForEach( brush => CreateBrushPolygons( ref brush.Faces ) );

			Dictionary<MapMaterial, MapRenderSurface> renderSurfaces = new();
			
			brushEntity.Brushes.ForEach( brush =>
			{
				brush.Faces.ForEach( face =>
				{
					if ( face.MaterialName == "NULL" )
					{
						return;
					}

					// Subdivide the polygon into triangles
					Polygon3D polygon = face.Polygon;
					if ( !polygon.IsValid() )
					{
						return;
					}

					MapRenderSurface renderSurface = renderSurfaces.GetOrAdd( face.Material );
					for ( int i = 2; i < polygon.Points.Count; i++ )
					{
						renderSurface.VertexIndices.Add( renderSurface.VertexCount );
						renderSurface.VertexIndices.Add( renderSurface.VertexCount + i - 1 );
						renderSurface.VertexIndices.Add( renderSurface.VertexCount + i );
					}
					renderSurface.VertexCount += polygon.Points.Count;

					Vector3 planeNormal = polygon.Plane.Normal;
					polygon.Points.ForEach( position =>
					{
						renderSurface.Uvs.Add( face.CalculateUV( position, face.Material.Width, face.Material.Height ) );
						renderSurface.Normals.Add( planeNormal );
						renderSurface.Vertices.Add( position );
					} );
				} );
			} );

			Node3D parentNode = Nodes.CreateNode<Node3D>();

			for ( int renderSurfaceId = 0; renderSurfaceId < renderSurfaces.Count; renderSurfaceId++ )
			{
				MapMaterial mapMaterial = renderSurfaces.Keys.ElementAt( renderSurfaceId );
				MapRenderSurface surface = renderSurfaces.Values.ElementAt( renderSurfaceId );

				Node3D surfaceNode = CreateMapRenderSurfaceNode( parentNode, mapMaterial, surface );
			}

			return parentNode;
		}
	}
}
