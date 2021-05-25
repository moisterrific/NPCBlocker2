# NPC Blocker
A plugin for TShock that allows admins to block certain NPCs from spawning in the world.

- Originally made by [Olink](https://github.com/Olink)
- Updated for **TShock** `4.5.3` on **Terraria** `1.4.2.3` by yours truly.
- Fixed most of the issues from the previous version.
- Now supports the latest TShock silent command specifier 

## How to Install
1. Put the .dll into the `\ServerPlugins\` folder.
2. Restart the server.
3. Give your desired group the `plugin.npc.blocker` permission.

## How to Use
### Commands
- `blocknpc` (same as `bannpc` and `banmob`)
- `unblocknpc` (same as `unbannpc` and `unbanmob`)

### Usage and Examples
- `/blocknpc <NPC name or ID> <Max spawn limit (Default 0)`
- `/unblocknpc <NPC name or ID>`
- `.banmob Zombie 5` - Allows Zombies to spawn in the world only 5 times, and will silently send a message
- `/unbanmob king slime` - Tells all players on the server you unbanned King Slime from spawning
- `/npc-reload` - Reloads the config

### Config Options
- `NPCAddedMessageSlient` - The message to send to a player silently when an NPC is banned
- `NPCAddedMessage` - The message to send to all players when an NPC is banned
- `NPCRemovedMessageSilent` - The message to send to a player silently when an NPC is unbanned
- `NPCRemovedMessage` - The message to send to all players when an NPC is unbanned
- `BlockBanMessageToOthers`- Whether or not to send a message silently regardless of the command specifier

### Important
- To reset the database, delete `npc_blocker.sqlite` in the `tshock` folder
- All commands require the `plugin.npc.blocker` permission

## Known Issues and Workarounds
- Banning a segmented enemy (like worm types) is kind of glitchy, if you ban a body part of their segment, they will still spawn but die almost instantly afterwards, banning the head part prevents the spawn completely. 
- I've not completely tested this against more specific instances such as bosses with multiple parts (moon lord?).

## How to Build
1. Download the source code.
2. Open the `.sln` file.
3. Check to make sure the references are all correct and up to date.
4. Build.

## Notes
- I plan on adding more features to this plugin down the road, such as listing the current amount of banned NPCs and optionally ban NPCs based on a specific threshold (for example: allow no more than 5 wraiths to exist in the world).
- Also maybe develop this into a more robust plugin where NPCs can be banned based on number of votes, game time, progression level, etc.
- The original version was very outdated with the last update in 2015. I did the best I could to update it to work with the latest TShock/Terraria versions and fixed most if not all of the problems with the original, such as only being able to input NPC ID, inadequate command output messages, and more.
- Still, despite all these issues Olink did a great job because I totally wouldn't be able to make something like this from scratch.

## Original plugin info
https://github.com/Olink/NPCBlocker
