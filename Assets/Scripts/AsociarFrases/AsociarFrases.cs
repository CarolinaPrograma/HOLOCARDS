using NN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
using System.Linq;

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
    private string[] cards = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };
    int[] aleatorio = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51 };
    int[] rojas = { 1, 2, 5, 6, 9, 10, 13, 14, 17, 18, 21, 22, 25, 26, 29, 30, 33, 34, 37, 38, 41, 42, 45, 46, 49, 50 };
    int[] negras = { 3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51, 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48 };
    int[] diamantes = { 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49 };
    int[] corazones = { 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50 };
    int[] tréboles = { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48 };
    int[] picas = { 3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51 };

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
    private readonly string[] staticKeywords = { "Mostrar panel", "Pista", "Fin" };

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

    public TMP_Text modadalidad_frase;

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
    private int[] cards_modalidad;
    private int t_panel;
    private bool isGameActive = false;
    private string modalidad_string;


    // Tiempos, Final...
    public TMP_Text contador_UI;
    public GameObject finalPanel;
    private bool isChecking;

    // Mejora de reconocimiento
    private List<int> recentIdentifications = new List<int>();
    private int maxHistory = 10;

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
        modalidad_string = modalidad;

        if (modalidad == "Aleatorio") { cards_modalidad = aleatorio; }
        else if (modalidad == "Rojas") { cards_modalidad = rojas; modadalidad_frase.SetText("Esta partida solo reconoce " + modalidad_string); }
        else if (modalidad == "Negras") { cards_modalidad = negras; modadalidad_frase.SetText("Esta partida solo reconoce " + modalidad_string); }
        else if (modalidad == "Picas") { cards_modalidad = picas; modadalidad_frase.SetText("Esta partida solo reconoce " + modalidad_string); }
        else if (modalidad == "Treboles") { cards_modalidad = tréboles; modadalidad_frase.SetText("Esta partida solo reconoce " + modalidad_string); }
        else if (modalidad == "Diamantes") { cards_modalidad = diamantes; modadalidad_frase.SetText("Esta partida solo reconoce " + modalidad_string); }
        else if (modalidad == "Corazones") { cards_modalidad = corazones; modadalidad_frase.SetText("Esta partida solo reconoce " + modalidad_string); }


        LoadCardPrefabs();
        Random_Cartas(cards_modalidad);

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
        if (remainingTime <= 0)
        {
            EndGame("Tiempo agotado");
            exito = false;
            return;
        }

        if (detector.HasResults() && !uiPanel.activeSelf)
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

    private void Random_Cartas(int[] indices_disponibles)
    {
        HashSet<int> indices_cartas = new HashSet<int>();
        HashSet<int> indices_frases = new HashSet<int>();

        // Selección aleatoria de cartas usando índices disponibles
        while (indices_cartas.Count < numero)
        {
            // Elegimos un índice aleatorio de la lista de índices disponibles
            int randomIndex = Random.Range(0, indices_disponibles.Length);
            indices_cartas.Add(indices_disponibles[randomIndex]);
        }

        // Selección aleatoria de frases
        while (indices_frases.Count < numero)
        {
            indices_frases.Add(Random.Range(0, frases.Length));
        }

        cartas_frases.Clear();

        // Convertimos los sets en listas para acceder a sus elementos por índice
        List<int> lista_indices_cartas = new List<int>(indices_cartas);
        List<int> lista_indices_frases = new List<int>(indices_frases);

        string frasesTextContent = "";
        for (int i = 0; i < numero; i++)
        {
            // Accedemos a las cartas y frases seleccionadas
            int carta_index = lista_indices_cartas[i];
            int frase_index = lista_indices_frases[i];
            string carta = cards[carta_index]; // Usamos cards_aleatorio
            string frase = frases[frase_index];

            // Asociamos la carta con su frase
            cartas_frases.Add(carta, frase);

            // Creamos el contenido para las frases
            frasesTextContent += $"{frase} ";

            // Instanciamos la carta en el tablero
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

        if (isGameActive) {
            remainingTime_panel = 2;
        }
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
            recentIdentifications.Add(box.bestClassIndex);

            if (recentIdentifications.Count > maxHistory)
            {
                recentIdentifications.RemoveAt(0);
            }

            int mostFrequentClassIndex = GetMostFrequentIndex(recentIdentifications);
            UnityEngine.Debug.Log(mostFrequentClassIndex);

            if (mostFrequentClassIndex >= 0 && mostFrequentClassIndex < cards.Length)
            {
                var card = cards[mostFrequentClassIndex];

                if (cards_modalidad.Contains(mostFrequentClassIndex))
                {
                    UnityEngine.Debug.Log("LLamo a reconocer cartas " + card);
                    ReconocerCarta(card);
                }
            }
        }
    }

    private void ReconocerCarta(string card)
    {
        if (cartasReconocidas.Contains(card))
        {
            return; 
        }
        UnityEngine.Debug.Log("ReconocerCarta ");
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

        else if (args.text == "Fin")
        {
            EndGame("El mazo se ha acabado");
        }

        if (args.text == fraseReconocida)
        {
            panel_correcto_incorrecto.SetActive(true);
            UnityEngine.Debug.Log("¡Correcto!");
            cronometro.Stop();
            tiempo_carta.Add((int)cronometro.ElapsedMilliseconds);
            keywordRecognizer.Stop();
            //keywordRecognizer.Dispose();
            feedback.SetActive(true);
            texto_corrrecto_incorrecto.SetText($"¡Muy bien! Coja la siguiente carta");
            Invoke("Apagar_feedback", 3f);
            aciertos++; 
            fraseReconocidaActual = null;
            cartaReconocidaActual = null;
            keywordRecognizer.Start();
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
        cardContainers_actual.SetActive(false);
        isGameActive = false;
        keywordRecognizer.Stop();
        keywordRecognizer.Dispose();
        UnityEngine.Debug.Log(message);
        await MenuPrincipal.resultados_AsociarFrases(id_juego, tiempo_carta, exito, aciertos, fallos, numero_pistas);
        finalPanel.SetActive(true);
    }

    private int GetMostFrequentIndex(List<int> indices)
    {
        return indices
            .GroupBy(i => i) // Agrupar por índice
            .OrderByDescending(g => g.Count()) // Ordenar por frecuencia
            .First() // Obtener el grupo más frecuente
            .Key; // Obtener el índice (la moda)
    }
}