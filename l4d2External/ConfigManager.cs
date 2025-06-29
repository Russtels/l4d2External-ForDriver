// l4d2External/Config.cs (MODIFICADO)
using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace left4dead2Menu
{
    public class Config
    {
        // Aimbot
        public bool EnableAimbot { get; set; } = true;
        public float AimbotSmoothness { get; set; } = 0.1f;
        public AimbotTarget AimbotTargetSelection { get; set; } = AimbotTarget.Head;
        public bool AimbotOnBosses { get; set; } = true;
        public bool AimbotOnSpecials { get; set; } = true;
        public bool AimbotOnCommons { get; set; } = false;
        public bool AimbotOnSurvivors { get; set; } = false;
        public bool DrawFovCircle { get; set; } = true;
        public float FovCircleVisualRadius { get; set; } = 100.0f;
        public bool EnableAimbotArea { get; set; } = false;
        public float AimbotAreaRadius { get; set; } = 300.0f;
        public int AimbotAreaSegments { get; set; } = 40;
        public Vector4 AimbotAreaColor { get; set; } = new Vector4(1, 0, 1, 0.7f);

        // ESP
        public bool EnableEsp { get; set; } = true;
        public bool EspOnBosses { get; set; } = true;
        public bool EspOnSpecials { get; set; } = true;
        public bool EspOnCommons { get; set; } = true;
        public bool EspOnSurvivors { get; set; } = true;
        public bool EspDrawSkeleton { get; set; } = true;
        public bool EspDrawHeadBox { get; set; } = true;

        // Colores Personalizados ESP
        public Vector4 ColorEspBoxFill { get; set; } = new Vector4(0, 0, 0, 0.3f);
        public Vector4 ColorEspBoxBorder { get; set; } = new Vector4(1, 1, 1, 1);
        public Vector4 ColorEspSkeletonFill { get; set; } = new Vector4(1, 1, 1, 1);
        public Vector4 ColorEspSkeletonBorder { get; set; } = new Vector4(0, 0, 0, 1);
        public Vector4 ColorEspNameFill { get; set; } = new Vector4(1, 1, 1, 1);
        public Vector4 ColorEspNameBorder { get; set; } = new Vector4(0, 0, 0, 1);
        public Vector4 ColorEspHeadFill { get; set; } = new Vector4(1, 0, 0, 1);
        public Vector4 ColorEspHeadBorder { get; set; } = new Vector4(0, 0, 0, 1);

        // <<< NUEVO: Colores Barra de Vida >>>
        public Vector4 ColorHealthBarFull { get; set; } = new Vector4(0, 1, 0, 1);       // Verde
        public Vector4 ColorHealthBarEmpty { get; set; } = new Vector4(1, 0, 0, 1);    // Rojo
        public Vector4 ColorHealthBarBackground { get; set; } = new Vector4(0, 0, 0, 0.7f); // Negro semitransparente

        // Others
        public bool EnableBunnyHop { get; set; } = true;
        public bool EnableMeleeArea { get; set; } = true;
        public float MeleeAreaRadius { get; set; } = 80.0f;
        public int MeleeAreaSegments { get; set; } = 40;
        public Vector4 MeleeAreaColor { get; set; } = new Vector4(0, 1, 1, 0.7f);
        public bool MeleeOnCommons { get; set; } = true;
        public bool MeleeOnHunter { get; set; } = true;
        public bool MeleeOnSmoker { get; set; } = true;
        public bool MeleeOnBoomer { get; set; } = true;
        public bool MeleeOnJockey { get; set; } = true;
        public bool MeleeOnSpitter { get; set; } = false;
        public bool MeleeOnCharger { get; set; } = false;
    }

    // El resto de la clase ConfigManager y el convertidor se mantienen igual
    public class ConfigManager
    {
        private static readonly string configPath = "config.json";
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new Vector4Converter() }
        };

        public static void SaveConfig(Config config)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configPath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la configuración: {ex.Message}");
            }
        }

        public static Config LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                return new Config();
            }

            try
            {
                string jsonString = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<Config>(jsonString, options) ?? new Config();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar la configuración: {ex.Message}");
                return new Config();
            }
        }
    }

    public class Vector4Converter : JsonConverter<Vector4>
    {
        public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
            Vector4 vec = new Vector4();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) return vec;
                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
                string? propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "X": vec.X = reader.GetSingle(); break;
                    case "Y": vec.Y = reader.GetSingle(); break;
                    case "Z": vec.Z = reader.GetSingle(); break;
                    case "W": vec.W = reader.GetSingle(); break;
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteNumber("W", value.W);
            writer.WriteEndObject();
        }
    }
}