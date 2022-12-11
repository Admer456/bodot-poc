// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

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

		public virtual void Destroy()
		{

		}

		public virtual void Think()
		{

		}

		public virtual void PhysicsUpdate( float delta )
		{

		}

		protected List<Node> mComponents = new();
	}
}
