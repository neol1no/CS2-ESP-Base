using MTRX_WARE;
using Swed64;
using System.Numerics;
using System.Drawing;

// main logic

// console logic
ConsoleUtils.EnableVT();
Console.Clear();

string asciiArt = @"
  __  __ _______ _____  __   __       __          __     _____  ______ 
 |  \/  |__   __|  __ \ \ \ / /       \ \        / /\   |  __ \|  ____|
 | \  / |  | |  | |__) | \ V / ______  \ \  /\  / /  \  | |__) | |__   
 | |\/| |  | |  |  _  /   > < |______|  \ \/  \/ / /\ \ |  _  /|  __|  
 | |  | |  | |  | | \ \  / . \           \  /\  / ____ \| | \ \| |____ 
 |_|  |_|  |_|  |_|  \_\/_/ \_\           \/  \/_/    \_\_|  \_\______|
                                                                       
";

ConsoleUtils.WriteGradient(asciiArt, System.Drawing.Color.FromArgb(138, 43, 226), System.Drawing.Color.Red);

Console.ResetColor();
int windowWidth = Console.WindowWidth;
string copyright = "credit: github.com/neol1no";
Console.SetCursorPosition(windowWidth - copyright.Length - 2, Console.CursorTop);
Console.WriteLine(copyright);
Console.WriteLine();

Console.WriteLine("Select Product:");
Console.WriteLine("[1] mtrx-ware (esp-base)");
Console.Write("\nSelection: ");

var key = Console.ReadKey(true);
if (key.Key != ConsoleKey.D1 && key.Key != ConsoleKey.NumPad1)
{
    Console.WriteLine("\nInvalid selection. Exiting...");
    Thread.Sleep(2000);
    return;
}

Console.WriteLine("1");

await ConsoleUtils.DisplayLoadingScreen();

await OffsetManager.Load();

await Renderer.CheckAndDownloadFonts();

Console.WriteLine("Starting MTRX-WARE...");
Thread.Sleep(1000);
Console.Clear();

// init swed
Swed swed = new Swed("cs2");

// get client module 
IntPtr client = swed.GetModuleBase("client.dll");

// init render
Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();

// get screen size
Vector2 screenSize = renderer.screenSize;

// store entities
List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();
Entity currentEntity = new Entity();

// offsets

    // offsets.cs
    int dwEntityList = OffsetManager.Offsets.dwEntityList;
    int dwViewMatrix = OffsetManager.Offsets.dwViewMatrix;
    int dwLocalPlayerPawn = OffsetManager.Offsets.dwLocalPlayerPawn;

    // client_dll.cs
    int m_vOldOrigin = OffsetManager.Offsets.m_vOldOrigin;
    int m_iTeamNum = OffsetManager.Offsets.m_iTeamNum;
    int m_lifeState = OffsetManager.Offsets.m_lifeState;
    int m_hPlayerPawn = OffsetManager.Offsets.m_hPlayerPawn;
    int m_vecViewOffset = OffsetManager.Offsets.m_vecViewOffset;
    int m_iszPlayerName = OffsetManager.Offsets.m_iszPlayerName;
    int m_modelState = OffsetManager.Offsets.m_modelState;
    int m_pGameSceneNode = OffsetManager.Offsets.m_pGameSceneNode;
    int m_entitySpottedState = OffsetManager.Offsets.m_entitySpottedState;
    int m_bSpotted = OffsetManager.Offsets.m_bSpotted;

// ESP loop
while (true)
{
    entities.Clear();

    IntPtr entityList = swed.ReadPointer(client, dwEntityList);

    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    IntPtr localPlayerPawn = swed.ReadPointer(client, dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayerPawn, m_iTeamNum);

    for (int i = 0; i < 64; i++)
    {
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x70);
        if (currentController == IntPtr.Zero) continue;

        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
        if (pawnHandle == 0) continue;

        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        if (listEntry2 == IntPtr.Zero) continue;

        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x70 * (pawnHandle & 0x1FF));
        if (currentPawn == IntPtr.Zero) continue;

        int lifeState = swed.ReadInt(currentPawn, m_lifeState);
        if (lifeState != 256) continue;

        float[] viewMatrix = swed.ReadMatrix(client + dwViewMatrix);

        IntPtr sceneNode = swed.ReadPointer(currentPawn, m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, m_modelState + 0x80);

        Entity entity = new Entity();

        entity.name = swed.ReadString(currentController, m_iszPlayerName, 16).Split("\0")[0];
        entity.team = swed.ReadInt(currentPawn, m_iTeamNum);
        entity.spotted = swed.ReadBool(currentPawn, m_entitySpottedState + m_bSpotted);
        entity.position = swed.ReadVec(currentPawn, m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);

        localPlayer.position = swed.ReadVec(localPlayerPawn, m_vOldOrigin);

        foreach (var otherEntity in entities)
        {
            otherEntity.distance = Vector3.Distance(otherEntity.position, localPlayer.position);
        }

        entity.bones = Calculate.ReadBones(boneMatrix, swed);
        entity.bones2d = Calculate.ReadBones2d(entity.bones, viewMatrix, screenSize);

        entities.Add(entity);
    }


    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);

    // Performance mode
    if (renderer.GetPerformanceMode())
    {
        Thread.Sleep(1);
    }
}
