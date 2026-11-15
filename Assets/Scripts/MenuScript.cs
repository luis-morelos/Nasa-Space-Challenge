using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SelectorMisionMenu : MonoBehaviour
{
    [Header("Textos de los 2 Botones")]
    public TMP_Text textoBotonSuperior;
    public TMP_Text textoBotonInferior;

    [Header("Escena Siguiente")]
    [Tooltip("Escribe el nombre exacto de la escena de ensamblaje")]
    public string escenaSalaEnsamblaje = "SalaEnsamblaje";

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
            tipoMisionElegida = "Tripulada";
            PasarASeleccionOrbita();
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
        // Guardamos los datos para leerlos al entrar a la Sala de Ensamblaje
        PlayerPrefs.SetString("TipoMision", tipoMisionElegida);
        PlayerPrefs.SetString("TipoOrbita", orbitaElegida);
        PlayerPrefs.Save();

        Debug.Log($"Guardado: {tipoMisionElegida} | {orbitaElegida}. Cargando: {escenaSalaEnsamblaje}");

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
        textoBotonInferior.text = "Mision Tripulada";
    }
}