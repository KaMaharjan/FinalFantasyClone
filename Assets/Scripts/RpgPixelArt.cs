using System.Collections.Generic;
using UnityEngine;

public static class RpgPixelArt
{
    private const int BattleWidth = 480;
    private const int BattleHeight = 320;

    private sealed class PixelSurface
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Color32[] Pixels;

        public PixelSurface(int width, int height, Color32 clearColor)
        {
            Width = width;
            Height = height;
            Pixels = new Color32[width * height];
            Fill(clearColor);
        }

        public void Fill(Color32 color)
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                Pixels[i] = color;
            }
        }

        public void Set(int x, int y, Color32 color)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Pixels[(Height - y - 1) * Width + x] = color;
            }
        }

        public Color32 Get(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return new Color32(0, 0, 0, 0);
            }

            return Pixels[(Height - y - 1) * Width + x];
        }

        public void Rect(int x, int y, int width, int height, Color32 color)
        {
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                {
                    Set(px, py, color);
                }
            }
        }

        public void Line(int x0, int y0, int x1, int y1, Color32 color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                Set(x0, y0, color);
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int doubledError = error * 2;
                if (doubledError >= dy)
                {
                    error += dy;
                    x0 += sx;
                }
                if (doubledError <= dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        public void Polygon(Color32 color, params int[] coordinates)
        {
            int minY = Height;
            int maxY = 0;
            int minX = Width;
            int maxX = 0;
            int pointCount = coordinates.Length / 2;
            for (int i = 0; i < pointCount; i++)
            {
                int x = coordinates[i * 2];
                int y = coordinates[i * 2 + 1];
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            minX = Mathf.Clamp(minX, 0, Width - 1);
            maxX = Mathf.Clamp(maxX, 0, Width - 1);
            minY = Mathf.Clamp(minY, 0, Height - 1);
            maxY = Mathf.Clamp(maxY, 0, Height - 1);
            List<float> intersections = new List<float>(pointCount);

            for (int y = minY; y <= maxY; y++)
            {
                intersections.Clear();
                for (int i = 0; i < pointCount; i++)
                {
                    int next = (i + 1) % pointCount;
                    int x1 = coordinates[i * 2];
                    int y1 = coordinates[i * 2 + 1];
                    int x2 = coordinates[next * 2];
                    int y2 = coordinates[next * 2 + 1];
                    if ((y1 <= y && y < y2) || (y2 <= y && y < y1))
                    {
                        intersections.Add(x1 + (y - y1) * (x2 - x1) / (float)(y2 - y1));
                    }
                }

                intersections.Sort();
                for (int i = 0; i + 1 < intersections.Count; i += 2)
                {
                    int startX = Mathf.Max(minX, Mathf.CeilToInt(intersections[i]));
                    int endX = Mathf.Min(maxX, Mathf.FloorToInt(intersections[i + 1]));
                    for (int x = startX; x <= endX; x++)
                    {
                        Set(x, y, color);
                    }
                }
            }
        }

        public void Disc(int centerX, int centerY, int radius, Color32 color)
        {
            int radiusSquared = radius * radius;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radiusSquared)
                    {
                        Set(centerX + x, centerY + y, color);
                    }
                }
            }
        }
    }

    public static Texture2D CreateBattleBackground()
    {
        return CreateBattleBackground(2);
    }

    public static Texture2D CreateBattleBackground(int region)
    {
        PixelSurface image = new PixelSurface(BattleWidth, BattleHeight, new Color32(0, 0, 0, 255));
        const int horizon = 148;

        for (int y = 0; y < horizon + 8; y++)
        {
            for (int x = 0; x < BattleWidth; x++)
            {
                int noise = Hash(x / 2, y / 3, 17) % 23 - 11;
                int shiftedX = x + y / 12 + Hash(0, y / 19, 7) % 17;
                int striation = Mod(shiftedX, 69);
                int light = striation < 2 ? 16 : striation < 5 ? 7 : 0;
                int seam = y % 37 < 2 ? -8 : 0;
                int depth = y > 111 ? (y - 111) / 6 : 0;
                int vignette = Mathf.Abs(x - BattleWidth / 2) / 42;
                int value = noise + light + seam + depth - vignette;
                image.Set(x, y, new Color32(
                    ClampByte(18 + value),
                    ClampByte(30 + value),
                    ClampByte(48 + value + 7),
                    255));
            }
        }

        for (int x = 12; x < BattleWidth; x += 43)
        {
            int width = 5 + Hash(x, 3, 11) % 8;
            int length = 15 + Hash(x, 9, 12) % 49;
            for (int y = 0; y < length; y++)
            {
                int taper = Mathf.Max(1, width - y / 7);
                int center = x + (y / 5) % 5 - 2;
                for (int dx = -taper; dx <= taper; dx++)
                {
                    Color32 rock = dx < -taper / 2
                        ? new Color32(34, 49, 67, 255)
                        : new Color32(11, 19, 31, 255);
                    image.Set(center + dx, y, rock);
                }
            }
        }

        for (int x = 8; x < BattleWidth; x += 57)
        {
            int width = 5 + Hash(x, 21, 23) % 11;
            int tipY = 103 + Hash(x, 33, 24) % 39;
            for (int y = horizon - 1; y >= tipY; y--)
            {
                int taper = Mathf.Max(1, width - (horizon - y) / 6);
                int center = x - (horizon - y) / 11;
                for (int dx = -taper; dx <= taper; dx++)
                {
                    image.Set(center + dx, y, dx < 0
                        ? new Color32(30, 44, 62, 255)
                        : new Color32(13, 22, 35, 255));
                }
            }
        }

        for (int y = horizon; y < BattleHeight; y++)
        {
            float depth = (y - horizon) / (float)(BattleHeight - horizon);
            float rowPosition = Mathf.Sqrt(depth) * 14f;
            int row = Mathf.FloorToInt(rowPosition);
            float rowFraction = rowPosition - row;
            bool horizontalSeam = rowFraction < 0.045f || rowFraction > 0.955f;
            int columnWidth = 4 + Mathf.RoundToInt(depth * 31f);
            int stagger = row % 2 == 0 ? 0 : columnWidth / 2;

            for (int x = 0; x < BattleWidth; x++)
            {
                int shifted = x - BattleWidth / 2 + stagger;
                int column = Mathf.FloorToInt(shifted / (float)columnWidth);
                int withinTile = Mod(shifted, columnWidth);
                bool verticalSeam = withinTile <= 1 || withinTile >= columnWidth - 2;
                int variation = Hash(column, row, 31) % 27 - 13;
                int glow = Hash(column, row, 41) % 17;
                int floorShade = 37 + Mathf.RoundToInt(depth * 52f) + variation;
                bool reflection = Hash(x / 4, y / 5, 53) % 97 < 3
                    && withinTile > 3 && withinTile < columnWidth - 4
                    && rowFraction > 0.16f && rowFraction < 0.84f;

                Color32 pixel;
                if (horizontalSeam || verticalSeam)
                {
                    int seamLight = y > horizon + 24 && !horizontalSeam ? 9 : 0;
                    pixel = new Color32(
                        ClampByte(13 + seamLight),
                        ClampByte(23 + seamLight),
                        ClampByte(39 + seamLight),
                        255);
                }
                else
                {
                    int reflectionLight = reflection ? 38 : 0;
                    int tileLight = floorShade + (glow / 3) + reflectionLight;
                    pixel = new Color32(
                        ClampByte(24 + tileLight / 2),
                        ClampByte(39 + tileLight),
                        ClampByte(59 + tileLight + glow),
                        255);
                }

                image.Set(x, y, pixel);
            }
        }

        ApplyBattlePalette(image, region);
        return CreateTexture(image, "Region " + region + " Original 16-bit Battle Background",
            TextureFormat.RGB565);
    }

    private static void ApplyBattlePalette(PixelSurface image, int region)
    {
        for (int i = 0; i < image.Pixels.Length; i++)
        {
            Color32 pixel = image.Pixels[i];
            float r = pixel.r;
            float g = pixel.g;
            float b = pixel.b;
            if (region == 0)
            {
                pixel = new Color32(ClampByte(Mathf.RoundToInt(r * 0.66f)),
                    ClampByte(Mathf.RoundToInt(g * 1.27f)), ClampByte(Mathf.RoundToInt(b * 0.63f)), 255);
            }
            else if (region == 1)
            {
                pixel = new Color32(ClampByte(Mathf.RoundToInt(r * 0.50f)),
                    ClampByte(Mathf.RoundToInt(g * 1.20f)), ClampByte(Mathf.RoundToInt(b * 0.76f)), 255);
            }
            else if (region == 3)
            {
                pixel = new Color32(ClampByte(Mathf.RoundToInt(r * 1.22f)),
                    ClampByte(Mathf.RoundToInt(g * 0.58f)), ClampByte(Mathf.RoundToInt(b * 1.28f)), 255);
            }
            else
            {
                pixel = new Color32(ClampByte(Mathf.RoundToInt(r * 1.42f)),
                    ClampByte(Mathf.RoundToInt(g * 0.60f)), ClampByte(Mathf.RoundToInt(b * 0.48f)), 255);
            }
            image.Pixels[i] = pixel;
        }
    }

    public static Texture2D CreateOverworldMap(char[,] terrain, int mapWidth, int mapHeight, int tileSize)
    {
        int width = mapWidth * tileSize;
        int height = mapHeight * tileSize;
        PixelSurface image = new PixelSurface(width, height, new Color32(0, 0, 0, 255));

        for (int tileY = 0; tileY < mapHeight; tileY++)
        {
            for (int tileX = 0; tileX < mapWidth; tileX++)
            {
                char type = terrain[tileX, tileY];
                Color32 baseColor = GetTileColor(type, tileX, tileY);
                int originX = tileX * tileSize;
                int originY = tileY * tileSize;

                for (int y = 0; y < tileSize; y++)
                {
                    for (int x = 0; x < tileSize; x++)
                    {
                        int texture = Hash(tileX * tileSize + x, tileY * tileSize + y, type) % 13 - 6;
                        image.Set(originX + x, originY + y, Shade(baseColor, texture));
                    }
                }

                PaintTileDetails(image, type, tileX, tileY, originX, originY, tileSize);
                image.Line(originX, originY, originX + tileSize - 1, originY, new Color32(7, 13, 18, 190));
                image.Line(originX, originY, originX, originY + tileSize - 1, new Color32(7, 13, 18, 140));
            }
        }

        return CreateTexture(image, "Greenwick 16-bit Overworld Map RGB565", TextureFormat.RGB565);
    }

    public static Texture2D CreateVillagerSprite(bool healer)
    {
        return CreateVillagerSprite(healer ? 1 : 0);
    }

    public static Texture2D CreateVillagerSprite(int style)
    {
        PixelSurface image = new PixelSurface(48, 64, new Color32(0, 0, 0, 0));
        Color32 ink = Color32Color(22, 20, 25);
        Color32 robe = style == 1 ? Color32Color(78, 97, 66)
            : style == 2 ? Color32Color(70, 84, 128)
            : style == 3 ? Color32Color(116, 77, 112)
            : Color32Color(123, 74, 51);
        Color32 hair = style == 1 ? Color32Color(67, 96, 69)
            : style == 2 ? Color32Color(63, 60, 93)
            : style == 3 ? Color32Color(82, 55, 91)
            : Color32Color(91, 59, 43);
        image.Disc(24, 60, 15, new Color32(14, 20, 29, 135));
        image.Polygon(ink, 11, 26, 17, 21, 30, 21, 37, 28, 35, 50, 30, 56, 14, 54, 10, 42);
        image.Polygon(robe,
            14, 28, 19, 24, 29, 24, 34, 30, 32, 48, 28, 52, 16, 51, 13, 41);
        image.Polygon(Color32Color(213, 174, 133), 17, 9, 22, 5, 29, 7, 33, 14, 31, 24, 27, 28, 19, 25, 15, 19);
        image.Polygon(hair,
            15, 12, 20, 5, 29, 6, 34, 13, 31, 16, 22, 13, 16, 19);
        image.Rect(19, 16, 3, 2, ink);
        image.Rect(27, 16, 3, 2, ink);
        image.Polygon(ink, 14, 50, 22, 50, 21, 60, 14, 60);
        image.Polygon(ink, 26, 50, 33, 49, 35, 59, 28, 60);
        image.Rect(12, 58, 10, 4, Color32Color(52, 47, 46));
        image.Rect(27, 58, 10, 4, Color32Color(52, 47, 46));
        if (style == 1)
        {
            image.Line(23, 30, 24, 45, Color32Color(195, 168, 102));
            image.Rect(20, 35, 8, 3, Color32Color(195, 168, 102));
            image.Rect(23, 32, 3, 9, Color32Color(195, 168, 102));
        }
        else if (style == 2)
        {
            image.Line(18, 30, 30, 30, Color32Color(202, 172, 104));
            image.Rect(20, 28, 9, 5, Color32Color(181, 143, 78));
            image.Rect(22, 29, 5, 3, Color32Color(237, 207, 131));
        }
        else if (style == 3)
        {
            image.Line(23, 28, 24, 44, Color32Color(138, 115, 181));
            image.Disc(24, 27, 4, Color32Color(184, 128, 227));
        }
        else
        {
            image.Line(18, 30, 29, 30, Color32Color(192, 148, 76));
            image.Rect(21, 29, 5, 5, Color32Color(224, 191, 103));
        }

        return CreateTexture(image, "Region " + style + " 16-bit Villager Sprite",
            TextureFormat.RGBA4444);
    }

    public static Texture2D CreateChestSprite(bool opened)
    {
        PixelSurface image = new PixelSurface(32, 32, new Color32(0, 0, 0, 0));
        image.Disc(16, 29, 13, new Color32(16, 19, 24, 100));
        if (opened)
        {
            image.Polygon(Color32Color(64, 43, 30), 5, 13, 15, 8, 27, 12, 26, 17, 17, 22, 6, 19);
            image.Polygon(Color32Color(160, 105, 40), 4, 18, 27, 17, 27, 26, 5, 27);
            image.Line(5, 18, 26, 18, Color32Color(233, 187, 75));
            image.Rect(14, 20, 4, 6, Color32Color(92, 57, 28));
            image.Rect(9, 12, 3, 4, Color32Color(255, 223, 129));
            image.Rect(20, 10, 3, 5, Color32Color(255, 223, 129));
        }
        else
        {
            image.Polygon(Color32Color(62, 37, 25), 4, 12, 27, 12, 28, 25, 4, 26);
            image.Polygon(Color32Color(149, 86, 34), 5, 14, 26, 14, 26, 24, 5, 24);
            image.Line(5, 15, 26, 15, Color32Color(245, 193, 76));
            image.Line(5, 23, 26, 23, Color32Color(217, 161, 60));
            image.Rect(14, 16, 4, 7, Color32Color(241, 209, 116));
            image.Rect(15, 17, 2, 5, Color32Color(94, 69, 38));
            image.Rect(7, 17, 3, 3, Color32Color(199, 143, 53));
            image.Rect(21, 17, 3, 3, Color32Color(199, 143, 53));
        }

        return CreateTexture(image, opened ? "Opened Treasure Chest RGBA4444" : "Treasure Chest RGBA4444",
            TextureFormat.RGBA4444);
    }

    public static Texture2D CreateQuestMarker()
    {
        PixelSurface image = new PixelSurface(16, 20, new Color32(0, 0, 0, 0));
        image.Polygon(Color32Color(42, 33, 29), 8, 0, 15, 7, 14, 16, 8, 20, 1, 16, 0, 7);
        image.Polygon(Color32Color(235, 190, 78), 8, 2, 13, 7, 12, 14, 8, 18, 4, 14, 3, 7);
        image.Rect(7, 6, 3, 6, Color32Color(56, 44, 31));
        image.Rect(7, 14, 3, 2, Color32Color(56, 44, 31));
        image.Rect(4, 5, 2, 3, Color32Color(255, 230, 150));
        return CreateTexture(image, "Gold 16-bit Quest Marker RGBA4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreateVanguardSprite()
    {
        PixelSurface image = new PixelSurface(112, 144, new Color32(0, 0, 0, 0));
        Color32 ink = Color32Color(12, 17, 28);
        Color32 steelDark = Color32Color(49, 65, 81);
        Color32 steel = Color32Color(115, 139, 157);
        Color32 steelBright = Color32Color(190, 204, 205);
        Color32 gold = Color32Color(196, 157, 75);

        image.Polygon(new Color32(20, 30, 53, 185), 29, 48, 46, 39, 72, 43, 86, 61, 82, 119, 65, 132, 28, 122, 19, 91);
        image.Polygon(Color32Color(37, 48, 75), 28, 49, 42, 44, 40, 110, 31, 121, 25, 99);
        image.Polygon(ink, 37, 102, 52, 101, 49, 128, 34, 131, 29, 124);
        image.Polygon(ink, 57, 102, 73, 101, 79, 124, 72, 132, 58, 128);
        image.Polygon(Color32Color(43, 57, 70), 38, 106, 50, 106, 47, 126, 36, 127);
        image.Polygon(Color32Color(34, 44, 59), 60, 106, 71, 105, 76, 125, 65, 127);
        image.Rect(30, 126, 19, 6, Color32Color(74, 76, 75));
        image.Rect(61, 126, 18, 6, Color32Color(74, 76, 75));

        image.Polygon(ink, 29, 46, 41, 37, 69, 36, 83, 47, 79, 94, 69, 108, 39, 105, 26, 82);
        image.Polygon(steelDark, 33, 47, 43, 40, 67, 39, 78, 48, 74, 89, 66, 101, 42, 99, 32, 80);
        image.Polygon(steel, 43, 45, 66, 43, 72, 52, 67, 77, 43, 80, 37, 62);
        image.Polygon(steelBright, 47, 47, 64, 46, 67, 54, 61, 59, 45, 57);
        image.Line(43, 62, 67, 63, Color32Color(48, 61, 73));
        image.Line(46, 66, 64, 68, Color32Color(147, 161, 166));
        image.Polygon(Color32Color(139, 153, 164), 26, 45, 39, 40, 44, 51, 38, 66, 27, 61);
        image.Polygon(Color32Color(98, 119, 136), 69, 41, 82, 47, 85, 60, 73, 67, 67, 53);
        image.Rect(31, 52, 8, 3, steelBright);
        image.Rect(70, 49, 8, 3, steelBright);
        image.Rect(52, 59, 11, 8, gold);
        image.Rect(55, 61, 5, 4, Color32Color(236, 207, 128));

        image.Polygon(ink, 39, 15, 50, 7, 67, 9, 76, 20, 72, 41, 63, 48, 43, 44, 35, 32);
        image.Polygon(steelDark, 42, 15, 51, 10, 65, 11, 72, 21, 69, 37, 63, 42, 45, 40, 39, 30);
        image.Polygon(steelBright, 45, 14, 53, 11, 64, 13, 68, 20, 59, 23, 44, 21);
        image.Rect(45, 27, 23, 5, ink);
        image.Rect(48, 28, 16, 2, Color32Color(23, 31, 43));
        image.Rect(51, 30, 3, 2, Color32Color(228, 181, 76));
        image.Polygon(Color32Color(126, 43, 45), 51, 8, 57, 0, 63, 8, 60, 14, 53, 13);
        image.Line(39, 16, 48, 10, Color32Color(217, 226, 223));
        image.Line(70, 20, 66, 36, Color32Color(155, 174, 181));

        image.Polygon(ink, 80, 41, 87, 39, 96, 96, 91, 116, 83, 114, 84, 93);
        image.Polygon(Color32Color(130, 147, 160), 83, 43, 87, 43, 92, 95, 89, 107, 86, 105);
        image.Line(81, 91, 96, 89, gold);
        image.Rect(87, 101, 5, 15, Color32Color(115, 73, 43));
        image.Rect(82, 98, 16, 4, gold);
        image.Rect(28, 57, 9, 21, ink);
        image.Rect(31, 59, 6, 15, steel);
        image.Rect(71, 57, 9, 18, ink);
        image.Rect(73, 59, 6, 13, steelBright);

        for (int y = 46; y < 94; y++)
        {
            for (int x = 42; x < 70; x++)
            {
                if (image.Get(x, y).a > 0 && Hash(x, y, 61) % 41 == 0)
                {
                    image.Set(x, y, Color32Color(208, 218, 215));
                }
            }
        }

        return CreateTexture(image, "Vanguard Pixel Sprite RGB4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreateArcanistSprite()
    {
        PixelSurface image = new PixelSurface(112, 144, new Color32(0, 0, 0, 0));
        Color32 ink = Color32Color(16, 13, 30);
        Color32 robe = Color32Color(63, 39, 105);
        Color32 robeLight = Color32Color(111, 76, 158);
        Color32 robeBright = Color32Color(168, 126, 199);
        Color32 gold = Color32Color(205, 166, 87);

        image.Polygon(new Color32(22, 27, 50, 180), 32, 48, 74, 49, 85, 111, 66, 133, 24, 125, 19, 88);
        image.Line(82, 20, 87, 125, Color32Color(82, 54, 34));
        image.Line(84, 24, 90, 122, Color32Color(157, 111, 57));
        image.Disc(82, 16, 10, ink);
        image.Disc(82, 15, 7, Color32Color(39, 82, 120));
        image.Disc(81, 14, 4, Color32Color(125, 202, 220));
        image.Set(79, 12, Color32Color(222, 242, 226));

        image.Polygon(ink, 30, 42, 44, 36, 68, 41, 79, 54, 76, 112, 64, 127, 25, 124, 17, 102);
        image.Polygon(robe, 34, 47, 45, 41, 66, 45, 73, 56, 69, 109, 61, 120, 30, 117, 23, 100);
        image.Polygon(robeLight, 52, 47, 65, 49, 69, 59, 63, 105, 52, 113, 47, 101);
        image.Polygon(Color32Color(79, 53, 119), 31, 83, 46, 82, 43, 113, 29, 115, 23, 100);
        image.Line(34, 112, 65, 115, gold);
        image.Line(50, 57, 54, 105, Color32Color(217, 184, 110));
        image.Line(35, 67, 28, 84, ink);
        image.Polygon(ink, 24, 75, 36, 77, 31, 94, 19, 91);
        image.Polygon(robeLight, 24, 77, 31, 79, 27, 89, 21, 88);

        image.Polygon(ink, 36, 14, 47, 5, 67, 7, 77, 20, 73, 44, 63, 53, 43, 47, 34, 34);
        image.Polygon(robe, 39, 14, 48, 9, 64, 10, 72, 21, 68, 39, 61, 45, 45, 41, 37, 31);
        image.Polygon(Color32Color(218, 175, 132), 46, 24, 64, 24, 65, 36, 57, 42, 47, 38, 43, 31);
        image.Rect(48, 31, 4, 2, ink);
        image.Rect(60, 30, 4, 2, ink);
        image.Line(47, 39, 60, 41, Color32Color(106, 52, 64));
        image.Polygon(robeBright, 34, 16, 47, 5, 68, 8, 78, 19, 70, 22, 51, 17, 39, 26);
        image.Line(39, 18, 48, 9, Color32Color(213, 178, 220));
        image.Rect(45, 54, 23, 4, gold);
        image.Disc(56, 56, 4, Color32Color(106, 192, 218));

        for (int y = 52; y < 114; y++)
        {
            for (int x = 34; x < 69; x++)
            {
                Color32 existing = image.Get(x, y);
                if (existing.a > 0 && Hash(x, y, 71) % 53 == 0)
                {
                    image.Set(x, y, Color32Color(192, 155, 209));
                }
            }
        }

        return CreateTexture(image, "Arcanist Pixel Sprite RGB4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreateMenderSprite()
    {
        PixelSurface image = new PixelSurface(112, 144, new Color32(0, 0, 0, 0));
        Color32 ink = Color32Color(19, 19, 27);
        Color32 cloth = Color32Color(174, 180, 176);
        Color32 clothLight = Color32Color(227, 222, 199);
        Color32 trim = Color32Color(187, 148, 75);

        image.Polygon(new Color32(25, 31, 46, 175), 30, 45, 72, 46, 83, 112, 65, 132, 26, 126, 19, 91);
        image.Polygon(ink, 30, 47, 43, 40, 69, 40, 80, 54, 76, 112, 66, 127, 25, 123, 20, 96);
        image.Polygon(cloth, 34, 49, 45, 44, 67, 44, 74, 56, 69, 108, 61, 119, 30, 116, 25, 96);
        image.Polygon(clothLight, 47, 46, 62, 46, 67, 60, 61, 108, 53, 114, 46, 99);
        image.Polygon(Color32Color(126, 133, 141), 30, 76, 40, 79, 36, 112, 28, 114, 24, 96);
        image.Line(50, 56, 53, 111, trim);
        image.Line(35, 111, 65, 114, trim);
        image.Line(41, 46, 32, 57, clothLight);
        image.Polygon(ink, 37, 16, 48, 8, 66, 10, 75, 23, 70, 43, 63, 49, 43, 45, 35, 33);
        image.Polygon(clothLight, 41, 17, 49, 12, 63, 13, 70, 24, 66, 37, 60, 43, 45, 39, 38, 31);
        image.Polygon(Color32Color(194, 144, 104), 47, 25, 63, 25, 64, 35, 57, 40, 48, 37, 43, 31);
        image.Rect(47, 30, 4, 2, ink);
        image.Rect(59, 30, 4, 2, ink);
        image.Polygon(Color32Color(188, 197, 194), 33, 18, 44, 9, 65, 11, 75, 23, 68, 27, 56, 18, 42, 27);
        image.Line(36, 19, 47, 12, Color32Color(244, 241, 218));
        image.Rect(47, 58, 15, 4, trim);
        image.Disc(54, 60, 3, Color32Color(229, 205, 137));

        image.Polygon(ink, 75, 69, 98, 67, 103, 98, 79, 103);
        image.Polygon(Color32Color(112, 73, 45), 78, 72, 95, 70, 99, 95, 81, 98);
        image.Line(80, 74, 82, 95, Color32Color(231, 206, 145));
        image.Line(88, 73, 91, 96, Color32Color(168, 130, 73));
        image.Disc(87, 83, 3, Color32Color(214, 180, 92));
        image.Line(84, 83, 91, 83, Color32Color(242, 221, 160));
        image.Line(87, 79, 87, 87, Color32Color(242, 221, 160));

        for (int y = 54; y < 111; y++)
        {
            for (int x = 35; x < 67; x++)
            {
                if (image.Get(x, y).a > 0 && Hash(x, y, 83) % 59 == 0)
                {
                    image.Set(x, y, Color32Color(247, 242, 220));
                }
            }
        }

        return CreateTexture(image, "Mender Pixel Sprite RGB4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreateCreatureSprite(bool boss)
    {
        PixelSurface image = new PixelSurface(160, 192, new Color32(0, 0, 0, 0));
        Color32 ink = Color32Color(10, 16, 28);
        if (boss)
        {
            image.Disc(81, 168, 43, new Color32(10, 20, 37, 155));
            image.Polygon(ink, 39, 63, 52, 37, 76, 29, 101, 39, 124, 65, 130, 117, 115, 156, 99, 173, 57, 169, 35, 147, 26, 112);
            image.Polygon(Color32Color(34, 48, 70), 43, 66, 57, 43, 78, 36, 98, 44, 118, 68, 122, 115, 108, 151, 95, 164, 60, 160, 42, 142, 33, 111);
            image.Polygon(Color32Color(63, 81, 103), 52, 68, 70, 47, 91, 49, 107, 72, 103, 109, 91, 140, 65, 143, 49, 120);
            image.Polygon(Color32Color(27, 37, 56), 36, 72, 19, 51, 15, 31, 31, 44, 50, 62);
            image.Polygon(Color32Color(27, 37, 56), 112, 66, 134, 46, 144, 27, 149, 54, 126, 81);
            image.Polygon(Color32Color(19, 31, 53), 52, 41, 43, 20, 55, 27, 66, 34);
            image.Polygon(Color32Color(19, 31, 53), 96, 39, 110, 18, 114, 40, 105, 53);
            image.Polygon(Color32Color(20, 48, 63), 30, 94, 20, 77, 30, 72, 42, 85);
            image.Polygon(Color32Color(20, 48, 63), 120, 91, 138, 75, 142, 84, 127, 105);
            image.Polygon(Color32Color(25, 39, 59), 52, 75, 70, 66, 94, 70, 110, 92, 98, 126, 73, 137, 53, 116);
            image.Polygon(Color32Color(147, 85, 51), 64, 83, 88, 78, 100, 95, 93, 120, 71, 125, 57, 106);
            image.Polygon(Color32Color(226, 165, 79), 69, 89, 88, 86, 95, 98, 89, 114, 74, 117, 65, 104);
            image.Disc(58, 65, 5, Color32Color(235, 94, 68));
            image.Disc(101, 66, 5, Color32Color(235, 94, 68));
            image.Disc(59, 65, 2, Color32Color(255, 225, 159));
            image.Disc(102, 66, 2, Color32Color(255, 225, 159));
            image.Line(48, 53, 59, 59, Color32Color(112, 133, 150));
            image.Line(100, 56, 112, 48, Color32Color(112, 133, 150));
            image.Line(51, 137, 66, 151, Color32Color(105, 126, 139));
            image.Line(104, 133, 96, 150, Color32Color(105, 126, 139));

            for (int i = 0; i < 36; i++)
            {
                int x = 48 + Hash(i, 7, 97) % 65;
                int y = 53 + Hash(i, 11, 101) % 88;
                if (image.Get(x, y).a > 0 && Hash(x, y, 103) % 2 == 0)
                {
                    image.Set(x, y, Color32Color(102, 124, 143));
                }
            }
        }
        else
        {
            image.Disc(82, 162, 32, new Color32(10, 20, 37, 150));
            image.Polygon(ink, 39, 74, 51, 45, 77, 34, 105, 47, 120, 73, 113, 124, 99, 154, 62, 159, 42, 140, 31, 110);
            image.Polygon(Color32Color(48, 89, 69), 44, 75, 56, 51, 78, 41, 100, 51, 112, 77, 106, 120, 94, 145, 64, 149, 47, 134, 37, 108);
            image.Polygon(Color32Color(84, 125, 88), 52, 78, 68, 56, 88, 56, 102, 78, 98, 115, 80, 137, 61, 128, 47, 107);
            image.Polygon(Color32Color(36, 70, 62), 42, 58, 29, 37, 51, 48, 60, 64);
            image.Polygon(Color32Color(36, 70, 62), 102, 57, 120, 35, 117, 59, 108, 71);
            image.Polygon(Color32Color(55, 77, 75), 52, 88, 104, 86, 103, 101, 51, 104);
            image.Disc(62, 82, 5, Color32Color(227, 191, 89));
            image.Disc(97, 82, 5, Color32Color(227, 191, 89));
            image.Disc(63, 82, 2, Color32Color(255, 230, 161));
            image.Disc(98, 82, 2, Color32Color(255, 230, 161));
            image.Polygon(Color32Color(171, 138, 65), 66, 108, 92, 106, 97, 125, 78, 137, 65, 125);
            image.Line(46, 121, 57, 141, Color32Color(110, 149, 101));
            image.Line(105, 117, 96, 140, Color32Color(110, 149, 101));

            for (int i = 0; i < 22; i++)
            {
                int x = 49 + Hash(i, 17, 107) % 52;
                int y = 62 + Hash(i, 23, 109) % 69;
                if (image.Get(x, y).a > 0 && Hash(x, y, 113) % 2 == 0)
                {
                    image.Set(x, y, Color32Color(155, 182, 118));
                }
            }
        }

        return CreateTexture(image, boss ? "Gloam Warden Pixel Sprite RGB4444" : "Mossling Pixel Sprite RGB4444",
            TextureFormat.RGBA4444);
    }

    public static Texture2D CreateEnemySprite(int spriteIndex, bool boss)
    {
        int region = Mathf.Clamp(boss ? spriteIndex - 16 : spriteIndex / 4, 0, 3);
        int archetype = boss ? spriteIndex - 16 : spriteIndex % 4;
        Color32[] bodyColors =
        {
            Color32Color(69, 139, 82), Color32Color(70, 138, 103),
            Color32Color(190, 80, 54), Color32Color(98, 69, 151)
        };
        Color32[] highlightColors =
        {
            Color32Color(144, 201, 105), Color32Color(139, 190, 126),
            Color32Color(248, 161, 70), Color32Color(173, 128, 222)
        };
        Color32[] shadowColors =
        {
            Color32Color(28, 62, 48), Color32Color(31, 75, 64),
            Color32Color(83, 37, 44), Color32Color(47, 35, 87)
        };
        Color32 ink = Color32Color(10, 16, 26);
        Color32 body = bodyColors[region];
        Color32 light = highlightColors[region];
        Color32 dark = shadowColors[region];
        Color32 eye = region == 2 ? Color32Color(255, 207, 112) : Color32Color(255, 95, 74);
        PixelSurface image = new PixelSurface(112, 144, new Color32(0, 0, 0, 0));
        image.Disc(56, 127, boss ? 43 : 34, new Color32(12, 20, 34, 145));

        if (archetype == 0)
        {
            image.Polygon(ink, 18, 94, 23, 72, 35, 55, 51, 49, 73, 51, 90, 65,
                96, 88, 88, 112, 72, 122, 38, 121, 22, 110);
            image.Polygon(dark, 23, 94, 28, 74, 39, 60, 54, 55, 72, 57, 84, 69,
                89, 89, 82, 107, 69, 115, 41, 115, 28, 106);
            image.Polygon(body, 30, 94, 34, 77, 43, 66, 56, 62, 71, 64, 80, 75,
                83, 90, 75, 103, 66, 109, 43, 108, 33, 103);
            image.Polygon(light, 40, 84, 47, 70, 60, 69, 72, 76, 75, 88, 67, 97, 50, 97, 41, 92);
            image.Polygon(ink, 30, 108, 26, 121, 39, 117, 45, 110);
            image.Polygon(ink, 68, 109, 78, 117, 91, 121, 83, 107);
            image.Disc(48, 88, 4, eye);
            image.Disc(67, 88, 4, eye);
            image.Rect(47, 87, 2, 4, Color32Color(255, 241, 202));
            image.Rect(66, 87, 2, 4, Color32Color(255, 241, 202));
            image.Line(49, 101, 64, 101, ink);
        }
        else if (archetype == 1)
        {
            image.Polygon(ink, 18, 81, 26, 62, 46, 53, 68, 56, 84, 67, 101, 63,
                102, 81, 93, 98, 95, 115, 85, 119, 78, 101, 67, 100, 63, 124,
                51, 125, 47, 101, 37, 105, 30, 123, 19, 121, 23, 100);
            image.Polygon(dark, 24, 79, 31, 66, 48, 59, 67, 62, 81, 73, 94, 70,
                95, 82, 86, 94, 87, 110, 79, 113, 72, 97, 65, 95, 60, 118,
                54, 119, 51, 97, 39, 100, 33, 116, 25, 115, 29, 97);
            image.Polygon(body, 31, 78, 39, 67, 54, 65, 70, 70, 79, 79, 73, 93, 56, 101, 39, 94);
            image.Polygon(light, 41, 74, 54, 69, 66, 74, 70, 81, 59, 86, 42, 84);
            image.Polygon(ink, 36, 69, 26, 52, 43, 61);
            image.Polygon(ink, 67, 67, 82, 53, 77, 74);
            image.Polygon(light, 70, 76, 91, 80, 101, 89, 84, 91, 71, 85);
            image.Disc(52, 81, 3, eye);
            image.Disc(68, 82, 3, eye);
            image.Line(36, 127, 31, 135, ink);
            image.Line(60, 122, 62, 136, ink);
            image.Line(82, 121, 88, 132, ink);
        }
        else if (archetype == 2)
        {
            image.Polygon(ink, 50, 65, 35, 58, 19, 39, 8, 30, 10, 53, 24, 78,
                40, 89, 49, 109, 63, 109, 69, 88, 87, 79, 102, 55, 104, 31,
                91, 41, 78, 59, 63, 65);
            image.Polygon(dark, 49, 68, 35, 62, 20, 44, 15, 37, 17, 54, 29, 75,
                43, 84, 49, 104, 61, 104, 66, 84, 84, 75, 96, 54, 98, 38,
                91, 46, 76, 64, 63, 68);
            image.Polygon(body, 43, 71, 48, 61, 59, 57, 69, 66, 68, 96,
                59, 110, 47, 103, 41, 86);
            image.Polygon(light, 49, 71, 58, 64, 64, 70, 63, 88, 55, 97, 48, 89);
            image.Polygon(ink, 46, 65, 38, 47, 50, 57);
            image.Polygon(ink, 65, 64, 75, 47, 73, 67);
            image.Disc(50, 78, 3, eye);
            image.Disc(64, 78, 3, eye);
            image.Line(52, 104, 44, 128, dark);
            image.Line(62, 105, 73, 127, dark);
        }
        else
        {
            image.Polygon(ink, 28, 103, 31, 75, 37, 64, 75, 64, 84, 78,
                83, 107, 74, 118, 61, 123, 39, 118);
            image.Polygon(dark, 33, 100, 36, 77, 42, 69, 70, 69, 78, 81,
                77, 104, 69, 113, 60, 117, 43, 113);
            image.Polygon(body, 39, 98, 41, 79, 47, 73, 67, 74, 73, 83, 71, 104, 63, 111, 46, 108);
            image.Polygon(light, 43, 83, 50, 75, 65, 77, 70, 84, 61, 89, 46, 88);
            image.Polygon(ink, 36, 76, 30, 55, 42, 64);
            image.Polygon(ink, 70, 74, 82, 54, 79, 77);
            image.Rect(45, 91, 23, 4, ink);
            image.Rect(49, 93, 4, 2, eye);
            image.Rect(62, 93, 4, 2, eye);
            image.Polygon(ink, 40, 109, 53, 110, 51, 133, 40, 135, 35, 128);
            image.Polygon(ink, 61, 109, 73, 108, 79, 127, 72, 135, 62, 132);
            image.Polygon(light, 83, 92, 98, 81, 101, 84, 90, 102);
            image.Line(93, 85, 103, 71, Color32Color(175, 177, 182));
        }

        if (boss)
        {
            image.Polygon(ink, 32, 55, 27, 30, 41, 40, 54, 44, 67, 39, 84, 25, 79, 54);
            image.Polygon(light, 37, 48, 34, 36, 43, 44, 55, 46, 68, 42, 77, 31, 73, 50);
            image.Line(30, 61, 17, 52, light);
            image.Line(82, 61, 96, 49, light);
            image.Disc(56, 52, 8, new Color32(light.r, light.g, light.b, 120));
        }

        int detailCount = boss ? 38 : 18;
        for (int i = 0; i < detailCount; i++)
        {
            int x = 32 + Hash(i, spriteIndex, 193) % 49;
            int y = 70 + Hash(spriteIndex, i, 197) % 40;
            if (image.Get(x, y).a > 0 && Hash(x, y, 199) % 2 == 0)
            {
                image.Set(x, y, light);
            }
        }

        return CreateTexture(image, "Original 16-bit Enemy " + spriteIndex + " RGBA4444",
            TextureFormat.RGBA4444);
    }

    public static Texture2D CreatePartyPortrait(int heroIndex)
    {
        PixelSurface image = new PixelSurface(96, 96, Color32Color(8, 15, 29));
        Color32[] backgrounds =
        {
            Color32Color(38, 64, 89), Color32Color(69, 42, 107),
            Color32Color(55, 79, 79), Color32Color(69, 30, 71)
        };
        Color32[] clothing =
        {
            Color32Color(111, 128, 142), Color32Color(98, 64, 145),
            Color32Color(215, 215, 194), Color32Color(57, 37, 78)
        };
        Color32[] accents =
        {
            Color32Color(213, 171, 87), Color32Color(179, 129, 225),
            Color32Color(207, 174, 104), Color32Color(216, 102, 142)
        };
        int index = Mathf.Clamp(heroIndex, 0, 3);
        image.Rect(2, 2, 92, 92, Color32Color(166, 188, 204));
        image.Rect(5, 5, 86, 86, backgrounds[index]);
        for (int y = 7; y < 89; y++)
        {
            int glint = Hash(index, y / 5, 211) % 9;
            image.Line(7, y, 88, y, Shade(backgrounds[index], glint));
        }
        image.Polygon(Color32Color(17, 20, 29), 17, 95, 22, 67, 33, 56, 64, 56, 77, 72, 82, 95);
        image.Polygon(clothing[index], 23, 94, 27, 70, 37, 61, 61, 61, 72, 74, 76, 94);
        image.Rect(44, 55, 11, 14, Color32Color(211, 166, 126));
        image.Polygon(Color32Color(24, 18, 22), 29, 21, 38, 12, 57, 12, 69, 24,
            68, 51, 58, 62, 36, 60, 28, 48);
        image.Polygon(Color32Color(215, 174, 137), 34, 25, 41, 18, 55, 18, 63, 28,
            62, 46, 55, 55, 40, 53, 33, 43);
        image.Rect(38, 36, 5, 3, Color32Color(31, 25, 29));
        image.Rect(55, 36, 5, 3, Color32Color(31, 25, 29));
        image.Line(43, 48, 54, 49, Color32Color(131, 70, 74));
        image.Rect(39, 31, 3, 2, Color32Color(249, 222, 188));
        image.Rect(57, 31, 3, 2, Color32Color(249, 222, 188));
        if (index == 0)
        {
            image.Polygon(Color32Color(55, 72, 91), 25, 26, 30, 14, 43, 7, 59, 9,
                71, 19, 68, 34, 57, 26, 42, 24, 31, 33);
            image.Rect(42, 19, 13, 4, accents[index]);
            image.Polygon(Color32Color(144, 44, 44), 44, 9, 51, 0, 58, 10, 55, 16, 47, 15);
        }
        else if (index == 1)
        {
            image.Polygon(Color32Color(38, 23, 64), 22, 26, 31, 11, 49, 5, 68, 14,
                75, 29, 67, 58, 60, 50, 59, 28, 42, 26, 31, 38);
            image.Line(28, 17, 39, 9, accents[index]);
            image.Disc(72, 60, 5, Color32Color(119, 201, 231));
        }
        else if (index == 2)
        {
            image.Polygon(Color32Color(220, 220, 203), 23, 27, 31, 14, 46, 8, 63, 15,
                71, 27, 64, 31, 54, 24, 39, 24, 29, 34);
            image.Line(43, 65, 47, 91, accents[index]);
            image.Rect(41, 70, 15, 3, accents[index]);
            image.Rect(47, 64, 4, 15, accents[index]);
        }
        else
        {
            image.Polygon(Color32Color(33, 20, 49), 20, 29, 26, 13, 43, 5,
                62, 9, 76, 23, 74, 56, 65, 66, 59, 30, 43, 25, 29, 40);
            image.Polygon(Color32Color(57, 34, 82), 26, 24, 35, 11, 51, 8,
                67, 17, 70, 31, 59, 23, 42, 22, 30, 34);
            image.Rect(36, 36, 8, 3, accents[index]);
            image.Rect(54, 36, 8, 3, accents[index]);
            image.Line(43, 49, 56, 47, Color32Color(75, 31, 61));
            image.Polygon(Color32Color(71, 42, 86), 28, 71, 43, 64,
                56, 64, 73, 76, 78, 95, 22, 95);
        }
        image.Line(7, 7, 28, 7, accents[index]);
        image.Line(68, 88, 88, 88, accents[index]);
        return CreateTexture(image, "High Resolution 16-bit Party Portrait " + index,
            TextureFormat.RGBA4444);
    }

    public static Texture2D CreateVillagerPortrait(int style)
    {
        int index = Mathf.Clamp(style, 0, 3);
        PixelSurface image = new PixelSurface(96, 96, Color32Color(9, 17, 29));
        Color32[] backgrounds =
        {
            Color32Color(70, 67, 59), Color32Color(51, 83, 68),
            Color32Color(72, 64, 100), Color32Color(54, 78, 93)
        };
        Color32[] robes =
        {
            Color32Color(116, 79, 53), Color32Color(95, 121, 79),
            Color32Color(85, 91, 137), Color32Color(103, 75, 120)
        };
        image.Rect(2, 2, 92, 92, Color32Color(164, 186, 200));
        image.Rect(5, 5, 86, 86, backgrounds[index]);
        for (int y = 7; y < 89; y++)
        {
            image.Line(7, y, 88, y, Shade(backgrounds[index], Hash(index, y / 4, 223) % 7));
        }
        image.Polygon(Color32Color(19, 20, 27), 14, 96, 22, 67, 35, 59, 62, 59, 75, 69, 83, 96);
        image.Polygon(robes[index], 21, 95, 26, 72, 37, 63, 60, 63, 71, 73, 77, 95);
        image.Rect(43, 56, 12, 15, Color32Color(195, 145, 112));
        image.Polygon(Color32Color(39, 30, 25), 28, 24, 35, 13, 51, 9, 67, 19,
            68, 48, 60, 59, 39, 58, 29, 45);
        image.Polygon(Color32Color(215, 174, 136), 34, 28, 40, 20, 54, 19,
            62, 28, 61, 45, 55, 53, 41, 52, 34, 43);
        image.Rect(38, 35, 5, 3, Color32Color(31, 25, 28));
        image.Rect(55, 35, 5, 3, Color32Color(31, 25, 28));
        image.Line(43, 47, 53, 47, Color32Color(132, 71, 75));
        if (index == 0)
        {
            image.Polygon(Color32Color(178, 172, 148), 31, 35, 35, 49, 42, 55,
                54, 54, 62, 44, 65, 35, 57, 40, 51, 44, 42, 43, 36, 38);
            image.Line(27, 25, 35, 13, Color32Color(198, 181, 148));
        }
        else if (index == 1)
        {
            image.Polygon(Color32Color(233, 229, 202), 27, 25, 36, 13, 52, 8,
                69, 17, 71, 33, 61, 27, 42, 24, 31, 35);
            image.Rect(43, 66, 10, 4, Color32Color(204, 182, 123));
        }
        else if (index == 2)
        {
            image.Polygon(Color32Color(49, 45, 68), 25, 27, 31, 13, 50, 8,
                68, 17, 75, 31, 60, 24, 40, 24, 29, 34);
            image.Rect(39, 68, 19, 5, Color32Color(196, 152, 76));
            image.Rect(47, 63, 4, 15, Color32Color(196, 152, 76));
        }
        else
        {
            image.Polygon(Color32Color(49, 39, 55), 24, 29, 31, 13, 49, 7,
                67, 17, 74, 29, 62, 25, 40, 23, 29, 37);
            image.Line(31, 19, 42, 11, Color32Color(183, 156, 209));
        }
        image.Line(7, 7, 30, 7, Color32Color(222, 196, 126));
        image.Line(66, 88, 88, 88, Color32Color(222, 196, 126));
        return CreateTexture(image, "High Resolution 16-bit Villager Portrait " + index,
            TextureFormat.RGBA4444);
    }

    public static Texture2D CreateDialogueFrame(int width, int height)
    {
        PixelSurface image = new PixelSurface(width, height, new Color32(0, 0, 0, 0));
        image.Rect(0, 1, width, height - 2, Color32Color(18, 30, 55));
        image.Rect(2, 0, width - 4, height, Color32Color(171, 190, 207));
        image.Rect(4, 2, width - 8, height - 4, Color32Color(13, 24, 47));

        for (int y = 4; y < height - 4; y++)
        {
            float progress = (y - 4f) / Mathf.Max(1f, height - 8f);
            Color32 panel = Mix(Color32Color(47, 71, 186), Color32Color(13, 19, 90), progress);
            for (int x = 5; x < width - 5; x++)
            {
                int shimmer = y < 6 ? 20 : Hash(x / 3, y, 127) % 5 - 2;
                image.Set(x, y, Shade(panel, shimmer));
            }
        }

        image.Line(5, 3, width - 6, 3, Color32Color(235, 240, 230));
        image.Line(4, 4, 4, height - 5, Color32Color(230, 238, 233));
        image.Line(5, height - 4, width - 6, height - 4, Color32Color(70, 104, 161));
        image.Line(width - 5, 4, width - 5, height - 5, Color32Color(44, 62, 111));
        image.Rect(7, 7, width - 14, 1, new Color32(130, 150, 231, 255));

        return CreateTexture(image, "16-bit Indigo Dialogue Frame RGBA4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreatePanelFrame(int width, int height)
    {
        PixelSurface image = new PixelSurface(width, height, new Color32(0, 0, 0, 0));
        image.Rect(0, 1, width, height - 2, Color32Color(13, 20, 37));
        image.Rect(2, 0, width - 4, height, Color32Color(163, 183, 197));
        image.Rect(4, 2, width - 8, height - 4, Color32Color(13, 23, 47));

        for (int y = 4; y < height - 4; y++)
        {
            float progress = (y - 4f) / Mathf.Max(1f, height - 8f);
            Color32 panel = Mix(Color32Color(25, 45, 135), Color32Color(8, 17, 69), progress);
            for (int x = 5; x < width - 5; x++)
            {
                image.Set(x, y, Shade(panel, Hash(x / 4, y / 3, 131) % 5 - 2));
            }
        }

        image.Line(5, 3, width - 6, 3, Color32Color(223, 232, 223));
        image.Line(4, 4, 4, height - 5, Color32Color(219, 230, 224));
        image.Line(5, height - 4, width - 6, height - 4, Color32Color(75, 100, 142));
        image.Line(width - 5, 4, width - 5, height - 5, Color32Color(43, 58, 104));
        image.Rect(6, 6, 2, height - 12, Color32Color(85, 117, 191));

        return CreateTexture(image, "16-bit Sapphire Panel RGBA4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreateButtonFrame(int width, int height, bool selected)
    {
        PixelSurface image = new PixelSurface(width, height, new Color32(0, 0, 0, 0));
        Color32 trim = selected ? Color32Color(249, 214, 126) : Color32Color(131, 163, 199);
        Color32 baseColor = selected ? Color32Color(42, 65, 152) : Color32Color(24, 41, 98);
        image.Rect(0, 1, width, height - 2, Color32Color(7, 12, 24));
        image.Rect(1, 0, width - 2, height, trim);
        image.Rect(3, 2, width - 6, height - 4, Color32Color(9, 18, 43));

        for (int y = 4; y < height - 4; y++)
        {
            float progress = (y - 4f) / Mathf.Max(1f, height - 8f);
            Color32 row = Mix(baseColor, Color32Color(7, 13, 46), progress);
            image.Line(4, y, width - 5, y, row);
        }

        image.Line(4, 3, width - 5, 3, Color32Color(218, 228, 222));
        image.Line(3, 4, 3, height - 5, Color32Color(196, 214, 216));
        return CreateTexture(image, selected ? "Selected 16-bit Button RGBA4444" : "16-bit Button RGBA4444",
            TextureFormat.RGBA4444);
    }

    public static Texture2D CreateGaugeFrame(int width, int height)
    {
        PixelSurface image = new PixelSurface(width, height, new Color32(0, 0, 0, 0));
        image.Rect(0, 1, width, height - 2, Color32Color(8, 12, 19));
        image.Rect(1, 0, width - 2, height, Color32Color(181, 194, 195));
        image.Rect(2, 2, width - 4, height - 4, Color32Color(31, 39, 54));
        image.Line(3, 1, width - 4, 1, Color32Color(239, 237, 205));
        return CreateTexture(image, "16-bit Gauge Frame RGBA4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreateGaugeFill(int width, int height, Color32 baseColor, Color32 highlight)
    {
        PixelSurface image = new PixelSurface(width, height, new Color32(0, 0, 0, 0));
        for (int y = 0; y < height; y++)
        {
            float progress = y / Mathf.Max(1f, height - 1f);
            Color32 row = Mix(highlight, baseColor, progress);
            image.Line(0, y, width - 1, y, row);
        }

        image.Line(0, 0, width - 1, 0, Color32Color(245, 237, 200));
        return CreateTexture(image, "16-bit Gradient Gauge RGBA4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreateFastForwardIcon()
    {
        PixelSurface image = new PixelSurface(28, 28, new Color32(0, 0, 0, 0));
        image.Rect(1, 2, 26, 24, Color32Color(7, 13, 34));
        image.Rect(2, 1, 24, 26, Color32Color(177, 197, 208));
        image.Rect(4, 3, 20, 22, Color32Color(30, 51, 154));
        image.Rect(5, 4, 18, 3, Color32Color(104, 130, 231));
        image.Polygon(Color32Color(242, 242, 223), 7, 7, 14, 14, 7, 21);
        image.Polygon(Color32Color(242, 242, 223), 14, 7, 21, 14, 14, 21);
        image.Line(2, 25, 25, 25, Color32Color(91, 116, 179));
        return CreateTexture(image, "16-bit Crystal Emblem RGBA4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreateEffectParticle(Color32 color)
    {
        PixelSurface image = new PixelSurface(18, 18, new Color32(0, 0, 0, 0));
        Color32 haze = new Color32(color.r, color.g, color.b, 75);
        Color32 bright = new Color32(
            ClampByte(color.r + (255 - color.r) / 2),
            ClampByte(color.g + (255 - color.g) / 2),
            ClampByte(color.b + (255 - color.b) / 2),
            255);
        image.Disc(9, 9, 7, haze);
        image.Polygon(color, 9, 1, 12, 7, 17, 9, 12, 11, 9, 17, 7, 11, 1, 9, 7, 7);
        image.Rect(8, 5, 3, 8, bright);
        image.Rect(5, 8, 8, 3, bright);
        return CreateTexture(image, "16-bit Pixel Spark RGBA4444", TextureFormat.RGBA4444);
    }

    public static Texture2D CreatePartyAttackEffect(int heroIndex)
    {
        int index = Mathf.Clamp(heroIndex, 0, 2);
        PixelSurface image = new PixelSurface(32, 32, new Color32(0, 0, 0, 0));
        if (index == 0)
        {
            image.Line(4, 24, 24, 4, Color32Color(250, 225, 153));
            image.Line(2, 22, 22, 2, Color32Color(255, 255, 231));
            image.Line(8, 28, 28, 8, Color32Color(180, 191, 204));
            image.Line(19, 3, 27, 3, Color32Color(255, 221, 125));
            image.Line(26, 4, 26, 11, Color32Color(255, 221, 125));
        }
        else if (index == 1)
        {
            image.Disc(16, 16, 13, new Color32(75, 164, 240, 65));
            image.Polygon(Color32Color(129, 211, 247), 16, 1, 20, 11, 31, 16,
                20, 20, 16, 31, 12, 20, 1, 16, 12, 12);
            image.Rect(14, 7, 5, 18, Color32Color(226, 251, 255));
            image.Rect(7, 14, 18, 5, Color32Color(226, 251, 255));
        }
        else
        {
            image.Polygon(Color32Color(242, 222, 158), 16, 3, 19, 12, 29, 16,
                19, 20, 16, 29, 12, 20, 3, 16, 12, 12);
            image.Rect(14, 6, 4, 19, Color32Color(255, 250, 208));
            image.Rect(6, 14, 20, 4, Color32Color(255, 250, 208));
            image.Line(9, 8, 23, 23, Color32Color(183, 126, 60));
            image.Line(23, 8, 9, 23, Color32Color(183, 126, 60));
        }
        return CreateTexture(image, "16-bit Party Attack Effect " + index, TextureFormat.RGBA4444);
    }

    private static Texture2D CreateTexture(PixelSurface image, string textureName, TextureFormat format)
    {
        Texture2D texture = new Texture2D(image.Width, image.Height, format, false, false);
        texture.name = textureName;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.DontSave;
        texture.SetPixels32(image.Pixels);
        texture.Apply(false, false);
        return texture;
    }

    private static Color32 GetTileColor(char type, int x, int y)
    {
        switch (type)
        {
            case 'g':
                return (x * 7 + y * 3) % 5 == 0
                    ? Color32Color(44, 99, 62)
                    : Color32Color(34, 83, 52);
            case 'f':
                return (x * 5 + y * 7) % 4 == 0
                    ? Color32Color(40, 100, 62)
                    : Color32Color(25, 72, 49);
            case 'c':
                return (x + y * 3) % 4 == 0
                    ? Color32Color(77, 87, 105)
                    : Color32Color(49, 61, 79);
            case 'v':
                return (x * 3 + y * 5) % 5 == 0
                    ? Color32Color(97, 54, 112)
                    : Color32Color(55, 35, 77);
            case 'R':
                return Color32Color(55, 63, 78);
            case 'L':
                return Color32Color(106, 50, 39);
            case 'T':
                return Color32Color(20, 55, 42);
            case '.':
                return Color32Color(112, 93, 68);
            case '=':
                return Color32Color(126, 80, 46);
            case '~':
                return Color32Color(26, 73, 116);
            case 'H':
                return Color32Color(96, 62, 47);
            case 'S':
                return Color32Color(70, 75, 96);
            default:
                return Color32Color(64, 72, 78);
        }
    }

    private static void PaintTileDetails(PixelSurface image, char type, int tileX, int tileY,
        int originX, int originY, int tileSize)
    {
        switch (type)
        {
            case 'g':
            case 'f':
                for (int i = 0; i < 4; i++)
                {
                    int x = 4 + Hash(tileX, tileY + i, 149) % (tileSize - 8);
                    int y = 4 + Hash(tileX + i, tileY, 151) % (tileSize - 8);
                    Color32 blade = type == 'f' ? Color32Color(83, 157, 90) : Color32Color(105, 144, 76);
                    image.Set(originX + x, originY + y, blade);
                    image.Set(originX + x + 1, originY + y - 1,
                        type == 'f' ? Color32Color(55, 131, 77) : Color32Color(74, 126, 69));
                    image.Set(originX + x - 1, originY + y + 1,
                        type == 'f' ? Color32Color(63, 132, 71) : Color32Color(73, 122, 65));
                }
                break;
            case 'c':
                image.Line(originX + 5, originY + 7, originX + 13, originY + 4,
                    Color32Color(114, 131, 146));
                image.Line(originX + 16, originY + 19, originX + 24, originY + 15,
                    Color32Color(38, 47, 66));
                image.Rect(originX + 7, originY + 21, 3, 2, Color32Color(142, 164, 172));
                break;
            case 'v':
                image.Line(originX + 3, originY + 8, originX + 13, originY + 5,
                    Color32Color(129, 80, 150));
                image.Line(originX + 14, originY + 24, originX + 25, originY + 20,
                    Color32Color(37, 28, 56));
                image.Rect(originX + 8, originY + 14, 2, 2, Color32Color(193, 94, 136));
                break;
            case 'R':
                image.Polygon(Color32Color(33, 40, 54), originX + 3, originY + 20,
                    originX + 9, originY + 5, originX + 18, originY + 3,
                    originX + 26, originY + 16, originX + 22, originY + 25,
                    originX + 8, originY + 26);
                image.Line(originX + 9, originY + 7, originX + 15, originY + 6,
                    Color32Color(120, 127, 140));
                break;
            case 'L':
                image.Line(originX + 3, originY + 8, originX + 25, originY + 8,
                    Color32Color(244, 123, 54));
                image.Line(originX + 7, originY + 19, originX + 27, originY + 18,
                    Color32Color(218, 84, 44));
                image.Rect(originX + 12, originY + 11, 5, 3, Color32Color(255, 189, 74));
                break;
            case 'T':
                image.Polygon(Color32Color(10, 37, 31), originX + 4, originY + 18,
                    originX + 8, originY + 7, originX + 15, originY + 2, originX + 24, originY + 8,
                    originX + 26, originY + 19, originX + 21, originY + 24, originX + 8, originY + 23);
                image.Polygon(Color32Color(29, 85, 52), originX + 8, originY + 16,
                    originX + 11, originY + 6, originX + 18, originY + 4, originX + 24, originY + 11,
                    originX + 21, originY + 19, originX + 11, originY + 21);
                image.Rect(originX + 13, originY + 20, 5, 8, Color32Color(94, 61, 37));
                image.Rect(originX + 10, originY + 7, 3, 2, Color32Color(64, 124, 71));
                break;
            case '.':
                for (int i = 0; i < 5; i++)
                {
                    int x = 3 + Hash(tileX, i, 157) % (tileSize - 7);
                    int y = 3 + Hash(i, tileY, 163) % (tileSize - 7);
                    image.Rect(originX + x, originY + y, 3, 2,
                        i % 2 == 0 ? Color32Color(139, 119, 88) : Color32Color(86, 74, 58));
                }
                break;
            case '=':
                for (int y = 4; y < tileSize; y += 7)
                {
                    image.Line(originX + 2, originY + y, originX + tileSize - 3, originY + y,
                        Color32Color(76, 46, 29));
                    image.Rect(originX + 3, originY + y + 1, tileSize - 6, 1, Color32Color(157, 107, 65));
                }
                image.Line(originX + 9, originY + 3, originX + 9, originY + tileSize - 3,
                    Color32Color(85, 53, 33));
                image.Line(originX + 20, originY + 3, originX + 20, originY + tileSize - 3,
                    Color32Color(85, 53, 33));
                break;
            case '~':
                for (int i = 0; i < 3; i++)
                {
                    int y = 4 + Hash(tileX, tileY + i, 167) % (tileSize - 8);
                    int x = 3 + Hash(tileX + i, tileY, 173) % 14;
                    image.Line(originX + x, originY + y, originX + x + 8, originY + y,
                        Color32Color(68, 137, 178));
                    image.Line(originX + x + 3, originY + y + 1, originX + x + 10, originY + y + 1,
                        Color32Color(40, 101, 147));
                }
                break;
            case 'H':
                if (tileY == 4)
                {
                    image.Rect(originX + 2, originY + 4, tileSize - 4, 19, Color32Color(152, 110, 71));
                    image.Line(originX + 2, originY + 4, originX + tileSize - 3, originY + 4,
                        Color32Color(203, 156, 95));
                    image.Rect(originX + 8, originY + 12, 5, 11, Color32Color(75, 50, 38));
                    image.Rect(originX + 19, originY + 12, 5, 5, Color32Color(214, 180, 107));
                }
                else
                {
                    image.Polygon(Color32Color(62, 33, 38), originX + 1, originY + 14,
                        originX + 14, originY + 3, originX + 28, originY + 14,
                        originX + 27, originY + 20, originX + 2, originY + 20);
                    image.Rect(originX + 4, originY + 17, tileSize - 8, 9, Color32Color(133, 78, 51));
                    image.Line(originX + 5, originY + 18, originX + 23, originY + 18,
                        Color32Color(180, 111, 69));
                    image.Rect(originX + 12, originY + 20, 5, 6, Color32Color(66, 45, 36));
                }
                break;
            case 'S':
                image.Rect(originX + 5, originY + 5, 19, 19, Color32Color(106, 113, 133));
                image.Rect(originX + 9, originY + 7, 11, 15, Color32Color(172, 177, 183));
                image.Rect(originX + 13, originY + 9, 4, 12, Color32Color(69, 78, 99));
                image.Rect(originX + 4, originY + 22, 22, 4, Color32Color(207, 176, 103));
                image.Rect(originX + 11, originY + 4, 8, 3, Color32Color(211, 190, 131));
                break;
            default:
                image.Rect(originX + 3, originY + 3, tileSize - 6, tileSize - 6,
                    Color32Color(76, 83, 87));
                image.Line(originX + 6, originY + 7, originX + 12, originY + 5,
                    Color32Color(118, 128, 128));
                image.Line(originX + 17, originY + 21, originX + 23, originY + 18,
                    Color32Color(44, 53, 58));
                break;
        }
    }

    private static Color32 Mix(Color32 from, Color32 to, float t)
    {
        t = Mathf.Clamp01(t);
        return new Color32(
            (byte)Mathf.RoundToInt(Mathf.Lerp(from.r, to.r, t)),
            (byte)Mathf.RoundToInt(Mathf.Lerp(from.g, to.g, t)),
            (byte)Mathf.RoundToInt(Mathf.Lerp(from.b, to.b, t)),
            (byte)Mathf.RoundToInt(Mathf.Lerp(from.a, to.a, t)));
    }

    private static Color32 Shade(Color32 color, int amount)
    {
        return new Color32(
            ClampByte(color.r + amount),
            ClampByte(color.g + amount),
            ClampByte(color.b + amount),
            color.a);
    }

    private static Color32 Color32Color(int red, int green, int blue)
    {
        return new Color32(ClampByte(red), ClampByte(green), ClampByte(blue), 255);
    }

    private static byte ClampByte(int value)
    {
        return (byte)Mathf.Clamp(value, 0, 255);
    }

    private static int Mod(int value, int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }

    private static int Hash(int x, int y, int seed)
    {
        unchecked
        {
            uint value = (uint)(x * 374761393 + y * 668265263 + seed * 214748);
            value = (value ^ (value >> 13)) * 1274126177u;
            return (int)((value ^ (value >> 16)) & 0x7fffffffu);
        }
    }
}
