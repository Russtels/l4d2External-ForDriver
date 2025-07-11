// l4d2External/TriggerBot.cs
using l4d2External;
using System.Numerics;

namespace left4dead2Menu
{
    internal class TriggerBot
    {
        public bool IsEnabled { get; set; } = false;
        public float TriggerRadius { get; set; } = 10.0f; // Aumentado a un valor inicial más razonable
        public bool TriggerOnBosses { get; set; } = true;
        public bool TriggerOnSpecials { get; set; } = true;
        public bool TriggerOnCommons { get; set; } = false;
        public bool TriggerOnSurvivors { get; set; } = false;

        public void Update(
            Renderer renderer,
            float screenWidth,
            float screenHeight,
            List<Entity> allEntities)
        {
            if (!IsEnabled) return;

            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

            foreach (var entity in allEntities)
            {
                if (entity?.health <= 0 || entity.BonePositions == null || entity.BonePositions.Length == 0)
                {
                    continue;
                }

                bool isTargetable = false;
                switch (entity.SimpleName)
                {
                    case "Tank":
                    case "Witch":
                        if (TriggerOnBosses) isTargetable = true;
                        break;
                    case "Hunter":
                    case "Smoker":
                    case "Boomer":
                    case "Jockey":
                    case "Spitter":
                    case "Charger":
                        if (TriggerOnSpecials) isTargetable = true;
                        break;
                    case "Común":
                        if (TriggerOnCommons) isTargetable = true;
                        break;
                    case "Superviviente":
                        if (TriggerOnSurvivors) isTargetable = true;
                        break;
                }
                if (!isTargetable) continue;

                foreach (Vector3 bonePosition in entity.BonePositions)
                {
                    if (bonePosition == Vector3.Zero) continue;

                    if (renderer.WorldToScreen(bonePosition, out Vector2 boneScreenPos, screenWidth, screenHeight))
                    {
                        if (Vector2.Distance(screenCenter, boneScreenPos) < TriggerRadius)
                        {
                            NativeMethods.SimulateLeftClick();
                            return;
                        }
                    }
                }
            }
        }
    }
}