# Echoes of Aether

An original, procedural 2D JRPG demo for Unity 2022.3 LTS. Follow Kestrel, Mira, and Tavi through four connected regions, uncover Malachar's plan, and restore the stolen dawn.

## Open and play

1. Open this folder in Unity Hub with Unity **2022.3.36f1** or a compatible 2022.3 LTS editor.
2. Let Unity import the project. The editor setup script creates `Assets/Scenes/FirstLight.unity` and adds it to Build Settings if no enabled scene is present.
3. Open `Assets/Scenes/FirstLight.unity` and press **Play**.

## Controls

- **WASD** or **arrow keys**: move; Up moves north and Down moves south.
- **Space** or **Enter**: interact, confirm, target, or advance dialogue.
- **Escape** or **M**: open/close the journal and inventory. In battle, Escape returns to the previous command.
- **Battle**: Attack, Skill, Item, and Run. When there are several enemies, select an enemy target.
- **Journal**: Inventory, Party Profiles, Equipment, and Quick Travel. Travel destinations appear only after they have been discovered.
- **Towns**: talk to the healer to restore the party and the merchant to buy supplies and role-specific equipment. Equipment can be changed per character from the journal.

## The ten-minute journey

The Sunwell beneath Greenwick has gone dark. Elder Rowan sends the party east to recover four guardian relics and stop the masked sorcerer Malachar before he can open a rift over the world.

1. **Greenwick** — Talk to Elder Rowan, explore the village and river road, find the sidequest cache, win two encounters, and defeat the Gloam Warden for the **Sunstone**.
2. **Whisperwood** — Visit the Mossveil waystation, uncover Malachar's trail, and defeat the Briar Queen for the **Briarheart**. Malachar then kidnaps Mira and escapes into the caves.
3. **Emberdeep** — Kestrel and Tavi pursue the captor without Mira. Defeat the Ironbound Jailer to rescue her and claim the **Prison Key**.
4. **Nether March** — Discover the final gate, fight through Malachar's forces, and defeat the recurring villain for the **Void Crown** and the campaign ending.

Each region has its own raster map, settlement, merchant, healer or guide, optional field-cache sidequest, four regular enemy types, and one unique guardian boss with a region-specific relic reward. Regular encounters can contain multiple distinct enemies. Two regional victories open the guardian's altar. Level-ups raise stats and teach new skills; progression is paced for a level-5 finale.

## Included systems

- A three-member party with high-resolution pixel portraits, role-specific character art, party profiles, per-character weapons and armor, and learned skills.
- A multi-enemy turn-based battle system with distinct regional enemy sprites, guardian sprites, selectable targets, attack/skill/item/damage/flee animations, and distinct spell and party-attack effects.
- Regional exploration maps, wandering NPCs, dialogue cutscenes, gated story transitions, sidequest caches, shops, equipment, quick travel, and level progression.
- Original looping overworld and battle music with smooth mode transitions, plus synthesized movement, combat, magic, item, and escape sounds on a separate effects source.
- Procedurally generated raster artwork using RGB565/RGBA4444 textures and point filtering; no third-party packages or copyrighted audio are used.

Unity does not execute C++ source files as game scripts. This project uses C# `MonoBehaviour` scripts, Unity's native gameplay scripting format, so it runs without a separately compiled platform-specific C++ plugin.
