using NN;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SumarCartas : MonoBehaviour
{
    public Detector detector;
    public GameObject uiPanel;

    private string[] cards = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };
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
    private KeywordRecognizer keywordRecognizer;

    private int ActualSum;

    private bool HaDichoFrase;

    // Terminar partida
    private int parejasReconocidas = 0;
    private int numeroparejas = 2;

    // Ayudas, feedback...
    public List<GameObject> cardContainers;
    public List<GameObject> cardPrefabsList;
    public GameObject feedback;
    public TextMeshProUGUI texto_feedback;

    private void Start()
    {
        if (detector == null)
        {
            Debug.LogError("Detector reference not set in GameController.");
            return;
        }

        string[] keywords = new string[26];
        for (int i = 0; i < 26; i++)
        {
            keywords[i] = (i + 1).ToString();
        }

        ShowUIPanel();
        LoadCardPrefabs();

        keywordRecognizer = new KeywordRecognizer(keywords);
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void Update()
    {
        if (detector.HasResults())
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

        if (cardValues.ContainsKey(card))
        {
            int cardValue = cardValues[card];

            Debug.Log($"Carta reconocida: {card}. recognizedValues.Count: {recognizedValues.Count}   HaDichoFrase: {HaDichoFrase}");

            if (recognizedValues.Count == 2 && HaDichoFrase == false)
            {
                Debug.Log($"No sum provided. The correct sum was: {ActualSum}. Please provide the sum first.");
                feedback.SetActive(true);
                texto_feedback.SetText($"Se ha saltado la suma. Di la suma primero");
                Invoke("Apagar_feedback", 5f);
                return;
            }

            recognizedValues.Add(cardValue);
            Debug.Log(recognizedValues.Count);
            InstantiateCardPrefab(recognizedValues.Count - 1, card);


            if (recognizedValues.Count == 2)
            {
                Debug.Log($"{recognizedValues[0]}, {recognizedValues[1]}");
                ActualSum = recognizedValues[0] + recognizedValues[1];
                Debug.Log($"Sum of two cards: {ActualSum}");
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
        Debug.Log($"Recognized phrase: {args.text}");
        if (int.TryParse(args.text, out int spokenSum))
        {
            if (spokenSum == ActualSum)
            {
                HaDichoFrase = true;
                Debug.Log("Correct sum spoken!");
                recognizedValues.Clear();
                ActualSum = 0;
                EliminarCardContainers();
                feedback.SetActive(true);
                texto_feedback.SetText("¡Suma Correcta! Coge dos cartas nuevas");
                if (parejasReconocidas >= numeroparejas && recognizedValues.Count == 0)
                {
                    TerminarJuego();
                    return;
                }
                Invoke("Apagar_feedback", 3f);
                HaDichoFrase = false; 
            }
            else
            {
                if (!HaDichoFrase)
                {
                    Debug.Log($"Incorrect sum spoken! Try again. Actual sum is {ActualSum}");
                    feedback.SetActive(true);
                    texto_feedback.SetText("¡Suma incorrecta. Inténtalo otra vez.");
                    Invoke("Apagar_feedback", 3f);


                    HaDichoFrase = true;
                }
                else
                {
                    Debug.Log($"Incorrect sum spoken twice! Actual sum is {ActualSum}");

                    feedback.SetActive(true);
                    if (parejasReconocidas >= numeroparejas && recognizedValues.Count == 0)
                    {
                        TerminarJuego();
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

    private void InstantiateCardPrefab(int index, string card)
    {
        if (index < cardContainers.Count)
        {
            // Limpiar el contenedor antes de instanciar el nuevo prefab
            foreach (Transform child in cardContainers[index].transform)
            {
                Destroy(child.gameObject);
            }

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

    private void TerminarJuego()
    {
        Debug.Log("Juego terminado. Has encontrado 8 parejas de cartas.");
        feedback.SetActive(true);
        texto_feedback.SetText("¡Felicidades! El juego ha terminado.");
        Invoke("Apagar_feedback", 5f);

        // Aquí puedes agregar más lógica para finalizar el juego, como desactivar el reconocimiento de cartas o mostrar una pantalla de finalización.
        keywordRecognizer.Stop();
        // Desactivar otras funciones del juego si es necesario
    }
}
