// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Bodot.Utilities
{
	public static class PlaneExtensions
	{
		public static Vector3 GetClosestAxis( this Plane plane )
		{
			Vector3 normal = plane.Normal.Abs();

			if ( normal.z >= normal.x && normal.z >= normal.y )
			{
				return Vector3.Forward;
			}

			if ( normal.x >= normal.y )
			{
				return Vector3.Right;
			}

			return Vector3.Up;
		}
	}

	public static class VectorExtensions
	{
		public static Vector3 ToGodot( this Vector3 vector, float scale = 1.0f / 39.37f )
		{
			return new Vector3( -vector.y, vector.z, -vector.x ) * scale;
		}
		public static Vector3 Average( this Vector3[] vectors )
		{
			if ( vectors.Length == 0 )
			{
				return Vector3.Zero;
			}

			Vector3 sum = Vector3.Zero;
			for ( int i = 0; i < vectors.Length; i++ )
			{
				sum += vectors[i];
			}

			return sum / (float)vectors.Length;
		}
		public static Vector3 Forward( this Transform3D transform )
		{
			return -transform.basis.z;
		}
		public static Vector3 Back( this Transform3D transform )
		{
			return transform.basis.z;
		}
		public static Vector3 Right( this Transform3D transform )
		{
			return transform.basis.x;
		}
		public static Vector3 Left( this Transform3D transform )
		{
			return -transform.basis.x;
		}
		public static Vector3 Up( this Transform3D transform )
		{
			return transform.basis.y;
		}
		public static Vector3 Down( this Transform3D transform )
		{
			return -transform.basis.y;
		}
		public static Vector3 ToVector3( this Vector4 vector )
		{
			return new Vector3( vector.x, vector.y, vector.z );
		}
		public static Vector2 ToVector2( this Vector4 vector )
		{
			return new Vector2( vector.x, vector.y );
		}
		public static Vector2 ToVector2( this Vector3 vector )
		{
			return new Vector2( vector.x, vector.y );
		}
		public static Vector2 XZ( this Vector3 vector )
		{
			return new Vector2( vector.x, vector.z );
		}
	}

	public static class Nodes
	{
		// Creates a node and attaches it to the Main world node
		public static T CreateNode<T>() where T : Node, new()
		{
			return Main.Instance.CreateChild<T>();
		}

		// Creates a CollisionShape3D and creates either a ConcavePolygonShape3D or ConvexPolygonShape3D
		// depending on the concave parameter
		public static Shape3D CreateCollisionShape( ArrayMesh mesh, bool concave = true )
		{
			if ( !concave )
			{
				GD.PushWarning( "Nodes.CreateCollisionShape: 'concave = false' is not implemented yet, switching to true" );
			}

			// The collision mesh is a bunch of triangles, organised in triplets of Vector3
			List<Vector3> collisionMesh = new();

			// Since ArrayMesh is kinda difficult to get data from *directly*, we use MeshDataTool
			for ( int surfaceId = 0; surfaceId < mesh._Surfaces.Count; surfaceId++ )
			{
				MeshDataTool tool = new();
				tool.CreateFromSurface( mesh, surfaceId );

				for ( int faceId = 0; faceId < tool.GetFaceCount(); faceId++ )
				{
					for ( int vertexNum = 0; vertexNum < 3; vertexNum++ )
					{
						int vertexId = tool.GetFaceVertex( faceId, vertexNum );
						collisionMesh.Add( tool.GetVertex( vertexId ) );
					}
				}
			}

			ConcavePolygonShape3D meshShape = new();
			meshShape.Data = collisionMesh.ToArray();
			
			return meshShape;
		}
	}

	public static class NodeExtensions
	{
		// Creates a node and attaches it to this node
		public static T CreateChild<T>( this Node parent ) where T : Node, new()
		{
			T child = new T();
			parent.AddChild( child );
			return child;
		}
	}

	public static class Node3DExtensions
	{
		public static Vector3 Forward( this Node3D node )
		{
			return node.GlobalTransform.Forward();
		}
		public static Vector3 Right( this Node3D node )
		{
			return node.GlobalTransform.Right();
		}
		public static Vector3 Up( this Node3D node )
		{
			return node.GlobalTransform.Up();
		}
		public static Vector3 ForwardLocal( this Node3D node )
		{
			return node.Transform.Forward();
		}
		public static Vector3 RightLocal( this Node3D node )
		{
			return node.Transform.Right();
		}
		public static Vector3 UpLocal( this Node3D node )
		{
			return node.Transform.Up();
		}
	}
}
