// l4d2External/EntityManager.cs (RESTAURADO Y CORREGIDO)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using l4d2External;

namespace left4dead2Menu
{
    internal class EntityManager
    {
        private readonly GameMemory memory;
        private readonly Offsets offsets;
        private readonly Encoding encoding;

        // Diccionario para persistir la vida máxima. La clave es la dirección de memoria de la entidad.
        private readonly Dictionary<IntPtr, int> maxHealthStorage = new Dictionary<IntPtr, int>();

        private const int MAX_ENTITIES = 900;
        private const int ENTITY_LOOP_STRIDE = 0x10;

        public EntityManager(GameMemory memory, Offsets offsets, Encoding encoding)
        {
            this.memory = memory;
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

            localPlayer.address = memory.ReadPointer(clientModuleBase, offsets.localplayer);
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
            var activeEntities = new HashSet<IntPtr>();

            for (int i = 0; i < MAX_ENTITIES; i++)
            {
                IntPtr currentEntityAddress = memory.ReadPointer(entityListBase, i * ENTITY_LOOP_STRIDE);
                if (currentEntityAddress == IntPtr.Zero || currentEntityAddress == localPlayer.address)
                {
                    continue;
                }

                Entity currentEntity = new Entity { address = currentEntityAddress };
                UpdateSingleEntityProperties(currentEntity, localPlayer.origin);

                // Si la entidad no es válida según la lógica original, se ignora
                if (string.IsNullOrEmpty(currentEntity.modelName))
                {
                    continue;
                }

                activeEntities.Add(currentEntity.address); // Añadir a la lista de entidades activas esta ronda

                // Lógica de Max Health Corregida
                // Si la entidad no está en nuestro diccionario, la añadimos.
                // Su vida actual se considera la vida máxima.
                if (!maxHealthStorage.ContainsKey(currentEntity.address))
                {
                    // Asegurarse de que la vida tenga un valor lógico antes de asignarla como máxima
                    if (currentEntity.health > 0 && currentEntity.health <= 10000)
                        maxHealthStorage[currentEntity.address] = currentEntity.health;
                }

                // Asignar la vida máxima desde el diccionario. Si no existe, usará el valor por defecto de la entidad.
                if (maxHealthStorage.TryGetValue(currentEntity.address, out int maxHealth))
                {
                    currentEntity.maxHealth = maxHealth;
                }

                // Clasificar la entidad en su lista correspondiente
                if (!currentEntity.modelName.StartsWith("DEBUG"))
                {
                    string model = currentEntity.modelName.ToLower();

                    if (model.Contains("survivor")) survivors.Add(currentEntity);
                    else if (model.Contains("hulk") || model.Contains("witch")) bossInfected.Add(currentEntity);
                    else if (model.Contains("charger") || model.Contains("jockey") || model.Contains("spitter") ||
                             model.Contains("hunter") || model.Contains("smoker") || model.Contains("boom"))
                    {
                        specialInfected.Add(currentEntity);
                    }
                    else if (model.Contains("infected")) commonInfected.Add(currentEntity);
                }
            }

            // Limpiar el diccionario de entidades que ya no existen en el juego
            var inactiveEntities = maxHealthStorage.Keys.Except(activeEntities).ToList();
            foreach (var inactiveAddress in inactiveEntities)
            {
                maxHealthStorage.Remove(inactiveAddress);
            }
        }

        // <<< MÉTODO RESTAURADO A LA VERSIÓN ORIGINAL DEL REPOSITORIO >>>
        private void UpdateSingleEntityProperties(Entity entity, Vector3 localPlayerOriginForMagnitude, bool isLocalPlayerUpdate = false)
        {
            entity.health = memory.ReadInt(entity.address, offsets.Health);
            entity.lifeState = memory.ReadInt(entity.address, offsets.Lifestate);

            bool isValid = true;
            if (entity.lifeState == 0 || entity.lifeState > 100000)
            {
                if (entity.health > 1)
                {
                    isValid = true;
                }
                else
                    isValid = false;
            }

            if (!isValid && !isLocalPlayerUpdate)
            {
                entity.modelName = null;
                return;
            }

            entity.origin = memory.ReadVec(entity.address, offsets.Origin);
            entity.viewOffset = memory.ReadVec(entity.address, offsets.ViewOffset);
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);
            entity.magnitude = MathUtils.CalculateMagnitude(entity.origin, localPlayerOriginForMagnitude);
            entity.modelName = null;
            entity.SimpleName = "Desconocido";
            entity.BonePositions = null;
            entity.TeamNum = memory.ReadInt(entity.address, offsets.TeamNum);

            try
            {
                IntPtr ptrToObject = memory.ReadPointer(entity.address, offsets.ModelName);
                if (ptrToObject != IntPtr.Zero)
                {
                    byte[] buffer = memory.ReadBytes(ptrToObject + 0x04, 250);
                    entity.modelName = encoding.GetString(buffer).Split('\0')[0];
                }

                if (string.IsNullOrEmpty(entity.modelName))
                {
                    entity.modelName = null;
                    return;
                }

                string model = entity.modelName.ToLower();
                if (model.Contains("survivor")) entity.SimpleName = "Superviviente";
                else if (model.Contains("witch")) entity.SimpleName = "Witch";
                else if (model.Contains("hulk")) entity.SimpleName = "Tank";
                else if (model.Contains("smoker")) entity.SimpleName = "Smoker";
                else if (model.Contains("hunter")) entity.SimpleName = "Hunter";
                else if (model.Contains("jockey")) entity.SimpleName = "Jockey";
                else if (model.Contains("boom")) entity.SimpleName = "Boomer";
                else if (model.Contains("spitter")) entity.SimpleName = "Spitter";
                else if (model.Contains("charger")) entity.SimpleName = "Charger";
                else if (model.Contains("infected")) entity.SimpleName = "Común";

                IntPtr boneMatrixPtr = memory.ReadPointer(entity.address, offsets.BoneMatrix);
                if (boneMatrixPtr != IntPtr.Zero && entity.SimpleName != null && ESP.SkeletonDefinitions.TryGetValue(entity.SimpleName, out _))
                {
                    if (ESP.ActiveBoneSets.TryGetValue(entity.SimpleName, out var boneSet) && boneSet.Count > 0)
                    {
                        int maxBoneIndex = boneSet.Max();
                        if (maxBoneIndex > 0 && maxBoneIndex < 128)
                        {
                            int bytesToRead = (maxBoneIndex + 1) * 48;
                            byte[] boneBytes = memory.ReadBytes(boneMatrixPtr, bytesToRead);
                            if (boneBytes != null && boneBytes.Length == bytesToRead)
                            {
                                entity.BonePositions = new Vector3[maxBoneIndex + 1];
                                for (int i = 0; i <= maxBoneIndex; i++)
                                {
                                    float x = BitConverter.ToSingle(boneBytes, i * 48 + 12);
                                    float y = BitConverter.ToSingle(boneBytes, i * 48 + 28);
                                    float z = BitConverter.ToSingle(boneBytes, i * 48 + 44);
                                    if (!float.IsNaN(x))
                                        entity.BonePositions[i] = new Vector3(x, y, z);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                entity.modelName = "ERR_NAME";
                entity.BonePositions = null;
            }
        }
    }
}