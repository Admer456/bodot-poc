
# "Bodot" proof of concept

This is a bit of research I'm doing to see if I can basically build an "engine" on top of Godot, which lets me have a workflow similar to GoldSRC/Source. Don't ask why. Ask why not.

End goal would be to have *sooomething* like this:
```
MyGame/
	game.exe
	mygame/
		game.dll
		maps/
			level1.map
			level2.map
		models/
			[buncha model files e.g. glTF2]
		textures/
			[buncha .png files]
	mymod/
		game.dll // optional if the mod doesn't override any code
		maps/
			[custom maps, maybe its own campaign]
		models/
			[buncha model files to replace mygame's]
```
...where `game.exe` is actually the exported "Godot game". In reality, it acts as an engine. There is an empty Godot scene, almost completely empty with just a single node that uses a "Main" script that acts as an entry point. It then loads all assets externally, just like Quake/GoldSRC/Source!

## Pics or it didn't happen

![](https://i.imgur.com/FoG1bcW.jpg)
In TrenchBroom

![](https://i.imgur.com/MJjZt5Y.jpg)
In "Bodot"

## Current state

~~Right now, all assets are part of the Godot Project and gotta be imported as such. In the upcoming days/weeks, I'll start messing with external game DLLs and actually try to see if this works at all as an exported project.~~  
The project, when exported, can load external assets just fine. Still haven't tested external game DLLs.

So far? We got a tiny bit of an entity system that uses Godot nodes behind the scenes, and a custom TrenchBroom map loader which, most likely, will end up in a custom map compiler. Loading brushes as-is is not optimal for larger levels, these would benefit from a custom occlusion culling solution, e.g. an octree-based PVS.

Supported entities are `light`, `func_detail`, `func_breakable` (does nothing) and `func_rotating` (rotates yaw at a hardcoded speed).

The proof of concept is mostly done. Remaining things to do include:
- improving the player movement code a bit (add surfing and crouching)
- testing external game DLLs
- loading an animated model

After that, I'll start working on a more serious prototype, with more proper code and a design doc as a guideline.

Godot features a `MainLoop` class which can be used to do this *without* a "main node", but it currently doesn't work in C#, at least til it's implemented as a GDExtension extension.

## ...Bodot?

The name is subject to change. The B stems from an engine project I've been working on called [BurekTech X](https://github.com/Admer456/btx-testbed), and both projects have more or less the exact same ideas.