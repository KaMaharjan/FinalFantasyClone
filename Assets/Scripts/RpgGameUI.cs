using UnityEngine;

public partial class RpgGame
{
    private const float DesignWidth = 960f;
    private const float DesignHeight = 640f;
    private const float TileSize = 29f;
    private const float MapOriginX = 24f;
    private const float MapOriginY = 98f;

    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle buttonStyle;
    private GUIStyle victoryStyle;
    private GUIStyle gameOverStyle;
    private GUIStyle battleDialogueStyle;
    private Texture2D[] regionBattleBackgroundTextures;
    private Texture2D[] partySpriteTextures;
    private Texture2D[] enemySpriteTextures;
    private Texture2D[] partyFaceTextures;
    private Texture2D[] villagerSpriteTextures;
    private Texture2D[] villagerPortraitTextures;
    private Texture2D dialoguePanelTexture;
    private Texture2D nameplateTexture;
    private Texture2D commandPanelTexture;
    private Texture2D statusPanelTexture;
    private Texture2D battleButtonTexture;
    private Texture2D selectedBattleButtonTexture;
    private Texture2D gaugeFrameTexture;
    private Texture2D hpGaugeTexture;
    private Texture2D mpGaugeTexture;
    private Texture2D enemyGaugeTexture;
    private Texture2D slashParticleTexture;
    private Texture2D[] partyAttackParticleTextures;
    private Texture2D magicParticleTexture;
    private Texture2D healingParticleTexture;
    private Texture2D fireParticleTexture;
    private Texture2D frostParticleTexture;
    private Texture2D starfallParticleTexture;
    private Texture2D fastForwardIconTexture;
    private Texture2D[] overworldMapTextures;
    private Texture2D questMarkerTexture;
    private Texture2D closedChestSpriteTexture;
    private Texture2D openedChestSpriteTexture;
    private Texture2D headerPanelTexture;
    private Texture2D worldFrameTexture;
    private Texture2D sidebarPanelTexture;
    private Texture2D partyPanelTexture;
    private Texture2D footerPanelTexture;
    private Texture2D menuModalTexture;
    private Texture2D dialogueModalTexture;
    private Texture2D endingModalTexture;
    private Texture2D[] menuButtonTextures;
    private Texture2D[] selectedMenuButtonTextures;

    private static readonly Vector2[] BattleHeroAnchors =
    {
        new Vector2(700f, 372f),
        new Vector2(822f, 314f),
        new Vector2(765f, 405f)
    };

    private void OnGUI()
    {
        EnsureStyles();
        EnsurePixelArt();
        float scale = Mathf.Min(Screen.width / DesignWidth, Screen.height / DesignHeight);
        if (scale <= 0f)
        {
            return;
        }

        Matrix4x4 previousMatrix = GUI.matrix;
        float offsetX = (Screen.width - DesignWidth * scale) * 0.5f;
        float offsetY = (Screen.height - DesignHeight * scale) * 0.5f;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, Vector3.one * scale);

        if (mode != GameMode.Battle)
        {
            FillRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0.035f, 0.045f, 0.075f));
        }

        if (mode == GameMode.Battle)
        {
            DrawBattleScreen();
        }
        else
        {
            DrawHeader("ECHOES OF AETHER", "Four regions. One stolen dawn.");
            DrawWorld();
            DrawSidebar();
            DrawFooter();

            if (mode == GameMode.Menu)
            {
                DrawMenu();
            }
            else if (mode == GameMode.Dialogue)
            {
                DrawDialogue();
            }
            else if (mode == GameMode.GameOver)
            {
                DrawEnding(false);
            }
            else if (mode == GameMode.Victory)
            {
                DrawEnding(true);
            }
        }

        if (isSceneTransitioning)
        {
            float alpha = Mathf.Clamp01(transitionTimer / RegionTransitionDuration);
            FillRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0f, 0f, 0f, alpha));
        }

        GUI.matrix = previousMatrix;
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = MakeLabelStyle(28, new Color(0.96f, 0.87f, 0.61f), FontStyle.Bold, TextAnchor.MiddleLeft);
        subtitleStyle = MakeLabelStyle(13, new Color(0.69f, 0.75f, 0.85f), FontStyle.Normal, TextAnchor.MiddleLeft);
        headingStyle = MakeLabelStyle(17, new Color(0.94f, 0.82f, 0.55f), FontStyle.Bold, TextAnchor.MiddleLeft);
        bodyStyle = MakeLabelStyle(14, new Color(0.9f, 0.92f, 0.96f), FontStyle.Normal, TextAnchor.MiddleLeft);
        smallStyle = MakeLabelStyle(11, new Color(0.72f, 0.78f, 0.87f), FontStyle.Normal, TextAnchor.MiddleLeft);
        victoryStyle = MakeLabelStyle(31, new Color(0.98f, 0.86f, 0.52f), FontStyle.Bold, TextAnchor.MiddleCenter);
        gameOverStyle = MakeLabelStyle(31, new Color(0.89f, 0.56f, 0.54f), FontStyle.Bold, TextAnchor.MiddleCenter);
        battleDialogueStyle = MakeLabelStyle(15, new Color(0.96f, 0.97f, 1f), FontStyle.Normal,
            TextAnchor.MiddleCenter);
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            padding = new RectOffset(7, 7, 5, 5)
        };
        buttonStyle.normal.textColor = new Color(0.95f, 0.95f, 0.98f);
        buttonStyle.hover.textColor = Color.white;
    }

    private void EnsurePixelArt()
    {
        if (regionBattleBackgroundTextures != null)
        {
            return;
        }

        regionBattleBackgroundTextures = new Texture2D[4];
        overworldMapTextures = new Texture2D[4];
        for (int i = 0; i < regionBattleBackgroundTextures.Length; i++)
        {
            regionBattleBackgroundTextures[i] = RpgPixelArt.CreateBattleBackground(i);
            overworldMapTextures[i] = RpgPixelArt.CreateOverworldMap(
                CreateRegionTerrain(i), MapWidth, MapHeight, (int)TileSize);
        }
        partySpriteTextures = new[]
        {
            RpgPixelArt.CreateVanguardSprite(),
            RpgPixelArt.CreateArcanistSprite(),
            RpgPixelArt.CreateMenderSprite()
        };
        villagerSpriteTextures = new[]
        {
            RpgPixelArt.CreateVillagerSprite(0),
            RpgPixelArt.CreateVillagerSprite(1),
            RpgPixelArt.CreateVillagerSprite(2),
            RpgPixelArt.CreateVillagerSprite(3)
        };
        villagerPortraitTextures = new[]
        {
            RpgPixelArt.CreateVillagerPortrait(0),
            RpgPixelArt.CreateVillagerPortrait(1),
            RpgPixelArt.CreateVillagerPortrait(2),
            RpgPixelArt.CreateVillagerPortrait(3)
        };
        partyFaceTextures = new[]
        {
            RpgPixelArt.CreatePartyPortrait(0),
            RpgPixelArt.CreatePartyPortrait(1),
            RpgPixelArt.CreatePartyPortrait(2),
            RpgPixelArt.CreatePartyPortrait(3)
        };
        questMarkerTexture = RpgPixelArt.CreateQuestMarker();
        closedChestSpriteTexture = RpgPixelArt.CreateChestSprite(false);
        openedChestSpriteTexture = RpgPixelArt.CreateChestSprite(true);
        enemySpriteTextures = new Texture2D[20];
        for (int i = 0; i < enemySpriteTextures.Length; i++)
        {
            enemySpriteTextures[i] = RpgPixelArt.CreateEnemySprite(i, i >= 16);
        }
        headerPanelTexture = RpgPixelArt.CreatePanelFrame(240, 20);
        worldFrameTexture = RpgPixelArt.CreatePanelFrame(148, 105);
        sidebarPanelTexture = RpgPixelArt.CreatePanelFrame(80, 105);
        partyPanelTexture = RpgPixelArt.CreatePanelFrame(147, 36);
        footerPanelTexture = RpgPixelArt.CreatePanelFrame(231, 25);
        menuModalTexture = RpgPixelArt.CreatePanelFrame(121, 104);
        dialogueModalTexture = RpgPixelArt.CreatePanelFrame(204, 44);
        endingModalTexture = RpgPixelArt.CreatePanelFrame(139, 75);
        menuButtonTextures = new[]
        {
            RpgPixelArt.CreateButtonFrame(202, 21, false),
            RpgPixelArt.CreateButtonFrame(195, 27, false),
            RpgPixelArt.CreateButtonFrame(115, 22, false),
            RpgPixelArt.CreateButtonFrame(75, 18, false)
        };
        selectedMenuButtonTextures = new[]
        {
            RpgPixelArt.CreateButtonFrame(202, 21, true),
            RpgPixelArt.CreateButtonFrame(195, 27, true),
            RpgPixelArt.CreateButtonFrame(115, 22, true),
            RpgPixelArt.CreateButtonFrame(75, 18, true)
        };
        dialoguePanelTexture = RpgPixelArt.CreateDialogueFrame(408, 34);
        nameplateTexture = RpgPixelArt.CreatePanelFrame(68, 12);
        commandPanelTexture = RpgPixelArt.CreatePanelFrame(188, 94);
        statusPanelTexture = RpgPixelArt.CreatePanelFrame(266, 94);
        battleButtonTexture = RpgPixelArt.CreateButtonFrame(92, 22, false);
        selectedBattleButtonTexture = RpgPixelArt.CreateButtonFrame(92, 22, true);
        gaugeFrameTexture = RpgPixelArt.CreateGaugeFrame(132, 8);
        hpGaugeTexture = RpgPixelArt.CreateGaugeFill(128, 4,
            new Color32(31, 149, 79, 255), new Color32(153, 231, 117, 255));
        mpGaugeTexture = RpgPixelArt.CreateGaugeFill(128, 4,
            new Color32(34, 102, 189, 255), new Color32(117, 201, 244, 255));
        enemyGaugeTexture = RpgPixelArt.CreateGaugeFill(128, 4,
            new Color32(180, 44, 52, 255), new Color32(245, 150, 87, 255));
        slashParticleTexture = RpgPixelArt.CreateEffectParticle(new Color32(255, 202, 107, 255));
        partyAttackParticleTextures = new[]
        {
            RpgPixelArt.CreatePartyAttackEffect(0),
            RpgPixelArt.CreatePartyAttackEffect(1),
            RpgPixelArt.CreatePartyAttackEffect(2)
        };
        magicParticleTexture = RpgPixelArt.CreateEffectParticle(new Color32(97, 184, 248, 255));
        healingParticleTexture = RpgPixelArt.CreateEffectParticle(new Color32(107, 225, 154, 255));
        fireParticleTexture = RpgPixelArt.CreateEffectParticle(new Color32(245, 100, 45, 255));
        frostParticleTexture = RpgPixelArt.CreateEffectParticle(new Color32(113, 209, 243, 255));
        starfallParticleTexture = RpgPixelArt.CreateEffectParticle(new Color32(240, 216, 117, 255));
        fastForwardIconTexture = RpgPixelArt.CreateFastForwardIcon();
    }

    private void OnDestroy()
    {
        DestroyPixelTextures(regionBattleBackgroundTextures);
        DestroyPixelTextures(overworldMapTextures);
        DestroyPixelTextures(villagerSpriteTextures);
        DestroyPixelTextures(villagerPortraitTextures);
        DestroyPixelTextures(partyFaceTextures);
        DestroyPixelTexture(questMarkerTexture);
        DestroyPixelTexture(closedChestSpriteTexture);
        DestroyPixelTexture(openedChestSpriteTexture);
        DestroyPixelTexture(headerPanelTexture);
        DestroyPixelTexture(worldFrameTexture);
        DestroyPixelTexture(sidebarPanelTexture);
        DestroyPixelTexture(partyPanelTexture);
        DestroyPixelTexture(footerPanelTexture);
        DestroyPixelTexture(menuModalTexture);
        DestroyPixelTexture(dialogueModalTexture);
        DestroyPixelTexture(endingModalTexture);
        DestroyPixelTextures(enemySpriteTextures);
        DestroyPixelTexture(dialoguePanelTexture);
        DestroyPixelTexture(nameplateTexture);
        DestroyPixelTexture(commandPanelTexture);
        DestroyPixelTexture(statusPanelTexture);
        DestroyPixelTexture(battleButtonTexture);
        DestroyPixelTexture(selectedBattleButtonTexture);
        DestroyPixelTexture(gaugeFrameTexture);
        DestroyPixelTexture(hpGaugeTexture);
        DestroyPixelTexture(mpGaugeTexture);
        DestroyPixelTexture(enemyGaugeTexture);
        DestroyPixelTexture(slashParticleTexture);
        DestroyPixelTextures(partyAttackParticleTextures);
        DestroyPixelTexture(magicParticleTexture);
        DestroyPixelTexture(healingParticleTexture);
        DestroyPixelTexture(fireParticleTexture);
        DestroyPixelTexture(frostParticleTexture);
        DestroyPixelTexture(starfallParticleTexture);
        DestroyPixelTexture(fastForwardIconTexture);
        DestroyPixelTextures(partySpriteTextures);
        DestroyPixelTextures(menuButtonTextures);
        DestroyPixelTextures(selectedMenuButtonTextures);
    }

    private static void DestroyPixelTexture(Texture2D texture)
    {
        if (texture != null)
        {
            Destroy(texture);
        }
    }

    private static void DestroyPixelTextures(Texture2D[] textures)
    {
        if (textures == null)
        {
            return;
        }

        for (int i = 0; i < textures.Length; i++)
        {
            DestroyPixelTexture(textures[i]);
        }
    }

    private static GUIStyle MakeLabelStyle(int size, Color color, FontStyle weight, TextAnchor alignment)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            fontStyle = weight,
            alignment = alignment,
            wordWrap = true
        };
        style.normal.textColor = color;
        return style;
    }

    private void DrawHeader(string title, string subtitle)
    {
        GUI.DrawTexture(new Rect(0f, 0f, DesignWidth, 78f), headerPanelTexture,
            ScaleMode.StretchToFill, true);
        GUI.Label(new Rect(24f, 8f, 470f, 38f), title, titleStyle);
        GUI.Label(new Rect(28f, 46f, 520f, 22f), subtitle, subtitleStyle);
        GUI.Label(new Rect(700f, 23f, 236f, 28f), RegionName(currentRegion).ToUpperInvariant(), smallStyle);
    }

    private void DrawWorld()
    {
        DrawPixelPanel(new Rect(19f, 93f, 590f, 418f), worldFrameTexture);
        GUI.DrawTexture(new Rect(MapOriginX, MapOriginY, MapWidth * TileSize, MapHeight * TileSize),
            overworldMapTextures[currentRegion], ScaleMode.StretchToFill, false);

        DrawChest();
        float gateX = MapOriginX + (GetRegionGatePosition(currentRegion).x + 0.5f) * TileSize;
        float gateY = MapOriginY + (GetRegionGatePosition(currentRegion).y + 0.5f) * TileSize;
        GUI.DrawTexture(new Rect(gateX - 8f, gateY - 29f, 16f, 20f),
            questMarkerTexture, ScaleMode.StretchToFill, true);
        for (int i = 0; i < npcs.Count; i++)
        {
            DrawVillagerSprite(npcs[i]);
        }
        float playerCenterX = MapOriginX + (playerVisualPosition.x + 0.5f) * TileSize;
        float playerBaseline = MapOriginY + (playerVisualPosition.y + 1f) * TileSize;
        DrawWorldHeroSprite(0, playerCenterX, playerBaseline, playerIsWalking);
    }

    private void DrawWorldHeroSprite(int heroIndex, float centerX, float baseline, bool walking)
    {
        if (heroIndex < 0 || heroIndex >= partySpriteTextures.Length)
        {
            return;
        }

        float bob = walking
            ? Mathf.Abs(Mathf.Sin(Time.unscaledTime * 18f)) * 1.5f
            : Mathf.Sin(Time.unscaledTime * 2.5f + heroIndex) * 0.6f;
        GUI.DrawTexture(new Rect(centerX - 15f, baseline - 40f - bob, 30f, 40f),
            partySpriteTextures[heroIndex], ScaleMode.StretchToFill, true);
    }

    private void DrawVillagerSprite(Npc npc)
    {
        float centerX = MapOriginX + (npc.VisualPosition.x + 0.5f) * TileSize;
        float baseline = MapOriginY + (npc.VisualPosition.y + 1f) * TileSize;
        float bob = Mathf.Sin(Time.unscaledTime * 2.5f + npc.Position.x) * 0.6f;
        int spriteIndex = npc.Kind == NpcKind.Healer ? 1 :
            npc.Kind == NpcKind.Merchant ? 2 : npc.Kind == NpcKind.Guide ? 3 : 0;
        Texture2D sprite = villagerSpriteTextures[spriteIndex];
        GUI.DrawTexture(new Rect(centerX - 14f, baseline - 38f - bob, 28f, 38f),
            sprite, ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(centerX - 6f, baseline - 53f - bob, 12f, 16f),
            questMarkerTexture, ScaleMode.StretchToFill, true);
    }

    private void DrawChest()
    {
        Rect tile = new Rect(MapOriginX + chestPosition.x * TileSize, MapOriginY + chestPosition.y * TileSize, TileSize, TileSize);
        GUI.DrawTexture(new Rect(tile.x - 1f, tile.y - 1f, TileSize + 2f, TileSize + 2f),
            chestOpened ? openedChestSpriteTexture : closedChestSpriteTexture,
            ScaleMode.StretchToFill, true);
    }

    private void DrawSidebar()
    {
        Rect panel = new Rect(624f, 93f, 317f, 418f);
        DrawPixelPanel(panel, sidebarPanelTexture);
        GUI.Label(new Rect(641f, 102f, 280f, 24f), "TRAVELING PARTY", headingStyle);

        for (int i = 0; i < party.Count; i++)
        {
            Hero hero = party[i];
            float y = 132f + i * 79f;
            DrawPixelPanel(new Rect(636f, y, 293f, 72f), partyPanelTexture);
            GUI.DrawTexture(new Rect(644f, y + 8f, 40f, 52f), partyFaceTextures[i], ScaleMode.StretchToFill, true);
            string state = hero.IsCaptured ? "  CAPTURED" : hero.Hp <= 0 ? "  DOWN" : string.Empty;
            GUI.Label(new Rect(690f, y + 4f, 150f, 21f), hero.Name + "  Lv " + hero.Level + state, bodyStyle);
            GUI.Label(new Rect(843f, y + 4f, 75f, 21f), hero.Role, smallStyle);
            GUI.Label(new Rect(690f, y + 28f, 28f, 17f), "HP", smallStyle);
            DrawBattleGauge(new Rect(716f, y + 31f, 119f, 11f),
                hero.Hp / (float)hero.MaxHp, hpGaugeTexture);
            GUI.Label(new Rect(839f, y + 26f, 82f, 20f), hero.Hp + "/" + hero.MaxHp, smallStyle);
            GUI.Label(new Rect(690f, y + 49f, 28f, 16f), "MP", smallStyle);
            DrawBattleGauge(new Rect(716f, y + 52f, 119f, 8f),
                hero.MaxMp == 0 ? 0f : hero.Mp / (float)hero.MaxMp, mpGaugeTexture);
            GUI.Label(new Rect(839f, y + 46f, 82f, 20f), hero.Mp + "/" + hero.MaxMp, smallStyle);
        }

        GUI.Label(new Rect(641f, 376f, 275f, 19f),
            "POTION " + GetItemCount(ItemKind.Potion) + "   ETHER " + GetItemCount(ItemKind.Ether), smallStyle);
        GUI.Label(new Rect(641f, 394f, 275f, 19f),
            "FEATHER " + GetItemCount(ItemKind.PhoenixFeather) + "   SMOKE " +
            GetItemCount(ItemKind.SmokeBomb) + "   GIL " + gil, smallStyle);
        string objective = bossDefeated
            ? "Guardian defeated. Use the marked gate to continue."
            : "Regional battles: " + regionEncountersWon + "/2\n" + RegionObjective(currentRegion);
        GUI.Label(new Rect(641f, 416f, 283f, 43f), objective, smallStyle);
        GUI.Label(new Rect(641f, 464f, 283f, 32f),
            "Sidequest cache: " + (sideQuestCompleted[currentRegion] ? "complete" : "nearby") +
            "    Menu: M / ESC", smallStyle);
    }

    private void DrawFooter()
    {
        DrawPixelPanel(new Rect(19f, 518f, 922f, 101f), footerPanelTexture);
        GUI.Label(new Rect(35f, 528f, 890f, 25f), statusMessage, bodyStyle);
        GUI.Label(new Rect(35f, 562f, 890f, 27f),
            "MOVE  WASD / ARROWS   TALK / ACT  SPACE / ENTER   MENU / TRAVEL  M",
            smallStyle);
    }

    private void DrawMenu()
    {
        Rect modal = new Rect(238f, 112f, 484f, 414f);
        DrawModal(modal);
        GUI.Label(new Rect(264f, 132f, 430f, 32f),
            menuPage == MenuPage.Shop ? RegionShopName(currentRegion).ToUpperInvariant() : "TRAVELER'S JOURNAL",
            headingStyle);

        if (menuPage == MenuPage.Root)
        {
            string travelLabel = HasDiscoveredAnotherRegion() ? "Quick travel" : "Quick travel (discover a region first)";
            string[] labels = { "Inventory", "Party profiles", "Equipment", travelLabel, "Return to the road" };
            for (int i = 0; i < labels.Length; i++)
            {
                int selection = i;
                DrawTextButton(new Rect(285f, 184f + i * 52f, 390f, 43f), labels[i],
                    menuSelection == i, delegate { OpenMenuSelection(selection); });
            }
            GUI.Label(new Rect(270f, 453f, 420f, 30f), "UP / DOWN selects    ENTER confirms    ESC closes", smallStyle);
        }
        else if (menuPage == MenuPage.Items)
        {
            if (!choosingItemTarget)
            {
                string selectedDescription = menuSelection >= 0 && menuSelection < ItemCatalog.Length
                    ? ItemCatalog[menuSelection].Description
                    : string.Empty;
                GUI.Label(new Rect(270f, 168f, 420f, 25f),
                    "GIL " + gil + "   " + selectedDescription, bodyStyle);
                for (int i = 0; i < ItemCatalog.Length; i++)
                {
                    int itemIndex = i;
                    ItemDefinition item = ItemCatalog[i];
                    string label = item.Name + " x" + GetItemCount(item.Kind) +
                        (item.CanUseOutsideBattle ? string.Empty : "  |  Relic");
                    DrawTextButton(new Rect(278f, 196f + i * 31f, 404f, 28f), label, menuSelection == i,
                        delegate { SelectInventoryItem(itemIndex); });
                }
                DrawTextButton(new Rect(365f, 453f, 230f, 36f), "Back", false,
                    delegate { menuPage = MenuPage.Root; });
                GUI.Label(new Rect(270f, 489f, 420f, 22f), "UP / DOWN selects an item", smallStyle);
            }
            else
            {
                ItemDefinition item = ItemCatalog[menuSelection];
                GUI.Label(new Rect(270f, 177f, 420f, 25f), item.Name + ": choose a party member.", bodyStyle);
                for (int i = 0; i < party.Count; i++)
                {
                    int heroIndex = i;
                    Hero hero = party[i];
                    string label = hero.Name + "     HP " + hero.Hp + "/" + hero.MaxHp +
                                   "     MP " + hero.Mp + "/" + hero.MaxMp +
                                   (hero.IsCaptured ? "  CAPTURED" : string.Empty);
                    DrawTextButton(new Rect(278f, 218f + i * 58f, 404f, 46f), label, menuTargetIndex == i,
                        delegate { UseInventoryItemOnHero(heroIndex); });
                }
                DrawTextButton(new Rect(365f, 411f, 230f, 43f), "Back to items", false,
                    delegate { choosingItemTarget = false; });
                GUI.Label(new Rect(270f, 464f, 420f, 28f), "UP / DOWN chooses a target", smallStyle);
            }
        }
        else if (menuPage == MenuPage.Status)
        {
            for (int i = 0; i < party.Count; i++)
            {
                Hero hero = party[i];
                int y = 180 + i * 96;
                GUI.DrawTexture(new Rect(272f, y, 70f, 82f), partyFaceTextures[i], ScaleMode.StretchToFill, true);
                GUI.Label(new Rect(354f, y, 330f, 23f),
                    hero.Name + " the " + hero.Role + "  |  Level " + hero.Level +
                    (hero.IsCaptured ? "  CAPTURED" : string.Empty), bodyStyle);
                GUI.Label(new Rect(354f, y + 24f, 330f, 20f),
                    "HP " + hero.Hp + "/" + hero.MaxHp + "   MP " + hero.Mp + "/" + hero.MaxMp, smallStyle);
                GUI.Label(new Rect(354f, y + 45f, 330f, 19f),
                    "ATK " + (hero.Attack + hero.WeaponAttack) + "   DEF " +
                    (hero.Defense + hero.ArmorDefense) + "   MAG " + (hero.Magic + hero.WeaponMagic), smallStyle);
                GUI.Label(new Rect(354f, y + 65f, 330f, 19f),
                    "Skills: " + LearnedSkillNames(hero) +
                    (hero.Level >= 5 ? "  |  EXP MAX" : string.Empty), smallStyle);
            }
            DrawTextButton(new Rect(365f, 432f, 230f, 43f), "Back", false, delegate { menuPage = MenuPage.Root; });
        }
        else if (menuPage == MenuPage.Equipment)
        {
            Hero hero = party[equipmentHeroIndex];
            GUI.DrawTexture(new Rect(275f, 186f, 104f, 122f),
                partyFaceTextures[equipmentHeroIndex], ScaleMode.StretchToFill, true);
            GUI.Label(new Rect(394f, 186f, 284f, 26f), hero.Name + "  |  " + hero.Role, bodyStyle);
            GUI.Label(new Rect(394f, 218f, 284f, 24f),
                "ATK " + (hero.Attack + hero.WeaponAttack) + "   DEF " +
                (hero.Defense + hero.ArmorDefense) + "   MAG " + (hero.Magic + hero.WeaponMagic), smallStyle);
            GUI.Label(new Rect(394f, 253f, 284f, 25f), "WEAPON", smallStyle);
            DrawTextButton(new Rect(394f, 276f, 284f, 38f), hero.WeaponName,
                equipmentWeaponSlot, delegate { equipmentWeaponSlot = true; CycleEquipment(equipmentHeroIndex, true, 1); });
            GUI.Label(new Rect(394f, 323f, 284f, 25f), "ARMOR", smallStyle);
            DrawTextButton(new Rect(394f, 346f, 284f, 38f), hero.ArmorName,
                !equipmentWeaponSlot, delegate { equipmentWeaponSlot = false; CycleEquipment(equipmentHeroIndex, false, 1); });
            GUI.Label(new Rect(274f, 399f, 410f, 28f),
                "UP / DOWN: character   LEFT / RIGHT: slot   ENTER: cycle owned gear", smallStyle);
            DrawTextButton(new Rect(365f, 443f, 230f, 38f), "Back", false,
                delegate { menuPage = MenuPage.Root; });
        }
        else if (menuPage == MenuPage.Shop)
        {
            GUI.Label(new Rect(270f, 164f, 420f, 23f),
                "GIL " + gil + "    Buyer: " + party[shopHeroIndex].Name + " (LEFT / RIGHT)", bodyStyle);
            int first = Mathf.Clamp(shopSelection - 3, 0, Mathf.Max(0, ShopOffers.Length - 7));
            for (int row = 0; row < ShopOffers.Length && row < 7; row++)
            {
                int offerIndex = first + row;
                ShopOffer offer = ShopOffers[offerIndex];
                DrawTextButton(new Rect(278f, 192f + row * 36f, 404f, 33f),
                    offer.Name + "  " + offer.Price + "g",
                    shopSelection == offerIndex, delegate { BuyShopOffer(offerIndex, shopHeroIndex); });
            }
            GUI.Label(new Rect(270f, 445f, 420f, 18f),
                ShopOffers[shopSelection].Description, smallStyle);
            GUI.Label(new Rect(270f, 463f, 420f, 18f),
                "UP / DOWN: goods   LEFT / RIGHT: buyer   ENTER: buy and equip", smallStyle);
            DrawTextButton(new Rect(365f, 486f, 230f, 34f), "Leave shop", false,
                delegate { menuPage = MenuPage.Root; });
        }
        else if (menuPage == MenuPage.Travel)
        {
            GUI.Label(new Rect(270f, 168f, 420f, 24f),
                "Destinations become available when first discovered.", bodyStyle);
            for (int i = 0; i < discoveredRegions.Length; i++)
            {
                if (!discoveredRegions[i])
                {
                    continue;
                }
                int region = i;
                string label = RegionName(region) + (region == currentRegion ? "  (HERE)" : string.Empty);
                DrawTextButton(new Rect(278f, 204f + i * 55f, 404f, 44f),
                    label, travelSelection == region, delegate { TravelToRegion(region); });
            }
            DrawTextButton(new Rect(365f, 453f, 230f, 36f), "Back", false,
                delegate { menuPage = MenuPage.Root; });
        }
    }

    private void DrawDialogue()
    {
        Rect modal = new Rect(73f, 414f, 814f, 173f);
        DrawModal(modal);
        Texture2D speakerPortrait = activeSpeakerFace < partyFaceTextures.Length
            ? partyFaceTextures[activeSpeakerFace]
            : villagerPortraitTextures[activeSpeakerFace - partyFaceTextures.Length];
        GUI.DrawTexture(new Rect(91f, 449f, 88f, 100f), speakerPortrait,
            ScaleMode.StretchToFill, true);
        GUI.Label(new Rect(196f, 427f, 650f, 29f), dialogueTitle, headingStyle);
        if (dialogueLines.Count > 0)
        {
            GUI.Label(new Rect(196f, 464f, 650f, 67f), dialogueLines[dialogueIndex], bodyStyle);
        }
        DrawTextButton(new Rect(700f, 538f, 150f, 36f), "Continue", false, AdvanceDialogue);
        GUI.Label(new Rect(105f, 542f, 350f, 26f), "SPACE / ENTER to continue", smallStyle);
    }

    private void DrawBattleScreen()
    {
        GUI.DrawTexture(new Rect(0f, 0f, DesignWidth, DesignHeight),
            regionBattleBackgroundTextures[currentRegion],
            ScaleMode.StretchToFill, false);
        GUI.DrawTexture(new Rect(119f, 22f, 818f, 68f), dialoguePanelTexture,
            ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(45f, 29f, 56f, 56f), fastForwardIconTexture,
            ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(136f, 76f, 136f, 24f), nameplateTexture,
            ScaleMode.StretchToFill, true);
        GUI.Label(new Rect(144f, 77f, 120f, 21f), "BATTLE REPORT", smallStyle);
        string dialogue = battle.Log.Count > 0
            ? battle.Log[battle.Log.Count - 1]
            : battle.Enemies[0].Name + " blocks the passage.";
        GUI.Label(new Rect(141f, 28f, 770f, 48f), dialogue, battleDialogueStyle);

        GUI.Label(new Rect(30f, 111f, 290f, 24f),
            battle.IsBoss ? RegionName(currentRegion).ToUpperInvariant() + " GUARDIAN" :
            RegionName(currentRegion).ToUpperInvariant() + " WILD ENCOUNTER", headingStyle);
        for (int i = 0; i < battle.Enemies.Count; i++)
        {
            Rect enemyBounds;
            if (battle.IsBoss)
            {
                enemyBounds = new Rect(146f, 135f, 230f, 242f);
            }
            else if (battle.Enemies.Count == 1)
            {
                enemyBounds = new Rect(175f, 180f, 184f, 200f);
            }
            else
            {
                enemyBounds = new Rect(52f + i * 185f, 202f, 154f, 164f);
            }
            DrawEnemySprite(i, enemyBounds);
            float labelWidth = battle.Enemies.Count == 1 ? 272f : 166f;
            float labelX = battle.Enemies.Count == 1 ? 117f : 47f + i * 185f;
            GUI.Label(new Rect(labelX, 373f, labelWidth, 18f),
                (battle.Menu == BattleMenu.EnemyTarget && battle.Selection == i ? "> " : string.Empty) +
                battle.Enemies[i].Name + "  " + battle.Enemies[i].Hp + "/" + battle.Enemies[i].MaxHp,
                smallStyle);
            DrawBattleGauge(new Rect(labelX, 393f, labelWidth, 12f),
                battle.Enemies[i].Hp / (float)battle.Enemies[i].MaxHp, enemyGaugeTexture);
        }

        for (int i = 0; i < party.Count; i++)
        {
            DrawBattleHeroSprite(i);
        }

        DrawBattleEffect();
        GUI.DrawTexture(new Rect(26f, 426f, 368f, 188f), commandPanelTexture,
            ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(402f, 426f, 532f, 188f), statusPanelTexture,
            ScaleMode.StretchToFill, true);
        DrawBattlePartyStatus();
        DrawBattleCommands();
    }

    private void DrawBattlePartyStatus()
    {
        GUI.Label(new Rect(420f, 433f, 480f, 22f), "PARTY STATUS", headingStyle);
        for (int i = 0; i < party.Count; i++)
        {
            Hero hero = party[i];
            float y = 459f + i * 47f;
            string activeMarker = i == battle.ActorIndex ? "> " : string.Empty;
            GUI.Label(new Rect(420f, y, 174f, 18f),
                activeMarker + hero.Name + "  " + hero.Role +
                (hero.IsCaptured ? "  CAPTURED" : hero.Hp <= 0 ? "  DOWN" : string.Empty), smallStyle);
            GUI.Label(new Rect(601f, y, 117f, 18f), "HP " + hero.Hp + "/" + hero.MaxHp, smallStyle);
            GUI.Label(new Rect(729f, y, 97f, 18f), "MP " + hero.Mp + "/" + hero.MaxMp, smallStyle);
            DrawBattleGauge(new Rect(420f, y + 23f, 283f, 10f),
                hero.Hp / (float)hero.MaxHp, hpGaugeTexture);
            DrawBattleGauge(new Rect(729f, y + 23f, 184f, 10f),
                hero.Mp / (float)hero.MaxMp, mpGaugeTexture);
        }
    }

    private void DrawBattleGauge(Rect rect, float value, Texture2D fillTexture)
    {
        float innerWidth = Mathf.Max(0f, rect.width - 6f);
        GUI.DrawTexture(rect, gaugeFrameTexture, ScaleMode.StretchToFill, true);
        float fillWidth = innerWidth * Mathf.Clamp01(value);
        if (fillWidth > 0f)
        {
            GUI.DrawTexture(new Rect(rect.x + 3f, rect.y + 3f, fillWidth, Mathf.Max(1f, rect.height - 6f)),
                fillTexture, ScaleMode.StretchToFill, true);
        }
    }

    private void DrawBattleHeroSprite(int heroIndex)
    {
        if (heroIndex < 0 || heroIndex >= partySpriteTextures.Length)
        {
            return;
        }
        if (party[heroIndex].IsCaptured)
        {
            return;
        }

        Vector2 anchor = BattleHeroAnchors[heroIndex];
        float motionX = 0f;
        float motionY = Mathf.Sin(Time.unscaledTime * 3.5f + heroIndex * 1.4f) * 2f;
        bool flashing = heroHitIndex == heroIndex && heroHitTimer > 0f;
        if (fleeAnimationTimer > 0f)
        {
            float progress = 1f - fleeAnimationTimer / FleeAnimationDuration;
            motionX = progress * 245f;
            motionY += Mathf.Abs(Mathf.Sin(Time.unscaledTime * 18f + heroIndex)) * 5f;
        }
        else if (battleAnimationTimer > 0f)
        {
            float progress = 1f - battleAnimationTimer / BattleActionDuration;
            float arc = Mathf.Sin(progress * Mathf.PI);
            if (battleAnimationHeroIndex == heroIndex)
            {
                if (battleAnimation == BattleAnimation.Attack)
                {
                    motionX = -arc * (heroIndex == 0 ? 42f : heroIndex == 1 ? 24f : 18f);
                    motionY -= arc * 7f;
                }
                else if (battleAnimation == BattleAnimation.Skill)
                {
                    motionY -= arc * (heroIndex == 1 ? 18f : 11f);
                }
                else if (battleAnimation == BattleAnimation.Item)
                {
                    motionY -= arc * 7f;
                }
            }

            if (battleAnimation == BattleAnimation.Damage && battleAnimationTargetIndex == heroIndex)
            {
                motionX = Mathf.Sin(progress * 28f) * 6f;
                flashing = true;
            }
        }

        Rect spriteRect = new Rect(anchor.x - 57f + motionX, anchor.y - 150f + motionY, 114f, 150f);
        Color previousColor = GUI.color;
        if (flashing)
        {
            GUI.color = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.5f), 0.4f);
        }
        GUI.DrawTexture(spriteRect, partySpriteTextures[heroIndex], ScaleMode.StretchToFill, true);
        GUI.color = previousColor;
    }

    private void DrawEnemySprite(int enemyIndex, Rect bounds)
    {
        Enemy enemy = battle.Enemies[enemyIndex];
        bool boss = enemy.IsBoss;
        int archetype = enemy.SpriteIndex % 4;
        float phase = Time.unscaledTime * (3.4f + archetype * 0.55f) + enemyIndex;
        float bob = Mathf.Sin(phase) * (boss ? 5f : 3.5f);
        float motionX = archetype == 1 || archetype == 3 ? Mathf.Sin(phase * 0.5f) * 2.5f : 0f;
        float scaleX = archetype == 2 ? 1f + Mathf.Sin(phase * 1.6f) * 0.035f : 1f;
        float scaleY = archetype == 0 ? 1f + Mathf.Sin(phase * 1.4f) * 0.035f : 1f;
        float shake = enemyHitTimer > 0f
            ? Mathf.Sin((0.18f - enemyHitTimer) * 180f + enemyIndex) * 7f
            : 0f;
        if (battleAnimationTimer > 0f && battleAnimation == BattleAnimation.Damage &&
            battleAnimationEnemyIndex == enemyIndex)
        {
            float progress = 1f - battleAnimationTimer / BattleActionDuration;
            shake -= Mathf.Sin(progress * Mathf.PI) * 20f;
        }
        if (battleAnimationTimer > 0f && battleAnimation == BattleAnimation.Damage &&
            battleAnimationEnemyIndex == enemyIndex)
        {
            float progress = 1f - battleAnimationTimer / BattleActionDuration;
            motionX += Mathf.Sin(progress * Mathf.PI) * 14f;
        }
        Color previousColor = GUI.color;
        if (enemyHitTimer > 0f)
        {
            GUI.color = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.5f), enemyHitTimer * 2f);
        }

        float width = bounds.width * scaleX;
        float height = bounds.height * scaleY;
        GUI.DrawTexture(new Rect(bounds.center.x - width * 0.5f + shake + motionX,
            bounds.center.y - height * 0.5f + bob, width, height),
            enemySpriteTextures[enemy.SpriteIndex], ScaleMode.StretchToFill, true);
        GUI.color = previousColor;
    }

    private void DrawBattleEffect()
    {
        if (battleEffectTimer <= 0f)
        {
            return;
        }

        float progress = 1f - battleEffectTimer / 0.38f;
        Texture2D particle;
        if (battleEffect == BattleEffect.Magic)
        {
            particle = magicParticleTexture;
        }
        else if (battleEffect == BattleEffect.Fire)
        {
            particle = fireParticleTexture;
        }
        else if (battleEffect == BattleEffect.Frost)
        {
            particle = frostParticleTexture;
        }
        else if (battleEffect == BattleEffect.Starfall)
        {
            particle = starfallParticleTexture;
        }
        else if (battleEffect == BattleEffect.VanguardAttack)
        {
            particle = partyAttackParticleTextures[0];
        }
        else if (battleEffect == BattleEffect.ArcanistAttack)
        {
            particle = partyAttackParticleTextures[1];
        }
        else if (battleEffect == BattleEffect.MenderAttack)
        {
            particle = partyAttackParticleTextures[2];
        }
        else if (battleEffect == BattleEffect.Healing)
        {
            particle = healingParticleTexture;
        }
        else
        {
            particle = slashParticleTexture;
        }

        Vector2 center;
        if (battleEffectHeroIndex >= 0 && battleEffectHeroIndex < party.Count)
        {
            Vector2 anchor = BattleHeroAnchors[battleEffectHeroIndex];
            center = new Vector2(anchor.x, anchor.y - 77f);
        }
        else
        {
            center = battle.Enemies.Count > 1 && battle.TargetEnemyIndex >= 0
                ? new Vector2(129f + battle.TargetEnemyIndex * 185f, 282f)
                : new Vector2(278f, 282f);
        }

        Color previousColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 1f - progress * 0.65f);
        for (int i = 0; i < 6; i++)
        {
            float angle = (i * 60f + progress * 180f) * Mathf.Deg2Rad;
            float radius = 14f + progress * 52f;
            float x;
            float y;
            if (battleEffect == BattleEffect.Fire)
            {
                angle += progress * 6f;
                x = center.x + Mathf.Cos(angle) * radius;
                y = center.y + Mathf.Sin(angle) * radius * 0.58f - progress * 22f;
            }
            else if (battleEffect == BattleEffect.Frost)
            {
                radius = 18f + progress * 38f;
                angle += (i % 2 == 0 ? progress : -progress) * 2f;
                x = center.x + Mathf.Cos(angle) * radius;
                y = center.y + Mathf.Sin(angle) * radius;
            }
            else if (battleEffect == BattleEffect.Starfall)
            {
                float fall = Mathf.Clamp01(progress * 1.45f - i * 0.08f);
                x = center.x + (i - 2.5f) * 13f + Mathf.Sin(progress * 9f + i) * 8f;
                y = center.y - 66f + fall * 79f;
            }
            else if (battleEffect == BattleEffect.Healing)
            {
                x = center.x + Mathf.Sin(angle) * radius * 0.45f;
                y = center.y + Mathf.Cos(angle) * radius * 0.45f - progress * 40f;
            }
            else
            {
                x = center.x + Mathf.Cos(angle) * radius;
                y = center.y + Mathf.Sin(angle) * radius;
            }
            float size = Mathf.Max(8f, 25f * (1f - progress));
            GUI.DrawTexture(new Rect(x - size * 0.5f, y - size * 0.5f, size, size),
                particle, ScaleMode.StretchToFill, true);
        }
        GUI.color = previousColor;
    }

    private void DrawBattleCommands()
    {
        if (fleeAnimationTimer > 0f)
        {
            GUI.Label(new Rect(48f, 479f, 326f, 53f),
                fleeWillSucceed ? "The party is escaping..." : "The party searches for an escape route...",
                headingStyle);
            return;
        }

        if (battleAnimationTimer > 0f)
        {
            string actionMessage;
            switch (battleAnimation)
            {
                case BattleAnimation.Attack:
                    actionMessage = "The party attacks!";
                    break;
                case BattleAnimation.Skill:
                    actionMessage = "A skill is unleashed!";
                    break;
                case BattleAnimation.Item:
                    actionMessage = "The item takes effect!";
                    break;
                case BattleAnimation.Damage:
                    actionMessage = "The party takes a hit!";
                    break;
                default:
                    actionMessage = "The battle continues...";
                    break;
            }
            GUI.Label(new Rect(48f, 479f, 326f, 53f), actionMessage, headingStyle);
            return;
        }

        if (pendingBattleVictory)
        {
            GUI.Label(new Rect(48f, 479f, 326f, 53f), "The enemy is defeated!", headingStyle);
            return;
        }

        if (pendingGameOver)
        {
            GUI.Label(new Rect(48f, 479f, 326f, 53f), "The party has fallen...", headingStyle);
            return;
        }

        if (pendingEnemyTurn)
        {
            GUI.Label(new Rect(48f, 479f, 326f, 53f), "The enemy is preparing to act...", headingStyle);
            return;
        }

        if (battle.Menu == BattleMenu.Commands)
        {
            Hero actor = party[battle.ActorIndex];
            GUI.Label(new Rect(48f, 435f, 328f, 25f),
                actor.Name + "'s turn  |  " + actor.Skills.Count + " skills learned", headingStyle);
            string[] commands = { "Attack", "Skill", "Item", "Run" };
            for (int i = 0; i < commands.Length; i++)
            {
                int selection = i;
                Rect button = new Rect(48f + (i % 2) * 164f, 465f + (i / 2) * 47f, 154f, 38f);
                if (DrawPixelButton(button, commands[i], battle.Selection == i))
                {
                    battle.Selection = selection;
                    ConfirmBattleSelection();
                    return;
                }
            }
            GUI.Label(new Rect(48f, 560f, 326f, 37f), "ARROWS / WASD select; ENTER confirms.", smallStyle);
        }
        else if (battle.Menu == BattleMenu.Skill)
        {
            Hero actor = party[battle.ActorIndex];
            GUI.Label(new Rect(48f, 435f, 328f, 25f), actor.Name + "  |  Learned skills", headingStyle);
            for (int i = 0; i < actor.Skills.Count; i++)
            {
                int skillIndex = i;
                CharacterSkill skill = actor.Skills[i];
                string skillText = SkillName(skill) + "  (" + GetSkillCost(skill) + " MP)";
                if (DrawPixelButton(new Rect(48f, 464f + i * 32f, 326f, 29f), skillText,
                    battle.Selection == i))
                {
                    battle.Selection = skillIndex;
                    ConfirmBattleSelection();
                    return;
                }
            }
            int backSelection = actor.Skills.Count;
            if (DrawPixelButton(new Rect(48f, 464f + actor.Skills.Count * 32f, 326f, 29f),
                "Back to commands", battle.Selection == backSelection))
            {
                battle.Selection = backSelection;
                ConfirmBattleSelection();
                return;
            }
            if (battle.Selection < actor.Skills.Count)
            {
                CharacterSkill selectedSkill = actor.Skills[battle.Selection];
                GUI.Label(new Rect(48f, 586f, 326f, 20f), SkillDescription(selectedSkill), smallStyle);
            }
        }
        else if (battle.Menu == BattleMenu.Item)
        {
            GUI.Label(new Rect(48f, 435f, 328f, 25f), "Choose an item", headingStyle);
            int usableItemCount = BattleItemCount();
            for (int i = 0; i < usableItemCount; i++)
            {
                int itemIndex = i;
                ItemDefinition item = ItemCatalog[i];
                Rect button = new Rect(48f + (i % 2) * 164f, 464f + (i / 2) * 43f, 154f, 37f);
                string label = item.Name + " x" + GetItemCount(item.Kind);
                if (DrawPixelButton(button, label, battle.Selection == i))
                {
                    battle.Selection = itemIndex;
                    ConfirmBattleSelection();
                    return;
                }
            }

            if (DrawPixelButton(new Rect(48f, 555f, 154f, 35f), "Back", battle.Selection == usableItemCount))
            {
                battle.Selection = usableItemCount;
                ConfirmBattleSelection();
                return;
            }
            string description = battle.Selection < usableItemCount
                ? ItemCatalog[battle.Selection].Description
                : "Return to battle commands.";
            GUI.Label(new Rect(211f, 555f, 163f, 35f), description, smallStyle);
        }
        else if (battle.Menu == BattleMenu.ItemTarget)
        {
            ItemDefinition item = GetItemDefinition(battle.SelectedItem);
            GUI.Label(new Rect(48f, 435f, 328f, 25f), item.Name + "  |  Choose a target", headingStyle);
            for (int i = 0; i < party.Count; i++)
            {
                int target = i;
                Hero hero = party[i];
                Rect button = new Rect(48f, 464f + i * 38f, 326f, 34f);
                string label = hero.Name + "  " + hero.Hp + "/" + hero.MaxHp + " HP";
                if (DrawPixelButton(button, label, battle.Selection == i))
                {
                    battle.Selection = target;
                    ConfirmBattleSelection();
                    return;
                }
            }

            if (DrawPixelButton(new Rect(48f, 581f, 154f, 28f), "Back", battle.Selection == party.Count))
            {
                battle.Selection = party.Count;
                ConfirmBattleSelection();
                return;
            }
            GUI.Label(new Rect(211f, 580f, 163f, 31f), item.Description, smallStyle);
        }
        else
        {
            GUI.Label(new Rect(48f, 435f, 328f, 25f), "Choose an enemy target", headingStyle);
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                int targetIndex = i;
                Enemy enemy = battle.Enemies[i];
                string label = enemy.Name + "  " + enemy.Hp + "/" + enemy.MaxHp + " HP";
                if (DrawPixelButton(new Rect(48f, 466f + i * 43f, 326f, 38f), label,
                    battle.Selection == i))
                {
                    battle.Selection = targetIndex;
                    ConfirmBattleSelection();
                    return;
                }
            }
            GUI.Label(new Rect(48f, 560f, 326f, 35f),
                battle.IsSelectingSkillTarget ? SkillName(battle.SelectedSkill) + " target" : "Attack target",
                smallStyle);
        }
    }

    private static string SkillName(CharacterSkill skill)
    {
        switch (skill)
        {
            case CharacterSkill.PowerStrike:
                return "Power Strike";
            case CharacterSkill.Spark:
                return "Spark";
            case CharacterSkill.Cure:
                return "Cure";
            case CharacterSkill.FlameArc:
                return "Flame Arc";
            case CharacterSkill.FrostNova:
                return "Frost Nova";
            case CharacterSkill.GreaterCure:
                return "Greater Cure";
            default:
                return "Starfall";
        }
    }

    private static string SkillDescription(CharacterSkill skill)
    {
        switch (skill)
        {
            case CharacterSkill.PowerStrike:
                return "A heavy weapon attack that deals extra damage.";
            case CharacterSkill.Spark:
                return "A bolt of magic that strikes the enemy.";
            case CharacterSkill.Cure:
                return "Restores HP to the most injured ally.";
            case CharacterSkill.FlameArc:
                return "A focused arc of fire lashes one enemy.";
            case CharacterSkill.FrostNova:
                return "A sharp burst of frost strikes one enemy.";
            case CharacterSkill.GreaterCure:
                return "Restores a large amount of HP to an ally.";
            default:
                return "A falling star of aether strikes one enemy.";
        }
    }

    private static string LearnedSkillNames(Hero hero)
    {
        string result = string.Empty;
        for (int i = 0; i < hero.Skills.Count; i++)
        {
            if (i > 0)
            {
                result += ", ";
            }
            result += SkillName(hero.Skills[i]);
        }
        return result;
    }

    private void DrawEnding(bool won)
    {
        FillRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0.015f, 0.025f, 0.05f, 0.86f));
        DrawModal(new Rect(203f, 174f, 554f, 300f));
        GUI.Label(new Rect(240f, 210f, 480f, 48f), won ? "DAWN RETURNS" : "THE PARTY HAS FALLEN",
            won ? victoryStyle : gameOverStyle);
        GUI.Label(new Rect(258f, 277f, 444f, 63f),
            won
                ? "The four guardian relics seal Malachar's rift. Dawn returns to Greenwick, Whisperwood, Emberdeep, and the Nether March."
                : "The road was too dangerous this time. Rest in town and try again.",
            bodyStyle);
        DrawTextButton(new Rect(342f, 371f, 276f, 52f), "Begin a new journey", false, ResetGame);
        GUI.Label(new Rect(310f, 436f, 340f, 24f), "ENTER starts over", smallStyle);
    }

    private void DrawTextButton(Rect rect, string text, bool selected, System.Action action)
    {
        if (DrawButton(rect, text, selected))
        {
            action();
        }
    }

    private bool DrawButton(Rect rect, string text, bool selected)
    {
        int textureIndex = GetMenuButtonTextureIndex(rect);
        GUI.DrawTexture(rect, selected ? selectedMenuButtonTextures[textureIndex] : menuButtonTextures[textureIndex],
            ScaleMode.StretchToFill, true);
        bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
        GUI.Label(rect, text, buttonStyle);
        return clicked;
    }

    private static int GetMenuButtonTextureIndex(Rect rect)
    {
        float aspect = rect.width / rect.height;
        if (aspect >= 8.5f)
        {
            return 0;
        }
        if (aspect >= 6.1f)
        {
            return 1;
        }
        if (aspect >= 4.5f)
        {
            return 2;
        }

        return 3;
    }

    private bool DrawPixelButton(Rect rect, string text, bool selected)
    {
        GUI.DrawTexture(rect, selected ? selectedBattleButtonTexture : battleButtonTexture,
            ScaleMode.StretchToFill, true);
        bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
        GUI.Label(rect, text, buttonStyle);
        return clicked;
    }

    private void DrawModal(Rect rect)
    {
        Texture2D texture = rect.width > DesignWidth * 0.8f
            ? dialogueModalTexture
            : rect.width > 500f ? endingModalTexture : menuModalTexture;
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
    }

    private static void DrawPixelPanel(Rect rect, Texture2D texture)
    {
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
    }

    private static void FillRect(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }
}
