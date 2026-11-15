using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SelectorMisionMenu : MonoBehaviour
{
    [Header("Textos de los 2 Botones")]
    public TMP_Text textoBotonSuperior;
    public TMP_Text textoBotonInferior;

    [Header("Escena Siguiente")]
    [Tooltip("Debe coincidir exactamente con el nombre en File -> Build Profiles")]
    public string escenaSalaEnsamblaje = "Esamblaje";

    private int pasoActual = 0; // 0 = Tipo de misión, 1 = Órbita
    private string tipoMisionElegida = "";
    private string orbitaElegida = "";

    void Start()
    {
        ReiniciarMenu();
    }

    public void AlPresionarBotonSuperior()
    {
        if (pasoActual == 0)
        {
            // Este botón es el de Misión No Tripulada (Permitido)
            tipoMisionElegida = "No Tripulada";
            PasarASeleccionOrbita();
        }
        else if (pasoActual == 1)
        {
            orbitaElegida = "Orbita Baja";
            IrASalaEnsamblaje();
        }
    }

    public void AlPresionarBotonInferior()
    {
        if (pasoActual == 0)
        {
            // RESTRICCIÓN: Las misiones tripuladas están desactivadas por ahora
            Debug.LogWarning("Las misiones tripuladas están deshabilitadas temporalmente.");
            return; // No hace nada y detiene la ejecución
        }
        else if (pasoActual == 1)
        {
            orbitaElegida = "Orbita Alta";
            IrASalaEnsamblaje();
        }
    }

    private void PasarASeleccionOrbita()
    {
        pasoActual = 1;
        textoBotonSuperior.text = "Orbita Baja";
        textoBotonInferior.text = "Orbita Alta";
    }

    private void IrASalaEnsamblaje()
    {
        PlayerPrefs.SetString("TipoMision", tipoMisionElegida);
        PlayerPrefs.SetString("TipoOrbita", orbitaElegida);
        PlayerPrefs.Save();

        Debug.Log($"[Menú] Guardado -> Misión: {tipoMisionElegida} | Órbita: {orbitaElegida}. Cargando: {escenaSalaEnsamblaje}");

        if (!string.IsNullOrEmpty(escenaSalaEnsamblaje))
        {
            SceneManager.LoadScene(escenaSalaEnsamblaje);
        }
        else
        {
            Debug.LogWarning("No has escrito el nombre de la escena en el Inspector.");
        }
    }

    public void ReiniciarMenu()
    {
        pasoActual = 0;
        textoBotonSuperior.text = "Mision No Tripulada";
        textoBotonInferior.text = "Mision Tripulada (Bloqueada)"; // Opcional: indicar visualmente que está bloqueado
    }

}