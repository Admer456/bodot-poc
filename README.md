
*Update 17th May 2023:*  
*This is now an actual project! Please check out [Elegy Engine](https://github.com/ElegyEngine) for more details.*

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

It's done!

* We got a tiny bit of an **entity system** that uses Godot nodes behind the scenes.

* Also got a custom TrenchBroom **map loader** which, most likely, will end up in a custom map compiler. Loading brushes as-is is not optimal for larger levels, these would benefit from a custom occlusion culling solution, e.g. an octree-based PVS.

* Supported **entities** are `light`, `func_detail`, `func_breakable` (does nothing), `prop_test` and `func_rotating` (rotates yaw at a hardcoded speed).

* **Animated models** can be generated from code, which will be leveraged in the prototype when asset loader plugins are a thing. Right now, static GLTFs are loaded as-is.

* **External DLLs** are supported. This one was the trickiest to do because of problems with having GodotSharp as a dependency in external DLLs. Using a custom `AssemblyLoadContext` to resolve dependencies for external DLLs helped.

* Everything **works** outside the Godot Editor as an exported projects, which is the most important part.

The only remaining questions are about audio, UI and interacting with Godot's `RenderingServer` to maybe directly draw surfaces from a custom UI system (e.g. somebody may want a lightweight HTML/CSS solution). Godot's existing UI system is pretty good, custom UI is just a thing of curiosity.

Soon I'll start working on a more serious prototype, with more proper code and a design doc as a guideline.

Sidenote: Godot features a `MainLoop` class which can be used to do this *without* a "main node", but it currently doesn't work in C#, at least til it's implemented as a GDExtension extension.

## ...Bodot?

The name is subject to change. The B stems from an engine project I've been working on called [BurekTech X](https://github.com/Admer456/btx-testbed), and both projects have more or less the exact same ideas.

## Ayo, where's the external game DLL code?

Here's the code:
```
using Godot;

namespace TestGameModule
{
	public class Game
	{
		public static void CreateGame()
		{
			GD.Print( "Hello from <TestGameModule>!" );
		}
	}
}
```

1) Create a project called `TestGameModule`
2) Install the `GodotSharp` (beta4.0.0-v5) package
3) Build and place into `<Godot project>/bin/TestGameModule.dll` along with any relevant JSON files

Ignore the fact that it's called `CreateGame`, it was originally meant to implement an `IGame` interface but there were many many troubles working with dependencies, until I finally figured out how to resolve it all, and I just didn't bother setting it back to how it was. Wait a couple more months for a more proper prototype. :3

