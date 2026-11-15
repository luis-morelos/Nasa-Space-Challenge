using UnityEngine;
using TMPro;

public class ControladorEnsamblaje : MonoBehaviour
{
    public static ControladorEnsamblaje Instance;

    [Header("Datos recibidos del Menú")]
    public string tipoMision;
    public string tipoOrbita;

    [Header("Pantallas / UI de Visualización")]
    public TMP_Text textoResumenMision;
    public TMP_Text textoPCComunicaciones;

    [Header("Recursos y Límites de la Misión")]
    public float presupuestoTotal;
    public float presupuestoRestante;

    // Rango de peso ideal (1,000 kg a 1,350 kg)
    public float pesoMinimoRequerido = 1000f;
    public float pesoMaximoIdeal = 1350f;

    // Propiedad de compatibilidad
    public float cargaMinimaRequerida
    {
        get { return pesoMinimoRequerido; }
        set { pesoMinimoRequerido = value; }
    }

    [Header("1. PC_Materiales (Inversión y Carga)")]
    [Range(1, 3)] public int nivelMateriales = 1;
    public float costoMateriales = 0f;
    public float capacidadCargaMax = 0f;

    [Header("2. PC_Colaboradores (Científicos)")]
    public float presupuestoCientificos = 0f;
    [Range(0f, 100f)] public float probabilidadExitoLanzamiento = 0f;

    [Header("4. PC_Instrumentacion (Recursos Físicos)")]
    [Range(1, 3)] public int nivelInstrumentacion = 1;
    public float costoInstrumentacion = 80000f;

    [Header("5. PC_TiempoDeVidaUtil (Duración)")]
    [Range(1, 36)] public int mesesVidaUtil = 6;

    [Header("3. PC_Comunicaciones (Resultado Calculado)")]
    [Range(0, 100)] public int porcentajeInformacionUtil = 0; // Empieza estrictamente en 0%

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // <--- ¡Asegúrate de tener esto para que persista en la sala de resultados!
    }

    void Start()
    {
        // Limpieza absoluta al iniciar la partida
        costoMateriales = 0f;
        capacidadCargaMax = 0f;
        presupuestoCientificos = 0f;
        nivelMateriales = 1;
        nivelInstrumentacion = 1;
        mesesVidaUtil = 6;
        porcentajeInformacionUtil = 0;
        probabilidadExitoLanzamiento = 0f;

        tipoMision = PlayerPrefs.GetString("TipoMision", "No Tripulada");
        tipoOrbita = PlayerPrefs.GetString("TipoOrbita", "Orbita Baja");

        float presupuestoRespaldo = Random.Range(70, 110) * 10000f; // $700k - $1.1M
        presupuestoTotal = PlayerPrefs.GetFloat("PresupuestoInicial", presupuestoRespaldo);

        ConfigurarParametrosIniciales();
        RecalcularParametrosMision();
    }

    private void ConfigurarParametrosIniciales()
    {
        if (tipoOrbita == "Orbita Alta")
        {
            pesoMinimoRequerido = 1100f;
            pesoMaximoIdeal = 1450f;
        }
        else
        {
            pesoMinimoRequerido = 1000f;
            pesoMaximoIdeal = 1350f;
        }

        presupuestoRestante = presupuestoTotal;
        ActualizarUIResumen();
    }

    public void RecalcularParametrosMision()
    {
        costoInstrumentacion = nivelInstrumentacion * 80000f;
        float costoOperacionTiempo = mesesVidaUtil * 10000f;

        float gastoTotal = costoMateriales + presupuestoCientificos + costoInstrumentacion + costoOperacionTiempo;
        presupuestoRestante = presupuestoTotal - gastoTotal;

        // --- CÁLCULO EQUILIBRADO 50% / 50% ---

        // 1. Aporte del Hardware y Materiales (Hasta 50%)
        // Se basa en el nivel de materiales y en qué tan cerca estás del peso ideal (1000 - 1350 kg)
        float proporcionPeso = Mathf.Clamp01(capacidadCargaMax / pesoMaximoIdeal);
        float aporteMateriales = (nivelMateriales * 15f) + (proporcionPeso * 35f); // Máximo ~50%

        // 2. Aporte de los Colaboradores / Científicos (Hasta 50%)
        // Se calcula en función del presupuesto invertido en científicos (ej. hasta $200,000 para el 50%)
        float aporteColaboradores = Mathf.Clamp((presupuestoCientificos / 200000f) * 50f, 0f, 50f); // Máximo 50%

        // Penalización suave si te pasas de los 1350 kg
        float penalizacionSobrepeso = 0f;
        if (capacidadCargaMax > pesoMaximoIdeal)
        {
            float exceso = capacidadCargaMax - pesoMaximoIdeal;
            penalizacionSobrepeso = exceso * 0.10f;
        }

        // Suma total de las dos partes (50% materiales + 50% colaboradores)
        float resultadoFinal = aporteMateriales + aporteColaboradores - penalizacionSobrepeso;

        // Si no se ha comprado nada ni asignado científicos, se queda limpia y estrictamente en 0%
        if (capacidadCargaMax == 0 && presupuestoCientificos == 0)
        {
            porcentajeInformacionUtil = 0;
            probabilidadExitoLanzamiento = 0f;
        }
        else
        {
            porcentajeInformacionUtil = Mathf.Clamp(Mathf.RoundToInt(resultadoFinal), 0, 100);
            probabilidadExitoLanzamiento = Mathf.Clamp(Mathf.RoundToInt(30f + aporteColaboradores + (nivelInstrumentacion * 5f)), 10, 99f);
        }

        ActualizarUIResumen();
    }

    private void ActualizarUIResumen()
    {
        if (textoResumenMision != null)
        {
            string estadoPeso = "";

            // Sin mensajes molestos de poco peso, solo estado neutral u óptimo/sobrepeso
            if (capacidadCargaMax > pesoMaximoIdeal)
            {
                int exceso = Mathf.RoundToInt(capacidadCargaMax - pesoMaximoIdeal);
                estadoPeso = $"<color=red>(¡Exceso! +{exceso} kg sobre el límite)</color>";
            }
            else if (capacidadCargaMax >= pesoMinimoRequerido && capacidadCargaMax <= pesoMaximoIdeal)
            {
                estadoPeso = "<color=green>(Peso Óptimo OK)</color>";
            }
            else
            {
                estadoPeso = "<color=white>(Ensamblando...)</color>"; // Estado neutral sin alarmas
            }

            string alertaPresupuesto = (presupuestoRestante < 0) ? "<color=red>(¡DÉFICIT!)</color>" : "";

            textoResumenMision.text = $"<b>MISIÓN:</b> {tipoMision} ({tipoOrbita})\n" +
                                      $"<b>PRESUPUESTO:</b> ${presupuestoRestante:N0} / ${presupuestoTotal:N0} {alertaPresupuesto}\n" +
                                      $"<b>PESO ACTUAL:</b> {capacidadCargaMax} kg (Ideal: {pesoMinimoRequerido}-{pesoMaximoIdeal} kg)\n" +
                                      $"<b>ESTADO:</b> {estadoPeso}";
        }

        if (textoPCComunicaciones != null)
        {
            textoPCComunicaciones.text = $"INFORMACIÓN ÚTIL ESPERADA\n<size=150%><b>{porcentajeInformacionUtil}%</b></size>";
        }
    }
}