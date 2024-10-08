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

public class RecordarCartas : MonoBehaviour
{
    private int numero = 4;

    public Detector detector;
    public GameObject uiPanel;
    public List<GameObject> cardContainers;
    public List<GameObject> cardPrefabsList;

    private List<string> cartasReconocidas = new List<string>();

    private string[] cards = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };
    public GameObject boton_mostrar_panel;
    public GameObject boton_Comprobar;
    private List<string> cartas_random = new List<string>();

    private bool isChecking = false;

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

    void Start()
    {
        if (detector == null)
        {
            Debug.LogError("Detector reference not set in GameController.");
            return;
        }

        LoadCardPrefabs();
        Random_Cartas();
        ShowUIPanel();

        // Configuración de palabras clave
        keywords.Add("Mostrar panel", ShowUIPanel);
        keywords.Add("Pista", Pista);
        keywords.Add("Comprobar", boton_comprobar);

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
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
                Debug.LogWarning($"Prefab for card {card} not found in Resources!");
            }
        }
    }

    // Se eligen 3 cartas aleatorias
    private void Random_Cartas()
    {
        HashSet<int> indices_cartas = new HashSet<int>();


        while (indices_cartas.Count < 4)
        {
            indices_cartas.Add(Random.Range(0, cards.Length));
        }

        List<int> lista_indices_cartas = new List<int>(indices_cartas);

        for (int i = 0; i < numero; i++)
        {
            int carta_index = lista_indices_cartas[i];
            string carta = cards[carta_index];
            Debug.Log($"{carta}");
            cartas_random.Add(carta);
            InstantiateCardPrefab(i, carta);
        }
    }

    private void InstantiateCardPrefab(int index, string card)
    {
        if (index < cardContainers.Count)
        {
            // Buscar el prefab de la carta en la lista
            GameObject prefab = cardPrefabsList.Find(p => p.name == card);
            if (prefab != null)
            {
                GameObject cardInstance = Instantiate(prefab, cardContainers[index].transform);
            }
            else
            {
                Debug.LogWarning($"Prefab for card {card} not found in list!");
            }
        }

        else
        {
            Debug.LogWarning("Index out of range for cardContainers or fraseTexts.");
        }
    }

    private void EliminarCardContainers()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (Transform child in cardContainers[i].transform)
            {
                Destroy(child.gameObject);
            }
        }
    }


    void Update()
    {
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
        Debug.Log("Reconociendo...");
        yield return new WaitForSeconds(7); // Espera 5 segundos
        Invoke("Cerrar_Feedback", 0f);
        isChecking = false;
    }

    private void ReconocerCarta(string card)
    {
        if (cartasReconocidas.Contains(card))
        {
            //Debug.Log($"La carta {card} ya ha sido reconocida anteriormente.");
            return; // Salir si la carta ya ha sido reconocida
        }

        if (containerIndex_reconocidas < cardContainers.Count)
        {
            InstantiateCardPrefab(containerIndex_reconocidas, card);
            containerIndex_reconocidas++;
        }

        cartasReconocidas.Add(card);
        Debug.Log($"Carta reconocida: {card}");

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
            Debug.Log($"Carta reconcoida: {carta}");
        }

        if (areEqual)
        {
            Debug.Log("Las cartas reconocidas coinciden con las cartas prefabricadas.");
            feedback.SetActive(true);
            texto_feedback.SetText("Correcto!");
            Invoke("Cerrar_Feedbak", 4f);
            EliminarCardContainers();

            TerminarJuego();
        }
        else
        {
            Debug.Log("Las cartas reconocidas no coinciden con las cartas prefabricadas.");
            feedback.SetActive(true);
            texto_feedback.SetText("Vuelve a intentarlo!");
            cartasReconocidas.Clear();
            Invoke("Cerrar_Feedback", 4f);
            EliminarCardContainers();
        }
    }

    private bool comparar_Listas(List<string> a, List<string> b)
    {
        bool equal = true;
        foreach (var element in a)
        {
            if (!b.Contains(element))
            {
                Debug.Log($"No contiene {element}");
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
        Debug.Log("Entro a OnPhraseRecognized");
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    public void ShowUIPanel()
    {
        uiPanel.SetActive(true);
    }

    public void Pista()
    {
        Debug.Log("Pista activada");
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
        Debug.Log("GenerarPistaNumeros");
        ClearImagesAndTexts();
        List<string> numeros = cartas_random.Select(c => c.Substring(0, c.Length - 1)).Distinct().ToList();
        Debug.Log($"{numeros}");
        for (int i = 0; i < numeros.Count && i < textMeshPros.Count; i++)
        {
            textMeshPros[i].text = numeros[i];
            textMeshPros[i].color = Color.black;
        }
        return "Los números de las cartas son: " + string.Join(", ", numeros);
    }

    private string GenerarPistaPalos()
    {
        Debug.Log("GenerarPistaPalos");
        ClearImagesAndTexts();
        for (int i = 0; i < cartas_random.Count && i < rawImages.Count; i++)
        {
            string palo = cartas_random[i].Last().ToString();
            string imagenPath = $"imagenes/{GetPaloName(palo)}";
            Texture2D paloImage = Resources.Load<Texture2D>(imagenPath);

            if (paloImage != null)
            {
                rawImages[i].texture = paloImage;
                textMeshPros[i].text = ""; // Clear any text
            }
            else
            {
                Debug.LogWarning($"Image for {palo} not found at path {imagenPath}");
            }
        }
        return "Los palos de las cartas son: " + string.Join(", ", cartas_random.Select(c => c.Last().ToString()).Distinct());
    }

    private string GenerarPistaCartasEnteras()
    {
        Debug.Log("GenerarPistaCartasEnteras");
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
            Debug.LogWarning($"Image for {palo} not found at path {imagenPath}");
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

    private void TerminarJuego()
    {
        Debug.Log("Juego terminado.");
        feedback.SetActive(true);
        texto_feedback.SetText("¡Felicidades! Has completado el juego.");

        // Desactivar botones y otros elementos de UI que ya no son necesarios
        boton_mostrar_panel.SetActive(false);
        boton_Comprobar.SetActive(false);
        uiPanel.SetActive(false);
    }
}
