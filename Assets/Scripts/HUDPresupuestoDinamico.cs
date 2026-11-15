using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDPresupuestoDinamico : MonoBehaviour
{
    private TMP_Text textoHUD;
    private Transform xrCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            xrCameraTransform = Camera.main.transform;
        }

        if (ControladorEnsamblaje.Instance == null)
        {
            GameObject controladorEmergencia = new GameObject("ControladorEnsamblaje_Emergencia");
            controladorEmergencia.AddComponent<ControladorEnsamblaje>();
        }

        CrearHUDMundoVirtual();
    }

    void CrearHUDMundoVirtual()
    {
        GameObject canvasHUDObj = new GameObject("HUD_VR_Dinamico", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasHUDObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 999;

        canvasHUDObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        if (xrCameraTransform != null)
        {
            canvasHUDObj.transform.position = xrCameraTransform.position + (xrCameraTransform.forward * 1.2f) + (xrCameraTransform.up * 0.35f) + (xrCameraTransform.right * 0.45f);
            canvasHUDObj.transform.rotation = xrCameraTransform.rotation;
            canvasHUDObj.transform.SetParent(xrCameraTransform);
        }
        else
        {
            canvasHUDObj.transform.position = new Vector3(0f, 2.5f, 2f);
        }

        GameObject panelFondoObj = new GameObject("Panel_Fondo_HUD", typeof(RectTransform), typeof(Image));
        panelFondoObj.transform.SetParent(canvasHUDObj.transform, false);

        RectTransform rectPanel = panelFondoObj.GetComponent<RectTransform>();
        rectPanel.sizeDelta = new Vector2(460f, 150f);

        Image imagenFondo = panelFondoObj.GetComponent<Image>();
        imagenFondo.color = new Color(0.05f, 0.05f, 0.12f, 0.9f);

        GameObject textoObj = new GameObject("Texto_HUD", typeof(RectTransform));
        textoObj.transform.SetParent(panelFondoObj.transform, false);

        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.sizeDelta = Vector2.zero;
        rectTexto.anchoredPosition = Vector2.zero;

        textoHUD = textoObj.AddComponent<TextMeshProUGUI>();
        textoHUD.alignment = TextAlignmentOptions.MidlineLeft;
        textoHUD.fontSize = 18;
        textoHUD.color = Color.white;
        textoHUD.text = "Sincronizando telemetría...";
    }

    void Update()
    {
        if (textoHUD != null)
        {
            if (ControladorEnsamblaje.Instance != null)
            {
                float presupuestoTotal = ControladorEnsamblaje.Instance.presupuestoTotal;
                float presupuestoRestante = ControladorEnsamblaje.Instance.presupuestoRestante;
                string tipoMision = ControladorEnsamblaje.Instance.tipoMision;
                string tipoOrbita = ControladorEnsamblaje.Instance.tipoOrbita;
                float pesoActual = ControladorEnsamblaje.Instance.capacidadCargaMax;
                float pesoMinimo = ControladorEnsamblaje.Instance.pesoMinimoRequerido;
                float pesoMaximo = ControladorEnsamblaje.Instance.pesoMaximoIdeal;

                string alertaPresupuesto = (presupuestoRestante < 0) ? "<color=red>¡DÉFICIT!</color>" : "<color=green>OK</color>";

                string alertaPeso = "";
                if (pesoActual > pesoMaximo)
                    alertaPeso = "<color=red>¡SOBREPESO!</color>";
                else if (pesoActual >= pesoMinimo)
                    alertaPeso = "<color=green>ÓPTIMO</color>";
                else
                    alertaPeso = "<color=white>EN PROCESO</color>";

                textoHUD.text = $"<b>{tipoMision.ToUpper()} ({tipoOrbita})</b>\n" +
                                $"• Presupuesto: <b>${presupuestoRestante:N0}</b> / ${presupuestoTotal:N0} {alertaPresupuesto}\n" +
                                $"• Peso Actual: <b>{pesoActual} kg</b> ({pesoMinimo}-{pesoMaximo}) {alertaPeso}";
            }
            else
            {
                textoHUD.text = "<color=yellow>Buscando controlador de misión...</color>";
            }
        }
    }
}