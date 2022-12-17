// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Bodot.Utilities;

namespace Bodot.Entities
{
	public abstract class Entity
	{
		public virtual void Spawn()
		{

		}

		public virtual void PostSpawn()
		{

		}

		public virtual void KeyValue( Dictionary<string, string> pairs )
		{
			// The origin is already converted to Godot units
			if ( pairs.TryGetValue( "origin", out string originString ) )
			{
				GD.Print( $"'{pairs["classname"]}' origin is: '{originString}'" );
				mRootNode.GlobalPosition = originString.ToVector3();
			}
		}

		public virtual void Destroy()
		{

		}

		public virtual void Think()
		{

		}

		public virtual void PhysicsUpdate( float delta )
		{

		}

		// This is a very very improper way to do this, but I needed it for a quick way of setting a brush model
		public void AddBrushModel( Assets.MapEntity mapEntity )
		{
			mComponents.Add( Assets.MapGeometry.CreateBrushModelNode( mapEntity ) );
			if ( mRootNode == null )
			{
				mRootNode = mComponents.Last() as Node3D;
			}
		}

		protected Node3D mRootNode = null;
		protected List<Node> mComponents = new();
	}
}
