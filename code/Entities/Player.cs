// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Bodot.Client;
using Bodot.Utilities;

namespace Bodot.Entities
{
	public class Player : Entity, IPlayerControllable
	{
		public const float PlayerHeight = 1.83f;
		public const float EyeHeight = 1.74f;

		public override void Spawn()
		{
			mBody = Nodes.CreateNode<CharacterBody3D>();
			mBody.GlobalPosition += Vector3.Up * 1.5f;

			mCapsule = new CapsuleShape3D();
			mCapsule.Radius = 0.5f;
			mCapsule.Height = PlayerHeight;

			mShape = mBody.CreateChild<CollisionShape3D>();
			mShape.Shape = mCapsule;
		}

		public override void Destroy()
		{

		}

		public override void Think()
		{

		}

		public override void PhysicsUpdate( float delta )
		{
			// On ground, apply friction'n'stuff
			if ( mBody.TestMove( mBody.GlobalTransform, Vector3.Down * delta * 2.0f ) )
			{
				mBody.Velocity += mLastCommands.MovementDirection * 24.0f * delta;
				mBody.Velocity *= 0.85f;

				if ( mLastCommands.MovementDirection.y > 0.0f )
				{
					mBody.Velocity += Vector3.Up * 160.0f * delta;
				}
			}
			else
			{
				mBody.Velocity += mLastCommands.MovementDirection * 2.0f * delta;
			}

			mBody.MoveAndSlide();

			mBody.Velocity += Vector3.Down * 9.81f * delta;
			mBody.MoveAndSlide();
		}

		public void HandleClientInput( ClientCommands commands )
		{
			mLastCommands = commands;
		}

		public PlayerControllerState GenerateControllerState()
		{
			return new()
			{
				Position = mBody.GlobalPosition + Vector3.Up * EyeHeight * 0.5f,
				Angles = mBody.GlobalRotation
			};
		}

		ClientCommands mLastCommands;
		
		CharacterBody3D mBody;
		CollisionShape3D mShape;
		CapsuleShape3D mCapsule;
	}
}
