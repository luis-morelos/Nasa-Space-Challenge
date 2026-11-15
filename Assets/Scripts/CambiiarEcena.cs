using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Compatible con tu New Input System

public class CambiarEscena : MonoBehaviour
{
    [Header("Opcional: Avance automático con tecla")]
    [Tooltip("Activa esto si quieres que también cambie a la siguiente escena al presionar Espacio")]
    public bool avanzarConEspacio = true;

    private bool cambiando = false;

    void Update()
    {
        if (!avanzarConEspacio || cambiando) return;

        // Detecta la barra espaciadora con el New Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            cambiando = true;
            SiguienteEscena();
        }
    }

    // Puedes llamar a este método desde un botón UI o desde otro script
    public void SiguienteEscena()
    {
        int totalEscenas = SceneManager.sceneCountInBuildSettings;

        if (totalEscenas == 0)
        {
            Debug.LogError("[CambiarEscena] ¡No hay escenas agregadas en Build Settings!");
            return;
        }

        // Obtiene el índice de la escena actual y suma 1.
        // El operador '%' (módulo) hace que si llegas al total, regrese automáticamente a 0.
        int indiceActual = SceneManager.GetActiveScene().buildIndex;
        int siguienteIndice = (indiceActual + 1) % totalEscenas;

        Debug.Log($"[CambiarEscena] Pasando de escena {indiceActual} a {siguienteIndice}");
        SceneManager.LoadScene(siguienteIndice);
    }

    // Conservamos tus métodos anteriores por si algún botón de tu UI ya los está llamando
    public void IrAEstacion()
    {
        SceneManager.LoadScene("EstacionEspacial");
    }

    public void IrAlMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}