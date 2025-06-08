using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Swed32;
using l4d2External; // Para Entity y Offsets

namespace left4dead2Menu
{
    internal class EntityManager
    {
        private readonly Swed swed;
        private readonly Offsets offsets;
        private readonly Encoding encoding;

        // Constantes que podrían estar en Offsets.cs o GameConstants.cs
        private const int MAX_ENTITIES = 900; // Límite del bucle de entidades
        private const int ENTITY_LOOP_STRIDE = 0x10; // Distancia entre punteros de entidad en la lista
        private const int ENTITY_MODELNAME_POINTER_OFFSET = 0x10; // Offset al puntero del nombre del modelo
        private const int ENTITY_MODELNAME_STRING_LENGTH = 10; // Longitud a leer para el nombre del modelo


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
            List<Entity> survivors,
            IntPtr clientModuleBase)
        {
            commonInfected.Clear();
            specialInfected.Clear();
            survivors.Clear();

            localPlayer.address = swed.ReadPointer(clientModuleBase, offsets.localplayer);
            if (localPlayer.address != IntPtr.Zero)
            {
                // Actualizamos el jugador local. Su magnitud será 0 respecto a sí mismo (Vector3.Zero).
                UpdateSingleEntityProperties(localPlayer, Vector3.Zero, isLocalPlayerUpdate: true);
            }

            // Solo proceder si el jugador local es válido para calcular magnitudes relativas
            if (localPlayer.address != IntPtr.Zero)
            {
                PopulateEntityLists(localPlayer, commonInfected, specialInfected, survivors, clientModuleBase);
            }

            // Ordenar listas como en el original
            if (commonInfected.Count > 0) commonInfected.Sort((a, b) => a.magnitude.CompareTo(b.magnitude));
            if (specialInfected.Count > 0) specialInfected.Sort((a, b) => a.magnitude.CompareTo(b.magnitude));
        }

        private void PopulateEntityLists(
            Entity localPlayer,
            List<Entity> commonInfected,
            List<Entity> specialInfected,
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

                if (currentEntity.lifeState != GameConstants.LIFE_STATE_ALIVE)
                {
                    continue;
                }

                if (currentEntity.teamNum == GameConstants.SURVIVOR_TEAM && currentEntity.health > 0)
                {
                    survivors.Add(currentEntity);
                }
                else if (currentEntity.teamNum == GameConstants.INFECTED_TEAM && currentEntity.health > 0)
                {
                    if (currentEntity.modelName != null && currentEntity.modelName.Contains("inf"))
                    {
                        commonInfected.Add(currentEntity);
                    }
                    else if (currentEntity.modelName != null) // No es "inf" y el nombre no es nulo
                    {
                        specialInfected.Add(currentEntity);
                    }
                }
            }
        }

        private void UpdateSingleEntityProperties(Entity entity, Vector3 localPlayerOriginForMagnitude, bool isLocalPlayerUpdate = false)
        {
            entity.lifeState = swed.ReadInt(entity.address, offsets.Lifestate);

            // En el original, solo se leen más datos si lifeState ES 2 (LIFE_STATE_ALIVE).
            // Para el jugador local, podríamos querer siempre los datos, pero seguiremos la lógica original.
            if (entity.lifeState != GameConstants.LIFE_STATE_ALIVE)
            {
                // Si es el jugador local y no está "vivo" según este estado, aún podríamos querer su 'origin'
                // pero otras propiedades como 'health' podrían no ser válidas o relevantes.
                // El código original retornaba aquí para todos, así que lo mantenemos.
                return;
            }

            entity.origin = swed.ReadVec(entity.address, offsets.Origin);
            entity.viewOffset = swed.ReadVec(entity.address, offsets.ViewOffset);
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);

            entity.health = swed.ReadInt(entity.address, offsets.Health);
            entity.teamNum = swed.ReadInt(entity.address, offsets.TeamNum);
            entity.jumpflag = swed.ReadInt(entity.address, offsets.JumpFlag);

            if (!isLocalPlayerUpdate) // La magnitud del jugador local respecto a sí mismo no es necesaria aquí.
            {
                entity.magnitude = MathUtils.CalculateMagnitude(entity.origin, localPlayerOriginForMagnitude);
            }
            else
            {
                entity.magnitude = 0; // Magnitud del jugador local a sí mismo.
            }

            // Lectura del nombre del modelo, replicando la lógica original de Program.cs
            var modelNamePtr = swed.ReadPointer(entity.address, ENTITY_MODELNAME_POINTER_OFFSET);
            if (modelNamePtr != IntPtr.Zero)
            {
                try
                {
                    entity.modelName = encoding.GetString(swed.ReadBytes(modelNamePtr, ENTITY_MODELNAME_STRING_LENGTH)).Split('\0')[0]; // Tomar hasta el primer nulo
                }
                catch
                {
                    entity.modelName = "ERR_NAME";
                }
            }
            else
            {
                entity.modelName = "NULL_NAME_PTR";
            }
        }
    }
}