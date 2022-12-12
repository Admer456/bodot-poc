// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Bodot.Assets;
using Bodot.Utilities;
using System;
using System.Diagnostics;

// This is the entry point to our "engine"
// Should inherit from MainLoop here, however it doesn't work yet
// Look: https://github.com/godotengine/godot/issues/69795
public partial class Main : Node3D
{
	public static Node3D Instance { get; private set; } = null;
	public static MapDocument Map { get; private set; } = null;
	private static Node3D Worldspawn = null;

	private void LoadMap( string path )
	{
		Map = MapDocument.FromValve220MapFile( $"{path}.map" );
		if ( Map == null )
		{
			return;
		}

		Map.MapEntities.ForEach( entity => SpawnEntity( entity ) );

		Worldspawn = MapGeometry.CreateMapNode( Map.MapEntities[0] );
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;

		// The scene is completely empty before this point.
		// This loads TrenchBroom/J.A.C.K./Hammer maps so long as they're in the Valve220 format
		LoadMap( "maps/tunnel_jack2" );

		// Todo: load a map or something and spawn
		// players on info_player_start
		mPlayerEntity = new();
		mPlayerEntity.Spawn();
		mEntities.Add( mPlayerEntity );

		// Clients use controller entities to actually
		// interact w/ the game world. In theory this can even
		// be an NPC or some vehicle, but right now we have
		// a dedicated Player entity
		mClient = new() { Controller = mPlayerEntity };
	}

	private void SpawnEntity( MapEntity entity )
	{
		try
		{
			if ( entity.ClassName == "light" )
			{
				OmniLight3D light = Nodes.CreateNode<OmniLight3D>();
				if ( entity.Pairs.TryGetValue( "targetname", out string targetname ) )
				{
					light.Name = targetname;
				}
				
				light.GlobalPosition = entity.Centre.ToGodot();
				light.OmniAttenuation = 1.0f;
				light.OmniShadowMode = OmniLight3D.ShadowMode.DualParaboloid;
				light.ShadowEnabled = true;

				if ( entity.Pairs.ContainsKey( "_light" ) )
				{
					Vector4 lightValues = entity.Pairs["_light"].ToVector4();
					if ( lightValues.w == 0.0f )
					{
						lightValues.w = 200.0f;
					}

					string lightString = entity.Pairs["_light"];
					GD.Print( $"Light values: <{lightValues}> (string: '{lightString}')" );

					// Convert from [0-255] to [0-1]
					lightValues = lightValues * (1.0f / 255.0f);

					light.LightColor = new Color( lightValues.x, lightValues.y, lightValues.z, 1.0f );
					light.LightEnergy = lightValues.w;
					light.OmniRange = lightValues.w * 10.0f;
				}
			}
		}
		catch ( Exception ex )
		{
			GD.PushError( $"Exception '{ex.Message}'" );
			GD.PushError( $"Stack trace: {ex.StackTrace}" );
		}
	}

	public override void _Input( InputEvent @event )
	{
		mClient.UserInput( @event );
	}

	// Called when stuff exits basically.
	public override void _ExitTree()
	{
		mEntities.ForEach( entity => entity.Destroy() );
		mEntities.Clear();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta )
	{
		mEntities.ForEach( entity => entity.Think() );

		mClient.Update();
		mClient.UpdateController();

		if ( Worldspawn == null && Input.IsKeyPressed( Key.C ) )
		{
			LoadMap( "maps/tunnel" );
		}
	}

	public override void _PhysicsProcess( double delta )
	{
		mEntities.ForEach( entity => entity.PhysicsUpdate( (float)delta ) );
	}

	private Bodot.Client.Client mClient;
	private Bodot.Entities.Player mPlayerEntity;
	private List<Bodot.Entities.Entity> mEntities = new();
}
