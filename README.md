
# "Bodot" proof of concept

This is a bit of research I'm doing to see if I can basically build an "engine" on top of Godot, which lets me have a workflow similar to GoldSRC/Source. Don't ask why. Ask why not.

End goal would be to have something like this:
```
MyGame/
	game.exe
	mygame/
		game.dll
		gameConfig.json
		maps/
			level1.map
			level2.map
		models/
			enemy.mdl
			gun.mdl
		textures/
			concrete.png
			wood.png
	mymod/
		game.dll // optional if the mod doesn't override any code
		gameConfig.json
		models/
			enemy2.mdl
			barrel.mdl
```
...where `game.exe` is actually the exported "Godot game". In reality, it acts as an engine. There is an empty Godot scene, almost completely empty with just a single node that uses a "Main" script that acts as an entry point. Godot features a `MainLoop` class which can be used to do this *without* a "main node", but it currently doesn't work in C#, at least til it's implemented as a GDExtension extension.

## Current state

Right now, all assets are part of the Godot Project and gotta be imported as such. In the upcoming days/weeks, I'll start messing with external game DLLs and actually try to see if this works at all as an exported project.

So far? We got a tiny bit of an entity system that uses Godot nodes behind the scenes, and a custom TrenchBroom map loader which, most likely, will end up in an external tool so it can compile levels into a custom format.

## Pics or it didn't happen

![](https://i.imgur.com/VGDENIx.jpg)
In TrenchBroom

![](https://i.imgur.com/bvS6WvW.jpg)
In "Bodot"

## ...Bodot?

The name is subject to change. The B stems from an engine project I've been working on called [BurekTech X](https://github.com/Admer456/btx-testbed), and both projects have more or less the exact same ideas.