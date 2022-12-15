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
			const float Speed = 28.0f;
			const float AirSpeed = Speed / 5.0f;
			// 16 units converted to Godot units
			const float Acceleration = 50.0f;
			const float AccelerationMultiplier = 15.0f;
			const float Friction = 4.0f;

			float speedModifier = 1.0f;
			if ( mLastCommands.ActionStates.HasFlag( ClientActions.Sprint ) )
			{
				speedModifier = 1.666f;
			}

			// On ground, apply friction'n'stuff
			KinematicCollision3D collision = new();
			bool onGround = mBody.TestMove( mBody.GlobalTransform, Vector3.Down * delta * 2.0f, collision );
			float groundDot = onGround ? collision.GetNormal().Dot( Vector3.Up ) : 0.0f;

			if ( onGround )
			{
				mBody.Velocity += mLastCommands.MovementDirection * Speed * speedModifier * delta;

				// Cannot jump on slopes
				if ( groundDot >= 0.7f )
				{
					mBody.Velocity *= 0.85f;
	
					if ( mLastCommands.MovementDirection.y > 0.0f )
					{
						mBody.Velocity += Vector3.Up * 160.0f * delta;
					}
				}
			}
			else
			{
				// We'd like the player to air accelerate only if moving perpendicular to the current velocity
				// So the dot product of movedir and velocity needs to be transformed from 1,0,-1 into 0,1,0
				Vector3 moveDirectionFlat = mLastCommands.MovementDirection * new Vector3( 1.0f, 0.0f, 1.0f );
				Vector2 moveDirectionFlatNormalised = mLastCommands.MovementDirection.XZ().Normalized();
				Vector2 bodyVelocityFlat = mBody.Velocity.XZ().Normalized();
				float wishSpeed = Speed * moveDirectionFlatNormalised.Length();
				float wishSpeedClamped = Mathf.Min( wishSpeed, 0.76f );

				/*
				float dotForward = moveDirectionFlatNormalised.Dot( bodyVelocityFlat );
				float absDotSide = Mathf.Sqrt( 1.0f - Mathf.Abs( dotForward ) );

				GD.Print( $"dotForward: {dotForward} (abs side: {absDotSide})" );

				mBody.Velocity += mLastCommands.MovementDirection * new Vector3( 1.0f, 0.0f, 1.0f ) * AirSpeed * absDotSide * delta;
				*/

				float currentSpeed = moveDirectionFlatNormalised.Dot( bodyVelocityFlat );
				float addSpeed = wishSpeedClamped - currentSpeed;
				if ( addSpeed > 0.0f )
				{
					float accelerationSpeed = Mathf.Min( Acceleration * wishSpeed * Friction, addSpeed );
					mBody.Velocity += accelerationSpeed * moveDirectionFlat * delta * AccelerationMultiplier;
				}
			}

			mBody.MoveAndSlide();

			mBody.Velocity += Vector3.Down * 9.81f * delta * (1.0f - Mathf.Abs( groundDot * groundDot ));
			mBody.MoveAndSlide();

			mBody.GlobalRotation = new Vector3( 0.0f, mLastCommands.ViewAngles.y, 0.0f );
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
