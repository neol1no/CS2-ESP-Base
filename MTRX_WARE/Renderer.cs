using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Http;
using System.IO;

namespace MTRX_WARE
{
    public class Renderer : Overlay
    {
        // render variables
        public Vector2 screenSize = new Vector2(1920, 1080);
        
        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        // Gui elements
        private bool enableESP = true;
        private bool VisCheck = true;
        private bool enableNameESP = true;
        private bool enableLineESP = true;
        private bool enableBoxESP = true;
        private bool enableSkeletonESP = true;
        private bool enableDistanceESP = true;
        private bool performanceMode = false; 
        private Vector4 enemyColor = new Vector4(1, 0, 0, 1); 
        private Vector4 hiddenColor = new Vector4(0, 0, 0, 1); 
        private Vector4 teamColor = new Vector4(0, 1, 0, 1); 
        private Vector4 nameColor = new Vector4(1, 1, 1, 1); 
        private Vector4 boneColor = new Vector4(1, 1, 1, 1); 
        private Vector4 distanceColor = new Vector4(1, 1, 1, 1); 

        Vector4 windowBgColor = new Vector4(0.2f, 0.2f, 0.2f, 0.8f);
        Vector4 boxBgColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        Vector4 headerColor = new Vector4(0.18f, 0.18f, 0.18f, 1.0f);
        Vector4 buttonColor = new Vector4(0.18f, 0.18f, 0.18f, 1.0f);
        Vector4 textColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        Vector4 checkboxColor = new Vector4(0.2f, 0.8f, 0.2f, 1.0f); 

        Vector4 checkboxInactiveColor = new Vector4(1.0f, 0.6f, 0.6f, 1.0f);

        // Menu Theme Colors
        private Vector4 sidebarBgColor = new Vector4(0.12f, 0.12f, 0.15f, 1.0f);
        private Vector4 contentBgColor = new Vector4(0.10f, 0.10f, 0.13f, 1.0f);
        private Vector4 titleGradientStart = new Vector4(0.54f, 0.17f, 0.89f, 1.0f); 
        private Vector4 titleGradientEnd = new Vector4(0.0f, 0.0f, 0.0f, 1.0f); 

        float boneThickness = 20;

        ImDrawListPtr drawlist;

        private int switchTabs = 0;
        
        private ImFontPtr mainFont;
        private bool fontsLoaded = false;

        private readonly string FontsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts");
        private readonly string MainFontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts", "Roboto-Regular.ttf");
        private readonly string IconFontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts", "fa-solid-900.ttf");

        private const string MainFontUrl = "https://raw.githubusercontent.com/arielm/Unicode/master/fonts/Roboto-Regular.ttf";
        private const string IconFontUrl = "https://raw.githubusercontent.com/FortAwesome/Font-Awesome/6.x/webfonts/fa-solid-900.ttf";

        public Renderer()
        {
            // Font downloading moved to Program.cs before initialization
        }

        public static async Task CheckAndDownloadFonts()
        {
            string FontsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts");
            string MainFontPath = Path.Combine(FontsFolder, "Roboto-Regular.ttf");
            string IconFontPath = Path.Combine(FontsFolder, "fa-solid-900.ttf");
            
            var mainFontUrl = MainFontUrl;
            const string IconFontUrl = "https://raw.githubusercontent.com/FortAwesome/Font-Awesome/6.x/webfonts/fa-solid-900.ttf";
            
            if (!Directory.Exists(FontsFolder))
            {
                Directory.CreateDirectory(FontsFolder);
            }

            using (HttpClient client = new HttpClient())
            {
                if (!File.Exists(MainFontPath))
                {
                    Console.WriteLine("Downloading Main Font...");
                    var data = await client.GetByteArrayAsync(mainFontUrl);
                    await File.WriteAllBytesAsync(MainFontPath, data);
                }

                if (!File.Exists(IconFontPath))
                {
                    Console.WriteLine("Downloading Icon Font...");
                    var data = await client.GetByteArrayAsync(IconFontUrl);
                    await File.WriteAllBytesAsync(IconFontPath, data);
                }
            }
        }

        // Animation States
        private Dictionary<string, float> animationStates = new Dictionary<string, float>();

        private float GetAnimationState(string id, bool active, float speed = 0.1f)
        {
            if (!animationStates.ContainsKey(id))
                animationStates[id] = 0.0f;

            float target = active ? 1.0f : 0.0f;
            float current = animationStates[id];
            
            if (Math.Abs(current - target) > 0.001f)
            {
                current = Lerp(current, target, speed);
                animationStates[id] = current;
            }
            return current;
        }

        private void LoadFonts()
        {
            if (fontsLoaded) return;

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NoMouseCursorChange;

            ReplaceFont(MainFontPath, 16, FontGlyphRangeType.English);
            
            fontsLoaded = true;
        }
        

        

        public unsafe void RenderMenu()
        {
            ImGui.SetNextWindowSize(new Vector2(800, 500), ImGuiCond.FirstUseEver);
            ImGui.Begin("MTRX-WARE", ref isMenuVisible, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar);

            var drawList = ImGui.GetWindowDrawList();
            var p = ImGui.GetCursorScreenPos();
            var windowSize = ImGui.GetWindowSize();
            
            drawList.AddRectFilled(p, new Vector2(p.X + 200, p.Y + windowSize.Y), ImGui.ColorConvertFloat4ToU32(sidebarBgColor), 10.0f, ImDrawFlags.RoundCornersLeft);
            drawList.AddRectFilled(new Vector2(p.X + 200, p.Y), new Vector2(p.X + windowSize.X, p.Y + windowSize.Y), ImGui.ColorConvertFloat4ToU32(contentBgColor), 10.0f, ImDrawFlags.RoundCornersRight);

            // Sidebar
            ImGui.BeginGroup();
            {
                ImGui.SetCursorPos(new Vector2(20, 30));
                
                var titlePos = ImGui.GetCursorScreenPos();
                ImGui.SetWindowFontScale(2.5f);
                RenderGradientText("MTRX", titlePos);
                ImGui.SetWindowFontScale(1.0f);
                ImGui.Dummy(new Vector2(0, 50));

                ImGui.SetCursorPosY(120);

                RenderSidebarButton("visuals", "Visuals", 0);
                RenderSidebarButton("menu", "Menu", 1);
                
                ImGui.SetCursorPos(new Vector2(20, windowSize.Y - 30));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.3f, 0.3f, 0.35f, 0.6f));
                ImGui.Text(GetVersionString());
                ImGui.PopStyleColor();
            }
            ImGui.EndGroup();

            // Content Area
            ImGui.SetCursorPos(new Vector2(220, 30));
            ImGui.BeginGroup();
            {
                if (switchTabs == 0) // Visuals
                {
                    ImGui.Text("Visual Settings");
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Columns(2, "visuals_columns", false);
                    
                    ImGui.BeginGroup();
                    {
                        RenderToggle("Enable ESP", ref enableESP);
                        if (enableESP)
                        {
                            RenderToggle("Box ESP", ref enableBoxESP);
                            RenderToggle("Skeleton ESP", ref enableSkeletonESP);
                            RenderToggle("Line ESP", ref enableLineESP);
                            RenderToggle("Name ESP", ref enableNameESP);
                            RenderToggle("Distance ESP", ref enableDistanceESP);
                            
                            ImGui.Spacing();
                            ImGui.Text("Colors");
                            RenderColorPicker("Team Color", ref teamColor);
                            RenderColorPicker("Enemy Color", ref enemyColor);
                            RenderColorPicker("Bone Color", ref boneColor);
                            RenderColorPicker("Name Color", ref nameColor);
                        }
                    }
                    ImGui.EndGroup();

                    ImGui.NextColumn();

                    ImGui.BeginGroup();
                    {
                        ImGui.Text("Preview");
                        var previewPos = ImGui.GetCursorScreenPos();
                        var previewSize = new Vector2(250, 300);
                        
                        drawList.AddRectFilled(previewPos, previewPos + previewSize, ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.10f, 1.0f)), 5.0f);
                        drawList.AddRect(previewPos, previewPos + previewSize, ImGui.ColorConvertFloat4ToU32(new Vector4(0.25f, 0.25f, 0.30f, 1.0f)), 5.0f);

                        DrawESPPreview(drawList, previewPos, previewSize);
                    }
                    ImGui.EndGroup();
                    
                    ImGui.Columns(1);
                }
                else if (switchTabs == 1) // Menu
                {
                    ImGui.Text("Menu Customization");
                    ImGui.Separator();
                    ImGui.Spacing();
                    
                    ImGui.Text("Performance");
                    ImGui.Spacing();
                    RenderToggle("Performance Mode", ref performanceMode);
                    ImGui.Text("(Adds delay to ESP loop to reduce CPU usage)");
                    
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();
                    
                    ImGui.Text("Theme Colors");
                    ImGui.Spacing();
                    
                    RenderColorPicker("Sidebar Background", ref sidebarBgColor);
                    RenderColorPicker("Content Background", ref contentBgColor);
                    ImGui.Spacing();
                    
                    ImGui.Text("Title Gradient");
                    RenderColorPicker("Start Color", ref titleGradientStart);
                    RenderColorPicker("End Color", ref titleGradientEnd);
                    
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();
                    
                    if (ImGui.Button("Unload / Exit", new Vector2(150, 40)))
                    {
                        Environment.Exit(0);
                    }
                }
            }
            ImGui.EndGroup();

            ImGui.End();
        }

        private unsafe void RenderSidebarButton(string iconType, string label, int tabIndex)
        {
            bool isSelected = switchTabs == tabIndex;
            var p = ImGui.GetCursorScreenPos();
            var size = new Vector2(160, 45);
            
            bool hovered = ImGui.IsMouseHoveringRect(p, p + size);
            
            float t = GetAnimationState($"sidebar_{label}", isSelected || hovered);
            
            // Interpolate background color
            var baseColor = new Vector4(0,0,0,0);
            var activeColor = new Vector4(0.15f, 0.15f, 0.20f, 1.0f);
            var bgColor = Lerp(baseColor, activeColor, t);
            
            // Interpolate text color
            var baseText = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
            var activeText = new Vector4(0.54f, 0.17f, 0.89f, 1.0f);
            var textColor = Lerp(baseText, activeText, isSelected ? 1.0f : (hovered ? 0.5f : 0.0f));

            ImGui.GetWindowDrawList().AddRectFilled(p, p + size, ImGui.ColorConvertFloat4ToU32(bgColor), 5.0f);

            if (ImGui.InvisibleButton(label, size))
            {
                switchTabs = tabIndex;
            }

            RenderIcon(iconType, p + new Vector2(20, 22), 16.0f, textColor);

            ImGui.GetWindowDrawList().AddText(p + new Vector2(50, 14), ImGui.ColorConvertFloat4ToU32(textColor), label);

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        }

        private void RenderIcon(string type, Vector2 center, float size, Vector4 color)
        {
            var drawList = ImGui.GetWindowDrawList();
            uint col = ImGui.ColorConvertFloat4ToU32(color);
            
            if (type == "visuals") // Eye
            {
                drawList.AddCircle(center, size * 0.6f, col, 0, 1.5f);
                drawList.AddCircleFilled(center, size * 0.25f, col);
            }
            else if (type == "menu") // Cog
            {
                drawList.AddCircle(center, size * 0.6f, col, 0, 1.5f);
                drawList.AddCircle(center, size * 0.2f, col, 0, 1.5f);
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * (float)(Math.PI * 2 / 8);
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (size * 0.8f);
                    drawList.AddLine(center + offset * 0.6f, center + offset, col, 2.0f);
                }
            }
        }

        private void RenderToggle(string label, ref bool value)
        {
            // Custom toggle rendering
            var p = ImGui.GetCursorScreenPos();
            var drawList = ImGui.GetWindowDrawList();
            var height = 24.0f;
            var width = 45.0f;
            var radius = height * 0.50f;

            ImGui.InvisibleButton(label, new Vector2(width, height));
            if (ImGui.IsItemClicked())
                value = !value;

            float t = GetAnimationState($"toggle_{label}", value, 0.15f);
            
            var trailColor = new Vector4(0.08f, 0.08f, 0.10f, 1.0f);
            
            var colCircleOff = new Vector4(0.5f, 0.5f, 0.5f, 1.0f); 
            var colCircleOn = new Vector4(0.54f, 0.17f, 0.89f, 1.0f); 
            var colCircle = Lerp(colCircleOff, colCircleOn, t);

            drawList.AddRectFilled(p, new Vector2(p.X + width, p.Y + height), ImGui.ColorConvertFloat4ToU32(trailColor), radius);
            drawList.AddCircleFilled(new Vector2(p.X + radius + t * (width - height), p.Y + radius), radius - 2.0f, ImGui.ColorConvertFloat4ToU32(colCircle));

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            ImGui.Text(label);
            ImGui.Spacing();
        }

        private string GetVersionString()
        {
            byte[] data = { 109, 97, 100, 101, 32, 98, 121, 32, 110, 101, 111, 108, 49, 110, 111 };
            return System.Text.Encoding.ASCII.GetString(data);
        }

        private void RenderGradientText(string text, Vector2 pos)
        {
            var drawList = ImGui.GetWindowDrawList();
            float time = (float)DateTime.Now.TimeOfDay.TotalSeconds * 2.0f;
            
            float x = pos.X;
            float y = pos.Y;
            
            for (int i = 0; i < text.Length; i++)
            {
                float t = (float)Math.Sin(time + i * 0.5f) * 0.5f + 0.5f;
                
                var col = Lerp(titleGradientStart, titleGradientEnd, t);
                
                string charStr = text[i].ToString();
                drawList.AddText(new Vector2(x, y), ImGui.ColorConvertFloat4ToU32(col), charStr);
                
                var textSize = ImGui.CalcTextSize(charStr);
                x += textSize.X;
            }
        }

        private void RenderColorPicker(string label, ref Vector4 color)
        {
            ImGui.ColorEdit4($"##{label}", ref color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaPreviewHalf);
            ImGui.SameLine();
            ImGui.Text(label);
        }










        private DateTime lastInsertPressTime = DateTime.MinValue;

        private const float keyPressDelay = 1.0f;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetAsyncKeyState(int vKey);
        private bool isMenuVisible = false;
        private bool IsInsertKeyPressed()
        {
            const int VK_INSERT = 0x2D;
            short keyState = GetAsyncKeyState(VK_INSERT);
            if ((keyState & 0x8000) != 0)
            {
                DateTime currentTime = DateTime.Now;

                if ((currentTime - lastInsertPressTime).TotalSeconds >= keyPressDelay)
                {
                    lastInsertPressTime = currentTime; 
                    return true;
                }
            }

            return false;
        }

        protected override void Render()
        {
            if (!fontsLoaded)
            {
                LoadFonts();
                ApplyTheme();
            }

            if (IsInsertKeyPressed())
            {
                Console.WriteLine("Insert key pressed!");
                isMenuVisible = !isMenuVisible; 
            }

            if (isMenuVisible)
            {
                RenderMenu();
            }
            else
            {
                DrawOverlay(screenSize);
                
                drawlist = ImGui.GetWindowDrawList();

                if (enableESP)
                {
                    if (enableBoxESP)
                    {
                        foreach(var entity in entities)
                        {
                            if (EntityOnScreen(entity))
                            {
                                DrawBox(entity);
                            }
                        }
                    }
                    if (enableLineESP)
                    {
                        foreach (var entity in entities)
                        {
                            if (EntityOnScreen(entity))
                            {
                                DrawLine(entity);
                            }
                        }
                    }
                    if (enableNameESP)
                    {
                        foreach (var entity in entities)
                        {
                            if (EntityOnScreen(entity))
                            {
                                DrawName(entity, 20);
                            }
                        }
                    }
                    if (enableSkeletonESP)
                    {
                        foreach (var entity in entities)
                        {
                            if (EntityOnScreen(entity))
                            {
                                DrawBones(entity);
                            }
                        }
                    }
                   if (enableDistanceESP)
                    {
                        foreach (var entity in entities)
                        {
                            if (EntityOnScreen(entity) && entity != localPlayer)
                            {
                                entity.distance = Vector3.Distance(localPlayer.position, entity.position);
                                DrawDistance(localPlayer, entity); 
                            }
                        }
                    }
                }
                
                ImGui.End();
            }
        }

        private void DrawESPPreview(ImDrawListPtr previewDrawList, Vector2 previewPosition, Vector2 previewSize)
        {
            var center = previewPosition + previewSize / 2;
            
            var dummyPos2D = center + new Vector2(0, 50);
            var dummyHead2D = center - new Vector2(0, 50);
            var dummyHeight = dummyPos2D.Y - dummyHead2D.Y;
            var dummyWidth = dummyHeight / 3;

            // Box
            if (enableBoxESP)
            {
                var rectTop = new Vector2(dummyHead2D.X - dummyWidth, dummyHead2D.Y);
                var rectBottom = new Vector2(dummyHead2D.X + dummyWidth, dummyPos2D.Y);
                previewDrawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(enemyColor));
            }

            // Line
            if (enableLineESP)
            {
                previewDrawList.AddLine(new Vector2(previewPosition.X + previewSize.X / 2, previewPosition.Y + previewSize.Y), dummyPos2D, ImGui.ColorConvertFloat4ToU32(enemyColor));
            }

            // Name
            if (enableNameESP)
            {
                var text = "Enemy";
                var textSize = ImGui.CalcTextSize(text);
                previewDrawList.AddText(new Vector2(dummyHead2D.X - textSize.X / 2, dummyHead2D.Y - 20), ImGui.ColorConvertFloat4ToU32(nameColor), text);
            }

            // Distance
            if (enableDistanceESP)
            {
                var text = "15.0m";
                var textSize = ImGui.CalcTextSize(text);
                previewDrawList.AddText(new Vector2(dummyPos2D.X - textSize.X / 2, dummyPos2D.Y + 5), ImGui.ColorConvertFloat4ToU32(distanceColor), text);
            }

            if (enableSkeletonESP)
            {
                uint col = ImGui.ColorConvertFloat4ToU32(boneColor);
                previewDrawList.AddLine(dummyHead2D, dummyPos2D - new Vector2(0, dummyHeight * 0.4f), col, 2.0f);
                previewDrawList.AddLine(dummyHead2D + new Vector2(0, 10), dummyHead2D + new Vector2(-20, 30), col, 2.0f);
                previewDrawList.AddLine(dummyHead2D + new Vector2(0, 10), dummyHead2D + new Vector2(20, 30), col, 2.0f);
                previewDrawList.AddLine(dummyPos2D - new Vector2(0, dummyHeight * 0.4f), dummyPos2D + new Vector2(-15, 0), col, 2.0f);
                previewDrawList.AddLine(dummyPos2D - new Vector2(0, dummyHeight * 0.4f), dummyPos2D + new Vector2(15, 0), col, 2.0f);
                previewDrawList.AddCircle(dummyHead2D, 10.0f, col);
            }
        }

        bool EntityOnScreen(Entity entity)
        {
            if (entity.position2D.X > 0 && entity.position2D.X < screenSize.X && entity.position2D.Y > 0 && entity.position2D.Y < screenSize.Y)
            {
                return true;
            }
            return false;
        }


        private void DrawBones(Entity entity)
        {
            uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);
            float currentBoneThickness = 1 + boneThickness / entity.distance;

            drawlist.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
            drawlist.AddCircle(entity.bones2d[2], 8 + currentBoneThickness, uintColor);
        }

        public void DrawName(Entity entity, int yOffset)
        {
            Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.viewPosition2D.Y - yOffset);
            drawlist.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}");
        }

        public void DrawDistance(Entity localPlayer, Entity entity)
        {
            float distance = Vector3.Distance(localPlayer.position, entity.position);

            Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.position2D.Y + 5);

            drawlist.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(distanceColor), $"{distance:F1}m");
        }

        public void DrawBox(Entity entity)
        {
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.viewPosition2D.X + entityHeight / 3, entity.position2D.Y);

            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            drawlist.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }
        private void DrawLine(Entity entity)
        {   
            Vector4 lineColor = localPlayer.team == entity.team ? teamColor: enemyColor;
            drawlist.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(lineColor));
        }

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock(entityLock)
            {
                localPlayer = newEntity;
            }
        }

        public Entity GetLocalPlayer()
        {
            lock(entityLock) {
            return localPlayer;
            }
        }

        public bool GetPerformanceMode()
        {
            return performanceMode;
        }

        void DrawOverlay(Vector2 screenSize) 
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }
        private void ApplyTheme()
        {
            var style = ImGui.GetStyle();
            style.WindowRounding = 10.0f;
            style.FrameRounding = 5.0f;
            style.GrabRounding = 5.0f;
            style.PopupRounding = 5.0f;
            style.ScrollbarRounding = 5.0f;
            style.TabRounding = 5.0f;

            var colors = style.Colors;
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.10f, 0.10f, 0.13f, 0.95f); // Dark background
            colors[(int)ImGuiCol.Header] = new Vector4(0.54f, 0.17f, 0.89f, 1.0f); // Purple
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.60f, 0.20f, 0.95f, 1.0f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.54f, 0.17f, 0.89f, 1.0f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.15f, 0.15f, 0.20f, 1.0f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.20f, 0.20f, 0.25f, 1.0f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.54f, 0.17f, 0.89f, 1.0f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.15f, 0.15f, 0.20f, 1.0f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.20f, 0.20f, 0.25f, 1.0f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.54f, 0.17f, 0.89f, 1.0f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.54f, 0.17f, 0.89f, 1.0f);
            colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.25f, 0.25f, 0.30f, 0.5f);
            colors[(int)ImGuiCol.Separator] = new Vector4(0.25f, 0.25f, 0.30f, 0.5f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.10f, 0.10f, 0.13f, 1.0f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.10f, 0.10f, 0.13f, 1.0f);
        }

        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private Vector4 Lerp(Vector4 a, Vector4 b, float t)
        {
            return new Vector4(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t),
                Lerp(a.Z, b.Z, t),
                Lerp(a.W, b.W, t)
            );
        }
    }
}
