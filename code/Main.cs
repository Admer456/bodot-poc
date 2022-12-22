// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

using Bodot.Assets;
using Bodot.Entities;

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

		Worldspawn = MapGeometry.CreateBrushModelNode( Map.MapEntities[0] );

		Map.MapEntities.ForEach( entity => SpawnEntity( entity ) );
		mEntities.ForEach( entity => entity.PostSpawn() );
	}

	public T CreateAndSpawnEntity<T>() where T : Entity, new()
	{
		T entity = new T();
		entity.Spawn();
		mEntities.Add( entity );

		return entity;
	}

	// Quasi-entry point of the "engine"
	public override void _Ready()
	{
		Instance = this;

		// The scene is completely empty before this point.
		// This loads raw TrenchBroom/J.A.C.K./Hammer maps, not BSPs keep in mind
		LoadMap( "maps/surf" );

		// Todo: deferred player spawning on actual spawnpoint entities
		mPlayerEntity = CreateAndSpawnEntity<Player>();

		// Clients use controller entities to actually interact w/ the game world.
		// In theory this can even be an NPC or some vehicle, but right now we have
		// a dedicated Player entity
		mClient = new() { Controller = mPlayerEntity };
	}

	private void SpawnEntity( MapEntity entity )
	{
		Entity ent;

		switch ( entity.ClassName )
		{
			case "light": ent = CreateAndSpawnEntity<Light>(); break;
			case "func_detail": ent = CreateAndSpawnEntity<FuncDetail>(); break;
			case "func_breakable": ent = CreateAndSpawnEntity<FuncBreakable>(); break;
			case "func_rotating": ent = CreateAndSpawnEntity<FuncRotating>(); break;
			case "func_water": ent = CreateAndSpawnEntity<FuncWater>(); break;
			case "prop_test": ent = CreateAndSpawnEntity<PropTest>(); break;
			default: GD.Print( $"SpawnEntity: unknown class '{entity.ClassName}'" ); return;
		}

		// Brush entity
		if ( entity.Brushes.Count > 0 )
		{
			ent.AddBrushModel( entity );
		}

		ent.KeyValue( entity.Pairs );
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
	}

	public override void _PhysicsProcess( double delta )
	{
		mEntities.ForEach( entity => entity.PhysicsUpdate( (float)delta ) );
	}

	private Bodot.Client.Client mClient;
	private Player mPlayerEntity;
	private List<Entity> mEntities = new();
}
