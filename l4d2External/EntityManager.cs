// EntityManager.cs (Tu versión funcional)
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

        private const int MAX_ENTITIES =900;
        private const int ENTITY_LOOP_STRIDE = 0x10;
        private const int MAX_BONES = 128;

        public EntityManager(GameMemory memory, Offsets offsets, Encoding encoding) // Cambiado de Swed a GameMemory
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

            for (int i = 0; i < MAX_ENTITIES; i++)
            {
                Entity currentEntity = new Entity();
                currentEntity.address = memory.ReadPointer(entityListBase, i * ENTITY_LOOP_STRIDE);

                if (currentEntity.address == IntPtr.Zero || currentEntity.address == localPlayer.address)
                {
                    continue;
                }

                // Esta función ahora valida la entidad internamente.
                UpdateSingleEntityProperties(currentEntity, localPlayer.origin);

                // <<< CAMBIO CLAVE >>>
                // La validación de vida ahora está dentro de UpdateSingleEntityProperties.
                // Si la entidad no estaba viva, su modelName será nulo. Usamos eso como el nuevo check.
                if (string.IsNullOrEmpty(currentEntity.modelName))
                {
                    continue;
                }

                if (!currentEntity.modelName.StartsWith("DEBUG"))
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
                             model.Contains("hunter") || model.Contains("smoker") || model.Contains("boom"))
                    {
                        specialInfected.Add(currentEntity);
                    }
                    else if (model.Contains("infected"))
                    {
                        commonInfected.Add(currentEntity);
                    }
                }
            }
        }

        private void UpdateSingleEntityProperties(Entity entity, Vector3 localPlayerOriginForMagnitude, bool isLocalPlayerUpdate = false)
        {
            entity.lifeState = memory.ReadInt(entity.address, offsets.Lifestate);

            // <<< CAMBIO 1: FILTRO MÁS SIMPLE Y SEGURO >>>
            // Usamos solo la vida para validar. Es posible que el check de 'lifeState' anterior
            // estuviera filtrando a la Witch o a los Comunes incorrectamente.
            if (entity.lifeState == 0 || entity.lifeState >50)
            {
                entity.modelName = null;
                return;
            }

            // Leemos el resto de datos básicos solo si la entidad está viva.
            entity.lifeState = memory.ReadInt(entity.address, offsets.Lifestate);
            entity.origin = memory.ReadVec(entity.address, offsets.Origin);
            entity.viewOffset = memory.ReadVec(entity.address, offsets.ViewOffset);
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);
            entity.magnitude = MathUtils.CalculateMagnitude(entity.origin, localPlayerOriginForMagnitude);
            entity.modelName = null;
            entity.SimpleName = "Desconocido";
            entity.BonePositions = null;

            try
            {
                // Lectura de nombre
                IntPtr ptrToObject = memory.ReadPointer(entity.address, offsets.ModelName);
                if (ptrToObject != IntPtr.Zero)
                {
                    byte[] buffer = memory.ReadBytes(ptrToObject + 0x04, 250);
                    entity.modelName = encoding.GetString(buffer).Split('\0')[0];
                }

                if (!string.IsNullOrEmpty(entity.modelName))
                {
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
                }

                // Lectura de huesos
                IntPtr boneMatrixPtr = memory.ReadPointer(entity.address, offsets.BoneMatrix);

                // <<< CAMBIO 2: LÍNEA DE DIAGNÓSTICO >>>
                // Imprime el estado del puntero solo para las entidades que nos interesan.
                if (entity.SimpleName == "Witch" || entity.SimpleName == "Común")
                {
                    Console.WriteLine($"DIAGNÓSTICO: Entidad={entity.SimpleName}, Vida={entity.health}, Puntero Huesos={(boneMatrixPtr == IntPtr.Zero ? "NULO" : "VÁLIDO")}");
                }

                if (boneMatrixPtr != IntPtr.Zero)
                {
                    byte[] firstBoneBytes = memory.ReadBytes(boneMatrixPtr, 48);
                    if (firstBoneBytes != null && firstBoneBytes.Length == 48)
                    {
                        float x_test = BitConverter.ToSingle(firstBoneBytes, 12);
                        if (!float.IsNaN(x_test))
                        {
                            entity.BonePositions = new Vector3[MAX_BONES];
                            entity.BonePositions[0] = new Vector3(x_test, BitConverter.ToSingle(firstBoneBytes, 28), BitConverter.ToSingle(firstBoneBytes, 44));
                            for (int i = 1; i < MAX_BONES; i++)
                            {
                                byte[] singleBoneBytes = memory.ReadBytes(boneMatrixPtr + (i * 48), 48);
                                if (singleBoneBytes != null && singleBoneBytes.Length == 48)
                                {
                                    entity.BonePositions[i] = new Vector3(BitConverter.ToSingle(singleBoneBytes, 12), BitConverter.ToSingle(singleBoneBytes, 28), BitConverter.ToSingle(singleBoneBytes, 44));
                                }
                                else break;
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