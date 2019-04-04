# FlatPMSDK For Developers

This Project Made for PUBG Mobile TGB Emulator Modding.
Developers can use it for making mod (Plugin) for the game.

## Getting Started

Look at the produced examples.

### Prerequisites

Basic C# Knowledge. you must install theese Packages for buid. you can use Nuget PM For that.

Your Plugin name Must be match with filename like LSP_Aimbot {LSP_} is our Plugin Prefix but When you developing and testing your own Project Prefix must be {DLSP_} Like DLSP_Aimbot

example : 
```
Assembly name : DLSP_Aimbot
public override PluginInfo info { get => new PluginInfo() { author = "OwnerName", name = "Plugin name without Prefix", version = 1 }; }
```

```
  <package id="GameOverlay.Net" version="4.0.3" targetFramework="net461" />
  <package id="Newtonsoft.Json" version="12.0.1" targetFramework="net461" />
  <package id="SharpDisasm" version="1.1.11" targetFramework="net461" />
  <package id="SharpDX" version="4.2.0" targetFramework="net461" />
  <package id="SharpDX.Direct2D1" version="4.2.0" targetFramework="net461" />
  <package id="SharpDX.DXGI" version="4.2.0" targetFramework="net461" />
  <package id="SharpDX.Mathematics" version="4.2.0" targetFramework="net461" />
```

### Installing

When you build project you must copy of your assembly to %temp% path to run Plugin.
```
example copy DLSP_PluginName.dll to %temp% path. the core will load that plugin if everything is correct. plugin name assembly name etc..
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

SharpDX for Drawing.
NewtonSoft.Json for serializing.
GameOverylay.net for Overlay.
SharpDisasm for core stuff.


## Authors

* **LeftSpace** - *Core & SDK* 
* **Rexy** - *did Plugin Loader classes etc.*
* **Pikachu7** - *some offsets for core.*
* **XMaze** - *some offsets for core.*

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

