using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControladorPCComunicaciones : MonoBehaviour
{
    [Header("Solo arrastra el Panel de Pantalla_PC_Comunicaciones:")]
    public RectTransform panelContenedor;

    private TMP_Text textoTitulo;
    private TMP_Text textoPorcentaje;
    private TMP_Text textoDiagnostico;
    private int ultimoPorcentajeMostrado = -1;

    void Start()
    {
        if (panelContenedor == null)
        {
            Debug.LogError("[PC Comunicaciones] Falta asignar el Panel Contenedor en el Inspector.");
            return;
        }

        PrepararPanel();
        GenerarInterfazPorCodigo();
        ActualizarPantalla(true);
    }

    void Update()
    {
        // Revisa continuamente si cambió el porcentaje en el controlador central para actualizarse en vivo
        ActualizarPantalla(false);
    }

    void PrepararPanel()
    {
        // Limpiar cualquier elemento previo en el panel
        foreach (Transform hijo in panelContenedor)
        {
            Destroy(hijo.gameObject);
        }

        LayoutGroup layoutViejo = panelContenedor.GetComponent<LayoutGroup>();
        if (layoutViejo != null) DestroyImmediate(layoutViejo);

        VerticalLayoutGroup vLayout = panelContenedor.gameObject.AddComponent<VerticalLayoutGroup>();
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
        vLayout.childForceExpandWidth = true;
        vLayout.childForceExpandHeight = true;
        vLayout.spacing = 10f;
        vLayout.padding = new RectOffset(20, 20, 20, 20);
    }

    void GenerarInterfazPorCodigo()
    {
        // 1. TÍTULO DE LA ESTACIÓN
        textoTitulo = CrearTextoUI("TXT_TituloComunicaciones", 12, 22, Color.cyan);
        textoTitulo.text = "<b>TELEMETRÍA Y DATOS ÚTILES</b>";

        // 2. PORCENTAJE PRINCIPAL (0% - 100%)
        textoPorcentaje = CrearTextoUI("TXT_PorcentajeDatos", 24, 54, Color.white);
        textoPorcentaje.text = "0%";

        // 3. DIAGNÓSTICO DE CALIDAD DE SEÑAL / COMPONENTES
        textoDiagnostico = CrearTextoUI("TXT_DiagnosticoSenal", 10, 18, Color.white);
        textoDiagnostico.text = "Evaluando componentes...";
    }

    TMP_Text CrearTextoUI(string nombre, float minSize, float maxSize, Color colorInicial)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(panelContenedor, false);
        obj.transform.localScale = Vector3.one;

        TMP_Text tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = minSize;
        tmp.fontSizeMax = maxSize;
        tmp.color = colorInicial;
        return tmp;
    }

    void ActualizarPantalla(bool forzar)
    {
        if (ControladorEnsamblaje.Instance == null || textoPorcentaje == null) return;

        int porcentajeActual = Mathf.Clamp(ControladorEnsamblaje.Instance.porcentajeInformacionUtil, 0, 100);

        // Solo actualiza la UI si el porcentaje cambió (optimización para VR)
        if (!forzar && porcentajeActual == ultimoPorcentajeMostrado) return;
        ultimoPorcentajeMostrado = porcentajeActual;

        textoPorcentaje.text = $"<b>{porcentajeActual}%</b>";

        // Cambiar color y diagnóstico según la calidad de los componentes elegidos
        if (porcentajeActual < 35)
        {
            textoPorcentaje.color = new Color(1f, 0.3f, 0.3f); // Rojo
            textoDiagnostico.text = "<color=#FF5555>SEÑAL CRÍTICA</color>\nComponentes básicos: pérdida alta de datos científicos.";
        }
        else if (porcentajeActual < 75)
        {
            textoPorcentaje.color = new Color(1f, 0.85f, 0.2f); // Amarillo
            textoDiagnostico.text = "<color=#FFD833>TRANSMISIÓN ESTABLE</color>\nRendimiento moderado de telemetría.";
        }
        else
        {
            textoPorcentaje.color = new Color(0.3f, 1f, 0.4f); // Verde
            textoDiagnostico.text = "<color=#4CFF66>ANCHO DE BANDA ÓPTIMO</color>\nComponentes de alta gama: máxima recolección de datos.";
        }
    }
}