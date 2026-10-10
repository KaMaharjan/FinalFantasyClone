using System.Collections.Generic;
using UnityEngine;

public partial class RpgGame : MonoBehaviour
{
    private const int MapWidth = 20;
    private const int MapHeight = 14;
    private const float BattleActionDuration = 0.42f;
    private const float FleeAnimationDuration = 0.66f;

    private enum GameMode
    {
        Explore,
        Dialogue,
        Menu,
        Battle,
        GameOver,
        Victory
    }

    private enum CharacterSkill
    {
        PowerStrike,
        Spark,
        Cure,
        FlameArc,
        FrostNova,
        GreaterCure,
        Starfall
    }

    private enum BattleMenu
    {
        Commands,
        Skill,
        Item,
        ItemTarget,
        EnemyTarget
    }

    private enum MenuPage
    {
        Root,
        Items,
        Status,
        Equipment,
        Shop,
        Travel
    }

    private enum NpcKind
    {
        Elder,
        Healer,
        Merchant,
        Guide
    }

    private enum DialogueEvent
    {
        None,
        UnlockForest,
        KidnapMira,
        RescueMira,
        CompleteCampaign,
        EnterRegion
    }

    private enum EnemyKind
    {
        GreenwickSlime, ThornWolf, GraveMoth, RoadBandit,
        Briarling, MossStalker, ThornWisp, BarkBeetle,
        EmberBat, AshCrawler, MagmaHound, CrystalGolem,
        NetherImp, SoulWraith, AbyssKnight, RiftDrake,
        GloamWarden, BriarQueen, IronboundJailer, Malachar
    }

    private enum ItemKind
    {
        Potion,
        Ether,
        PhoenixFeather,
        SmokeBomb,
        Sunstone,
        Briarheart,
        PrisonKey,
        VoidCrown
    }

    private enum BattleEffect
    {
        Slash,
        Magic,
        Healing,
        Fire,
        Frost,
        Starfall,
        VanguardAttack,
        ArcanistAttack,
        MenderAttack
    }

    private enum BattleAnimation
    {
        None,
        Attack,
        Skill,
        Item,
        Damage
    }

    private sealed class ItemDefinition
    {
        public ItemKind Kind;
        public string Name;
        public string Description;
        public bool CanUseOutsideBattle;
        public bool CanUseInBattle;
        public bool TargetsParty;

        public ItemDefinition(ItemKind kind, string name, string description, bool canUseOutsideBattle,
            bool canUseInBattle, bool targetsParty)
        {
            Kind = kind;
            Name = name;
            Description = description;
            CanUseOutsideBattle = canUseOutsideBattle;
            CanUseInBattle = canUseInBattle;
            TargetsParty = targetsParty;
        }
    }

    private sealed class Hero
    {
        public string Name;
        public string Role;
        public CharacterSkill Skill;
        public int Level;
        public int Experience;
        public int MaxHp;
        public int Hp;
        public int MaxMp;
        public int Mp;
        public int Attack;
        public int Defense;
        public int Magic;
        public bool IsCaptured;
        public string WeaponName;
        public string ArmorName;
        public int WeaponAttack;
        public int ArmorDefense;
        public int WeaponMagic;
        public readonly List<CharacterSkill> Skills = new List<CharacterSkill>();

        public int ExperienceToNextLevel
        {
            get { return Level >= 5 ? int.MaxValue : 30 + Level * 20; }
        }
    }

    private sealed class Enemy
    {
        public string Name;
        public int MaxHp;
        public int Hp;
        public int Attack;
        public int Defense;
        public int Experience;
        public int Gil;
        public EnemyKind Kind;
        public bool IsBoss;
        public int SpriteIndex;
    }

    private sealed class Npc
    {
        public string Name;
        public Vector2Int Position;
        public NpcKind Kind;
        public Vector2 VisualPosition;
        public Vector2 MoveStart;
        public Vector2 MoveTarget;
        public float MoveElapsed;
        public float NextWanderTime;
    }

    private sealed class BattleState
    {
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public bool IsBoss;
        public int ActorIndex;
        public int ActionsRemaining;
        public int Selection;
        public ItemKind SelectedItem;
        public int TargetEnemyIndex;
        public CharacterSkill SelectedSkill;
        public bool IsSelectingSkillTarget;
        public BattleMenu Menu;
        public readonly List<string> Log = new List<string>();
    }

    private static readonly ItemDefinition[] ItemCatalog =
    {
        new ItemDefinition(ItemKind.Potion, "Potion", "Restore up to 28 HP.", true, true, true),
        new ItemDefinition(ItemKind.Ether, "Ether", "Restore up to 8 MP.", true, true, true),
        new ItemDefinition(ItemKind.PhoenixFeather, "Phoenix Feather", "Revive an ally with half HP.", true, true, true),
        new ItemDefinition(ItemKind.SmokeBomb, "Smoke Bomb", "Escape from a battle.", false, true, false),
        new ItemDefinition(ItemKind.Sunstone, "Sunstone", "Greenwick's unique guardian relic.", false, false, false),
        new ItemDefinition(ItemKind.Briarheart, "Briarheart", "A living seed from the Briar Queen.", false, false, false),
        new ItemDefinition(ItemKind.PrisonKey, "Prison Key", "The key to Mira's iron cell.", false, false, false),
        new ItemDefinition(ItemKind.VoidCrown, "Void Crown", "Malachar's broken crown.", false, false, false)
    };

    private readonly List<Hero> party = new List<Hero>();
    private readonly List<Npc> npcs = new List<Npc>();
    private readonly List<string> dialogueLines = new List<string>();
    private readonly Dictionary<ItemKind, int> inventory = new Dictionary<ItemKind, int>();
    private readonly bool[] discoveredRegions = new bool[4];
    private readonly bool[] unlockedRegions = new bool[4];
    private readonly bool[] sideQuestCompleted = new bool[4];
    private readonly bool[] defeatedBosses = new bool[4];
    private readonly int[] regionalBattleWins = new int[4];

    private char[,] terrain;
    private Vector2Int playerPosition;
    private Vector2Int chestPosition;
    private Vector2Int shrinePosition;
    private GameMode mode;
    private GameMode returnMode;
    private MenuPage menuPage;
    private BattleState battle;
    private string dialogueTitle;
    private string statusMessage;
    private string levelUpReport;
    private string learnedSkillsReport;
    private int dialogueIndex;
    private int menuSelection;
    private int menuTargetIndex;
    private int gil;
    private int grassSteps;
    private int currentRegion;
    private int regionEncountersWon;
    private int equipmentHeroIndex;
    private int shopHeroIndex;
    private int shopSelection;
    private int travelSelection;
    private int activeSpeakerFace;
    private int capturedPartyIndex = -1;
    private DialogueEvent dialogueEvent;
    private Npc activeNpc;
    private bool equipmentWeaponSlot;
    private float transitionTimer;
    private bool isSceneTransitioning;
    private float nextMoveTime;
    private Vector2 playerVisualPosition;
    private Vector2 playerMoveStart;
    private Vector2 playerMoveTarget;
    private float playerMoveElapsed;
    private float enemyHitTimer;
    private float heroHitTimer;
    private float battleEffectTimer;
    private float battleAnimationTimer;
    private float fleeAnimationTimer;
    private int heroHitIndex;
    private int battleEffectHeroIndex;
    private int battleAnimationHeroIndex;
    private int battleAnimationTargetIndex;
    private int battleAnimationEnemyIndex;
    private BattleEffect battleEffect;
    private BattleAnimation battleAnimation;
    private bool playerIsWalking;
    private bool choosingItemTarget;
    private bool chestOpened;
    private bool bossDefeated;
    private bool fleeWillSucceed;
    private bool pendingBattleVictory;
    private bool pendingEnemyTurn;
    private bool pendingGameOver;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureGameExists()
    {
        if (FindObjectOfType<RpgGame>() != null)
        {
            return;
        }

        new GameObject("Echoes of Aether").AddComponent<RpgGame>();
    }

    private void Awake()
    {
        ResetGame();
    }

    private void Update()
    {
        UpdateAnimations();
        switch (mode)
        {
            case GameMode.Explore:
                UpdateExploration();
                break;
            case GameMode.Dialogue:
                UpdateDialogue();
                break;
            case GameMode.Menu:
                UpdateMenu();
                break;
            case GameMode.Battle:
                UpdateBattle();
                break;
            case GameMode.GameOver:
            case GameMode.Victory:
                if (ConfirmPressed())
                {
                    ResetGame();
                }
                break;
        }
    }

    private void ResetGame()
    {
        party.Clear();
        party.Add(new Hero
        {
            Name = "Kestrel",
            Role = "Vanguard",
            Skill = CharacterSkill.PowerStrike,
            Level = 1,
            MaxHp = 48,
            Hp = 48,
            MaxMp = 5,
            Mp = 5,
            Attack = 9,
            Defense = 5,
            Magic = 2,
            WeaponName = "Iron Broadsword",
            ArmorName = "Full Plate",
            WeaponAttack = 1,
            ArmorDefense = 1
        });
        party.Add(new Hero
        {
            Name = "Mira",
            Role = "Arcanist",
            Skill = CharacterSkill.Spark,
            Level = 1,
            MaxHp = 32,
            Hp = 32,
            MaxMp = 15,
            Mp = 15,
            Attack = 4,
            Defense = 2,
            Magic = 8,
            WeaponName = "Ashwood Staff",
            ArmorName = "Violet Weave",
            WeaponMagic = 1,
            ArmorDefense = 1
        });
        party.Add(new Hero
        {
            Name = "Tavi",
            Role = "Mender",
            Skill = CharacterSkill.Cure,
            Level = 1,
            MaxHp = 38,
            Hp = 38,
            MaxMp = 12,
            Mp = 12,
            Attack = 5,
            Defense = 3,
            Magic = 7,
            WeaponName = "Traveler's Tome",
            ArmorName = "White Vestments",
            WeaponMagic = 1,
            ArmorDefense = 1
        });
        for (int i = 0; i < party.Count; i++)
        {
            party[i].Skills.Add(party[i].Skill);
        }

        currentRegion = 0;
        InitializeStartingEquipment();
        capturedPartyIndex = -1;
        regionEncountersWon = 0;
        equipmentHeroIndex = 0;
        shopSelection = 0;
        shopHeroIndex = 0;
        travelSelection = 0;
        equipmentWeaponSlot = true;
        dialogueEvent = DialogueEvent.None;
        activeNpc = null;
        isSceneTransitioning = false;
        transitionTimer = 0f;
        for (int i = 0; i < discoveredRegions.Length; i++)
        {
            discoveredRegions[i] = i == 0;
            unlockedRegions[i] = i == 0;
            sideQuestCompleted[i] = false;
            defeatedBosses[i] = false;
            regionalBattleWins[i] = 0;
        }
        BuildWorld();
        playerPosition = new Vector2Int(5, 6);
        playerVisualPosition = new Vector2(playerPosition.x, playerPosition.y);
        playerMoveStart = playerVisualPosition;
        playerMoveTarget = playerVisualPosition;
        playerMoveElapsed = 0f;
        playerIsWalking = false;
        enemyHitTimer = 0f;
        heroHitTimer = 0f;
        battleEffectTimer = 0f;
        battleAnimationTimer = 0f;
        fleeAnimationTimer = 0f;
        heroHitIndex = -1;
        battleEffectHeroIndex = -1;
        battleAnimationHeroIndex = -1;
        battleAnimationTargetIndex = -1;
        battleAnimationEnemyIndex = -1;
        battleAnimation = BattleAnimation.None;
        fleeWillSucceed = false;
        pendingBattleVictory = false;
        pendingEnemyTurn = false;
        pendingGameOver = false;
        PopulateRegionNpcs(currentRegion);

        mode = GameMode.Explore;
        returnMode = GameMode.Explore;
        menuPage = MenuPage.Root;
        battle = null;
        dialogueLines.Clear();
        dialogueIndex = 0;
        dialogueTitle = string.Empty;
        menuSelection = 0;
        menuTargetIndex = 0;
        choosingItemTarget = false;
        inventory.Clear();
        AddItem(ItemKind.Potion, 3);
        AddItem(ItemKind.Ether, 2);
        AddItem(ItemKind.PhoenixFeather, 1);
        AddItem(ItemKind.SmokeBomb, 1);
        gil = 125;
        grassSteps = 0;
        nextMoveTime = 0f;
        chestOpened = false;
        bossDefeated = false;
        statusMessage = "The Sunwell is fading. Speak with Elder Rowan in Greenwick.";
        dialogueEvent = DialogueEvent.None;
        activeNpc = null;
        RpgAudio.Instance.PlayOverworldMusic();
    }

    private void UpdateAnimations()
    {
        float deltaTime = Time.unscaledDeltaTime;
        if (playerIsWalking)
        {
            playerMoveElapsed += deltaTime;
            float progress = Mathf.Clamp01(playerMoveElapsed / 0.12f);
            float easedProgress = progress * progress * (3f - 2f * progress);
            playerVisualPosition = Vector2.Lerp(playerMoveStart, playerMoveTarget, easedProgress);
            if (progress >= 1f)
            {
                playerVisualPosition = playerMoveTarget;
                playerIsWalking = false;
            }
        }

        enemyHitTimer = Mathf.Max(0f, enemyHitTimer - deltaTime);
        heroHitTimer = Mathf.Max(0f, heroHitTimer - deltaTime);
        battleEffectTimer = Mathf.Max(0f, battleEffectTimer - deltaTime);
        battleAnimationTimer = Mathf.Max(0f, battleAnimationTimer - deltaTime);
        transitionTimer = Mathf.Max(0f, transitionTimer - deltaTime);
        if (isSceneTransitioning && transitionTimer <= 0f)
        {
            isSceneTransitioning = false;
        }
        UpdateNpcWandering(deltaTime);

        if (fleeAnimationTimer > 0f)
        {
            fleeAnimationTimer = Mathf.Max(0f, fleeAnimationTimer - deltaTime);
            if (fleeAnimationTimer == 0f)
            {
                CompleteFleeAnimation();
            }
        }

        if (battleAnimationTimer <= 0f)
        {
            battleAnimation = BattleAnimation.None;
            if (pendingBattleVictory && battle != null && mode == GameMode.Battle)
            {
                pendingBattleVictory = false;
                EndBattle(true);
            }
            else if (pendingGameOver && mode == GameMode.Battle)
            {
                pendingGameOver = false;
                mode = GameMode.GameOver;
                RpgAudio.Instance.PlayDefeat();
                RpgAudio.Instance.PlayOverworldMusic();
            }
            else if (pendingEnemyTurn && battle != null && mode == GameMode.Battle)
            {
                pendingEnemyTurn = false;
                PerformEnemyTurn();
            }
        }
    }

    private void BuildWorld()
    {
        terrain = CreateRegionTerrain(currentRegion);
        chestPosition = GetRegionChestPosition(currentRegion);
        shrinePosition = GetRegionBossPosition(currentRegion);
    }

    private void PaintRect(int x, int y, int width, int height, char tile)
    {
        for (int row = y; row < y + height; row++)
        {
            for (int column = x; column < x + width; column++)
            {
                if (column >= 0 && column < MapWidth && row >= 0 && row < MapHeight)
                {
                    terrain[column, row] = tile;
                }
            }
        }
    }

    private void UpdateExploration()
    {
        if (isSceneTransitioning)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
        {
            mode = GameMode.Menu;
            menuPage = MenuPage.Root;
            menuSelection = 0;
            return;
        }

        if (ConfirmPressed())
        {
            Interact();
            return;
        }

        if (Time.unscaledTime < nextMoveTime)
        {
            return;
        }

        Vector2Int direction = ReadMovement();
        if (direction != Vector2Int.zero)
        {
            MovePlayer(direction);
        }
    }

    private Vector2Int ReadMovement()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            return new Vector2Int(0, -1);
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            return new Vector2Int(0, 1);
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            return Vector2Int.left;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            return Vector2Int.right;
        }

        return Vector2Int.zero;
    }

    private void MovePlayer(Vector2Int direction)
    {
        nextMoveTime = Time.unscaledTime + 0.14f;
        Vector2Int destination = playerPosition + direction;
        if (!IsInsideMap(destination) || !IsWalkable(terrain[destination.x, destination.y]))
        {
            return;
        }

        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i].Position == destination)
            {
                statusMessage = npcs[i].Name + " is just ahead. Press Space to speak.";
                return;
            }
        }

        Vector2Int origin = playerPosition;
        playerPosition = destination;
        playerMoveStart = new Vector2(origin.x, origin.y);
        playerMoveTarget = new Vector2(destination.x, destination.y);
        playerVisualPosition = playerMoveStart;
        playerMoveElapsed = 0f;
        playerIsWalking = true;
        RpgAudio.Instance.PlayFootstep();
        if (IsEncounterTerrain(terrain[destination.x, destination.y]))
        {
            grassSteps++;
            if (grassSteps >= 3 && Random.value < 0.15f)
            {
                grassSteps = 0;
                StartWildBattle();
            }
        }
        else
        {
            grassSteps = 0;
        }
    }

    private static bool IsWalkable(char tile)
    {
        return tile == 'g' || tile == 'f' || tile == 'a' || tile == 'c' || tile == 'v' ||
               tile == '.' || tile == '=' || tile == 'S' || tile == 'X' || tile == 'C';
    }

    private bool IsInsideMap(Vector2Int position)
    {
        return position.x >= 0 && position.x < MapWidth &&
               position.y >= 0 && position.y < MapHeight;
    }

    private void Interact()
    {
        RpgAudio.Instance.PlayInteract();
        for (int i = 0; i < npcs.Count; i++)
        {
            if (ManhattanDistance(playerPosition, npcs[i].Position) <= 1)
            {
                TalkTo(npcs[i]);
                return;
            }
        }

        if (ManhattanDistance(playerPosition, chestPosition) <= 1)
        {
            OpenChest();
            return;
        }

        if (ManhattanDistance(playerPosition, shrinePosition) <= 1)
        {
            ApproachShrine();
            return;
        }

        if (ManhattanDistance(playerPosition, GetRegionGatePosition(currentRegion)) <= 1)
        {
            UseRegionGate();
            return;
        }

        BeginDialogue(RegionName(currentRegion), "The road is quiet here.", RegionObjective(currentRegion));
    }

    private static int ManhattanDistance(Vector2Int left, Vector2Int right)
    {
        return Mathf.Abs(left.x - right.x) + Mathf.Abs(left.y - right.y);
    }

    private void TalkTo(Npc npc)
    {
        activeNpc = npc;
        HandleTalkTo(npc);
    }

    private void RestoreParty()
    {
        for (int i = 0; i < party.Count; i++)
        {
            if (!party[i].IsCaptured)
            {
                party[i].Hp = party[i].MaxHp;
                party[i].Mp = party[i].MaxMp;
            }
        }
        statusMessage = "Mara restored the party.";
        RpgAudio.Instance.PlayHealing();
    }

    private void OpenChest()
    {
        HandleRegionCache();
    }

    private void ApproachShrine()
    {
        HandleRegionBoss();
    }

    private void BeginDialogue(string title, params string[] lines)
    {
        returnMode = mode;
        mode = GameMode.Dialogue;
        dialogueTitle = title;
        dialogueLines.Clear();
        dialogueLines.AddRange(lines);
        dialogueIndex = 0;
        dialogueEvent = DialogueEvent.None;
        activeSpeakerFace = ResolveSpeakerFace(title);
    }

    private void BeginStoryDialogue(string title, DialogueEvent storyEvent, params string[] lines)
    {
        BeginDialogue(title, lines);
        dialogueEvent = storyEvent;
    }

    private void UpdateDialogue()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && dialogueEvent == DialogueEvent.None)
        {
            mode = returnMode;
            activeNpc = null;
            return;
        }

        if (ConfirmPressed())
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        if (dialogueIndex + 1 < dialogueLines.Count)
        {
            dialogueIndex++;
        }
        else
        {
            mode = returnMode;
            activeNpc = null;
            CompleteDialogueEvent();
        }
    }

    private void UpdateMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
        {
            if (menuPage == MenuPage.Root)
            {
                mode = GameMode.Explore;
            }
            else if (menuPage == MenuPage.Items && choosingItemTarget)
            {
                choosingItemTarget = false;
            }
            else
            {
                menuPage = MenuPage.Root;
                menuSelection = 0;
                choosingItemTarget = false;
            }
            return;
        }

        if (menuPage == MenuPage.Root)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                menuSelection = (menuSelection + 4) % 5;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                menuSelection = (menuSelection + 1) % 5;
            }
            else if (ConfirmPressed())
            {
                OpenMenuSelection(menuSelection);
            }
            return;
        }

        if (menuPage == MenuPage.Items)
        {
            if (choosingItemTarget)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    menuTargetIndex = (menuTargetIndex + party.Count - 1) % party.Count;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    menuTargetIndex = (menuTargetIndex + 1) % party.Count;
                }
                else if (ConfirmPressed())
                {
                    UseInventoryItemOnHero(menuTargetIndex);
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                menuSelection = (menuSelection + ItemCatalog.Length - 1) % ItemCatalog.Length;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                menuSelection = (menuSelection + 1) % ItemCatalog.Length;
            }
            else if (ConfirmPressed())
            {
                SelectInventoryItem(menuSelection);
            }
            return;
        }

        if (menuPage == MenuPage.Equipment)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                equipmentHeroIndex = (equipmentHeroIndex + party.Count - 1) % party.Count;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                equipmentHeroIndex = (equipmentHeroIndex + 1) % party.Count;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) ||
                     Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                equipmentWeaponSlot = !equipmentWeaponSlot;
            }
            else if (ConfirmPressed())
            {
                CycleEquipment(equipmentHeroIndex, equipmentWeaponSlot, 1);
            }
            return;
        }

        if (menuPage == MenuPage.Shop)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                shopSelection = (shopSelection + ShopOffers.Length - 1) % ShopOffers.Length;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                shopSelection = (shopSelection + 1) % ShopOffers.Length;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                shopHeroIndex = (shopHeroIndex + party.Count - 1) % party.Count;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                shopHeroIndex = (shopHeroIndex + 1) % party.Count;
            }
            else if (ConfirmPressed())
            {
                BuyShopOffer(shopSelection, shopHeroIndex);
            }
            return;
        }

        if (menuPage == MenuPage.Travel)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                MoveTravelSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                MoveTravelSelection(1);
            }
            else if (ConfirmPressed())
            {
                if (!discoveredRegions[travelSelection])
                {
                    statusMessage = "That destination has not been discovered yet.";
                }
                else if (travelSelection == currentRegion)
                {
                    statusMessage = "You are already in " + RegionName(currentRegion) + ".";
                }
                else
                {
                    TravelToRegion(travelSelection);
                }
            }
            return;
        }

        if (ConfirmPressed())
        {
            menuPage = MenuPage.Root;
        }
    }

    private void OpenMenuSelection(int selection)
    {
        menuSelection = selection;
        if (selection == 0)
        {
            menuPage = MenuPage.Items;
            menuSelection = 0;
            choosingItemTarget = false;
        }
        else if (selection == 1)
        {
            menuPage = MenuPage.Status;
        }
        else if (selection == 2)
        {
            menuPage = MenuPage.Equipment;
        }
        else if (selection == 3)
        {
            if (!HasDiscoveredAnotherRegion())
            {
                statusMessage = "Quick travel unlocks after discovering another region.";
                return;
            }
            menuPage = MenuPage.Travel;
            travelSelection = currentRegion;
        }
        else
        {
            mode = GameMode.Explore;
        }
    }

    private void SelectInventoryItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= ItemCatalog.Length)
        {
            return;
        }

        menuSelection = itemIndex;
        ItemDefinition item = ItemCatalog[itemIndex];
        if (GetItemCount(item.Kind) <= 0)
        {
            statusMessage = "You have no " + item.Name + " left.";
            return;
        }

        if (!item.CanUseOutsideBattle)
        {
            statusMessage = item.CanUseInBattle
                ? item.Name + " can only be used in battle."
                : item.Name + " is a story relic, not a consumable.";
            return;
        }

        if (!item.TargetsParty)
        {
            statusMessage = item.Name + " has no valid target here.";
            return;
        }

        choosingItemTarget = true;
        menuTargetIndex = FindPreferredItemTarget(item.Kind);
    }

    private int FindPreferredItemTarget(ItemKind kind)
    {
        if (kind == ItemKind.PhoenixFeather)
        {
            for (int i = 0; i < party.Count; i++)
            {
                if (!party[i].IsCaptured && party[i].Hp <= 0)
                {
                    return i;
                }
            }
        }

        for (int i = 0; i < party.Count; i++)
        {
            if (!party[i].IsCaptured)
            {
                return i;
            }
        }
        return -1;
    }

    private void UseInventoryItemOnHero(int heroIndex)
    {
        ItemKind kind = ItemCatalog[menuSelection].Kind;
        string result;
        if (TryUseTargetedItem(kind, party[heroIndex], out result))
        {
            statusMessage = result;
            choosingItemTarget = false;
            RpgAudio.Instance.PlayItem();
        }
        else
        {
            statusMessage = result;
        }
    }

    private bool TryUseTargetedItem(ItemKind kind, Hero target, out string result)
    {
        if (GetItemCount(kind) <= 0)
        {
            result = "You have no " + GetItemDefinition(kind).Name + " left.";
            return false;
        }

        if (target.IsCaptured)
        {
            result = target.Name + " cannot be reached while captured.";
            return false;
        }

        switch (kind)
        {
            case ItemKind.Potion:
                if (target.Hp <= 0)
                {
                    result = "A potion cannot revive " + target.Name + ".";
                    return false;
                }
                if (target.Hp >= target.MaxHp)
                {
                    result = target.Name + " is already at full health.";
                    return false;
                }

                int restoredHp = Mathf.Min(28, target.MaxHp - target.Hp);
                target.Hp += restoredHp;
                result = target.Name + " recovered " + restoredHp + " HP.";
                break;
            case ItemKind.Ether:
                if (target.Hp <= 0)
                {
                    result = "An ether cannot revive " + target.Name + ".";
                    return false;
                }
                if (target.Mp >= target.MaxMp)
                {
                    result = target.Name + " already has full MP.";
                    return false;
                }

                int restoredMp = Mathf.Min(8, target.MaxMp - target.Mp);
                target.Mp += restoredMp;
                result = target.Name + " recovered " + restoredMp + " MP.";
                break;
            case ItemKind.PhoenixFeather:
                if (target.Hp > 0)
                {
                    result = target.Name + " is not knocked out.";
                    return false;
                }

                target.Hp = Mathf.Max(1, target.MaxHp / 2);
                result = target.Name + " returned with " + target.Hp + " HP.";
                break;
            default:
                result = GetItemDefinition(kind).Name + " cannot be used on a party member.";
                return false;
        }

        ConsumeItem(kind);
        return true;
    }

    private void AddItem(ItemKind kind, int amount)
    {
        inventory[kind] = GetItemCount(kind) + Mathf.Max(0, amount);
    }

    private int GetItemCount(ItemKind kind)
    {
        int count;
        return inventory.TryGetValue(kind, out count) ? count : 0;
    }

    private bool ConsumeItem(ItemKind kind)
    {
        int count = GetItemCount(kind);
        if (count <= 0)
        {
            return false;
        }

        inventory[kind] = count - 1;
        return true;
    }

    private static ItemDefinition GetItemDefinition(ItemKind kind)
    {
        for (int i = 0; i < ItemCatalog.Length; i++)
        {
            if (ItemCatalog[i].Kind == kind)
            {
                return ItemCatalog[i];
            }
        }

        return ItemCatalog[0];
    }

    private void UpdateBattle()
    {
        if (battle == null)
        {
            mode = GameMode.Explore;
            return;
        }

        if (battleAnimationTimer > 0f || fleeAnimationTimer > 0f || pendingBattleVictory
            || pendingEnemyTurn || pendingGameOver)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            if (battle.Menu == BattleMenu.ItemTarget)
            {
                battle.Menu = BattleMenu.Item;
                battle.Selection = GetItemCatalogIndex(battle.SelectedItem);
            }
            else if (battle.Menu == BattleMenu.EnemyTarget)
            {
                battle.Menu = battle.IsSelectingSkillTarget ? BattleMenu.Skill : BattleMenu.Commands;
                battle.Selection = 0;
            }
            else if (battle.Menu != BattleMenu.Commands)
            {
                battle.Menu = BattleMenu.Commands;
                battle.Selection = 0;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            MoveBattleSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MoveBattleSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveBattleSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveBattleSelection(1);
        }
        else if (ConfirmPressed())
        {
            ConfirmBattleSelection();
        }
    }

    private void MoveBattleSelection(int direction)
    {
        int optionCount;
        if (battle.Menu == BattleMenu.Commands)
        {
            optionCount = 4;
        }
        else if (battle.Menu == BattleMenu.Skill)
        {
            optionCount = party[battle.ActorIndex].Skills.Count + 1;
        }
        else if (battle.Menu == BattleMenu.Item)
        {
            optionCount = BattleItemCount() + 1;
        }
        else if (battle.Menu == BattleMenu.ItemTarget)
        {
            optionCount = party.Count + 1;
        }
        else
        {
            optionCount = battle.Enemies.Count;
        }

        battle.Selection = (battle.Selection + direction + optionCount) % optionCount;
    }

    private void ConfirmBattleSelection()
    {
        if (battle.Menu == BattleMenu.Commands)
        {
            switch (battle.Selection)
            {
                case 0:
                    BeginEnemyTargeting(false);
                    return;
                case 1:
                    battle.Menu = BattleMenu.Skill;
                    battle.Selection = 0;
                    return;
                case 2:
                    battle.Menu = BattleMenu.Item;
                    battle.Selection = 0;
                    return;
                default:
                    TryRunAway();
                    return;
            }
        }

        if (battle.Menu == BattleMenu.Skill)
        {
            Hero actor = party[battle.ActorIndex];
            if (battle.Selection < actor.Skills.Count)
            {
                CharacterSkill selectedSkill = actor.Skills[battle.Selection];
                if (selectedSkill == CharacterSkill.Cure || selectedSkill == CharacterSkill.GreaterCure)
                {
                    if (PerformSkill(selectedSkill))
                    {
                        FinishBattleAction();
                    }
                }
                else
                {
                    battle.SelectedSkill = selectedSkill;
                    BeginEnemyTargeting(true);
                }
            }
            else
            {
                battle.Menu = BattleMenu.Commands;
                battle.Selection = 0;
            }
            return;
        }

        if (battle.Menu == BattleMenu.Item)
        {
            if (battle.Selection >= BattleItemCount())
            {
                battle.Menu = BattleMenu.Commands;
                battle.Selection = 0;
                return;
            }

            ItemDefinition item = ItemCatalog[battle.Selection];
            if (GetItemCount(item.Kind) <= 0)
            {
                AddBattleLog("You have no " + item.Name + " left.");
                return;
            }
            if (!item.CanUseInBattle)
            {
                AddBattleLog(item.Name + " cannot be used in battle.");
                return;
            }

            if (item.TargetsParty)
            {
                battle.SelectedItem = item.Kind;
                battle.Menu = BattleMenu.ItemTarget;
                battle.Selection = FindPreferredItemTarget(item.Kind);
            }
            else
            {
                UseSmokeBombInBattle();
            }
            return;
        }

        if (battle.Menu == BattleMenu.EnemyTarget)
        {
            if (battle.Selection < 0 || battle.Selection >= battle.Enemies.Count ||
                battle.Enemies[battle.Selection].Hp <= 0)
            {
                battle.Selection = FirstLivingEnemyIndex();
            }
            battle.TargetEnemyIndex = battle.Selection;
            if (battle.IsSelectingSkillTarget)
            {
                if (PerformSkill(battle.SelectedSkill))
                {
                    FinishBattleAction();
                }
            }
            else
            {
                PerformAttack();
                FinishBattleAction();
            }
            return;
        }

        if (battle.Selection < party.Count)
        {
            if (party[battle.Selection].IsCaptured)
            {
                AddBattleLog(party[battle.Selection].Name + " is not here to receive the item.");
                return;
            }
            if (UseBattleItemOnHero(battle.SelectedItem, battle.Selection))
            {
                FinishBattleAction();
            }
            return;
        }

        battle.Menu = BattleMenu.Item;
        battle.Selection = GetItemCatalogIndex(battle.SelectedItem);
    }

    private int GetItemCatalogIndex(ItemKind kind)
    {
        for (int i = 0; i < ItemCatalog.Length; i++)
        {
            if (ItemCatalog[i].Kind == kind)
            {
                return i;
            }
        }

        return 0;
    }

    private static int BattleItemCount()
    {
        int count = 0;
        for (int i = 0; i < ItemCatalog.Length; i++)
        {
            if (ItemCatalog[i].CanUseInBattle)
            {
                count++;
            }
        }
        return count;
    }

    private bool UseBattleItemOnHero(ItemKind kind, int heroIndex)
    {
        string result;
        if (!TryUseTargetedItem(kind, party[heroIndex], out result))
        {
            AddBattleLog(result);
            return false;
        }

        AddBattleLog(result);
        RpgAudio.Instance.PlayItem();
        TriggerBattleEffect(kind == ItemKind.Ether ? BattleEffect.Magic : BattleEffect.Healing, heroIndex);
        StartBattleAnimation(BattleAnimation.Item, battle.ActorIndex, heroIndex);
        battle.Menu = BattleMenu.Commands;
        battle.Selection = 0;
        return true;
    }

    private void UseSmokeBombInBattle()
    {
        if (!ConsumeItem(ItemKind.SmokeBomb))
        {
            AddBattleLog("There are no smoke bombs left.");
            return;
        }

        RpgAudio.Instance.PlayItem();
        StartFleeAnimation(true, "The smoke bomb hides the party's escape.");
    }

    private void PerformAttack()
    {
        Hero actor = party[battle.ActorIndex];
        Enemy target = battle.Enemies[battle.TargetEnemyIndex];
        int damage = Mathf.Max(1, actor.Attack + actor.WeaponAttack +
            Random.Range(1, 5) - target.Defense);
        target.Hp = Mathf.Max(0, target.Hp - damage);
        RpgAudio.Instance.PlayAttack();
        BattleEffect attackEffect = battle.ActorIndex == 0 ? BattleEffect.VanguardAttack :
            battle.ActorIndex == 1 ? BattleEffect.ArcanistAttack : BattleEffect.MenderAttack;
        TriggerBattleEffect(attackEffect, -1);
        StartBattleAnimation(BattleAnimation.Attack, battle.ActorIndex, -1);
        enemyHitTimer = 0.18f;
        AddBattleLog(actor.Name + " strikes " + target.Name + " for " + damage + " damage.");
        CheckForBattleVictory();
    }

    private bool PerformSkill(CharacterSkill skill)
    {
        Hero actor = party[battle.ActorIndex];
        int cost = GetSkillCost(skill);
        if (actor.Mp < cost)
        {
            AddBattleLog(actor.Name + " does not have enough MP.");
            battle.Menu = BattleMenu.Commands;
            battle.Selection = 0;
            return false;
        }

        if (skill == CharacterSkill.Cure || skill == CharacterSkill.GreaterCure)
        {
            Hero target = MostInjuredHero();
            if (target == null || target.Hp == target.MaxHp)
            {
                AddBattleLog("Everyone is already at full health.");
                battle.Menu = BattleMenu.Commands;
                battle.Selection = 0;
                return false;
            }

            actor.Mp -= cost;
            int potency = skill == CharacterSkill.GreaterCure ? 32 : 18;
            int restored = Mathf.Min(potency + actor.Magic * 2, target.MaxHp - target.Hp);
            target.Hp += restored;
            RpgAudio.Instance.PlayHealing();
            int targetIndex = party.IndexOf(target);
            TriggerBattleEffect(BattleEffect.Healing, targetIndex);
            StartBattleAnimation(BattleAnimation.Skill, battle.ActorIndex, targetIndex);
            AddBattleLog(actor.Name + " uses " + SkillName(skill) + " to restore " + restored +
                " HP to " + target.Name + ".");
            battle.Menu = BattleMenu.Commands;
            battle.Selection = 0;
            return true;
        }

        actor.Mp -= cost;
        Enemy enemy = battle.Enemies[battle.TargetEnemyIndex];
        int damage;
        BattleEffect effect;
        bool physical = skill == CharacterSkill.PowerStrike;
        switch (skill)
        {
            case CharacterSkill.PowerStrike:
                damage = Mathf.Max(1, (actor.Attack + actor.WeaponAttack) * 2 +
                    Random.Range(0, 4) - enemy.Defense);
                effect = BattleEffect.Slash;
                break;
            case CharacterSkill.FlameArc:
                damage = Mathf.Max(1, actor.Magic * 2 + actor.WeaponMagic + Random.Range(10, 17) -
                    enemy.Defense / 2);
                effect = BattleEffect.Fire;
                break;
            case CharacterSkill.FrostNova:
                damage = Mathf.Max(1, actor.Magic * 2 + actor.WeaponMagic + Random.Range(8, 15) -
                    enemy.Defense / 2);
                effect = BattleEffect.Frost;
                break;
            case CharacterSkill.Starfall:
                damage = Mathf.Max(1, actor.Magic * 3 + actor.WeaponMagic + Random.Range(12, 20) -
                    enemy.Defense / 2);
                effect = BattleEffect.Starfall;
                break;
            default:
                damage = Mathf.Max(1, actor.Magic * 2 + actor.WeaponMagic + Random.Range(6, 11) -
                    enemy.Defense / 2);
                effect = BattleEffect.Magic;
                break;
        }
        if (physical)
        {
            RpgAudio.Instance.PlayAttack();
        }
        else
        {
            RpgAudio.Instance.PlayMagic();
        }

        enemy.Hp = Mathf.Max(0, enemy.Hp - damage);
        enemyHitTimer = 0.18f;
        TriggerBattleEffect(effect, -1);
        StartBattleAnimation(physical ? BattleAnimation.Attack : BattleAnimation.Skill, battle.ActorIndex, -1);
        AddBattleLog(actor.Name + " uses " + SkillName(skill) + " on " + enemy.Name +
            " for " + damage + " damage.");
        battle.Menu = BattleMenu.Commands;
        battle.Selection = 0;
        CheckForBattleVictory();
        return true;
    }

    private static int GetSkillCost(CharacterSkill skill)
    {
        switch (skill)
        {
            case CharacterSkill.PowerStrike:
                return 2;
            case CharacterSkill.Spark:
                return 4;
            case CharacterSkill.FlameArc:
            case CharacterSkill.FrostNova:
                return 5;
            case CharacterSkill.GreaterCure:
                return 6;
            case CharacterSkill.Starfall:
                return 8;
            default:
                return 3;
        }
    }

    private void TryRunAway()
    {
        StartFleeAnimation(Random.value < 0.65f, "The party escaped safely.");
    }

    private void StartFleeAnimation(bool succeeds, string successMessage)
    {
        fleeWillSucceed = succeeds;
        fleeAnimationTimer = FleeAnimationDuration;
        if (succeeds)
        {
            statusMessage = successMessage;
        }

        RpgAudio.Instance.PlayFlee();
    }

    private void CompleteFleeAnimation()
    {
        if (battle == null || mode != GameMode.Battle)
        {
            return;
        }

        if (fleeWillSucceed)
        {
            battle = null;
            mode = GameMode.Explore;
            nextMoveTime = Time.unscaledTime + 0.15f;
            RpgAudio.Instance.PlayOverworldMusic();
            return;
        }

        AddBattleLog("The escape route is blocked.");
        FinishBattleAction();
    }

    private void FinishBattleAction()
    {
        if (battle == null || mode != GameMode.Battle)
        {
            return;
        }

        battle.ActionsRemaining--;
        if (battle.ActionsRemaining <= 0)
        {
            pendingEnemyTurn = true;
            return;
        }

        battle.ActorIndex = FindLivingHero((battle.ActorIndex + 1) % party.Count);
        battle.Menu = BattleMenu.Commands;
        battle.Selection = 0;
    }

    private void PerformEnemyTurn()
    {
        for (int enemyIndex = 0; enemyIndex < battle.Enemies.Count; enemyIndex++)
        {
            Enemy enemy = battle.Enemies[enemyIndex];
            if (enemy.Hp <= 0)
            {
                continue;
            }

            int attackCount = enemy.IsBoss ? 2 : 1;
            for (int attackIndex = 0; attackIndex < attackCount; attackIndex++)
            {
                if (CountLivingHeroes() == 0)
                {
                    pendingGameOver = true;
                    battleAnimationTimer = Mathf.Max(battleAnimationTimer, BattleActionDuration);
                    statusMessage = "The party has fallen.";
                    return;
                }

                int targetIndex = FindLivingHero(Random.Range(0, party.Count));
                Hero target = party[targetIndex];
                int damage = Mathf.Max(1, enemy.Attack + Random.Range(-1, 4) -
                    (target.Defense + target.ArmorDefense) / 2);
                target.Hp = Mathf.Max(0, target.Hp - damage);
                RpgAudio.Instance.PlayAttack();
                RpgAudio.Instance.PlayHit();
                TriggerHeroHit(targetIndex);
                battleAnimationEnemyIndex = enemyIndex;
                AddBattleLog(enemy.Name + " hits " + target.Name + " for " + damage + " damage.");
                if (CountLivingHeroes() == 0)
                {
                    pendingGameOver = true;
                    statusMessage = "The party has fallen.";
                    return;
                }
            }
        }

        battle.ActionsRemaining = CountLivingHeroes();
        battle.ActorIndex = FindLivingHero(0);
        battle.Menu = BattleMenu.Commands;
        battle.Selection = 0;
    }

    private int FindLivingHero(int startIndex)
    {
        for (int offset = 0; offset < party.Count; offset++)
        {
            int index = (startIndex + offset) % party.Count;
            if (!party[index].IsCaptured && party[index].Hp > 0)
            {
                return index;
            }
        }

        return -1;
    }

    private int CountLivingHeroes()
    {
        int count = 0;
        for (int i = 0; i < party.Count; i++)
        {
            if (!party[i].IsCaptured && party[i].Hp > 0)
            {
                count++;
            }
        }
        return count;
    }

    private Hero MostInjuredHero()
    {
        Hero mostInjured = null;
        int greatestMissingHp = 0;
        for (int i = 0; i < party.Count; i++)
        {
            Hero candidate = party[i];
            int missingHp = candidate.MaxHp - candidate.Hp;
            if (!candidate.IsCaptured && candidate.Hp > 0 && missingHp > greatestMissingHp)
            {
                mostInjured = candidate;
                greatestMissingHp = missingHp;
            }
        }
        return mostInjured;
    }

    private void StartWildBattle()
    {
        int enemyCount = Random.value < 0.38f ? 2 : 1;
        List<Enemy> encounter = new List<Enemy>(enemyCount);
        int firstRosterIndex = Random.Range(0, 4);
        for (int i = 0; i < enemyCount; i++)
        {
            int rosterIndex = i == 0
                ? firstRosterIndex
                : (firstRosterIndex + Random.Range(1, 4)) % 4;
            EnemyKind kind = (EnemyKind)(currentRegion * 4 + rosterIndex);
            encounter.Add(CreateRegionEnemy(kind));
        }
        StartBattle(encounter, false);
    }

    private void StartBattle(Enemy enemy, bool isBoss)
    {
        StartBattle(new List<Enemy> { enemy }, isBoss);
    }

    private void StartBattle(List<Enemy> enemies, bool isBoss)
    {
        pendingBattleVictory = false;
        pendingEnemyTurn = false;
        pendingGameOver = false;
        fleeAnimationTimer = 0f;
        battleAnimationTimer = 0f;
        battle = new BattleState
        {
            IsBoss = isBoss,
            ActorIndex = FindLivingHero(0),
            ActionsRemaining = CountLivingHeroes(),
            Menu = BattleMenu.Commands
        };
        battle.Enemies.AddRange(enemies);
        battle.Log.Add(isBoss
            ? RegionBossName(currentRegion) + " blocks the way."
            : BuildEncounterMessage(enemies));
        mode = GameMode.Battle;
        RpgAudio.Instance.PlayBattleMusic();
        RpgAudio.Instance.PlayEncounter();
    }

    private void CheckForBattleVictory()
    {
        if (battle != null && CountLivingEnemies() == 0)
        {
            pendingBattleVictory = true;
            battleAnimationTimer = Mathf.Max(battleAnimationTimer, BattleActionDuration);
        }
    }

    private void EndBattle(bool won)
    {
        if (!won || battle == null)
        {
            return;
        }

        pendingBattleVictory = false;
        pendingEnemyTurn = false;
        pendingGameOver = false;
        int rewardExperience = 0;
        int rewardGil = 0;
        for (int i = 0; i < battle.Enemies.Count; i++)
        {
            rewardExperience += battle.Enemies[i].Experience;
            rewardGil += battle.Enemies[i].Gil;
        }
        bool wasBoss = battle.IsBoss;
        gil += rewardGil;
        levelUpReport = string.Empty;
        learnedSkillsReport = string.Empty;
        for (int i = 0; i < party.Count; i++)
        {
            int previousLevel = party[i].Level;
            GrantExperience(party[i], rewardExperience);
            if (party[i].Level > previousLevel)
            {
                if (levelUpReport.Length > 0)
                {
                    levelUpReport += ", ";
                }
                levelUpReport += party[i].Name + " Lv " + party[i].Level;
            }
        }

        if (wasBoss)
        {
            bossDefeated = true;
            defeatedBosses[currentRegion] = true;
            ItemKind bossReward = BossReward(currentRegion);
            AddItem(bossReward, 1);
            battle = null;
            mode = GameMode.Explore;
            statusMessage = RegionBossName(currentRegion) + " is defeated. " +
                ItemName(bossReward) + " was added to the relics." +
                (levelUpReport.Length > 0 ? " Level up: " + levelUpReport + "." : string.Empty) +
                (learnedSkillsReport.Length > 0 ? " New skills: " + learnedSkillsReport + "." : string.Empty);
            RpgAudio.Instance.PlayVictory();
            RpgAudio.Instance.PlayOverworldMusic();
            BeginRegionBossStory();
            return;
        }

        regionalBattleWins[currentRegion]++;
        regionEncountersWon = regionalBattleWins[currentRegion];
        battle = null;
        mode = GameMode.Explore;
        statusMessage = "Victory! The party earned " + rewardExperience + " EXP and " +
            rewardGil + " gil." +
            (levelUpReport.Length > 0 ? " Level up: " + levelUpReport + "." : string.Empty) +
            (learnedSkillsReport.Length > 0 ? " New skills: " + learnedSkillsReport + "." : string.Empty);
        RpgAudio.Instance.PlayVictory();
        RpgAudio.Instance.PlayOverworldMusic();
    }

    private void GrantExperience(Hero hero, int experience)
    {
        if (hero.Level >= 5 || experience <= 0)
        {
            return;
        }

        hero.Experience += experience;
        while (hero.Level < 5 && hero.Experience >= hero.ExperienceToNextLevel)
        {
            hero.Experience -= hero.ExperienceToNextLevel;
            hero.Level++;
            hero.MaxHp += Random.Range(4, 8);
            hero.MaxMp += hero.Role == "Vanguard" ? 1 : 2;
            hero.Attack++;
            hero.Defense++;
            hero.Magic++;
            hero.Hp = hero.MaxHp;
            hero.Mp = hero.MaxMp;
            LearnLevelSkills(hero);
        }
        if (hero.Level >= 5)
        {
            hero.Experience = 0;
        }
    }

    private void AddBattleLog(string line)
    {
        if (battle == null)
        {
            return;
        }

        battle.Log.Add(line);
        while (battle.Log.Count > 5)
        {
            battle.Log.RemoveAt(0);
        }
    }

    private void TriggerBattleEffect(BattleEffect effect, int heroIndex)
    {
        battleEffect = effect;
        battleEffectHeroIndex = heroIndex;
        battleEffectTimer = 0.38f;
        if (heroIndex < 0)
        {
            enemyHitTimer = 0.18f;
        }
    }

    private void TriggerHeroHit(int heroIndex)
    {
        heroHitIndex = heroIndex;
        heroHitTimer = 0.2f;
        StartBattleAnimation(BattleAnimation.Damage, -1, heroIndex);
    }

    private void StartBattleAnimation(BattleAnimation animation, int heroIndex, int targetIndex)
    {
        battleAnimation = animation;
        battleAnimationHeroIndex = heroIndex;
        battleAnimationTargetIndex = targetIndex;
        battleAnimationEnemyIndex = -1;
        battleAnimationTimer = BattleActionDuration;
    }

    private static bool ConfirmPressed()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }
}
