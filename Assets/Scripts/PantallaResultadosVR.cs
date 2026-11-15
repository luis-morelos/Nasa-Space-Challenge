using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PantallaResultadosVR : MonoBehaviour
{
    [Header("Configuración del Panel en la Sala")]
    [Tooltip("Distancia en metros frente a la cámara donde aparecerá flotando el panel")]
    public float distanciaFrenteCamara = 2.0f;
    [Tooltip("Altura adicional respecto al centro de la cámara")]
    public float alturaPanel = 0.0f;

    [Header("Navegación (Inspector)")]
    [Tooltip("Escribe aquí exactamente el nombre de la escena del menú principal a la que volverás")]
    public string nombreEscenaMenu = "MenuPrincipal";

    void Start()
    {
        GenerarPanelResultadosDinamico();
    }

    void Update()
    {
        bool presionoMenu = false;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            presionoMenu = true;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null && (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame))
        {
            presionoMenu = true;
        }
#else
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            presionoMenu = true;
        }
#endif

        if (presionoMenu)
        {
            RegresarAlMenu();
        }
    }

    void RegresarAlMenu()
    {
        if (string.IsNullOrEmpty(nombreEscenaMenu))
        {
            Debug.LogError("[Navegación] ¡El nombre de la escena del menú está vacío en el Inspector!");
            return;
        }

        Debug.Log("[Navegación VR] Regresando al menú: " + nombreEscenaMenu);
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    void GenerarPanelResultadosDinamico()
    {
        // 1. Obtener los datos almacenados en el ControladorEnsamblaje
        float presupuestoRestante = 0f;
        float presupuestoTotal = 1000000f;
        float pesoActual = 0f;
        float pesoMin = 1000f;
        float pesoMax = 1350f;
        int infoUtil = 0;

        if (ControladorEnsamblaje.Instance != null)
        {
            presupuestoRestante = ControladorEnsamblaje.Instance.presupuestoRestante;
            presupuestoTotal = ControladorEnsamblaje.Instance.presupuestoTotal;
            pesoActual = ControladorEnsamblaje.Instance.capacidadCargaMax;
            pesoMin = ControladorEnsamblaje.Instance.pesoMinimoRequerido;
            pesoMax = ControladorEnsamblaje.Instance.pesoMaximoIdeal;
            infoUtil = ControladorEnsamblaje.Instance.porcentajeInformacionUtil;

            Debug.Log($"[Resultados] Datos capturados con éxito. Peso: {pesoActual}, Info: {infoUtil}%, Presupuesto Restante: {presupuestoRestante}");
        }
        else
        {
            Debug.LogWarning("[Resultados] ⚠️ No se encontró 'ControladorEnsamblaje.Instance'. Usando datos de prueba por defecto.");
        }

        // 2. Encontrar la cámara principal de la sala 3D
        Camera camaraActual = Camera.main;
        if (camaraActual == null)
        {
            Debug.LogError("[Resultados] ¡No se encontró ninguna cámara principal (Camera.main) en esta sala!");
            return;
        }

        // 3. Crear el Canvas en Modo World Space
        GameObject canvasObj = new GameObject("Canvas_Resultados_3D", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 9999;

        canvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        Vector3 posicionIdeal = camaraActual.transform.position + (camaraActual.transform.forward * distanciaFrenteCamara) + (Vector3.up * alturaPanel);
        canvasObj.transform.position = posicionIdeal;
        canvasObj.transform.rotation = Quaternion.LookRotation(canvasObj.transform.position - camaraActual.transform.position);

        // 4. Crear el Panel de Fondo
        GameObject panelObj = new GameObject("Panel_Fondo", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(950f, 650f);

        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0.02f, 0.03f, 0.10f, 0.90f);

        // 5. Crear el objeto de Texto Dinámico
        GameObject textoObj = new GameObject("Texto_Feedback", typeof(RectTransform));
        textoObj.transform.SetParent(panelObj.transform, false);

        RectTransform textoRect = textoObj.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.sizeDelta = new Vector2(-60f, -60f);
        textoRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmpText = textoObj.AddComponent<TextMeshProUGUI>();
        tmpText.fontSize = 24;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.TopLeft;

        // 6. Construir el Feedback Dinámico basado en las decisiones del usuario
        string feedbackPresupuesto = presupuestoRestante >= 0
            ? $"<color=green>• Gestión Presupuestaria: ¡Excelente! Te sobraron ${presupuestoRestante:N0}.</color>"
            : $"<color=red>• Gestión Presupuestaria: ¡Déficit! Terminaste con un saldo en contra de ${Mathf.Abs(presupuestoRestante):N0}.</color>";

        // --- AJUSTE DE PESO: Solo el exceso se considera negativo ---
        string feedbackPeso = "";
        if (pesoActual > pesoMax)
        {
            feedbackPeso = $"<color=red>• Peso Estructural: EXCESO ({pesoActual} kg). Sobrecargaste la nave superando el límite ideal.</color>";
        }
        else
        {
            feedbackPeso = $"<color=green>• Peso Estructural: ¡ÓPTIMO Y SEGURO! ({pesoActual} kg). Nave ligera y eficiente dentro del rango permitido.</color>";
        }

        string calificacionMision = "";
        if (infoUtil >= 80 && presupuestoRestante >= 0 && pesoActual <= pesoMax)
            calificacionMision = "<color=#00FFFF><b>CALIFICACIÓN: ÉXITO ROTUNDO (Misión Legendaria)</b></color>";
        else if (infoUtil >= 50 && presupuestoRestante >= -50000 && pesoActual <= pesoMax)
            calificacionMision = "<color=green><b>CALIFICACIÓN: MISIÓN EXITOSA</b></color>";
        else
            calificacionMision = "<color=orange><b>CALIFICACIÓN: MISIÓN PARCIALMENTE DEFICIENTE</b></color>";

        // Asignar el texto formateado final
        tmpText.text = $"<size=120%><b>INFORME OFICIAL DE MISIÓN ESPACIAL</b></size>\n\n" +
                       $"• <b>Información Útil Recolectada:</b> {infoUtil}%\n" +
                       $"• <b>Presupuesto Final:</b> ${presupuestoRestante:N0} / ${presupuestoTotal:N0}\n" +
                       $"• <b>Peso Total:</b> {pesoActual} kg (Rango Ideal Máx: {pesoMax} kg)\n\n" +
                       $"<size=110%><b>EVALUACIÓN Y FEEDBACK DE TUS DECISIONES:</b></size>\n" +
                       $"{feedbackPresupuesto}\n" +
                       $"{feedbackPeso}\n" +
                       $"• <b>Transmisión de Datos:</b> {(infoUtil >= 70 ? "Estable, alta velocidad y cobertura óptima." : "Pérdida de paquetes por instrumentación o científicos limitados.")}\n\n" +
                       $"--------------------------------------------------------------------------------------------------\n" +
                       $"{calificacionMision}\n\n" +
                       $"<size=90%><color=#AAAAAA>👉 Presiona <b>Espacio</b> (o el botón asignado) para volver al menú.</color></size>";

        Debug.Log("[Resultados Sala 3D] Panel generado con éxito.");
    }
}