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

[MIT License](https://opensource.org/licenses/MIT). You can use, copy, change, and share this mod. The LICENSE file shipped with the package has the full text.
