using NN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Diagnostics;

public class SumarCartas_n : MonoBehaviour
{
    public Detector detector;
    public GameObject uiPanel;

    public TMP_Text contador_UI;

    // Parámetros
    private string id_juego;
    private int numeroparejas;
    private int tiempo_total;
    private string[] currentCards;

    //  Modalidades
    private string[] cards_con_figuras = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };
    private string[] cards_sin_figuras = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S"};

    private Dictionary<string, int> cardValues = new Dictionary<string, int>()
    {
        { "2C", 2 }, { "2D", 2 }, { "2H", 2 }, { "2S", 2 },
        { "3C", 3 }, { "3D", 3 }, { "3H", 3 }, { "3S", 3 },
        { "4C", 4 }, { "4D", 4 }, { "4H", 4 }, { "4S", 4 },
        { "5C", 5 }, { "5D", 5 }, { "5H", 5 }, { "5S", 5 },
        { "6C", 6 }, { "6D", 6 }, { "6H", 6 }, { "6S", 6 },
        { "7C", 7 }, { "7D", 7 }, { "7H", 7 }, { "7S", 7 },
        { "8C", 8 }, { "8D", 8 }, { "8H", 8 }, { "8S", 8 },
        { "9C", 9 }, { "9D", 9 }, { "9H", 9 }, { "9S", 9 },
        { "10C", 10 }, { "10D", 10 }, { "10H", 10 }, { "10S", 10 },
        { "JC", 11 }, { "JD", 11 }, { "JH", 11 }, { "JS", 11 },
        { "QC", 12 }, { "QD", 12 }, { "QH", 12 }, { "QS", 12 },
        { "KC", 13 }, { "KD", 13 }, { "KH", 13 }, { "KS", 13 },
        { "AC", 1 }, { "AD", 1 }, { "AH", 1 }, { "AS", 1 }
    };

    private HashSet<string> cartasReconocidas = new HashSet<string>();
    private List<int> recognizedValues = new List<int>();
    private int ActualSum;

    private List<int> recentIdentifications = new List<int>();
    private int maxHistory = 10;

    private KeywordRecognizer keywordRecognizer;
    private bool HaDichoFrase;

    //private bool empezarjuego = false;

    // Terminar partida
    private int parejasReconocidas = 0;
    private float remainingTime;
    private bool isGameActive = false;
    public GameObject finalPanel;


    // Ayudas, feedback...
    public List<GameObject> cardContainers;
    public List<GameObject> cardPrefabsList;
    public GameObject feedback;
    public TextMeshProUGUI texto_feedback;

    // Parámetros finales
    private List<int> tiempo_suma = new List<int>();
    private int tiempo_tardado;
    private bool exito;
    private int aciertos = 0;
    private int fallos= 0;
    private Stopwatch cronometro = new Stopwatch();
    public MenuPrincipal MenuPrincipal;
    public void Sumar_Cartas(string id, int parejas, int tiempo, string modalidad)
    {
        keywordRecognizer = null;

        UnityEngine.Debug.Log("Entro a Sumar Cartas");
        if (detector == null)
        {
            UnityEngine.Debug.LogError("Detector reference not set in GameController.");
            return;
        }

        id_juego = id;
        numeroparejas = parejas;
        tiempo_total = tiempo;
        remainingTime = tiempo_total;

        if (modalidad == "Solo números")
        {
            currentCards = cards_sin_figuras;
        }
        else
        {
            currentCards = cards_con_figuras;
        }


        string[] keywords = new string[26];
        for (int i = 0; i < 26; i++)
        {
            keywords[i] = (i + 1).ToString();
        }

        LoadCardPrefabs();

        keywordRecognizer = new KeywordRecognizer(keywords);
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

    }

    public void EmpezarJuego()
    {
        StartCoroutine(MostrarUIPanelPorTiempo());
    }

    private IEnumerator MostrarUIPanelPorTiempo()
    {
        ShowUIPanel();
        yield return new WaitForSeconds(5);
        ShowUIPanel();
        isGameActive = true;
    }

    private void Update()
    {
        if (!isGameActive) return;

        // Controlar el tiempo restante
        remainingTime -= Time.deltaTime;
        contador_UI.text = $"{Convert.ToInt32(remainingTime)}";
        if (remainingTime <= 0)
        {
            tiempo_tardado = tiempo_total;
            exito = false;
            EndGame("Tiempo agotado");
            return;
        }

        if (detector.HasResults())
        {
            UnityEngine.Debug.Log("detector.HasResults");
            var results = detector.GetResults();
            HandleResults(results);
        }
    }

    private void HandleResults(IEnumerable<ResultBox> results)
    {
        UnityEngine.Debug.Log("Entro a HandleResults");
        foreach (var box in results)
        {
            recentIdentifications.Add(box.bestClassIndex);

            if (recentIdentifications.Count > maxHistory)
            {
                recentIdentifications.RemoveAt(0);
            }

            int mostFrequentClassIndex = GetMostFrequentIndex(recentIdentifications);

            if (mostFrequentClassIndex >= 0 && mostFrequentClassIndex < cards_con_figuras.Length)
            {
                var card = cards_con_figuras[mostFrequentClassIndex];
                if (currentCards.Contains(card))
                {
                    ReconocerCarta(card);
                }
                else
                {
                    feedback.SetActive(true);
                    texto_feedback.SetText("Esta partida es sin figuras!");
                    Invoke("Apagar_feedback", 2f);
                }

                //// Verificar si se ha alcanzado el número de parejas necesarias
                //if (parejasReconocidas >= numeroparejas)
                //{
                //    EndGame("¡Número de parejas completado!");
                //    return;
                //}
            }
        }
    }

    private void ReconocerCarta(string card)
    {
        UnityEngine.Debug.Log("Entro a ReconocerCarta");

        if (cartasReconocidas.Contains(card))
        {
            return;
        }

        UnityEngine.Debug.Log("Nueva carta reconocida: " + card);
        if (cardValues.ContainsKey(card))
        {
            int cardValue = cardValues[card];

            UnityEngine.Debug.Log($"Carta reconocida: {card}. recognizedValues.Count: {recognizedValues.Count}   HaDichoFrase: {HaDichoFrase}");

            if (recognizedValues.Count == 2 && HaDichoFrase == false)
            {
                UnityEngine.Debug.Log($"No sum provided. The correct sum was: {ActualSum}. Please provide the sum first.");
                feedback.SetActive(true);
                texto_feedback.SetText($"Se ha saltado la suma. Di la suma primero");
                Invoke("Apagar_feedback", 5f);
                return;
            }

            recognizedValues.Add(cardValue);
            UnityEngine.Debug.Log(recognizedValues.Count);
            InstantiateCardPrefab(recognizedValues.Count - 1, card);


            if (recognizedValues.Count == 2)
            {
                UnityEngine.Debug.Log($"{recognizedValues[0]}, {recognizedValues[1]}");
                ActualSum = recognizedValues[0] + recognizedValues[1];
                UnityEngine.Debug.Log($"Sum of two cards: {ActualSum}");

                cronometro.Restart();

                if (HaDichoFrase)
                {
                    recognizedValues.Clear();
                    EliminarCardContainers();  // Eliminar ambos contenedores de cartas antes de instanciar la nueva
                }
                parejasReconocidas++;
            }
            cartasReconocidas.Add(card);
        }
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        UnityEngine.Debug.Log($"Recognized phrase: {args.text}");
        if (int.TryParse(args.text, out int spokenSum))
        {
            if (spokenSum == ActualSum)
            {
                HaDichoFrase = true;
                UnityEngine.Debug.Log("Correct sum spoken!");
                cronometro.Stop();
                tiempo_suma.Add((int)cronometro.ElapsedMilliseconds);

                aciertos += aciertos;
                recognizedValues.Clear();
                ActualSum = 0;
                EliminarCardContainers();
                feedback.SetActive(true);
                texto_feedback.SetText("¡Suma Correcta! Coge dos cartas nuevas");
                if (parejasReconocidas >= numeroparejas && recognizedValues.Count == 0)
                {
                    exito = true;
                    EndGame("¡Número de parejas completado!");
                    return;
                }
                Invoke("Apagar_feedback", 3f);
                HaDichoFrase = false;
            }
            else
            {
                if (!HaDichoFrase)
                {
                    UnityEngine.Debug.Log($"Incorrect sum spoken! Try again. Actual sum is {ActualSum}");

                    feedback.SetActive(true);
                    texto_feedback.SetText("¡Suma incorrecta. Inténtalo otra vez.");
                    Invoke("Apagar_feedback", 3f);


                    HaDichoFrase = true;
                }
                else
                {
                    UnityEngine.Debug.Log($"Incorrect sum spoken twice! Actual sum is {ActualSum}");
                    fallos += fallos;

                    cronometro.Stop();
                    tiempo_suma.Add((int)cronometro.ElapsedMilliseconds);

                    feedback.SetActive(true);
                    if (parejasReconocidas >= numeroparejas && recognizedValues.Count == 0)
                    {
                        exito = true;
                        EndGame("¡Número de parejas completado!");
                        return;
                    }
                    texto_feedback.SetText($"Suma incorrecta. La suma correcta era: {ActualSum}.");
                    Invoke("Apagar_feedback", 3f);

                    recognizedValues.Clear();
                    ActualSum = 0;
                    EliminarCardContainers();
                    HaDichoFrase = false;
                    //parejasReconocidas++;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }


    // Ayudas
    public void ShowUIPanel()
    {
        if (!uiPanel.activeSelf)
        {
            uiPanel.SetActive(true);
        }
        else
        {
            uiPanel.SetActive(false);
        }
    }

    private void LoadCardPrefabs()
    {
        foreach (var card in cards_con_figuras)
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

    private int GetMostFrequentIndex(List<int> indices)
    {
        return indices
            .GroupBy(i => i) // Agrupar por índice
            .OrderByDescending(g => g.Count()) // Ordenar por frecuencia
            .First() // Obtener el grupo más frecuente
            .Key; // Obtener el índice (la moda)
    }

    private void InstantiateCardPrefab(int index, string card)
    {
        if (index < cardContainers.Count)
        {

            foreach (Transform child in cardContainers[index].transform)
            {
                Destroy(child.gameObject);
            }

            GameObject prefab = cardPrefabsList.Find(p => p.name == card);
            if (prefab != null)
            {
                GameObject cardInstance = Instantiate(prefab, cardContainers[index].transform);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Prefab for card {card} not found in list!");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("Index out of range for cardContainers or fraseTexts.");
        }
    }

    private void EliminarCardContainers()
    {
        foreach (Transform child in cardContainers[0].transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in cardContainers[1].transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void Apagar_feedback()
    {
        feedback.SetActive(false);
    }

    private async void EndGame(string message)
    {
        isGameActive = false;
        keywordRecognizer.Stop();
        keywordRecognizer.Dispose();
        UnityEngine.Debug.Log(message);
        await MenuPrincipal.resultados_SumarCartas(id_juego, tiempo_suma, tiempo_tardado, exito, aciertos, fallos);
        finalPanel.SetActive(true);
    }
}
