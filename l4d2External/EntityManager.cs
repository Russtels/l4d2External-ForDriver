// EntityManager.cs (Tu versión funcional)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Swed32;
using l4d2External;

namespace left4dead2Menu
{
    internal class EntityManager
    {
        private readonly Swed swed;
        private readonly Offsets offsets;
        private readonly Encoding encoding;

        private const int MAX_ENTITIES = 900;
        private const int ENTITY_LOOP_STRIDE = 0x10;
        private const int FALLBACK_MODELNAME_POINTER_OFFSET = 0x10;

        public EntityManager(Swed swed, Offsets offsets, Encoding encoding)
        {
            this.swed = swed;
            this.offsets = offsets;
            this.encoding = encoding;
        }

        public void ReloadEntities(
            Entity localPlayer,
            List<Entity> commonInfected,
            List<Entity> specialInfected,
            List<Entity> bossInfected,
            List<Entity> survivors,
            IntPtr clientModuleBase)
        {
            commonInfected.Clear();
            specialInfected.Clear();
            bossInfected.Clear();
            survivors.Clear();

            localPlayer.address = swed.ReadPointer(clientModuleBase, offsets.localplayer);
            if (localPlayer.address != IntPtr.Zero)
            {
                UpdateSingleEntityProperties(localPlayer, Vector3.Zero, true);
            }

            if (localPlayer.address != IntPtr.Zero)
            {
                PopulateEntityLists(localPlayer, commonInfected, specialInfected, bossInfected, survivors, clientModuleBase);
            }
        }

        private void PopulateEntityLists(
            Entity localPlayer,
            List<Entity> commonInfected,
            List<Entity> specialInfected,
            List<Entity> bossInfected,
            List<Entity> survivors,
            IntPtr clientModuleBase)
        {
            IntPtr entityListBase = clientModuleBase + offsets.entityList;

            for (int i = 0; i < MAX_ENTITIES; i++)
            {
                Entity currentEntity = new Entity();
                currentEntity.address = swed.ReadPointer(entityListBase, i * ENTITY_LOOP_STRIDE);

                if (currentEntity.address == IntPtr.Zero || currentEntity.address == localPlayer.address)
                {
                    continue;
                }

                UpdateSingleEntityProperties(currentEntity, localPlayer.origin);
                if (currentEntity.lifeState <= 0 || currentEntity.lifeState >100)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(currentEntity.modelName) && !currentEntity.modelName.StartsWith("DEBUG"))
                {
                    string model = currentEntity.modelName.ToLower();

                    if (model.Contains("survivor"))
                    {
                        survivors.Add(currentEntity);
                    }
                    else if (model.Contains("hulk") || model.Contains("witch"))
                    {
                        bossInfected.Add(currentEntity);
                    }
                    else if (model.Contains("charger") || model.Contains("jockey") || model.Contains("spitter") ||
                             model.Contains("hunter") || model.Contains("smoker") || model.Contains("boomer"))
                    {
                        specialInfected.Add(currentEntity);
                    }
                    else if (model.Contains("infected/common"))
                    {
                        commonInfected.Add(currentEntity);
                    }
                }
            }
        }

        private void UpdateSingleEntityProperties(Entity entity, Vector3 localPlayerOriginForMagnitude, bool isLocalPlayerUpdate = false)
        {
            entity.lifeState = swed.ReadInt(entity.address, offsets.Lifestate);
            entity.health = swed.ReadInt(entity.address, offsets.Health);
            entity.origin = swed.ReadVec(entity.address, offsets.Origin);
            entity.viewOffset = swed.ReadVec(entity.address, offsets.ViewOffset);
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);
            entity.magnitude = MathUtils.CalculateMagnitude(entity.origin, localPlayerOriginForMagnitude);

            entity.modelName = null;
            try
            {
                IntPtr ptrToObject = swed.ReadPointer(entity.address, offsets.ModelName);
                if (ptrToObject != IntPtr.Zero)
                {
                    byte[] buffer = swed.ReadBytes(ptrToObject + 0x04, 120);
                    entity.modelName = encoding.GetString(buffer).Split('\0')[0];
                }

                if (string.IsNullOrEmpty(entity.modelName))
                {
                    IntPtr stringPtrFallback = swed.ReadPointer(entity.address, FALLBACK_MODELNAME_POINTER_OFFSET);
                    if (stringPtrFallback != IntPtr.Zero)
                    {
                        byte[] buffer = swed.ReadBytes(stringPtrFallback, 120);
                        entity.modelName = encoding.GetString(buffer).Split('\0')[0];
                    }
                }
            }
            catch { entity.modelName = "ERR_NAME"; }
        }
    }
}