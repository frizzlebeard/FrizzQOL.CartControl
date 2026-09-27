# FrizzQOL Cart Control

Sets each cart's storage, empty pull weight, and cargo limit. Carts can also be kept from breaking, and the hammer can take them apart.

## Config

`BepInEx/config/com.frizzqol.cartcontrol.cfg`

The first launch writes one section per cart, using that cart's normal size and empty weight.

| Setting | What it does |
| --- | --- |
| Width | Storage columns. Stops at 8. 0 keeps the normal width. |
| Height | Storage rows. 0 keeps the normal height. |
| CartWeight | Pull weight of the empty cart. 0 is weightless. Below 0 keeps the normal empty weight. |
| CargoWeight | Most item weight the cart will accept. 0 means no storage limit. This does not change pull weight. |
| CargoPull | How much of the cargo counts while you pull. 1 is normal. 0.25 makes a full cart about one quarter as heavy. 0 means cargo adds no pull weight. Below 0 keeps the normal pull. |

`Invincible` under `[General]` defaults to on. Carts take no damage and do not wear down. Set it to false for normal breaking.

`HammerDeconstruct` under `[General]` defaults to on. Hammer middle-click empties the cart onto the ground, then removes the cart and returns its materials.

## Multiplayer

Install this on the dedicated server and on every client. Use the same config on each of them.

## Install

Install with r2modman or the Thunderstore Mod Manager.

To install by hand, copy `FrizzQOL.CartControl.dll` into `BepInEx/plugins`.

## Requirements

- Valheim
- [BepInExPack for Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)

## License

[MIT](LICENSE). You can use, copy, change, and share this mod. Keep the copyright notice with any copy.

## Building

1. Copy `Environment.props.example` to `Environment.props`.
2. Set your Valheim and BepInEx folders in that file.
3. From this folder, run:

```
dotnet build CartControl.sln -c Release
```

The plugin file is `FrizzQOL.CartControl.dll`, under the project `bin\Release\net48` folder.

`Environment.props` stays on your machine. It is listed in `.gitignore`.

## Publishing

This folder is ready to push as its own public repository. Create an empty GitHub repo named `FrizzQOL.CartControl`. Do not add a README, license, or gitignore on GitHub. Those files are already here. Then run:

```
git remote add origin https://github.com/<you>/FrizzQOL.CartControl.git
git push -u origin main
```

Set `website_url` in `Package/manifest.json` to that repository before the Thunderstore upload.

## Thunderstore package

A valid upload is a zip whose root contains `icon.png`, `README.md`, and `manifest.json`. `CHANGELOG.md` is optional and is included here. Copy `FrizzQOL.CartControl.dll` from the Release build into `Package`, then zip the files themselves. Do not zip the `Package` folder. If the files sit inside a folder in the zip, Thunderstore rejects the package.

- `manifest.json`
- `README.md`
- `CHANGELOG.md`
- `icon.png`
- `LICENSE`
- `FrizzQOL.CartControl.dll`

The dll belongs at the zip root. The mod manager installs those files under `BepInEx/plugins/<Team>-<PackageName>/`.

Before you upload, check the package readme in the [markdown preview](https://thunderstore.io/tools/markdown-preview/) and the manifest in the [manifest validator](https://thunderstore.io/tools/manifest-v1-validator/). The package rules are in [Creating a Package](https://wiki.thunderstore.io/mods/creating-a-package).
