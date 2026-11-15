using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Importante para el nuevo Input System

public class CambioSalaBotonA : MonoBehaviour
{
    [Header("CONFIGURACIÓN DE ESCENA")]
    [Tooltip("Escribe aquí exactamente el nombre de la siguiente sala o escena a la que quieres viajar")]
    public string nombreSiguienteSala = "SalaResultados";

    private bool transicionEjecutada = false;

    void Update()
    {
        if (transicionEjecutada) return;

        bool espacioPresionado = false;

        // Leer la barra espaciadora usando el New Input System de manera segura
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            espacioPresionado = true;
        }

        // Si se detecta la pulsación, ejecutamos el cambio de sala
        if (espacioPresionado)
        {
            transicionEjecutada = true;
            ViajarASiguienteSala();
        }
    }

    void ViajarASiguienteSala()
    {
        if (string.IsNullOrEmpty(nombreSiguienteSala))
        {
            Debug.LogError("[CambioSala] ¡El nombre de la siguiente sala está vacío en el Inspector!");
            return;
        }

        Debug.Log("[CambioSala] Tecla Espacio presionada. Cargando escena: " + nombreSiguienteSala);
        SceneManager.LoadScene(nombreSiguienteSala);
    }
}