using NN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;
using TMPro;

public class AsociarFrases : MonoBehaviour
{
    // Detector
    public Detector detector;

    //Panel
    public GameObject uiPanel;
    public TextMeshProUGUI frasesText;

    // Contenedores para las cartas y frases
    public List<GameObject> cardContainers;
    public List<TextMeshProUGUI> fraseTexts;

    // Almacenar los prefabs
    public List<GameObject> cardPrefabsList;


    private HashSet<string> cartasReconocidas = new HashSet<string>();

    private string[] cards = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };
    private string[] frases = { "¡Buenos días!", "Elefante", "Naipes", "Cielo", "Estrella", "Música", "Amor", "Sonrisa",
    "Montaña", "Río", "Sol", "Luna", "Flor", "Mariposa", "Corazón", "Amistad", "Familia",
    "Felicidad", "Libertad", "Esperanza", "Paz", "Aventura", "Sueño", "Libro", "Chocolate",
    "Arte", "Naturaleza", "Viaje", "Alegría", "Inspiración" };

    // Microfono
    private Dictionary<string, string> cartas_frases = new Dictionary<string, string>();
    private KeywordRecognizer keywordRecognizer;
    private string fraseReconocida;
    private readonly string[] staticKeywords = { "Mostrar panel", "Pista" };

    private string fraseReconocidaActual = null;
    private string cartaReconocidaActual = null;

    private int correctPhraseCount = 0;

    // Ayudas
    public GameObject boton_mostrar_panel;
    public GameObject panel_correcto_incorrecto;

    public GameObject feedback;
    public TextMeshProUGUI texto_corrrecto_incorrecto;
    public GameObject cardContainers_actual;
    private string CartaActual ="";

    // Estadísticas
    private int correctAnswers = 0;
    private int incorrectAnswers = 0;
    private int hintsUsed = 0;
    private float elapsedTime = 0f;
    private bool gameStarted = false;

    private List<string> pistas = new List<string>
    {
        "La palabra comienza con la letra '{0}'",
        "La palabra tiene {0} letras",
        "La última letra de la palabra es '{0}'",
        "{0}"
    };

    public GameObject handmenu;

    private void Start()
    {
        keywordRecognizer = null;

        if (detector == null)
        {
            Debug.LogError("Detector reference not set in GameController.");
            return;
        }

        LoadCardPrefabs();
        Random_Cartas();
        ShowUIPanel();


        keywordRecognizer = new KeywordRecognizer(staticKeywords);
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

        handmenu.SetActive(true);

        // Estadísticas
        correctAnswers = 0;
        incorrectAnswers = 0;
        hintsUsed = 0;
        elapsedTime = 0f;
        gameStarted = true;
    }

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

    private void Random_Cartas()
    {
        HashSet<int> indices_cartas = new HashSet<int>();
        HashSet<int> indices_frases = new HashSet<int>();

        while (indices_cartas.Count < 4)
        {
            indices_cartas.Add(Random.Range(0, cards.Length));
        }

        while (indices_frases.Count < 4)
        {
            indices_frases.Add(Random.Range(0, frases.Length));
        }

        cartas_frases.Clear();

        List<int> lista_indices_cartas = new List<int>(indices_cartas);
        List<int> lista_indices_frases = new List<int>(indices_frases);

        string frasesTextContent = "";
        for (int i = 0; i < 4; i++)
        {
            int carta_index = lista_indices_cartas[i];
            int frase_index = lista_indices_frases[i];
            string carta = cards[carta_index];
            string frase = frases[frase_index];
            cartas_frases.Add(carta, frase);

            frasesTextContent += $"{frase}";
            InstantiateCardPrefab(i, carta, frase);
        }
    }

    private void InstantiateCardPrefab(int index, string card, string frase)
    {
        if (index < cardContainers.Count && index < fraseTexts.Count)
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

            // Asignar la frase al TextMeshPro correspondiente
            fraseTexts[index].text = frase;
        }
        else
        {
            Debug.LogWarning("Index out of range for cardContainers or fraseTexts.");
        }
    }

    private void ShowUIPanel()
    {
        uiPanel.SetActive(true);
    }

    public void StartGame()
    {
        uiPanel.SetActive(false);
        boton_mostrar_panel.SetActive(true);
    }

    private void Update()
    {
        if (gameStarted)
        {
            elapsedTime += Time.deltaTime;
        }

        if (detector.HasResults() && !uiPanel.activeSelf)
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

    private void ReconocerCarta(string card)
    {
        if (cartasReconocidas.Contains(card))
        {
            return; // Salir si la carta ya ha sido reconocida
        }

        InstantiateCardPrefab_reconocida(card);
        CartaActual = card;

        if (fraseReconocidaActual != null && cartaReconocidaActual != null)
        {
            // AVISO DE ERROR!
            feedback.SetActive(true);
            texto_corrrecto_incorrecto.SetText($"No has dicho la palabra de la carta anterior: {fraseReconocidaActual}. Continúe");
            Invoke("Apagar_feedback", 5f);
            Debug.Log($"Error: se ha reconocido una nueva carta ({card}) antes de decir la frase de la carta anterior ({cartaReconocidaActual}).");
            cartaReconocidaActual = null;
            cartaReconocidaActual = null;

            incorrectAnswers++;
            correctPhraseCount++;

            if (correctPhraseCount >= 4)
            {
                TerminarJuego();
            }
        }

        if (cartas_frases.ContainsKey(card))
        {
            fraseReconocida = cartas_frases[card];
            fraseReconocidaActual = fraseReconocida;
            cartaReconocidaActual = card;

            UpdateKeywordRecognizer(fraseReconocida);

            // Agregar la carta al HashSet de cartas reconocidas
            cartasReconocidas.Add(card);
        }
    }

    private void UpdateKeywordRecognizer(string nuevaFrase)
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }

        string[] keywords = new string[staticKeywords.Length + 1];
        staticKeywords.CopyTo(keywords, 0);
        keywords[staticKeywords.Length] = nuevaFrase;

        keywordRecognizer = new KeywordRecognizer(keywords);
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log($"Escuchando... Di la frase: {nuevaFrase}");
        //texto_corrrecto_incorrecto.SetText($"Escuchando...");
    }

    private IEnumerator CountdownCoroutine(string card, string frase)
    {
        yield return new WaitForSeconds(3);
        if (!cartasReconocidas.Contains(card))
        {
            UpdateKeywordRecognizer(frase);
        }
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"Frase reconocida: {args.text}");

        if (args.text == "Mostrar panel")
        {
            ShowUIPanel();
            return;
        }
        else if (args.text == "Pista")
        {
            ShowHint();
            return;
        }

        if (args.text == fraseReconocida)
        {
            panel_correcto_incorrecto.SetActive(true);
            Debug.Log("¡Correcto!");
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
            feedback.SetActive(true);
            texto_corrrecto_incorrecto.SetText($"¡Muy bien! Coja la siguiente carta");
            Invoke("Apagar_feedback", 3f);
            correctAnswers++; // Incrementa los aciertos
            fraseReconocidaActual = null;
            cartaReconocidaActual = null;
        }
        else
        {
            panel_correcto_incorrecto.SetActive(true);
            Debug.Log($"No es la palabra. La palabra era {fraseReconocida}");
            texto_corrrecto_incorrecto.SetText($"No es la palabra. La palabra era {fraseReconocida}");
            feedback.SetActive(true);
            texto_corrrecto_incorrecto.SetText($"No has dicho la palabra de la carta anterior: {fraseReconocidaActual}");
            Invoke("Apagar_feedback", 5f);
            incorrectAnswers++; // Incrementa los errores
        }

        if (correctPhraseCount >= 4)
        {
            TerminarJuego();
        }
    }

    private void DetenerReconocimiento()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
            keywordRecognizer = null;
            texto_corrrecto_incorrecto.SetText($"Reconocimiento detenido.");
            Invoke("desaparecer_panel", 1f);
            Debug.Log("Reconocimiento detenido.");
        }
    }

    private void desaparecer_panel()
    {
        panel_correcto_incorrecto.SetActive(false);
    }

    private void ContarError()
    {
        panel_correcto_incorrecto.SetActive(true);
        texto_corrrecto_incorrecto.SetText("No se reconoció la frase a tiempo.");
        Invoke("desaparecer_panel", 3f);
        Debug.Log("Error: No se reconoció la frase a tiempo.");
    }

    private void Apagar_feedback()
    {
        feedback.SetActive(false);
    }

    private void InstantiateCardPrefab_reconocida(string card)
    {
        // Clean the container before instantiating the new prefab
        foreach (Transform child in cardContainers_actual.transform)
        {
            Destroy(child.gameObject);
        }

        // Find the prefab of the card in the list
        GameObject prefab = cardPrefabsList.Find(p => p.name == card);
        if (prefab != null)
        {
            Instantiate(prefab, cardContainers_actual.transform);
        }
        else
        {
            Debug.LogWarning($"Prefab for card {card} not found in list!");
        }
    }

    public void ShowHint()
    {
        if (CartaActual == "")
        {
            panel_correcto_incorrecto.SetActive(true);
            texto_corrrecto_incorrecto.SetText("Debes mirar una carta primero");
            Invoke("desaparecer_panel", 3f);
            return;
        }
        if (cartas_frases.ContainsKey(CartaActual))
        {
            string frase = cartas_frases[CartaActual];
            string pista = ObtenerPistaAleatoria(frase);

            panel_correcto_incorrecto.SetActive(true);
            texto_corrrecto_incorrecto.SetText(pista);
            hintsUsed++; // Incrementa las pistas usadas
            Invoke("desaparecer_panel", 5f);
        }
        else
        {
            panel_correcto_incorrecto.SetActive(true);
            texto_corrrecto_incorrecto.SetText("Esta carta no tiene palabra");
            Invoke("desaparecer_panel", 3f);
        }
    }
    private string ObtenerPistaAleatoria(string frase)
    {
        System.Random rand = new System.Random();
        int indice = rand.Next(pistas.Count);
        string pistaSeleccionada = pistas[indice];

        // Formatear la pista seleccionada con la información de la frase
        switch (indice)
        {
            case 0: // La frase comienza con la letra '{0}'
                return string.Format(pistaSeleccionada, frase[0]);
            case 1: // La frase tiene {0} letras
                return string.Format(pistaSeleccionada, frase.Length);
            case 2: // La última letra de la frase es '{0}'
                return string.Format(pistaSeleccionada, frase[frase.Length - 1]);
            case 3:
                return string.Format(pistaSeleccionada, GenerarPistaLetrasYGuiones(frase));
            default:
                return "No hay pista disponible";
        }
    }

    private string GenerarPistaLetrasYGuiones(string frase)
    {
        char[] pistaArray = new char[frase.Length];
        System.Random rand = new System.Random();

        for (int i = 0; i < frase.Length; i++)
        {
            if (char.IsWhiteSpace(frase[i]))
            {
                pistaArray[i] = ' ';
            }
            else
            {
                // Mostrar letra o guión bajo aleatoriamente
                pistaArray[i] = rand.Next(0, 2) == 0 ? '_' : frase[i];
            }
        }

        return new string(pistaArray);
    }

    private void TerminarJuego()
    {
        Debug.Log("Juego terminado. Has encontrado 8 parejas de cartas.");
        Debug.Log($"correctAnswers: {correctAnswers}, hintsUsed: {hintsUsed}, elapsedTime: {elapsedTime}");

        panel_correcto_incorrecto.SetActive(true);
        texto_corrrecto_incorrecto.SetText("¡Felicidades! El juego ha terminado.");
        Invoke("Apagar_feedback", 5f);

        gameStarted = false; // Detiene el contador de tiempo

        // Guardar estadísticas
        GameSession session = new GameSession(correctAnswers, hintsUsed, elapsedTime);
        StatsManager.Instance.AddGameSession(2, session); // Suponiendo que este es el juego 1

        // Detener el reconocimiento de palabras
        keywordRecognizer.Stop();
        keywordRecognizer.Dispose();
    }
}