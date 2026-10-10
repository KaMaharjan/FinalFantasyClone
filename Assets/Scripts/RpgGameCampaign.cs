using System.Collections.Generic;
using UnityEngine;

public partial class RpgGame
{
    private const float RegionTransitionDuration = 0.62f;

    private sealed class EquipmentDefinition
    {
        public readonly string Name;
        public readonly bool IsWeapon;
        public readonly string Role;
        public readonly int Attack;
        public readonly int Defense;
        public readonly int Magic;

        public EquipmentDefinition(string name, bool isWeapon, string role, int attack, int defense, int magic)
        {
            Name = name;
            IsWeapon = isWeapon;
            Role = role;
            Attack = attack;
            Defense = defense;
            Magic = magic;
        }
    }

    private sealed class ShopOffer
    {
        public readonly string Name;
        public readonly string Description;
        public readonly int Price;
        public readonly bool IsEquipment;
        public readonly ItemKind Item;
        public readonly EquipmentDefinition Equipment;

        public ShopOffer(string name, string description, int price, ItemKind item)
        {
            Name = name;
            Description = description;
            Price = price;
            IsEquipment = false;
            Item = item;
        }

        public ShopOffer(EquipmentDefinition equipment, int price)
        {
            Name = equipment.Name;
            Description = EquipmentDescription(equipment);
            Price = price;
            IsEquipment = true;
            Equipment = equipment;
        }
    }

    private static readonly ShopOffer[] ShopOffers =
    {
        new ShopOffer("Potion", "Restore up to 28 HP.", 18, ItemKind.Potion),
        new ShopOffer("Ether", "Restore up to 8 MP.", 24, ItemKind.Ether),
        new ShopOffer("Phoenix Feather", "Revive one fallen ally.", 36, ItemKind.PhoenixFeather),
        new ShopOffer(new EquipmentDefinition("Steel Broadsword", true, "Vanguard", 3, 0, 0), 55),
        new ShopOffer(new EquipmentDefinition("Moonfire Staff", true, "Arcanist", 0, 0, 3), 55),
        new ShopOffer(new EquipmentDefinition("Sage's Tome", true, "Mender", 0, 0, 3), 55),
        new ShopOffer(new EquipmentDefinition("Knight's Mantle", false, "Vanguard", 0, 3, 0), 48),
        new ShopOffer(new EquipmentDefinition("Moonweave Robe", false, "Arcanist", 0, 2, 1), 48),
        new ShopOffer(new EquipmentDefinition("Silver Vestments", false, "Mender", 0, 2, 1), 48)
    };

    private readonly List<EquipmentDefinition> ownedEquipment = new List<EquipmentDefinition>();

    private char[,] CreateRegionTerrain(int region)
    {
        char ground = region == 0 ? 'g' : region == 1 ? 'f' : region == 2 ? 'c' : 'v';
        char[,] map = new char[MapWidth, MapHeight];
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                map[x, y] = x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1
                    ? '#'
                    : ground;
            }
        }

        PaintRect(map, 2, 5, 16, 3, '.');
        PaintRect(map, 2, 2, 4, 4, '.');
        PaintRect(map, 15, 2, 3, 4, '.');
        PaintRect(map, 16, 6, 3, 1, '.');
        PaintRect(map, 3, 8, 2, 3, '.');
        PaintRect(map, 8, 9, 4, 1, '.');

        if (region == 0)
        {
            PaintRect(map, 10, 1, 2, 4, '~');
            PaintRect(map, 9, 5, 4, 3, '=');
            PaintRect(map, 3, 2, 4, 3, 'H');
            PaintRect(map, 1, 1, 2, 4, 'T');
            PaintRect(map, 1, 10, 2, 3, 'T');
            PaintRect(map, 8, 9, 2, 4, 'T');
            PaintRect(map, 17, 10, 2, 3, 'T');
        }
        else if (region == 1)
        {
            PaintRect(map, 1, 1, 3, 3, 'T');
            PaintRect(map, 7, 1, 2, 3, 'T');
            PaintRect(map, 12, 1, 3, 3, 'T');
            PaintRect(map, 2, 9, 2, 4, 'T');
            PaintRect(map, 6, 10, 3, 3, 'T');
            PaintRect(map, 12, 9, 2, 4, 'T');
            PaintRect(map, 16, 10, 3, 3, 'T');
            PaintRect(map, 10, 3, 2, 2, '~');
            PaintRect(map, 9, 4, 4, 1, '~');
        }
        else if (region == 2)
        {
            PaintRect(map, 2, 2, 3, 2, 'R');
            PaintRect(map, 7, 1, 2, 3, 'R');
            PaintRect(map, 13, 2, 2, 2, 'R');
            PaintRect(map, 5, 10, 3, 2, 'R');
            PaintRect(map, 13, 9, 3, 3, 'R');
            PaintRect(map, 10, 1, 2, 3, 'L');
            PaintRect(map, 10, 9, 2, 3, 'L');
            PaintRect(map, 9, 5, 4, 3, '=');
        }
        else
        {
            PaintRect(map, 2, 2, 2, 3, 'R');
            PaintRect(map, 6, 1, 2, 3, 'R');
            PaintRect(map, 12, 2, 2, 2, 'R');
            PaintRect(map, 15, 9, 3, 3, 'R');
            PaintRect(map, 4, 10, 3, 2, 'R');
            PaintRect(map, 9, 1, 2, 3, 'L');
            PaintRect(map, 9, 9, 2, 3, 'L');
            PaintRect(map, 16, 6, 2, 1, '=');
        }

        Vector2Int boss = GetRegionBossPosition(region);
        map[boss.x, boss.y] = 'S';
        return map;
    }

    private Vector2Int GetRegionSpawnPosition(int region)
    {
        if (region == 0)
        {
            return new Vector2Int(5, 6);
        }
        return new Vector2Int(2, 6);
    }

    private void PaintRect(char[,] map, int x, int y, int width, int height, char tile)
    {
        for (int row = y; row < y + height; row++)
        {
            for (int column = x; column < x + width; column++)
            {
                if (column >= 0 && column < MapWidth && row >= 0 && row < MapHeight)
                {
                    map[column, row] = tile;
                }
            }
        }
    }

    private Vector2Int GetRegionChestPosition(int region)
    {
        return region == 0
            ? new Vector2Int(7, 8)
            : region == 1
                ? new Vector2Int(5, 10)
                : region == 2
                    ? new Vector2Int(14, 8)
                    : new Vector2Int(5, 9);
    }

    private Vector2Int GetRegionBossPosition(int region)
    {
        return new Vector2Int(16, 2);
    }

    private Vector2Int GetRegionGatePosition(int region)
    {
        return new Vector2Int(region == 3 ? 2 : 18, 6);
    }

    private static bool IsEncounterTerrain(char tile)
    {
        return tile == 'g' || tile == 'f' || tile == 'c' || tile == 'v';
    }

    private void PopulateRegionNpcs(int region)
    {
        npcs.Clear();
        if (region == 0)
        {
            AddNpc("Elder Rowan", 4, 5, NpcKind.Elder);
            AddNpc("Mara", 7, 5, NpcKind.Healer);
            AddNpc("Tamsin", 8, 7, NpcKind.Merchant);
            AddNpc("Old Jory", 3, 8, NpcKind.Guide);
        }
        else if (region == 1)
        {
            AddNpc("Sylvi", 4, 6, NpcKind.Guide);
            AddNpc("Pollen", 8, 6, NpcKind.Merchant);
            AddNpc("Fern", 5, 8, NpcKind.Healer);
        }
        else if (region == 2)
        {
            AddNpc("Brann", 4, 6, NpcKind.Guide);
            AddNpc("Toma", 8, 6, NpcKind.Merchant);
            AddNpc("Mira's Echo", 5, 8, NpcKind.Healer);
        }
        else
        {
            AddNpc("Nox", 4, 6, NpcKind.Guide);
            AddNpc("Vesper", 8, 6, NpcKind.Merchant);
            AddNpc("Ashen Pilgrim", 8, 8, NpcKind.Healer);
        }
    }

    private void AddNpc(string name, int x, int y, NpcKind kind)
    {
        Vector2 start = new Vector2(x, y);
        npcs.Add(new Npc
        {
            Name = name,
            Position = new Vector2Int(x, y),
            Kind = kind,
            VisualPosition = start,
            MoveStart = start,
            MoveTarget = start,
            NextWanderTime = Time.unscaledTime + Random.Range(1.3f, 4.2f)
        });
    }

    private void UpdateNpcWandering(float deltaTime)
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            Npc npc = npcs[i];
            if (npc.MoveElapsed < 0.35f && npc != activeNpc)
            {
                npc.MoveElapsed += deltaTime;
                float progress = Mathf.Clamp01(npc.MoveElapsed / 0.35f);
                progress = progress * progress * (3f - 2f * progress);
                npc.VisualPosition = Vector2.Lerp(npc.MoveStart, npc.MoveTarget, progress);
            }
            else if (mode == GameMode.Explore && npc != activeNpc &&
                     Time.unscaledTime >= npc.NextWanderTime)
            {
                npc.NextWanderTime = Time.unscaledTime + Random.Range(1.8f, 4.8f);
                if (Random.value < 0.48f)
                {
                    continue;
                }

                Vector2Int[] directions =
                {
                    Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
                };
                Vector2Int next = npc.Position + directions[Random.Range(0, directions.Length)];
                if (!IsInsideMap(next) || !IsWalkable(terrain[next.x, next.y]) ||
                    ManhattanDistance(next, playerPosition) == 0 ||
                    ManhattanDistance(next, shrinePosition) == 0 ||
                    next == chestPosition ||
                    ManhattanDistance(next, GetRegionGatePosition(currentRegion)) == 0)
                {
                    continue;
                }

                bool occupied = false;
                for (int j = 0; j < npcs.Count; j++)
                {
                    if (i != j && npcs[j].Position == next)
                    {
                        occupied = true;
                        break;
                    }
                }
                if (occupied)
                {
                    continue;
                }

                npc.MoveStart = npc.VisualPosition;
                npc.Position = next;
                npc.MoveTarget = new Vector2(next.x, next.y);
                npc.MoveElapsed = 0f;
            }
        }
    }

    private void HandleTalkTo(Npc npc)
    {
        if (npc.Kind == NpcKind.Merchant)
        {
            activeNpc = null;
            mode = GameMode.Menu;
            menuPage = MenuPage.Shop;
            shopSelection = 0;
            shopHeroIndex = 0;
            statusMessage = npc.Name + " welcomes you to the " + RegionShopName(currentRegion) + ".";
            return;
        }

        if (npc.Kind == NpcKind.Healer)
        {
            RestoreParty();
            BeginDialogue(npc.Name, "Rest here before you continue.", "Your traveling companions are ready.");
            return;
        }

        if (npc.Kind == NpcKind.Elder)
        {
            if (regionEncountersWon < 2 && !bossDefeated)
            {
                BeginDialogue(npc.Name,
                    "The Gloam Warden has drained the Sunwell beneath the eastern shrine.",
                    "Win two road encounters to weaken its seal. The old cache by the bridge holds supplies.");
            }
            else if (!bossDefeated)
            {
                BeginDialogue(npc.Name, "The Sunwell seal is fading.", "Face the Gloam Warden at the eastern shrine.");
            }
            else
            {
                BeginDialogue(npc.Name, "Greenwick remembers the light.", "The road through Whisperwood is open. Find its eastern gate.");
            }
            return;
        }

        if (!sideQuestCompleted[currentRegion])
        {
            BeginDialogue(npc.Name,
                GuideStory(currentRegion),
                "I marked a field cache nearby. Search it for a keepsake and supplies.");
        }
        else if (capturedPartyIndex >= 0 && currentRegion == 2)
        {
            BeginDialogue(npc.Name, "The iron gaoler keeps Mira below the old foundry.", "Defeat it, and the prison key will open her cell.");
        }
        else if (currentRegion == 3)
        {
            BeginDialogue(npc.Name, "Malachar has chained the rift to the four guardian relics.", "Carry the Sunstone, Briarheart, Prison Key, and Void Crown into the final hall.");
        }
        else
        {
            BeginDialogue(npc.Name, "The path ahead is dangerous, but it is not without hope.", RegionObjective(currentRegion));
        }
    }

    private string GuideStory(int region)
    {
        if (region == 0)
        {
            return "Elder Rowan needs help restoring the Sunwell.";
        }
        if (region == 1)
        {
            return "The Briar Queen serves a masked sorcerer called Malachar.";
        }
        if (region == 2)
        {
            return "Malachar took Mira through the foundry and left the Jailer behind.";
        }
        return "The four relics are the only keys to Malachar's rift.";
    }

    private void HandleRegionCache()
    {
        if (sideQuestCompleted[currentRegion])
        {
            BeginDialogue("Field Cache", "The cache is empty. Its keepsake is safely in your pack.");
            return;
        }

        sideQuestCompleted[currentRegion] = true;
        chestOpened = true;
        AddItem(ItemKind.Potion, currentRegion == 0 ? 2 : 1);
        if (currentRegion > 0)
        {
            AddItem(ItemKind.Ether, 1);
        }
        gil += 30 + currentRegion * 10;
        EquipmentDefinition keepsake = CreateRegionKeepsake(currentRegion);
        AddOwnedEquipment(keepsake);
        BeginDialogue("Keepsake Recovered",
            "The cache held a " + keepsake.Name + ", field supplies, and " + (30 + currentRegion * 10) + " gil.",
            "The new equipment is available from the party's Equipment menu.");
    }

    private EquipmentDefinition CreateRegionKeepsake(int region)
    {
        if (region == 0)
        {
            return new EquipmentDefinition("Riverguard Mantle", false, string.Empty, 0, 1, 1);
        }
        if (region == 1)
        {
            return new EquipmentDefinition("Briarheart Charm", true, string.Empty, 1, 0, 2);
        }
        if (region == 2)
        {
            return new EquipmentDefinition("Deepstone Plate", false, "Vanguard", 0, 3, 0);
        }
        return new EquipmentDefinition("Riftglass Circlet", false, string.Empty, 0, 1, 3);
    }

    private void HandleRegionBoss()
    {
        if (bossDefeated)
        {
            BeginDialogue(RegionName(currentRegion), "The guardian's altar is quiet now.");
            return;
        }
        if (regionEncountersWon < 2)
        {
            BeginDialogue(RegionName(currentRegion) + " Guardian",
                "The guardian's seal is still strong.",
                "Win two encounters in this region before approaching the altar.");
            return;
        }

        StartBattle(CreateRegionBoss(currentRegion), true);
    }

    private Enemy CreateRegionBoss(int region)
    {
        int hp = 96 + region * 52;
        if (region == 2)
        {
            hp = 180;
        }
        if (region == 3)
        {
            hp = 235;
        }
        int attack = 11 + region * 3;
        int defense = 4 + region * 2;
        int experience = 105 + region * 36;
        int gilReward = 70 + region * 25;
        return new Enemy
        {
            Kind = (EnemyKind)(16 + region),
            SpriteIndex = 16 + region,
            Name = RegionBossName(region),
            MaxHp = hp,
            Hp = hp,
            Attack = attack,
            Defense = defense,
            Experience = experience,
            Gil = gilReward,
            IsBoss = true
        };
    }

    private Enemy CreateRegionEnemy(EnemyKind kind)
    {
        int index = (int)kind;
        int region = Mathf.Clamp(index / 4, 0, 3);
        int type = index % 4;
        string[][] names =
        {
            new[] { "Greenwick Ooze", "Thorn Wolf", "Grave Moth", "Road Bandit" },
            new[] { "Briarling", "Moss Stalker", "Thorn Wisp", "Bark Beetle" },
            new[] { "Ember Bat", "Ash Crawler", "Magma Hound", "Crystal Golem" },
            new[] { "Nether Imp", "Soul Wraith", "Abyss Knight", "Rift Drake" }
        };
        int hp = new[] { 34, 42, 31, 46 }[type] + region * 14;
        int attack = new[] { 7, 8, 6, 9 }[type] + region * 3;
        int defense = new[] { 2, 3, 1, 4 }[type] + region;
        int experience = new[] { 22, 25, 20, 29 }[type] + region * 12;
        int gilReward = new[] { 11, 14, 13, 18 }[type] + region * 8;
        return new Enemy
        {
            Kind = kind,
            SpriteIndex = index,
            Name = names[region][type],
            MaxHp = hp,
            Hp = hp,
            Attack = attack,
            Defense = defense,
            Experience = experience,
            Gil = gilReward
        };
    }

    private string BuildEncounterMessage(List<Enemy> enemies)
    {
        if (enemies.Count == 1)
        {
            return enemies[0].Name + " blocks the trail.";
        }
        return enemies[0].Name + " and " + enemies[1].Name + " rush the party!";
    }

    private void BeginRegionBossStory()
    {
        if (currentRegion == 0)
        {
            BeginStoryDialogue("Elder Rowan", DialogueEvent.UnlockForest,
                "The Gloam Warden falls, and the Sunwell shines again.",
                "Malachar's trail leads east, through Whisperwood. Take the Sunstone.");
        }
        else if (currentRegion == 1)
        {
            BeginStoryDialogue("Malachar", DialogueEvent.KidnapMira,
                "A masked sorcerer steps from the briars: \"The arcanist comes with me.\"",
                "Malachar tears open a rift and carries Mira into Emberdeep.",
                "Kestrel and Tavi pursue the sorcerer's fading trail.");
        }
        else if (currentRegion == 2)
        {
            BeginStoryDialogue("Mira", DialogueEvent.RescueMira,
                "The Ironbound Jailer collapses. The prison key turns in Mira's cell.",
                "Mira is free. Together, you resolve to confront Malachar in the Nether March.",
                "The demon realm's gate opens beyond the foundry.");
        }
        else
        {
            BeginStoryDialogue("Malachar", DialogueEvent.CompleteCampaign,
                "The four relics break Malachar's rift engine.",
                "Across Greenwick, Whisperwood, and Emberdeep, the stolen dawn returns.",
                "The Echoes of Aether fall quiet—for now.");
        }
    }

    private void CompleteDialogueEvent()
    {
        DialogueEvent completed = dialogueEvent;
        dialogueEvent = DialogueEvent.None;
        if (completed == DialogueEvent.UnlockForest)
        {
            unlockedRegions[1] = true;
            statusMessage = "Whisperwood is open. Reach the eastern gate or use Quick Travel after visiting it.";
            StartSceneTransition();
        }
        else if (completed == DialogueEvent.KidnapMira)
        {
            capturedPartyIndex = 1;
            party[capturedPartyIndex].IsCaptured = true;
            unlockedRegions[2] = true;
            TravelToRegion(2);
            statusMessage = "Mira was taken. Rescue her from the Ironbound Jailer in Emberdeep.";
        }
        else if (completed == DialogueEvent.RescueMira)
        {
            if (capturedPartyIndex >= 0)
            {
                Hero rescued = party[capturedPartyIndex];
                rescued.IsCaptured = false;
                rescued.Hp = Mathf.Max(1, rescued.MaxHp / 2);
                rescued.Mp = Mathf.Max(1, rescued.MaxMp / 2);
                capturedPartyIndex = -1;
            }
            unlockedRegions[3] = true;
            statusMessage = "Mira is rescued. The Nether March gate is open.";
            StartSceneTransition();
        }
        else if (completed == DialogueEvent.CompleteCampaign)
        {
            statusMessage = "The rift is sealed and dawn returns to the four regions.";
            mode = GameMode.Victory;
            RpgAudio.Instance.PlayVictory();
            RpgAudio.Instance.PlayOverworldMusic();
            StartSceneTransition();
        }
        else if (completed == DialogueEvent.EnterRegion)
        {
            TravelToRegion(currentRegion);
        }
    }

    private void UseRegionGate()
    {
        int next = currentRegion + 1;
        if (next < discoveredRegions.Length && unlockedRegions[next])
        {
            TravelToRegion(next);
            return;
        }

        int previous = currentRegion - 1;
        if (previous >= 0 && discoveredRegions[previous])
        {
            TravelToRegion(previous);
            return;
        }

        BeginDialogue("Sealed Route", "The next path is sealed.", RegionObjective(currentRegion));
    }

    private void TravelToRegion(int region)
    {
        if (region < 0 || region >= discoveredRegions.Length ||
            (!discoveredRegions[region] && !unlockedRegions[region]))
        {
            statusMessage = "That region has not been unlocked.";
            return;
        }

        currentRegion = region;
        discoveredRegions[region] = true;
        bossDefeated = defeatedBosses[region];
        regionEncountersWon = regionalBattleWins[region];
        chestOpened = sideQuestCompleted[region];
        BuildWorld();
        playerPosition = GetRegionSpawnPosition(region);
        playerVisualPosition = new Vector2(playerPosition.x, playerPosition.y);
        playerMoveStart = playerVisualPosition;
        playerMoveTarget = playerVisualPosition;
        playerIsWalking = false;
        grassSteps = 0;
        nextMoveTime = Time.unscaledTime + 0.2f;
        PopulateRegionNpcs(region);
        activeNpc = null;
        mode = GameMode.Explore;
        menuPage = MenuPage.Root;
        statusMessage = "Arrived in " + RegionName(region) + ". " + RegionObjective(region);
        RpgAudio.Instance.PlayOverworldMusic();
        StartSceneTransition();
    }

    private void StartSceneTransition()
    {
        isSceneTransitioning = true;
        transitionTimer = RegionTransitionDuration;
    }

    private bool HasDiscoveredAnotherRegion()
    {
        for (int i = 0; i < discoveredRegions.Length; i++)
        {
            if (i != currentRegion && discoveredRegions[i])
            {
                return true;
            }
        }
        return false;
    }

    private void MoveTravelSelection(int direction)
    {
        for (int i = 0; i < discoveredRegions.Length; i++)
        {
            travelSelection = (travelSelection + direction + discoveredRegions.Length) %
                discoveredRegions.Length;
            if (discoveredRegions[travelSelection])
            {
                return;
            }
        }
    }

    private void BeginEnemyTargeting(bool skill)
    {
        battle.IsSelectingSkillTarget = skill;
        battle.TargetEnemyIndex = FirstLivingEnemyIndex();
        battle.Menu = BattleMenu.EnemyTarget;
        battle.Selection = battle.TargetEnemyIndex;
        if (CountLivingEnemies() == 1)
        {
            ConfirmBattleSelection();
        }
    }

    private int FirstLivingEnemyIndex()
    {
        if (battle == null)
        {
            return 0;
        }
        for (int i = 0; i < battle.Enemies.Count; i++)
        {
            if (battle.Enemies[i].Hp > 0)
            {
                return i;
            }
        }
        return 0;
    }

    private int CountLivingEnemies()
    {
        if (battle == null)
        {
            return 0;
        }
        int count = 0;
        for (int i = 0; i < battle.Enemies.Count; i++)
        {
            if (battle.Enemies[i].Hp > 0)
            {
                count++;
            }
        }
        return count;
    }

    private void LearnLevelSkills(Hero hero)
    {
        if (hero.Level == 2)
        {
            if (hero.Role == "Vanguard")
            {
                LearnSkill(hero, CharacterSkill.FlameArc);
            }
            else if (hero.Role == "Arcanist")
            {
                LearnSkill(hero, CharacterSkill.FlameArc);
            }
            else
            {
                LearnSkill(hero, CharacterSkill.Spark);
            }
        }
        else if (hero.Level == 3)
        {
            if (hero.Role == "Vanguard")
            {
                LearnSkill(hero, CharacterSkill.FrostNova);
            }
            else if (hero.Role == "Arcanist")
            {
                LearnSkill(hero, CharacterSkill.FrostNova);
            }
            else
            {
                LearnSkill(hero, CharacterSkill.GreaterCure);
            }
        }
        else if (hero.Level == 5 && hero.Role == "Arcanist")
        {
            LearnSkill(hero, CharacterSkill.Starfall);
        }
    }

    private void LearnSkill(Hero hero, CharacterSkill skill)
    {
        if (hero.Skills.Contains(skill))
        {
            return;
        }
        hero.Skills.Add(skill);
        if (learnedSkillsReport.Length > 0)
        {
            learnedSkillsReport += ", ";
        }
        learnedSkillsReport += hero.Name + " learned " + SkillName(skill);
    }

    private void InitializeStartingEquipment()
    {
        ownedEquipment.Clear();
        ownedEquipment.Add(new EquipmentDefinition("Iron Broadsword", true, "Vanguard", 1, 0, 0));
        ownedEquipment.Add(new EquipmentDefinition("Full Plate", false, "Vanguard", 0, 1, 0));
        ownedEquipment.Add(new EquipmentDefinition("Ashwood Staff", true, "Arcanist", 0, 0, 1));
        ownedEquipment.Add(new EquipmentDefinition("Violet Weave", false, "Arcanist", 0, 1, 0));
        ownedEquipment.Add(new EquipmentDefinition("Traveler's Tome", true, "Mender", 0, 0, 1));
        ownedEquipment.Add(new EquipmentDefinition("White Vestments", false, "Mender", 0, 1, 0));
    }

    private void AddOwnedEquipment(EquipmentDefinition equipment)
    {
        for (int i = 0; i < ownedEquipment.Count; i++)
        {
            if (ownedEquipment[i].Name == equipment.Name)
            {
                return;
            }
        }
        ownedEquipment.Add(equipment);
    }

    private void CycleEquipment(int heroIndex, bool weaponSlot, int direction)
    {
        Hero hero = party[heroIndex];
        List<EquipmentDefinition> options = new List<EquipmentDefinition>();
        for (int i = 0; i < ownedEquipment.Count; i++)
        {
            EquipmentDefinition item = ownedEquipment[i];
            if (item.IsWeapon == weaponSlot &&
                (string.IsNullOrEmpty(item.Role) || item.Role == hero.Role))
            {
                options.Add(item);
            }
        }
        if (options.Count < 2)
        {
            statusMessage = "No other " + (weaponSlot ? "weapon" : "armor") + " is owned for " + hero.Name + ".";
            return;
        }

        string equippedName = weaponSlot ? hero.WeaponName : hero.ArmorName;
        int current = 0;
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].Name == equippedName)
            {
                current = i;
                break;
            }
        }
        int next = (current + direction + options.Count) % options.Count;
        ApplyEquipment(hero, options[next]);
        statusMessage = hero.Name + " equipped " + options[next].Name + ".";
    }

    private void ApplyEquipment(Hero hero, EquipmentDefinition equipment)
    {
        if (equipment.IsWeapon)
        {
            hero.WeaponName = equipment.Name;
            hero.WeaponAttack = equipment.Attack;
            hero.WeaponMagic = equipment.Magic;
        }
        else
        {
            hero.ArmorName = equipment.Name;
            hero.ArmorDefense = equipment.Defense;
        }
    }

    private void BuyShopOffer(int offerIndex, int heroIndex)
    {
        if (offerIndex < 0 || offerIndex >= ShopOffers.Length)
        {
            return;
        }
        ShopOffer offer = ShopOffers[offerIndex];
        if (gil < offer.Price)
        {
            statusMessage = "You need " + offer.Price + " gil to buy " + offer.Name + ".";
            return;
        }

        if (offer.IsEquipment)
        {
            if (!string.IsNullOrEmpty(offer.Equipment.Role) &&
                offer.Equipment.Role != party[heroIndex].Role)
            {
                statusMessage = offer.Name + " is made for the " + offer.Equipment.Role + ".";
                return;
            }
            for (int i = 0; i < ownedEquipment.Count; i++)
            {
                if (ownedEquipment[i].Name == offer.Equipment.Name)
                {
                    statusMessage = "The party already owns " + offer.Name + ".";
                    return;
                }
            }
            AddOwnedEquipment(offer.Equipment);
            ApplyEquipment(party[heroIndex], offer.Equipment);
            statusMessage = party[heroIndex].Name + " equipped " + offer.Name + ".";
        }
        else
        {
            AddItem(offer.Item, 1);
            statusMessage = "Bought " + offer.Name + ".";
        }
        gil -= offer.Price;
        RpgAudio.Instance.PlayItem();
    }

    private static string EquipmentDescription(EquipmentDefinition equipment)
    {
        string slot = equipment.IsWeapon ? "Weapon" : "Armor";
        string role = string.IsNullOrEmpty(equipment.Role) ? "Any role" : equipment.Role;
        return role + " " + slot + "  ATK +" + equipment.Attack +
            " DEF +" + equipment.Defense + " MAG +" + equipment.Magic;
    }

    private static string RegionName(int region)
    {
        switch (region)
        {
            case 1: return "Whisperwood";
            case 2: return "Emberdeep";
            case 3: return "Nether March";
            default: return "Greenwick";
        }
    }

    private static string RegionShopName(int region)
    {
        switch (region)
        {
            case 1: return "Mossveil Waystation";
            case 2: return "Emberdeep Foundry";
            case 3: return "Ashen Market";
            default: return "Greenwick Market";
        }
    }

    private static string RegionBossName(int region)
    {
        switch (region)
        {
            case 1: return "Briar Queen";
            case 2: return "Ironbound Jailer";
            case 3: return "Malachar";
            default: return "Gloam Warden";
        }
    }

    private static ItemKind BossReward(int region)
    {
        switch (region)
        {
            case 1: return ItemKind.Briarheart;
            case 2: return ItemKind.PrisonKey;
            case 3: return ItemKind.VoidCrown;
            default: return ItemKind.Sunstone;
        }
    }

    private static string RegionObjective(int region)
    {
        switch (region)
        {
            case 1: return "Win two encounters, then face the Briar Queen.";
            case 2: return "Kestrel and Tavi must defeat the Ironbound Jailer to free Mira.";
            case 3: return "Win two encounters, then confront Malachar at the rift.";
            default: return "Win two encounters, then face the Gloam Warden.";
        }
    }

    private static string ItemName(ItemKind item)
    {
        return GetItemDefinition(item).Name;
    }

    private static int ResolveSpeakerFace(string title)
    {
        if (title.IndexOf("Malachar", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return 3;
        }
        if (title.IndexOf("Mira", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return 1;
        }
        if (title.IndexOf("Tavi", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return 2;
        }
        if (title.IndexOf("Kestrel", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return 0;
        }
        if (title.IndexOf("Elder", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            title.IndexOf("Rowan", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return 4;
        }
        if (title.IndexOf("Mara", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            title.IndexOf("Fern", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            title.IndexOf("Echo", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            title.IndexOf("Pilgrim", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return 5;
        }
        if (title.IndexOf("Tamsin", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            title.IndexOf("Pollen", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            title.IndexOf("Toma", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
            title.IndexOf("Vesper", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return 6;
        }
        return 7;
    }
}
