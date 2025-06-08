# 🎮 [L4D2] EXTERNAL CHEAT en C#

> ⚠️ **ADVERTENCIA DE USO**
> Este proyecto fue creado con fines puramente educativos para investigar cómo las aplicaciones externas interactúan con otros procesos en memoria. El uso de este software en servidores de juego en línea protegidos **resultará en una prohibición permanente (VAC Ban)**. Úsalo bajo tu propio riesgo y exclusivamente en entornos controlados (partidas locales con bots o servidores inseguros).

## 🎯 Resumen del Proyecto

`l4d2External` es una prueba de concepto de un "cheat" externo para el videojuego **Left 4 Dead 2**. A diferencia de las trampas internas que se inyectan como un DLL, esta herramienta opera como un proceso separado que lee y escribe en la memoria del juego (`l4d2.exe`) para obtener información y habilitar ciertas funcionalidades.

## ✨ Características Implementadas

El código base se centra en las siguientes características, comunes en este tipo de herramientas:

* ✅ **Glow ESP**: Hace que los supervivientes, infectados especiales y objetos brillen a través de las paredes, revelando su posición.
* ✅ **Bunnyhop (BHop)**: Automatiza los saltos para permitir un movimiento continuo y rápido.
* ✅ **Aimbot**: Dispara automáticamente a los infectados especiales .

## 🧠 ¿Cómo Funciona?

La herramienta está escrita en **C#** y utiliza la **API de Windows (WinAPI)** para realizar sus operaciones:

1.  **Obtención del Proceso**: Primero, busca la ventana del juego y obtiene un "handle" al proceso `l4d2.exe`.
2.  **Lectura de Memoria**: Utiliza `ReadProcessMemory` para leer datos clave directamente de la memoria del juego. Para ello, se basa en "offsets" (direcciones de memoria) que apuntan a información como la ubicación de los jugadores, la salud, el equipo, etc.
3.  **Bucle Principal**: La herramienta se ejecuta en un bucle infinito que actualiza constantemente la información y aplica las funcionalidades (por ejemplo, activa el brillo de un jugador cercano).

> **Nota Importante**: Los "offsets" de memoria cambian con cada actualización del juego. Para que esta herramienta funcione, es necesario actualizarlos constantemente utilizando un "dumper" u obteniéndolos de comunidades de "reverse engineering".

## 🚀 Compilación y Ejecución

### Requisitos
* Los "offsets" más recientes para la versión actual de Left 4 Dead 2 (estos casi no cambian por cada update).
* Los Modulos de Client.dll y Engine.dll

### Pasos

1.  **Clonar el repositorio**:
    ```bash
    git clone [https://github.com/Russtels/l4d2External.git](https://github.com/Russtels/l4d2External.git)
    ```
2.  **Actualizar Offsets**: Busca los nuevos offsets y actualízalos en los archivos de cabecera correspondientes (ej. `offsets.h`).
3.  **Compilar**: Abre el proyecto en Visual Studio y compila la solución en modo **Release** para la arquitectura **x86**.
4.  **Ejecutar**:
    * Primero, inicia Left 4 Dead 2.
    * Luego, ejecuta el archivo `.exe` compilado. La consola debería indicar si se enganchó correctamente al proceso del juego.
