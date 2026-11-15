using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Playables;
using UnityEngine.SceneManagement; // Necesario para cambiar de sala/escena
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Se ejecuta al final para que ningun otro script de camara la sobrescriba
[DefaultExecutionOrder(1000)]
public class DespegueCohete : MonoBehaviour
{
    [Header("VELOCIDAD DE DESPEGUE")]
    public float velocidadInicial = 20f;
    public float aceleracion = 10f;
    public float velocidadMaxima = 60f;
    public float retrasoDespegue = 2f;

    [Header("ORBITA ALREDEDOR DE LA TIERRA")]
    [Tooltip("Arrastra aqui la Tierra. Si queda vacio se busca un objeto llamado Tierra o Earth")]
    public Transform tierra;
    [Tooltip("Se usa solo si no se encuentra la Tierra")]
    public Vector3 posicionTierraManual = new Vector3(0f, 160f, 0f);
    [Tooltip("0 = automatico: radio de la Tierra x factor + margen")]
    public float radioOrbita = 0f;
    [Tooltip("Que tan lejos de la superficie gira (1.5 = 50% mas grande que la Tierra)")]
    public float factorRadio = 1.5f;
    public float margenSobreTierra = 10f;
    [Tooltip("Grados por segundo")]
    public float velocidadOrbita = 12f;
    public bool sentidoHorario = false;
    [Tooltip("Segundos para curvarse del despegue a la orbita")]
    public float duracionTransicion = 4f;
    public bool fuegoDuranteOrbita = true;

    [Header("CAMBIO DE SALA (POST-MISIÓN EN VR)")]
    [Tooltip("Segundos totales desde que inicia el lanzamiento para cambiar de sala")]
    public float tiempoParaMostrarResultados = 10f;

    [Tooltip("Escribe aquí exactamente el nombre de la siguiente escena que quieres cargar")]
    public string nombreSiguienteSala = "NombreDeTuEscena";

    private bool cambioSalaEjecutado = false;

    [Header("EFECTOS DE FUEGO")]
    public Transform salidaMotores;
    public Material materialLlamas;
    public float tamanoLlamas = 1f;
    [Tooltip("Activo: el fuego se queda pegado a los motores y no se estira ni se rompe al acelerar")]
    public bool fuegoPegadoAlCohete = true;

    [Header("CAMARA")]
    public bool controlarCamara = true;
    [Tooltip("Si se deja vacio usa la Main Camera")]
    public Camera camara;
    [Tooltip("Tecla C para cambiar entre vista exterior y vista desde el cohete")]
    public bool empezarEnPrimeraPersona = false;
    public float sensibilidadRaton = 2f;
    public float velocidadTeclado = 90f;
    public float suavizadoCamara = 12f;

    [Header("CAMARA EXTERIOR (tercera persona)")]
    [Tooltip("0 = automatico segun el tamano del cohete")]
    public float distanciaCamara = 0f;
    public float velocidadZoom = 5f;
    public bool giroAutomatico = false;
    public float velocidadGiroAutomatico = 10f;

    [Header("CAMARA DESDE EL COHETE (primera persona)")]
    [Tooltip("Posicion de la ventana en el cohete: 0 = base, 1 = punta")]
    [Range(0f, 1.2f)] public float alturaVentana = 0.8f;
    [Tooltip("Que tan afuera del casco se coloca la camara (0 = centro)")]
    public float separacionDelCasco = 1.05f;
    public float limiteMiradaHorizontal = 170f;
    public float limiteMiradaVertical = 80f;

    [Header("CALIDAD VISUAL")]
    [Tooltip("Desactiva el Motion Blur del Volume (causa que el cohete se vea borroso al moverse)")]
    public bool quitarMotionBlur = true;
    [Tooltip("Obliga al modelo a usar su LOD de mayor calidad")]
    public bool forzarMaximaCalidadLOD = true;

    [Header("SEPARACION DE PROPULSORES")]
    [Tooltip("Activo: los propulsores se quedan unidos al cohete todo el vuelo")]
    public bool omitirSeparacion = true;

    [Header("ANTI DEFORMACION")]
    [Tooltip("Saca el cohete de un padre con escala desigual (causa que el modelo se estire al girar)")]
    public bool separarDePadreDeformado = true;
    [Tooltip("Quita el Temporal Anti-Aliasing (deja estelas y 'derrite' objetos que se mueven rapido)")]
    public bool quitarTAA = true;
    [Tooltip("Un campo de vision muy alto estira el cohete en los bordes de la pantalla")]
    [Range(30f, 90f)] public float campoDeVision = 60f;

    private struct PoseOriginal
    {
        public Transform pieza;
        public Transform padre;
        public Vector3 posicion;
        public Quaternion rotacion;
        public Vector3 escala;
    }
    private readonly List<PoseOriginal> poses = new List<PoseOriginal>();

    private enum Fase { Espera, Despegue, Transicion, Orbita }
    private Fase fase = Fase.Espera;

    private ParticleSystem fuego;
    private float velocidadActual;
    private float tiempo;

    // Orbita
    private float radioActual;
    private float radioTierra;
    private Vector3 direccionInicial;
    private float anguloOrbita;
    private float progresoTransicion;
    private Vector3 dirInicioTransicion;
    private float distInicioTransicion;
    private Quaternion rotInicioTransicion;

    // Modelo
    private Vector3 centroLocal;
    private float alturaCohete = 10f;
    private float anchoCohete = 2f;
    private float baseLocalY, puntaLocalY;

    // Camara
    private bool primeraPersona;
    private float yaw, pitch, yawObj, pitchObj;
    private float miraYaw, miraPitch, miraYawObj, miraPitchObj;
    private float distancia, distanciaObj;

    void Start()
    {
        velocidadActual = velocidadInicial;

        if (separarDePadreDeformado) CorregirPadreDeformado();
        RevisarEscalaPropia();

        if (omitirSeparacion) DesactivarSeparacion();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        if (forzarMaximaCalidadLOD)
        {
            foreach (LODGroup lod in GetComponentsInChildren<LODGroup>())
                lod.ForceLOD(0);
        }

        MedirCohete();
        BuscarTierra();
        CalcularRadioOrbita();
        CrearFuego();
        PrepararCamara();

        if (quitarMotionBlur) QuitarMotionBlur();

        Debug.Log("Cohete NASA preparado | Radio Tierra: " + radioTierra.ToString("F1") +
                  " | Radio orbita: " + radioActual.ToString("F1"));
    }

    void DesactivarSeparacion()
    {
        int desactivados = 0;
        foreach (Animator a in GetComponentsInChildren<Animator>(true))
        {
            a.Rebind();
            a.Update(0f);
            a.enabled = false;
            desactivados++;
        }

        foreach (Animation a in GetComponentsInChildren<Animation>(true))
        {
            a.Stop();
            a.enabled = false;
            desactivados++;
        }

        foreach (PlayableDirector d in GetComponentsInChildren<PlayableDirector>(true))
        {
            d.Stop();
            d.enabled = false;
            desactivados++;
        }

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true))
        {
            if (rb.transform == transform) continue;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (Joint j in GetComponentsInChildren<Joint>(true))
            j.breakForce = Mathf.Infinity;

        poses.Clear();
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t == transform) continue;
            poses.Add(new PoseOriginal
            {
                pieza = t,
                padre = t.parent,
                posicion = t.localPosition,
                rotacion = t.localRotation,
                escala = t.localScale
            });
        }
    }

    void BloquearPiezas()
    {
        for (int i = 0; i < poses.Count; i++)
        {
            PoseOriginal p = poses[i];
            if (p.pieza == null) continue;

            if (p.pieza.parent != p.padre && p.padre != null)
                p.pieza.SetParent(p.padre, false);

            p.pieza.localPosition = p.posicion;
            p.pieza.localRotation = p.rotacion;
            p.pieza.localScale = p.escala;

            if (!p.pieza.gameObject.activeSelf)
                p.pieza.gameObject.SetActive(true);
        }
    }

    static bool EscalaDesigual(Vector3 s)
    {
        float a = Mathf.Abs(s.x), b = Mathf.Abs(s.y), c = Mathf.Abs(s.z);
        float max = Mathf.Max(a, b, c);
        if (max < 0.0001f) return false;
        return (max - Mathf.Min(a, b, c)) / max > 0.01f;
    }

    void CorregirPadreDeformado()
    {
        Transform p = transform.parent;
        bool deformado = false;

        while (p != null)
        {
            if (EscalaDesigual(p.localScale)) { deformado = true; break; }
            p = p.parent;
        }

        if (deformado)
        {
            Vector3 escalaMundo = transform.lossyScale;
            transform.SetParent(null, true);
            transform.localScale = escalaMundo;
        }
    }

    void RevisarEscalaPropia()
    {
        if (!EscalaDesigual(transform.localScale)) return;

        foreach (Transform hijo in GetComponentsInChildren<Transform>())
        {
            if (hijo == transform) continue;
            if (Quaternion.Angle(hijo.localRotation, Quaternion.identity) > 0.5f) return;
        }
    }

    void CorregirCamaraDeformante()
    {
        if (camara == null) return;
        camara.fieldOfView = campoDeVision;

        if (quitarTAA)
        {
            UniversalAdditionalCameraData datos = camara.GetUniversalAdditionalCameraData();
            if (datos != null && datos.antialiasing == AntialiasingMode.TemporalAntiAliasing)
                datos.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }
    }

    bool CalcularLimites(Transform raiz, out Bounds limites)
    {
        limites = new Bounds();
        bool encontrado = false;

        foreach (Renderer r in raiz.GetComponentsInChildren<Renderer>())
        {
            if (!(r is MeshRenderer || r is SkinnedMeshRenderer)) continue;

            if (!encontrado) { limites = r.bounds; encontrado = true; }
            else limites.Encapsulate(r.bounds);
        }
        return encontrado;
    }

    void MedirCohete()
    {
        Bounds b;
        if (CalcularLimites(transform, out b))
        {
            centroLocal = transform.InverseTransformPoint(b.center);
            baseLocalY = transform.InverseTransformPoint(new Vector3(b.center.x, b.min.y, b.center.z)).y;
            puntaLocalY = transform.InverseTransformPoint(new Vector3(b.center.x, b.max.y, b.center.z)).y;
            alturaCohete = Mathf.Max(b.size.y, 0.1f);
            anchoCohete = Mathf.Max(b.size.x, b.size.z, 0.1f);
        }
        else
        {
            centroLocal = Vector3.zero;
            baseLocalY = -1f;
            puntaLocalY = 1f;
        }
    }

    void BuscarTierra()
    {
        if (tierra != null) return;

        string[] nombres = { "Tierra", "tierra", "Earth", "earth", "Planeta", "Planet" };
        foreach (string n in nombres)
        {
            GameObject g = GameObject.Find(n);
            if (g != null) { tierra = g.transform; break; }
        }
    }

    void CalcularRadioOrbita()
    {
        radioTierra = RadioTierra();
        radioActual = radioOrbita > 0f ? radioOrbita : radioTierra * factorRadio + margenSobreTierra + alturaCohete * 0.5f;
        radioActual = Mathf.Max(radioActual, radioTierra + alturaCohete);
    }

    float RadioTierra()
    {
        if (tierra == null) return 0f;

        Vector3 escala = tierra.lossyScale;
        float maxEscala = Mathf.Max(Mathf.Abs(escala.x), Mathf.Abs(escala.y), Mathf.Abs(escala.z));

        SphereCollider esfera = tierra.GetComponentInChildren<SphereCollider>();
        if (esfera != null) return esfera.radius * maxEscala;

        Bounds b;
        if (CalcularLimites(tierra, out b)) return Mathf.Max(b.extents.x, b.extents.y, b.extents.z);

        return maxEscala * 0.5f;
    }

    Vector3 CentroTierra()
    {
        return tierra != null ? tierra.position : posicionTierraManual;
    }

    void CrearFuego()
    {
        GameObject objeto = new GameObject("Fuego_Motores");
        objeto.transform.SetParent(transform, false);

        float escala = Mathf.Clamp(alturaCohete * 0.08f, 0.5f, 6f);

        if (salidaMotores != null)
            objeto.transform.position = salidaMotores.position;
        else
            objeto.transform.localPosition = new Vector3(centroLocal.x, baseLocalY, centroLocal.z);

        objeto.transform.rotation = Quaternion.LookRotation(-transform.up, transform.forward);

        Vector3 ls = transform.lossyScale;
        objeto.transform.localScale = new Vector3(
            1f / Mathf.Max(Mathf.Abs(ls.x), 0.0001f),
            1f / Mathf.Max(Mathf.Abs(ls.y), 0.0001f),
            1f / Mathf.Max(Mathf.Abs(ls.z), 0.0001f)
        );

        fuego = objeto.AddComponent<ParticleSystem>();

        var main = fuego.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = 0.8f;
        main.startSpeed = 10f * escala;
        main.startSize = escala * tamanoLlamas;
        main.startColor = Color.white;
        main.maxParticles = 400;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.simulationSpace = fuegoPegadoAlCohete ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;

        var emision = fuego.emission;
        emision.enabled = true;
        emision.rateOverTime = 150f;

        var forma = fuego.shape;
        forma.enabled = true;
        forma.shapeType = ParticleSystemShapeType.Cone;
        forma.angle = 10f;
        forma.radius = escala * 0.25f;

        var colores = fuego.colorOverLifetime;
        colores.enabled = true;

        Gradient degradado = new Gradient();
        degradado.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 1f, 0.1f), 0f),
                new GradientColorKey(new Color(1f, 0.45f, 0f), 0.25f),
                new GradientColorKey(new Color(1f, 0.05f, 0f), 0.65f),
                new GradientColorKey(new Color(0.4f, 0f, 0f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.3f),
                new GradientAlphaKey(0.7f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colores.color = new ParticleSystem.MinMaxGradient(degradado);

        var tamanos = fuego.sizeOverLifetime;
        tamanos.enabled = true;
        tamanos.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.1f));

        var renderizador = objeto.GetComponent<ParticleSystemRenderer>();
        renderizador.renderMode = ParticleSystemRenderMode.Billboard;

        if (materialLlamas != null)
        {
            renderizador.material = materialLlamas;
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null)
            {
                Material material = new Material(shader);
                material.SetFloat("_Surface", 1f);
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.One);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
                renderizador.material = material;
            }
        }

        fuego.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        // Temporizador para viajar automáticamente a la siguiente sala tras los segundos configurados en el Inspector
        if (!cambioSalaEjecutado && tiempo >= tiempoParaMostrarResultados)
        {
            cambioSalaEjecutado = true;
            ViajarASiguienteSala();
        }

        switch (fase)
        {
            case Fase.Espera:
                if (tiempo >= retrasoDespegue)
                {
                    fase = Fase.Despegue;
                    if (fuego != null) fuego.Play();
                }
                break;

            case Fase.Despegue: ActualizarDespegue(); break;
            case Fase.Transicion: ActualizarTransicion(); break;
            case Fase.Orbita: ActualizarOrbita(); break;
        }

        LeerTeclaCambioVista();
    }

    void ViajarASiguienteSala()
    {
        if (string.IsNullOrEmpty(nombreSiguienteSala))
        {
            Debug.LogError("[Transición VR] ¡El nombre de la siguiente sala está vacío en el Inspector!");
            return;
        }

        Debug.Log("[Transición VR] Han pasado " + tiempoParaMostrarResultados + " segundos. Cargando sala: " + nombreSiguienteSala);
        SceneManager.LoadScene(nombreSiguienteSala);
    }

    void ActualizarDespegue()
    {
        velocidadActual = Mathf.Min(velocidadActual + aceleracion * Time.deltaTime, velocidadMaxima);
        transform.position += Vector3.up * velocidadActual * Time.deltaTime;

        Vector3 centro = CentroTierra();
        float distancia = Vector3.Distance(transform.position, centro);

        if (distancia <= radioActual || transform.position.y >= centro.y)
            IniciarTransicion();
    }

    void IniciarTransicion()
    {
        CalcularRadioOrbita();

        Vector3 centro = CentroTierra();
        Vector3 desde = transform.position - centro;

        distInicioTransicion = Mathf.Max(desde.magnitude, 0.01f);
        dirInicioTransicion = desde / distInicioTransicion;

        Vector3 horizontal = Vector3.ProjectOnPlane(desde, Vector3.up);
        if (horizontal.sqrMagnitude < 0.01f) horizontal = Vector3.ProjectOnPlane(-transform.forward, Vector3.up);
        if (horizontal.sqrMagnitude < 0.01f) horizontal = Vector3.back;

        direccionInicial = horizontal.normalized;
        rotInicioTransicion = transform.rotation;
        progresoTransicion = 0f;
        anguloOrbita = 0f;

        fase = Fase.Transicion;
    }

    void ActualizarTransicion()
    {
        progresoTransicion += Time.deltaTime / Mathf.Max(0.01f, duracionTransicion);
        float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progresoTransicion));

        Vector3 dir = Vector3.Slerp(dirInicioTransicion, direccionInicial, s).normalized;
        float dist = Mathf.Lerp(distInicioTransicion, radioActual, s);
        dist = Mathf.Max(dist, radioTierra + alturaCohete * 0.5f);

        transform.position = CentroTierra() + dir * dist;
        transform.rotation = Quaternion.Slerp(rotInicioTransicion, RotacionEnOrbita(direccionInicial), s);

        if (progresoTransicion >= 1f)
        {
            fase = Fase.Orbita;
            if (!fuegoDuranteOrbita && fuego != null)
                fuego.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    void ActualizarOrbita()
    {
        anguloOrbita += velocidadOrbita * Signo() * Time.deltaTime;
        Vector3 dir = Quaternion.AngleAxis(anguloOrbita, Vector3.up) * direccionInicial;

        transform.position = CentroTierra() + dir * radioActual;
        transform.rotation = RotacionEnOrbita(dir);
    }

    Quaternion RotacionEnOrbita(Vector3 radial)
    {
        Vector3 tangente = Vector3.Cross(Vector3.up, radial) * Signo();
        return Quaternion.LookRotation(radial, tangente);
    }

    float Signo() { return sentidoHorario ? 1f : -1f; }

    void PrepararCamara()
    {
        if (camara == null) camara = Camera.main;
        if (camara == null) return;

        CorregirCamaraDeformante();

        if (!controlarCamara) return;

        Behaviour brain = camara.GetComponent("CinemachineBrain") as Behaviour;
        if (brain != null) brain.enabled = false;

        if (camara.transform.IsChildOf(transform))
            camara.transform.SetParent(null);

        camara.nearClipPlane = Mathf.Min(camara.nearClipPlane, 0.05f);
        camara.farClipPlane = Mathf.Max(camara.farClipPlane, (radioActual + radioTierra) * 6f + 2000f);

        distanciaObj = distanciaCamara > 0f ? distanciaCamara : Mathf.Max(alturaCohete * 2.2f, anchoCohete * 4f);
        distancia = distanciaObj;

        yaw = yawObj = transform.eulerAngles.y + 180f;
        pitch = pitchObj = 10f;

        primeraPersona = empezarEnPrimeraPersona;
    }

    void LeerTeclaCambioVista()
    {
        bool cambiar = false;
        bool soltarCursor = false;

#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            cambiar = k.cKey.wasPressedThisFrame || k.vKey.wasPressedThisFrame;
            soltarCursor = k.escapeKey.wasPressedThisFrame;
        }
#else
        cambiar = Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.V);
        soltarCursor = Input.GetKeyDown(KeyCode.Escape);
#endif

        if (cambiar)
        {
            primeraPersona = !primeraPersona;
            miraYaw = miraYawObj = 0f;
            miraPitch = miraPitchObj = 0f;
        }

        if (soltarCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LateUpdate()
    {
        if (omitirSeparacion) BloquearPiezas();

        if (!controlarCamara || camara == null) return;

        Vector2 raton;
        Vector2 teclas;
        float rueda;
        LeerEntrada(out raton, out teclas, out rueda);

        Vector2 giro = raton * sensibilidadRaton + teclas * velocidadTeclado * Time.deltaTime;
        float k = 1f - Mathf.Exp(-suavizadoCamara * Time.deltaTime);

        if (primeraPersona)
            CamaraPrimeraPersona(giro, k);
        else
            CamaraTerceraPersona(giro, rueda, k);
    }

    void CamaraTerceraPersona(Vector2 giro, float rueda, float k)
    {
        if (giro.sqrMagnitude > 0.0001f)
        {
            yawObj += giro.x;
            pitchObj -= giro.y;
        }
        else if (giroAutomatico && fase != Fase.Espera)
        {
            yawObj += velocidadGiroAutomatico * Time.deltaTime;
        }

        pitchObj = Mathf.Clamp(pitchObj, -85f, 85f);

        float minDist = Mathf.Max(alturaCohete * 0.8f, anchoCohete * 1.5f);
        float maxDist = alturaCohete * 20f + 50f;
        distanciaObj = Mathf.Clamp(distanciaObj * (1f - rueda * velocidadZoom * 0.02f), minDist, maxDist);

        yaw = Mathf.LerpAngle(yaw, yawObj, k);
        pitch = Mathf.Lerp(pitch, pitchObj, k);
        distancia = Mathf.Lerp(distancia, distanciaObj, k);

        Vector3 foco = transform.TransformPoint(centroLocal);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        camara.transform.position = foco - rot * Vector3.forward * distancia;
        camara.transform.rotation = rot;
    }

    void CamaraPrimeraPersona(Vector2 giro, float k)
    {
        miraYawObj = Mathf.Clamp(miraYawObj + giro.x, -limiteMiradaHorizontal, limiteMiradaHorizontal);
        miraPitchObj = Mathf.Clamp(miraPitchObj - giro.y, -limiteMiradaVertical, limiteMiradaVertical);

        miraYaw = Mathf.Lerp(miraYaw, miraYawObj, k);
        miraPitch = Mathf.Lerp(miraPitch, miraPitchObj, k);

        float y = Mathf.Lerp(baseLocalY, puntaLocalY, alturaVentana);
        Vector3 ventanaLocal = new Vector3(centroLocal.x, y, centroLocal.z);

        Vector3 frente = transform.forward;
        float separacion = anchoCohete * 0.5f * separacionDelCasco;
        Vector3 posicion = transform.TransformPoint(ventanaLocal) + frente * separacion;

        Quaternion baseRot = Quaternion.LookRotation(frente, transform.up);
        Quaternion mirada = baseRot * Quaternion.Euler(miraPitch, miraYaw, 0f);

        camara.transform.position = posicion;
        camara.transform.rotation = mirada;
    }

    void LeerEntrada(out Vector2 raton, out Vector2 teclas, out float rueda)
    {
        raton = Vector2.zero;
        teclas = Vector2.zero;
        rueda = 0f;

#if ENABLE_INPUT_SYSTEM
        Mouse m = Mouse.current;
        if (m != null)
        {
            if (m.leftButton.isPressed || m.rightButton.isPressed)
                raton = m.delta.ReadValue() * 0.1f;
            rueda = Mathf.Clamp(m.scroll.ReadValue().y, -1f, 1f);
        }

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) teclas.x -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) teclas.x += 1f;
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) teclas.y += 1f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed) teclas.y -= 1f;
        }
#else
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            raton = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        rueda = Mathf.Clamp(Input.mouseScrollDelta.y, -1f, 1f);

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) teclas.x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) teclas.x += 1f;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) teclas.y += 1f;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) teclas.y -= 1f;
#endif
    }

    void QuitarMotionBlur()
    {
        foreach (Volume v in FindObjectsByType<Volume>(FindObjectsSortMode.None))
        {
            if (v.sharedProfile == null) continue;

            foreach (VolumeComponent c in v.profile.components)
            {
                if (c.GetType().Name == "MotionBlur")
                    c.active = false;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 centro = tierra != null ? tierra.position : posicionTierraManual;
        float r = radioOrbita > 0f ? radioOrbita : Mathf.Max(radioActual, 1f);

        if (!Application.isPlaying && radioOrbita <= 0f && tierra != null)
        {
            Bounds b;
            float rt = CalcularLimites(tierra, out b) ? Mathf.Max(b.extents.x, b.extents.y, b.extents.z) : 0f;
            r = rt * factorRadio + margenSobreTierra;
        }

        Gizmos.color = Color.cyan;
        Vector3 anterior = centro + Vector3.forward * r;
        for (int i = 1; i <= 64; i++)
        {
            float a = i / 64f * Mathf.PI * 2f;
            Vector3 p = centro + new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * r;
            Gizmos.DrawLine(anterior, p);
            anterior = p;
        }
    }
}