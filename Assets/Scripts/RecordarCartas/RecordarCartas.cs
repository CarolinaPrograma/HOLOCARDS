using Google.Protobuf.Collections;
using NN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.Windows.Speech;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

public class RecordarCartas : MonoBehaviour
{
    public Detector detector;
    public GameObject uiPanel;
    public List<GameObject> cardPrefabsList;
    public GridLayoutGroup cardContainerGrid;

    private List<string> cartasReconocidas = new List<string>();
    private float remainingTime;

    private float remainingTime_panel;
    public TMP_Text time_panel_text;

    // Modalidades
    private string[] cards = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };
    int[] aleatorio = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51 };
    int[] rojas = { 1, 2, 5, 6, 9, 10, 13, 14, 17, 18, 21, 22, 25, 26, 29, 30, 33, 34, 37, 38, 41, 42, 45, 46, 49, 50 };
    int[] negras = { 3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51, 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48 };
    int[] diamantes = { 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49 }; 
    int[] corazones = { 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50 };
    int[] tréboles = { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48 }; 
    int[] picas = { 3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51 };


    public GameObject boton_mostrar_panel;
    public GameObject boton_Comprobar;
    private List<string> cartas_random = new List<string>();

    public TMP_Text contador_UI;
    public GameObject finalPanel;

    private bool isChecking = false;
    private bool isGameActive = false;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    public GameObject feedback;
    public TextMeshProUGUI texto_feedback;

    // HandMenu
    public GameObject handmenu;

    // Ayudas
    public GameObject ayuda;
    public List<RawImage> rawImages;
    public List<TMPro.TextMeshProUGUI> textMeshPros;
    private int containerIndex_reconocidas = 0;
    public GridLayoutGroup panel_reconocidas;

    // Parámetros
    private string id_juego;
    private int numero;
    private int tiempo_total;
    private int[] cards_modalidad;
    private int t_panel;

    // Estadísticas
    private int numero_pistas = 0;
    private bool exito;
    private int numero_intentos = 0;
    private int tiempo_tardado;
    public MenuPrincipal MenuPrincipal;

    public void Recordar_Cartas(string id, int cartas, int tiempo, string modalidad, int tiempo_panel)
    {
        keywordRecognizer = null; 
        if (detector == null)
        {
            UnityEngine.Debug.LogError("Detector reference not set in GameController.");
            return;
        }

        id_juego = id;
        numero = cartas;
        tiempo_total =  tiempo;
        remainingTime = tiempo_total;
        t_panel = tiempo_panel;
        remainingTime_panel = tiempo_panel;


        if (modalidad == "Aleatorio") { cards_modalidad = aleatorio; }
        else if (modalidad == "Rojas") { cards_modalidad = rojas; }
        else if (modalidad == "Negras") { cards_modalidad = negras; }
        else if (modalidad == "Picas") { cards_modalidad = picas; }
        else if (modalidad == "Treboles") { cards_modalidad = tréboles; }
        else if (modalidad == "Diamantes") { cards_modalidad = diamantes; }
        else if (modalidad == "Corazones") { cards_modalidad = corazones; }

        LoadCardPrefabs();
        Random_Cartas(cards_modalidad);

        // Configuración de palabras clave
        keywords.Add("Mostrar panel", ShowUIPanel);
        keywords.Add("Pista", Pista);
        keywords.Add("Comprobar", boton_comprobar);

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    public void EmpezarJuego()
    {
        ShowUIPanel();
    }

    // Se obtiene las prefabs de las cartas
    private void LoadCardPrefabs()
    {
        foreach (var card in cards)
        {
            GameObject prefab = Resources.Load<GameObject>($"{card}");
            if (prefab != null)
            {
                cardPrefabsList.Add(prefab);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Prefab for card {card} not found in Resources!");
            }
        }
    }


    // Método para ajustar la cuadrícula de distribución según la cantidad de cartas
    private void AdjustGridLayout(int count)
    {
        if (count <= 3)
        {
            cardContainerGrid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            cardContainerGrid.constraintCount = 1;
        }
        else if (count <= 6)
        {
            cardContainerGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cardContainerGrid.constraintCount = 2;
        }
        else
        {
            cardContainerGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cardContainerGrid.constraintCount = 3;
        }

        cardContainerGrid.cellSize = new Vector2(110, 110);
        cardContainerGrid.spacing = new Vector2(1, 1);
    }

    private void Random_Cartas(int[] indices_disponibles)
    {
        HashSet<int> indices_cartas = new HashSet<int>();

        // Selecciona índices de la lista de índices disponibles
        while (indices_cartas.Count < numero)
        {
            int randomIndex = Random.Range(0, indices_disponibles.Length);
            indices_cartas.Add(indices_disponibles[randomIndex]);
        }

        List<int> lista_indices_cartas = new List<int>(indices_cartas);

        AdjustGridLayout(numero);

        for (int i = 0; i < numero; i++)
        {
            int carta_index = lista_indices_cartas[i];
            string carta = cards[carta_index];
            UnityEngine.Debug.Log($"{carta}");
            cartas_random.Add(carta);
            InstantiateCardPrefab(carta, cardContainerGrid);
        }
    }

    private void InstantiateCardPrefab(string card, GridLayoutGroup container)
    {
        UnityEngine.Debug.Log("InstantiateCardPrefab");
        GameObject prefab = cardPrefabsList.Find(p => p.name == card);
        if (prefab != null)
        {
            Instantiate(prefab, container.transform);
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Prefab for card {card} not found in list!");
        }
    }

    private void EliminarCardContainers(GridLayoutGroup container)
    {
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void Update()
    {
        UnityEngine.Debug.Log("Entro a update");
        if (!isGameActive) return;

        // Controlar el tiempo restante
        remainingTime -= Time.deltaTime;
        contador_UI.text = $"{System.Convert.ToInt32(remainingTime)}";

        if (remainingTime <= 0)
        {
            exito = false;
            tiempo_tardado = tiempo_total;
            EndGame("Tiempo agotado");
            return;
        }

        if (isChecking && detector.HasResults() && !uiPanel.activeSelf)
        {
            var results = detector.GetResults();
            HandleResults(results);
        }
    }

    private void HandleResults(IEnumerable<ResultBox> results)
    {
        foreach (var box in results)
        {
            var card = cards[box.bestClassIndex];
            ReconocerCarta(card);

        }
    }

    public IEnumerator CheckForCards()
    {
        feedback.SetActive(true);
        texto_feedback.SetText("Reconociendo...");
        isChecking = true;
        UnityEngine.Debug.Log("Reconociendo...");
        yield return new WaitForSeconds(5); 
        Invoke("Cerrar_Feedback", 0f);
        isChecking = false;
    }

    private void ReconocerCarta(string card)
    {
        if (cartasReconocidas.Contains(card))
        {
            return;
        }

        InstantiateCardPrefab(card, panel_reconocidas);

        cartasReconocidas.Add(card);
        UnityEngine.Debug.Log($"Carta reconocida: {card}");

        if (cartasReconocidas.Count == numero)
        {
            ComprobarCartasReconocidas();
        }
    }

    private void ComprobarCartasReconocidas()
    {
        bool areEqual = comparar_Listas(cartasReconocidas, cartas_random);

        foreach (string carta in cartasReconocidas)
        {
            UnityEngine.Debug.Log($"Carta reconcoida: {carta}");
        }

        if (areEqual)
        {
            UnityEngine.Debug.Log("Las cartas reconocidas coinciden con las cartas prefabricadas.");
            feedback.SetActive(true);
            texto_feedback.SetText("Correcto!");
            Invoke("Cerrar_Feedbak", 2f);
            EliminarCardContainers(panel_reconocidas);
            exito = true;
            tiempo_tardado = (int)(tiempo_total - remainingTime);
            EndGame("¡Enhorabuena!");
        }
        else
        {
            UnityEngine.Debug.Log("Las cartas reconocidas no coinciden con las cartas prefabricadas.");
            feedback.SetActive(true);
            texto_feedback.SetText("Vuelve a intentarlo!");
            cartasReconocidas.Clear();
            Invoke("Cerrar_Feedback", 2f);
            numero_intentos++;
            EliminarCardContainers(panel_reconocidas);
        }
    }

    private bool comparar_Listas(List<string> a, List<string> b)
    {
        bool equal = true;
        foreach (var element in a)
        {
            if (!b.Contains(element))
            {
                UnityEngine.Debug.Log($"No contiene {element}");
                equal = false;
                break;
            }
        }
        return equal;
    }

    public void StartGame()
    {
        handmenu.SetActive(true);
        uiPanel.SetActive(false);
        boton_mostrar_panel.SetActive(true);
        boton_Comprobar.SetActive(true);
    }


    // AYUDAS Y VOZ

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        UnityEngine.Debug.Log("Entro a OnPhraseRecognized");
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    public void ShowUIPanel()
    {
        StartCoroutine(TiempoPanel());
    }

    public IEnumerator TiempoPanel()
    {
        uiPanel.SetActive(true);

        while (remainingTime_panel > 0)
        {

            time_panel_text.text = remainingTime_panel.ToString("F1") + "s";
            yield return null;
            remainingTime_panel -= Time.deltaTime;
        }

        time_panel_text.text = "0s";

        uiPanel.SetActive(false);
        isGameActive = true;
    }

    public void Pista()
    {
        numero_pistas++;
        UnityEngine.Debug.Log("Pista activada");
        string pista = GenerarPistaAleatoria();
        ayuda.SetActive(true);
        texto_feedback.text = pista;
        Invoke("Desactiar_ayuda", 5f);
    }

    public void boton_comprobar()
    {
        StartCoroutine(CheckForCards());
    }

    private void Cerrar_Feedback()
    {
        feedback.SetActive(false);
    }

    private void OnDestroy()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Dispose();
        }
    }

    private string GenerarPistaAleatoria()
    {
        System.Random rand = new System.Random();
        int tipoPista = rand.Next(3); // Generar un número aleatorio entre 0 y 2
        UnityEngine.Debug.Log(cartas_random);
        switch (tipoPista)
        {
            case 0:
                // Pista: Números de las cartas
                return GenerarPistaNumeros();

            case 1:
                // Pista: Palos de las cartas
                return GenerarPistaPalos();

            case 2:
                // Pista: Cartas enteras
                return GenerarPistaCartasEnteras();

            default:
                return "No hay pista disponible.";
        }
    }

    private string GenerarPistaNumeros()
    {
        UnityEngine.Debug.Log("GenerarPistaNumeros");
        ClearImagesAndTexts();
        List<string> numeros = cartas_random.Select(c => c.Substring(0, c.Length - 1)).Distinct().ToList();
        UnityEngine.Debug.Log($"{numeros}");
        for (int i = 0; i < numeros.Count && i < textMeshPros.Count; i++)
        {
            textMeshPros[i].text = numeros[i];
            textMeshPros[i].color = Color.black;
        }
        return "Los números de las cartas son: " + string.Join(", ", numeros);
    }

    private string GenerarPistaPalos()
    {
        UnityEngine.Debug.Log("GenerarPistaPalos");
        ClearImagesAndTexts();
        for (int i = 0; i < cartas_random.Count && i < rawImages.Count; i++)
        {
            string palo = cartas_random[i].Last().ToString();
            string imagenPath = $"imagenes/{GetPaloName(palo)}";
            Texture2D paloImage = Resources.Load<Texture2D>(imagenPath);

            if (paloImage != null)
            {
                rawImages[i].texture = paloImage;
                textMeshPros[i].text = ""; 
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Image for {palo} not found at path {imagenPath}");
            }
        }
        return "Los palos de las cartas son: " + string.Join(", ", cartas_random.Select(c => c.Last().ToString()).Distinct());
    }

    private string GenerarPistaCartasEnteras()
    {
        UnityEngine.Debug.Log("GenerarPistaCartasEnteras");
        ClearImagesAndTexts();
        if (cartas_random.Count == 0) return "No hay cartas disponibles.";

        System.Random rand = new System.Random();
        int randomIndex = rand.Next(cartas_random.Count);
        string carta = cartas_random[randomIndex];

        // Asignar la imagen del palo
        string palo = carta.Last().ToString();
        string imagenPath = $"imagenes/{GetPaloName(palo)}";
        Texture2D paloImage = Resources.Load<Texture2D>(imagenPath);

        if (paloImage != null)
        {
            rawImages[0].texture = paloImage; // Asignar a la primera imagen
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Image for {palo} not found at path {imagenPath}");
        }

        // Asignar el número de la carta
        textMeshPros[0].text = carta.Substring(0, carta.Length - 1);
        textMeshPros[0].color = Color.white;

        return $"Una de las cartas es: {carta}";
    }

    private string GetPaloName(string palo)
    {
        switch (palo)
        {
            case "H":
                return "corazones";
            case "D":
                return "diamantes";
            case "S":
                return "picas";
            case "C":
                return "treboles";
            default:
                return null;
        }
    }

    private void ClearImagesAndTexts()
    {
        foreach (var rawImage in rawImages)
        {
            rawImage.texture = null;
        }
        foreach (var textMeshPro in textMeshPros)
        {
            textMeshPro.text = "";
        }
    }

    private void Desactiar_ayuda()
    {
        ayuda.SetActive(false);
    }

    private async void EndGame(string message)
    {
        isGameActive = false;
        keywordRecognizer.Stop();
        keywordRecognizer.Dispose();
        UnityEngine.Debug.Log(message);
        await MenuPrincipal.resultados_RecordarCartas(id_juego, numero_pistas, exito, numero_intentos, tiempo_tardado);
        finalPanel.SetActive(true);
    }
}
