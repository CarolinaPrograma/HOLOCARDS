using NN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

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

    // Modalidades
    private string[] cards_aleatorio = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };
    string[] rojas = { "10D", "10H", "2D", "2H", "3D", "3H", "4D", "4H", "5D", "5H", "6D", "6H", "7D", "7H", "8D", "8H", "9D", "9H", "AD", "AH", "JD", "JH", "KD", "KH", "QD", "QH" };
    string[] negras = { "10C", "10S", "2C", "2S", "3C", "3S", "4C", "4S", "5C", "5S", "6C", "6S", "7C", "7S", "8C", "8S", "9C", "9S", "AC", "AS", "JC", "JS", "KC", "KS", "QC", "QS" };
    string[] diamantes = { "10D", "2D", "3D", "4D", "5D", "6D", "7D", "8D", "9D", "AD", "JD", "KD", "QD" };
    string[] corazones = { "10H", "2H", "3H", "4H", "5H", "6H", "7H", "8H", "9H", "AH", "JH", "KH", "QH" };
    string[] tréboles = { "10C", "2C", "3C", "4C", "5C", "6C", "7C", "8C", "9C", "AC", "JC", "KC", "QC" };
    string[] picas = { "10S", "2S", "3S", "4S", "5S", "6S", "7S", "8S", "9S", "AS", "JS", "KS", "QS" };

    private float remainingTime;
    private float remainingTime_panel;
    public TMP_Text time_panel_text;

    private HashSet<string> cartasReconocidas = new HashSet<string>();
    
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
    private int aciertos = 0;
    private int fallos = 0;
    private int numero_pistas = 0;
    private bool exito;
    private List<int> tiempo_carta = new List<int>();
    private Stopwatch cronometro = new Stopwatch();
    public MenuPrincipal MenuPrincipal;

    private List<string> pistas = new List<string>
    {
        "La palabra comienza con la letra '{0}'",
        "La palabra tiene {0} letras",
        "La última letra de la palabra es '{0}'",
        "{0}"
    };

    public GameObject handmenu;

    // Parámetros
    private string id_juego;
    private int numero;
    private int tiempo_total;
    private string[] cards;
    private int t_panel;
    private bool isGameActive = false;
    

    // Tiempos, Final...
    public TMP_Text contador_UI;
    public GameObject finalPanel;
    private bool isChecking;

    public void Asociar_Frases(string id, int cartas, int tiempo, string modalidad, int tiempo_panel)
    {
        keywordRecognizer = null;

        if (detector == null)
        {
            UnityEngine.Debug.LogError("Detector reference not set in GameController.");
            return;
        }

        id_juego = id;
        numero = cartas;
        tiempo_total = tiempo;
        remainingTime = tiempo_total;
        t_panel = tiempo_panel;
        remainingTime_panel = tiempo_panel;


        if      (modalidad == "Aleatorio") { cards = cards_aleatorio; }
        else if (modalidad == "Rojas") { cards = rojas; }
        else if (modalidad == "Negras") { cards = negras; }
        else if (modalidad == "Picas") { cards = picas; }
        else if (modalidad == "Treboles") { cards = tréboles; }
        else if (modalidad == "Diamantes") { cards = diamantes; }
        else if (modalidad == "Corazones") { cards = corazones; }


        LoadCardPrefabs();
        Random_Cartas();

        keywordRecognizer = new KeywordRecognizer(staticKeywords);
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

        handmenu.SetActive(true);
    }


    void Update()
    {
        UnityEngine.Debug.Log("Entro a update");
        if (!isGameActive) return;

        // Controlar el tiempo restante
        remainingTime -= Time.deltaTime;
        contador_UI.text = $"{System.Convert.ToInt32(remainingTime)}";

        UnityEngine.Debug.Log(remainingTime);
        if (remainingTime <= 0)
        {
            EndGame("Tiempo agotado");
            exito = false;
            return;
        }

        if (isChecking && detector.HasResults() && !uiPanel.activeSelf)
        {
            var results = detector.GetResults();
            HandleResults(results);
        }
    }

    public void EmpezarJuego()
    {
        ShowUIPanel();
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
                UnityEngine.Debug.LogWarning($"Prefab for card {card} not found in Resources!");
            }
        }
    }

    private void Random_Cartas()
    {
        HashSet<int> indices_cartas = new HashSet<int>();
        HashSet<int> indices_frases = new HashSet<int>();

        while (indices_cartas.Count < numero)
        {
            indices_cartas.Add(Random.Range(0, cards.Length));
        }

        while (indices_frases.Count < numero)
        {
            indices_frases.Add(Random.Range(0, frases.Length));
        }

        cartas_frases.Clear();

        List<int> lista_indices_cartas = new List<int>(indices_cartas);
        List<int> lista_indices_frases = new List<int>(indices_frases);

        string frasesTextContent = "";
        for (int i = 0; i < numero; i++)
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
            GameObject prefab = cardPrefabsList.Find(p => p.name == card);
            if (prefab != null)
            {
                GameObject cardInstance = Instantiate(prefab, cardContainers[index].transform);

            }
            else
            {
                UnityEngine.Debug.LogWarning($"Prefab for card {card} not found in list!");
            }

            fraseTexts[index].text = frase;
        }
        else
        {
            UnityEngine.Debug.LogWarning("Index out of range for cardContainers or fraseTexts.");
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
            return; 
        }

        InstantiateCardPrefab_reconocida(card);
        CartaActual = card;
        cronometro.Restart();
        if (fraseReconocidaActual != null && cartaReconocidaActual != null)
        {
            // AVISO DE ERROR!
            feedback.SetActive(true);
            texto_corrrecto_incorrecto.SetText($"No has dicho la palabra de la carta anterior: {fraseReconocidaActual}. Continúe");
            Invoke("Apagar_feedback", 5f);
            UnityEngine.Debug.Log($"Error: se ha reconocido una nueva carta ({card}) antes de decir la frase de la carta anterior ({cartaReconocidaActual}).");
            cartaReconocidaActual = null;
            cartaReconocidaActual = null;

            fallos += fallos;
            correctPhraseCount++;

            if (correctPhraseCount >= numero)
            {
                exito = true;
                EndGame("Se ha acabado");
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

        UnityEngine.Debug.Log($"Escuchando... Di la frase: {nuevaFrase}");
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
        UnityEngine.Debug.Log($"Frase reconocida: {args.text}");

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
            UnityEngine.Debug.Log("¡Correcto!");
            cronometro.Stop();
            tiempo_carta.Add((int)cronometro.ElapsedMilliseconds);
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
            feedback.SetActive(true);
            texto_corrrecto_incorrecto.SetText($"¡Muy bien! Coja la siguiente carta");
            Invoke("Apagar_feedback", 3f);
            aciertos++; 
            fraseReconocidaActual = null;
            cartaReconocidaActual = null;
        }
        else
        {
            panel_correcto_incorrecto.SetActive(true);
            UnityEngine.Debug.Log($"No es la palabra. La palabra era {fraseReconocida}");
            texto_corrrecto_incorrecto.SetText($"No es la palabra. La palabra era {fraseReconocida}");
            feedback.SetActive(true);
            texto_corrrecto_incorrecto.SetText($"No has dicho la palabra de la carta anterior: {fraseReconocidaActual}");
            Invoke("Apagar_feedback", 5f);
            fallos++; // Incrementa los errores
        }

        if (correctPhraseCount >= numero)
        {
            EndGame("Enhorabuena");
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
            UnityEngine.Debug.Log("Reconocimiento detenido.");
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
        UnityEngine.Debug.Log("Error: No se reconoció la frase a tiempo.");
    }

    private void Apagar_feedback()
    {
        feedback.SetActive(false);
    }

    private void InstantiateCardPrefab_reconocida(string card)
    {
        foreach (Transform child in cardContainers_actual.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject prefab = cardPrefabsList.Find(p => p.name == card);
        if (prefab != null)
        {
            Instantiate(prefab, cardContainers_actual.transform);
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Prefab for card {card} not found in list!");
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
        numero_pistas++;
        if (cartas_frases.ContainsKey(CartaActual))
        {
            string frase = cartas_frases[CartaActual];
            string pista = ObtenerPistaAleatoria(frase);

            panel_correcto_incorrecto.SetActive(true);
            texto_corrrecto_incorrecto.SetText(pista);
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

    private async void EndGame(string message)
    {
        isGameActive = false;
        keywordRecognizer.Stop();
        keywordRecognizer.Dispose();
        UnityEngine.Debug.Log(message);
        await MenuPrincipal.resultados_AsociarFrases(id_juego, tiempo_carta, exito, aciertos, fallos, numero_pistas);
        finalPanel.SetActive(true);
    }
}