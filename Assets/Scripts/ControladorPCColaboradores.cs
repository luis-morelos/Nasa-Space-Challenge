using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ControladorPCColaboradores : MonoBehaviour
{
    [Header("Solo arrastra estos 2 elementos:")]
    public RectTransform panelContenedor; // El objeto 'Panel' dentro de Pantalla_PC_Colaboradores
    public GameObject prefabBoton;        // Tu prefab de Botón

    [Header("Configuración de Presupuesto")]
    public float pasoPresupuesto = 50000f;
    public float presupuestoMaximo = 500000f;

    private float presupuestoActualLocal = 0f;
    private TMP_Text textoPresupuestoGenerado;
    private TMP_Text textoNotificacionGenerada;

    void Start()
    {
        if (panelContenedor == null || prefabBoton == null)
        {
            Debug.LogError("[PC Colaboradores] Faltan asignar el Panel Contenedor o el Prefab Boton.");
            return;
        }

        if (ControladorEnsamblaje.Instance != null)
        {
            presupuestoActualLocal = ControladorEnsamblaje.Instance.presupuestoCientificos;
        }

        PrepararPanel();
        GenerarTodoPorCodigo();
    }

    void PrepararPanel()
    {
        // Limpia cualquier cosa que hubiera antes en el panel (como el "New Text")
        foreach (Transform hijo in panelContenedor)
        {
            Destroy(hijo.gameObject);
        }

        LayoutGroup layoutViejo = panelContenedor.GetComponent<LayoutGroup>();
        if (layoutViejo != null) DestroyImmediate(layoutViejo);

        // Organiza todo en columna proporcionalmente
        VerticalLayoutGroup vLayout = panelContenedor.gameObject.AddComponent<VerticalLayoutGroup>();
        vLayout.childControlWidth = true;
        vLayout.childControlHeight = true;
        vLayout.childForceExpandWidth = true;
        vLayout.childForceExpandHeight = true;
        vLayout.spacing = 10f;
        vLayout.padding = new RectOffset(20, 20, 15, 15);
    }

    void GenerarTodoPorCodigo()
    {
        // 1. GENERAR TEXTO DEL DINERO EN MEDIO
        GameObject objMonto = new GameObject("TXT_MontoGenerado", typeof(RectTransform), typeof(TextMeshProUGUI));
        objMonto.transform.SetParent(panelContenedor, false);
        objMonto.transform.localScale = Vector3.one;

        textoPresupuestoGenerado = objMonto.GetComponent<TextMeshProUGUI>();
        textoPresupuestoGenerado.alignment = TextAlignmentOptions.Center;
        textoPresupuestoGenerado.enableAutoSizing = true;
        textoPresupuestoGenerado.fontSizeMin = 10;
        textoPresupuestoGenerado.fontSizeMax = 28;
        textoPresupuestoGenerado.color = Color.white;
        ActualizarTextoPresupuesto();

        // 2. GENERAR FILA PARA BOTONES (- Y +)
        GameObject filaControles = new GameObject("Fila_Botones", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        filaControles.transform.SetParent(panelContenedor, false);
        filaControles.transform.localScale = Vector3.one;

        HorizontalLayoutGroup hLayout = filaControles.GetComponent<HorizontalLayoutGroup>();
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;
        hLayout.spacing = 15f;

        CrearBoton(filaControles.transform, "Btn_Restar", "- $50k", RestarPresupuesto);
        CrearBoton(filaControles.transform, "Btn_Aumentar", "+ $50k", AumentarPresupuesto);

        // 3. GENERAR BOTÓN DE CONFIRMAR
        CrearBoton(panelContenedor, "Btn_Confirmar", "ASIGNAR PRESUPUESTO", ConfirmarAsignacionPresupuesto);

        // 4. GENERAR TEXTO DE NOTIFICACIÓN EMERGENTE
        GameObject objNotif = new GameObject("TXT_NotificacionGenerada", typeof(RectTransform), typeof(TextMeshProUGUI));
        objNotif.transform.SetParent(panelContenedor, false);
        objNotif.transform.localScale = Vector3.one;

        textoNotificacionGenerada = objNotif.GetComponent<TextMeshProUGUI>();
        textoNotificacionGenerada.alignment = TextAlignmentOptions.Center;
        textoNotificacionGenerada.enableAutoSizing = true;
        textoNotificacionGenerada.fontSizeMin = 8;
        textoNotificacionGenerada.fontSizeMax = 20;
        textoNotificacionGenerada.color = Color.yellow;
        textoNotificacionGenerada.text = ""; // Empieza invisible hasta que confirmen
    }

    void CrearBoton(Transform padre, string nombre, string textoBoton, UnityEngine.Events.UnityAction evento)
    {
        GameObject btn = Instantiate(prefabBoton, padre);
        btn.name = nombre;
        btn.transform.localScale = Vector3.one;

        TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = textoBoton;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 8;
            tmp.fontSizeMax = 22;
        }

        Button componenteBtn = btn.GetComponent<Button>();
        if (componenteBtn != null)
        {
            componenteBtn.onClick.RemoveAllListeners();
            componenteBtn.onClick.AddListener(evento);
        }
    }

    public void AumentarPresupuesto()
    {
        presupuestoActualLocal = Mathf.Min(presupuestoActualLocal + pasoPresupuesto, presupuestoMaximo);
        ActualizarTextoPresupuesto();
    }

    public void RestarPresupuesto()
    {
        presupuestoActualLocal = Mathf.Max(presupuestoActualLocal - pasoPresupuesto, 0f);
        ActualizarTextoPresupuesto();
    }

    private void ActualizarTextoPresupuesto()
    {
        if (textoPresupuestoGenerado != null)
        {
            textoPresupuestoGenerado.text = $"FONDOS CIENTÍFICOS\n<b>${presupuestoActualLocal:N0}</b>";
        }
    }

    public void ConfirmarAsignacionPresupuesto()
    {
        if (ControladorEnsamblaje.Instance != null)
        {
            ControladorEnsamblaje.Instance.presupuestoCientificos = presupuestoActualLocal;
            ControladorEnsamblaje.Instance.RecalcularParametrosMision();
        }

        StopAllCoroutines();
        StartCoroutine(MostrarNotificacion("El desempeño de los calculos dependen de tus colaboradores"));
    }

    private IEnumerator MostrarNotificacion(string mensaje)
    {
        textoNotificacionGenerada.text = mensaje;
        yield return new WaitForSeconds(4f);
        textoNotificacionGenerada.text = "";
    }
}