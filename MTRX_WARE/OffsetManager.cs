using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTRX_WARE
{
    public static class OffsetManager
    {
        public static class Offsets
        {
            // offsets.cs
            public static int dwEntityList;
            public static int dwViewMatrix;
            public static int dwLocalPlayerPawn;

            // client_dll.cs
            public static int m_vOldOrigin;
            public static int m_iTeamNum;
            public static int m_lifeState;
            public static int m_hPlayerPawn;
            public static int m_vecViewOffset;
            public static int m_iszPlayerName;
            public static int m_modelState;
            public static int m_pGameSceneNode;
            public static int m_entitySpottedState;
            public static int m_bSpotted;
        }

        private static readonly string OffsetsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "offsets");
        private static readonly string ClientDllPath = Path.Combine(OffsetsFolder, "client_dll.cs");
        private static readonly string OffsetsPath = Path.Combine(OffsetsFolder, "offsets.cs");

        private const string ClientDllUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/client_dll.cs";
        private const string OffsetsUrl = "https://raw.githubusercontent.com/a2x/cs2-dumper/main/output/offsets.cs";

        public static async Task<bool> CheckForUpdates()
        {
            if (!File.Exists(OffsetsPath)) return true;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string remoteContent = await client.GetStringAsync(OffsetsUrl);
                    string remoteDateLine = GetDateLine(remoteContent);

                    string localContent = await File.ReadAllTextAsync(OffsetsPath);
                    string localDateLine = GetDateLine(localContent);

                    if (string.IsNullOrEmpty(remoteDateLine) || string.IsNullOrEmpty(localDateLine))
                    {
                        return true; 
                    }

                    
                    return string.Compare(remoteDateLine, localDateLine) > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string GetDateLine(string content)
        {
            var match = Regex.Match(content, @"// \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");
            return match.Success ? match.Value : null;
        }

        public static async Task UpdateOffsets()
        {
            await FetchOffsets();
            ParseOffsets();
        }

        public static async Task Load()
        {
            if (!Directory.Exists(OffsetsFolder))
            {
                Directory.CreateDirectory(OffsetsFolder);
                await UpdateOffsets();
            }
            ParseOffsets();
        }

        public static void Init() 
        {
            Load();
        }

        private static async Task FetchOffsets()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string clientDllContent = await client.GetStringAsync(ClientDllUrl);
                    await File.WriteAllTextAsync(ClientDllPath, clientDllContent);
                    Console.WriteLine($"Downloaded client_dll.cs");

                    string offsetsContent = await client.GetStringAsync(OffsetsUrl);
                    await File.WriteAllTextAsync(OffsetsPath, offsetsContent);
                    Console.WriteLine($"Downloaded offsets.cs");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching offsets: {ex.Message}");
                }
            }
        }

        private static void ParseOffsets()
        {
            if (File.Exists(OffsetsPath))
            {
                string content = File.ReadAllText(OffsetsPath);

                foreach (var field in typeof(Offsets).GetFields())
                {
                    if (field.Name.StartsWith("dw"))
                    {

                        var match = Regex.Match(content, $@"public const nint {field.Name}\s*=\s*(0x[0-9A-Fa-f]+);");
                        if (match.Success)
                        {
                            int value = Convert.ToInt32(match.Groups[1].Value, 16);
                            field.SetValue(null, value);

                        }
                    }
                }
            }

            if (File.Exists(ClientDllPath))
            {
                string content = File.ReadAllText(ClientDllPath);
                
                foreach (var field in typeof(Offsets).GetFields())
                {
                    if (field.Name.StartsWith("m_"))
                    {
                        
                        var match = Regex.Match(content, $@"public const nint {field.Name}\s*=\s*(0x[0-9A-Fa-f]+);");
                        if (match.Success)
                        {
                            int value = Convert.ToInt32(match.Groups[1].Value, 16);
                            field.SetValue(null, value);
                        }
                    }
                }
            }
        }
    }
}
