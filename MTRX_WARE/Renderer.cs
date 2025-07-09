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
        private bool enableTeleport = false;
        private bool enableFlip = false;
        private bool enableDesync = false;
        private bool enableExtendedKnife = false;
        private bool enableBhop = false;
        private bool enableLineESP = true;
        private bool enableBoxESP = true;
        private bool enableSkeletonESP = true;
        private bool enableDistanceESP = true;
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

        float boneThickness = 20;

        ImDrawListPtr drawlist;

        private int switchTabs = 0;
        

        

        public void RenderMenu()
        {
            ImGui.Begin("MTRX-Ware.io");

            ImGui.PushStyleColor(ImGuiCol.WindowBg, windowBgColor);
            ImGui.PushStyleColor(ImGuiCol.TitleBg, headerColor);  
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, headerColor);  
            ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.22f, 0.22f, 0.22f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Text, textColor);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 10.0f);

            ImGui.PushStyleColor(ImGuiCol.CheckMark, checkboxColor);

            ImGui.PushStyleColor(ImGuiCol.FrameBg, checkboxInactiveColor);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.47058823529411764f, 0.47058823529411764f, 0.47058823529411764f, 0.9f)); 
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));

            ImGui.Columns(2, "menu_columns", true); 

            ImGui.SetColumnWidth(0, 150);
            ImGui.BeginGroup();
            if (ImGui.Button("Aimbot", new Vector2(130.0f, 30.0f)))
                switchTabs = 0;
            ImGui.Spacing();
            if (ImGui.Button("Visuals", new Vector2(130.0f, 30.0f)))
                switchTabs = 1;
            ImGui.Spacing();
            if (ImGui.Button("Exploits", new Vector2(130.0f, 30.0f)))
                switchTabs = 2;
            ImGui.Spacing();
            if (ImGui.Button("Misc", new Vector2(130.0f, 30.0f)))
                switchTabs = 3;
            ImGui.Spacing();
            if (ImGui.Button("Menu", new Vector2(130.0f, 30.0f)))
                switchTabs = 4;
            ImGui.EndGroup();

            
            ImGui.NextColumn();

            ImGui.BeginGroup();

            
            Vector2 windowPos = ImGui.GetWindowPos();
            float windowX = windowPos.X;
            float windowY = windowPos.Y;
            float columnX = ImGui.GetColumnOffset(1);
            float columnWidth = ImGui.GetColumnWidth(1);

            float boxX = windowX + columnX + 5.0f;
            float boxY = windowY + ImGui.GetCursorPosY();
            float boxWidth = columnWidth - 20.0f;
            float boxHeight = ImGui.GetTextLineHeightWithSpacing() * 14;

            
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(boxX, boxY),
                new Vector2(boxX + boxWidth, boxY + boxHeight),
                ImGui.ColorConvertFloat4ToU32(boxBgColor),
                10.0f,
                ImDrawFlags.RoundCornersAll
            );

            
            float xOffset = 20.0f;

            switch (switchTabs)
            {
                case 0: // AIMBOT
                    break;

                case 1: // VISUALS
                        
                    float yOffset = ImGui.GetCursorPosY() + 8;



                    float horizontalOffset = 190.0f;
                    Vector2 previewSize = new Vector2(200, 200);
                    Vector2 cursorStart = ImGui.GetCursorPos();

                    ImGui.SetCursorPos(new Vector2(cursorStart.X + horizontalOffset, cursorStart.Y));
                    ImGui.Text("ESP Preview:");

                    ImGui.SetCursorPos(new Vector2(cursorStart.X + horizontalOffset, cursorStart.Y + ImGui.GetTextLineHeightWithSpacing() + 5));

                    Vector2 previewPosition = ImGui.GetCursorScreenPos();

                    ImGui.GetWindowDrawList().AddRect(previewPosition, previewPosition + previewSize, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));

                    ImDrawListPtr previewDrawList = ImGui.GetWindowDrawList();

                    DrawESPPreview(previewDrawList, previewPosition, previewSize);

                    ImGui.Dummy(previewSize);



                    // ESP checkbox
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable ESP", ref enableESP);

                    // Box ESP checkbox
                    yOffset = ImGui.GetCursorPosY(); 
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Box ESP", ref enableBoxESP);

                    // Skeleton ESP checkbox
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Skeleton ESP", ref enableSkeletonESP);

                    // Line ESP checkbox
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Line ESP", ref enableLineESP);

                    // Name ESP checkbox
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Name ESP", ref enableNameESP);

                    // Distance ESP checkbox
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Distance ESP", ref enableDistanceESP);

                    // Team Color text
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Team Color");

                    // Team Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##teamcolor", ref teamColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf);

                    // Enemy Color text
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Enemy Color");

                    // Enemy Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##enemycolor", ref enemyColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf);

                    // Bone Color text
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Bone Color");

                    // Bone Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##bonecolor", ref boneColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf);

                    // Distance Color text
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Distance Color");

                    // Distance Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##distancecolor", ref distanceColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf);

                    break;

                case 2: // EXPLOITS
                    break;

                case 3: // MISC
                        // Bhop checkbox
                    yOffset = ImGui.GetCursorPosY() + 8;
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Bhop", ref enableBhop);
                    break;

                case 4: // menu
                    // Background Color picker
                    yOffset = ImGui.GetCursorPosY() + 8;
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Background Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##bgcolor", ref windowBgColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                    }

                    // Box Background Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Box Background Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##boxbgcolor", ref boxBgColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                    }

                    // Header Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Header Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##headercolor", ref headerColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                    }

                    // Button Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Button Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##buttoncolor", ref buttonColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                    }

                    // Text Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Text Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##textcolor", ref textColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                    }
                    break;
            }

            ImGui.EndGroup();
            ImGui.Columns(1);

            ImGui.PopStyleColor(7);
            ImGui.PopStyleVar();
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
            }

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
        }

        private void DrawESPPreview(ImDrawListPtr previewDrawList, Vector2 previewPosition, Vector2 previewSize)
        {
            List<Entity> previewEntities = new List<Entity>
    {
        new Entity { name = "Enemy1", team = 2, spotted = true, position2D = new Vector2(50, 50), viewPosition2D = new Vector2(50, 20) },
        
        new Entity { name = "Teammate1", team = 1, spotted = true, position2D = new Vector2(100, 150), viewPosition2D = new Vector2(100, 120) }
    };

            Entity dummyLocalPlayer = new Entity { position = new Vector3(0, 0, 0) };

            float scale = previewSize.X / 200.0f;

            foreach (var entity in previewEntities)
            {
                Vector2 scaledPosition2D = previewPosition + (entity.position2D * scale);
                Vector2 scaledViewPosition2D = previewPosition + (entity.viewPosition2D * scale);

                if (enableBoxESP)
                {
                    float entityHeight = scaledPosition2D.Y - scaledViewPosition2D.Y;
                    Vector2 rectTop = new Vector2(scaledViewPosition2D.X - entityHeight / 3, scaledViewPosition2D.Y);
                    Vector2 rectBottom = new Vector2(scaledViewPosition2D.X + entityHeight / 3, scaledPosition2D.Y);
                    Vector4 boxColor = entity.team == 1 ? teamColor : enemyColor;
                    previewDrawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
                }

                if (enableNameESP)
                {
                    previewDrawList.AddText(scaledViewPosition2D - new Vector2(0, 15), ImGui.ColorConvertFloat4ToU32(nameColor), entity.name);
                }

                if (enableDistanceESP)
                {
                    float distance = Vector3.Distance(dummyLocalPlayer.position, entity.position);
                    previewDrawList.AddText(scaledPosition2D + new Vector2(0, 5), ImGui.ColorConvertFloat4ToU32(distanceColor), $"{distance:F1}m");
                }

                if (enableLineESP)
                {
                    previewDrawList.AddLine(previewPosition + new Vector2(previewSize.X / 2, previewSize.Y), scaledPosition2D, ImGui.ColorConvertFloat4ToU32(enemyColor));
                }
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

            if (VisCheck)
                boxColor = entity.spotted == true ? boxColor : hiddenColor;

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
    }
}
