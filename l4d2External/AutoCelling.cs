// l4d2External/AutoCelling.cs (Movimiento Instantáneo por Escritura en Memoria)
using left4dead2Menu;
using System;

namespace l4d2External
{
    internal class AutoCelling
    {
        // El ángulo objetivo al que se forzará la cámara.
        private const float CellingAngle = 12.40f;
        // Tecla de activación (Botón central del mouse).
        private const int ActivationKey = 0x04;

        public void Update(GameMemory memory, IntPtr engineModule, Offsets offsets)
        {
            // Solo se ejecuta si se mantiene presionada la tecla de activación.
            if (NativeMethods.GetAsyncKeyState(ActivationKey) >= 0)
            {
                return;
            }

            try
            {
                // Calcula la dirección de memoria del ángulo de la cámara.
                IntPtr viewAnglesAddress = memory.ReadPointer(engineModule, offsets.engineAngles);
                if (viewAnglesAddress == IntPtr.Zero) return;

                // =======================================================================
                // <<< CAMBIO PRINCIPAL: ESCRITURA DIRECTA EN MEMORIA >>>
                // Escribe el valor deseado directamente en la dirección del ángulo X (pitch).
                // Esto resulta en un cambio instantáneo.
                // =======================================================================
                memory.WriteFloat(viewAnglesAddress + offsets.engineAnglesOffset, CellingAngle);
            }
            catch
            {
                // Ignorar errores para evitar crasheos.
            }
        }
    }
}