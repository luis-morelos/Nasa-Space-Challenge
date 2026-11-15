using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public struct ComponenteEspacial
{
    public string Nombre;
    public float Costo;
    public float PesoComponente; // Cambiado de CapacidadCarga a PesoComponente para evitar confusiones
    public string Descripcion;
    public string Ventajas;
    public string Desventajas;

    public ComponenteEspacial(string nombre, float costo, float pesoComponente, string descripcion, string ventajas, string desventajas)
    {
        Nombre = nombre;
        Costo = costo;
        PesoComponente = pesoComponente;
        Descripcion = descripcion;
        Ventajas = ventajas;
        Desventajas = desventajas;
    }
}

public class GeneradorTiendaMateriales : MonoBehaviour
{
    [Header("Referencias de UI")]
    public RectTransform contenedorBotones;
    public GameObject prefabBoton;
    public TMP_Text textoTooltip;

    [Header("Configuración")]
    public int maximoBotonesVisibles = 6;

    private List<ComponenteEspacial> mazoDisponible = new List<ComponenteEspacial>();
    private List<ComponenteEspacial> componentesEnPantalla = new List<ComponenteEspacial>();

    void Start()
    {
        if (contenedorBotones == null || prefabBoton == null)
        {
            Debug.LogError("[Tienda Materiales] Faltan asignar Contenedor Botones o Prefab Boton en el Inspector.");
            return;
        }

        ConfigurarCuadriculaPanelEstricta();
        InicializarCatalogo20Componentes();
        BarajarMazo();
        LlenarTiendaInicial();

        if (textoTooltip != null)
            textoTooltip.text = "Apunta a un componente para ver sus especificaciones.";
    }

    void ConfigurarCuadriculaPanelEstricta()
    {
        foreach (Transform child in contenedorBotones)
        {
            Destroy(child.gameObject);
        }

        LayoutGroup layoutViejo = contenedorBotones.GetComponent<LayoutGroup>();
        if (layoutViejo != null && !(layoutViejo is GridLayoutGroup))
        {
            DestroyImmediate(layoutViejo);
        }

        GridLayoutGroup grid = contenedorBotones.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = contenedorBotones.gameObject.AddComponent<GridLayoutGroup>();

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.spacing = new Vector2(8f, 8f);
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.cellSize = new Vector2(170f, 65f);
    }

    void InicializarCatalogo20Componentes()
    {
        mazoDisponible.Clear();

        mazoDisponible.Add(new ComponenteEspacial("Chasis Aluminio 6061", 75000f, 150f, "Estructura ligera estándar de aleación aeroespacial.", "Muy económico y rápido de ensamblar.", "Baja resistencia contra micrometeoritos."));
        mazoDisponible.Add(new ComponenteEspacial("Blindaje Titanio Ti-6Al", 260000f, 280f, "Fuselaje reforzado para soportar altas tensiones.", "Excelente durabilidad en órbita alta.", "Peso elevado que exige mayor propulsión."));
        mazoDisponible.Add(new ComponenteEspacial("Compuesto Fibra Carbono", 420000f, 350f, "Matriz ultraligera de carbono de grado orbital.", "Relación peso-resistencia insuperable.", "Costo de adquisición sumamente elevado."));
        mazoDisponible.Add(new ComponenteEspacial("Malla Kevlar Panal", 340000f, 300f, "Escudo Whipple interno absorbente de impactos.", "Protección superior contra basura espacial.", "Complejidad alta en integración de carga."));
        mazoDisponible.Add(new ComponenteEspacial("Bus Satelital Modular", 140000f, 200f, "Plataforma base adaptable para múltiples sensores.", "Permite ampliar instrumentación fácilmente.", "Menor rigidez estructural en lanzamientos."));
        mazoDisponible.Add(new ComponenteEspacial("Cofia Aerodinámica XL", 190000f, 220f, "Cubierta protectora de gran volumen para el ascenso.", "Maximiza el espacio para carga voluminosa.", "Aumenta la resistencia al aire al despegar."));
        mazoDisponible.Add(new ComponenteEspacial("Paneles Solares Silicio", 65000f, 100f, "Celdas fotovoltaicas convencionales desplegables.", "Bajo impacto en el presupuesto inicial.", "Degradación rápida por radiación solar."));
        mazoDisponible.Add(new ComponenteEspacial("Paneles Arseniuro Galio", 210000f, 180f, "Celdas solares de triple unión de alta eficiencia.", "Generan 40% más energía en menos espacio.", "Frágiles durante el despliegue mecánico."));
        mazoDisponible.Add(new ComponenteEspacial("Generador Nuclear RTG", 390000f, 290f, "Núcleo de radioisótopos con potencia ininterrumpida.", "Funciona sin depender de la luz solar.", "Elevado costo y protocolos estrictos."));
        mazoDisponible.Add(new ComponenteEspacial("Baterías Litio-Azufre", 115000f, 120f, "Banco de almacenamiento de alta densidad energética.", "Mantiene sistemas activos en eclipses.", "Sensibles a temperaturas extremas."));
        mazoDisponible.Add(new ComponenteEspacial("Propulsores Hidracina", 95000f, 140f, "Sistema químico clásico para corrección orbital.", "Respuesta inmediata y alta confiabilidad.", "Combustible tóxico y de reserva limitada."));
        mazoDisponible.Add(new ComponenteEspacial("Motor Iónico de Xenón", 330000f, 260f, "Propulsión eléctrica de impulso específico alto.", "Ideal para mantener órbita por años.", "Empuje muy lento para maniobras rápidas."));
        mazoDisponible.Add(new ComponenteEspacial("Ruedas de Reacción 3-Ejes", 155000f, 160f, "Giroscopios de precisión para apuntar antenas.", "Estabilidad milimétrica sin gastar gas.", "Pueden saturarse si hay mucha perturbación."));
        mazoDisponible.Add(new ComponenteEspacial("Tanque Criogénico Lox", 230000f, 310f, "Depósito aislado para propelente de alto rendimiento.", "Gran capacidad de empuje para órbita alta.", "Requiere aislamiento térmico constante."));
        mazoDisponible.Add(new ComponenteEspacial("Escudo Térmico Ablativo", 130000f, 190f, "Recubrimiento disipador de calor extremo.", "Protege el núcleo ante variaciones solares.", "Añade masa estructural permanente."));
        mazoDisponible.Add(new ComponenteEspacial("Radiadores de Amoníaco", 175000f, 210f, "Paneles disipadores para electrónica de alta potencia.", "Evita sobrecalentamiento de procesadores.", "Vulnerables a perforaciones por impactos."));
        mazoDisponible.Add(new ComponenteEspacial("Antena Parabólica Banda-X", 240000f, 230f, "Transceptor direccional de alta velocidad.", "Incrementa notablemente el envío de datos.", "Consume gran parte de la energía disponible."));
        mazoDisponible.Add(new ComponenteEspacial("Ordenador de Vuelo Rad-Hard", 310000f, 200f, "Procesador blindado contra rayos cósmicos.", "Cero reinicios por radiación en órbita alta.", "Alto costo de hardware especializado."));
        mazoDisponible.Add(new ComponenteEspacial("Aislamiento Multicapa MLI", 55000f, 80f, "Manta térmica reflectante de láminas doradas.", "Muy barata y prácticamente sin peso.", "No ofrece protección física contra golpes."));
        mazoDisponible.Add(new ComponenteEspacial("Sistema Acoplamiento Mag.", 280000f, 320f, "Anillo magnético de sujeción de carga pesada.", "Soporta módulos científicos de gran tonelaje.", "Requiere energía continua para mantener cierre."));
    }

    void BarajarMazo()
    {
        for (int i = 0; i < mazoDisponible.Count; i++)
        {
            ComponenteEspacial temp = mazoDisponible[i];
            int randomIndex = Random.Range(i, mazoDisponible.Count);
            mazoDisponible[i] = mazoDisponible[randomIndex];
            mazoDisponible[randomIndex] = temp;
        }
    }

    void LlenarTiendaInicial()
    {
        componentesEnPantalla.Clear();
        int cantidadInicial = Mathf.Min(maximoBotonesVisibles, mazoDisponible.Count);

        for (int i = 0; i < cantidadInicial; i++)
        {
            SacarSiguienteComponenteDeReserva();
        }
    }

    void SacarSiguienteComponenteDeReserva()
    {
        if (mazoDisponible.Count == 0) return;

        ComponenteEspacial siguiente = mazoDisponible[0];
        mazoDisponible.RemoveAt(0);
        componentesEnPantalla.Add(siguiente);

        CrearBotonComponente(siguiente);
    }

    void CrearBotonComponente(ComponenteEspacial comp)
    {
        GameObject nuevoBoton = Instantiate(prefabBoton, contenedorBotones);
        nuevoBoton.name = $"Btn_{comp.Nombre}";
        nuevoBoton.transform.localScale = Vector3.one;

        TMP_Text textoBtn = nuevoBoton.GetComponentInChildren<TMP_Text>();
        if (textoBtn != null)
        {
            textoBtn.text = $"{comp.Nombre}\n<size=80%>${comp.Costo:N0}</size>";
            textoBtn.enableAutoSizing = true;
            textoBtn.fontSizeMin = 8;
            textoBtn.fontSizeMax = 16;
        }

        Button btnComponente = nuevoBoton.GetComponent<Button>();
        if (btnComponente != null)
        {
            btnComponente.onClick.RemoveAllListeners();
            btnComponente.onClick.AddListener(() => ComprarYReemplazar(comp, nuevoBoton));
        }

        TooltipBoton tooltipScript = nuevoBoton.GetComponent<TooltipBoton>();
        if (tooltipScript == null) tooltipScript = nuevoBoton.AddComponent<TooltipBoton>();
        tooltipScript.Configurar(comp, textoTooltip);
    }

    void ComprarYReemplazar(ComponenteEspacial compComprado, GameObject botonActual)
    {
        if (ControladorEnsamblaje.Instance != null)
        {
            ControladorEnsamblaje.Instance.costoMateriales += compComprado.Costo;

            // Suma limpia del peso del componente al total acumulado de la nave
            ControladorEnsamblaje.Instance.capacidadCargaMax += compComprado.PesoComponente;

            if (compComprado.Costo >= 300000f)
                ControladorEnsamblaje.Instance.nivelMateriales = 3;
            else if (compComprado.Costo >= 140000f)
                ControladorEnsamblaje.Instance.nivelMateriales = 2;
            else
                ControladorEnsamblaje.Instance.nivelMateriales = 1;

            ControladorEnsamblaje.Instance.RecalcularParametrosMision();
        }

        componentesEnPantalla.Remove(compComprado);
        Destroy(botonActual);

        if (textoTooltip != null)
        {
            textoTooltip.text = $"<color=green>¡Adquirido: {compComprado.Nombre}!</color>";
        }

        if (mazoDisponible.Count > 0)
        {
            SacarSiguienteComponenteDeReserva();
        }
    }
}

public class TooltipBoton : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    private ComponenteEspacial datosComponente;
    private TMP_Text pantallaTooltip;

    public void Configurar(ComponenteEspacial comp, TMP_Text destinoTooltip)
    {
        datosComponente = comp;
        pantallaTooltip = destinoTooltip;
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (pantallaTooltip != null)
        {
            pantallaTooltip.text = $"<b>{datosComponente.Nombre}</b>\n" +
                                   $"Costo: ${datosComponente.Costo:N0} | Peso: {datosComponente.PesoComponente} kg\n" +
                                   $"<i>{datosComponente.Descripcion}</i>\n" +
                                   $"<color=green>• Ventaja:</color> {datosComponente.Ventajas}\n" +
                                   $"<color=red>• Desventaja:</color> {datosComponente.Desventajas}";
        }
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (pantallaTooltip != null)
        {
            pantallaTooltip.text = "Apunta a un componente para ver sus especificaciones.";
        }
    }
}