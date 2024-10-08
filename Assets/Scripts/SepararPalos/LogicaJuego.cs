using MixedReality.Toolkit.SpatialManipulation;
using NN;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicaJuego : MonoBehaviour
{
    public GameObject tapices;

    [Tooltip("Reference to the Detector script.")]
    [SerializeField]
    public Detector detector;

    private string[] cards = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };

    // Se deja el hueco central para considerarse en espera para ofrecer ayuda
    float[] imageWidth_corazones = { 0, 70 };
    float[] imageWidth_treboles = { 90, 150 };
    float[] imageWidth_diamantes = { 170, 230 };
    float[] imageWidth_picas = { 250, 310 };

    private Dictionary<string, string> cardCategory = new Dictionary<string, string>
    {
        { "2C", "tréboles" }, { "3C", "tréboles" }, { "4C", "tréboles" }, { "5C", "tréboles" }, { "6C", "tréboles" }, { "7C", "tréboles" }, { "8C", "tréboles" }, { "9C", "tréboles" }, { "10C", "tréboles" }, { "JC", "tréboles" }, { "QC", "tréboles" }, { "KC", "tréboles" }, { "AC", "tréboles" },
        { "2D", "diamantes" }, { "3D", "diamantes" }, { "4D", "diamantes" }, { "5D", "diamantes" }, { "6D", "diamantes" }, { "7D", "diamantes" }, { "8D", "diamantes" }, { "9D", "diamantes" }, { "10D", "diamantes" }, { "JD", "diamantes" }, { "QD", "diamantes" }, { "KD", "diamantes" }, { "AD", "diamantes" },
        { "2H", "corazones" }, { "3H", "corazones" }, { "4H", "corazones" }, { "5H", "corazones" }, { "6H", "corazones" }, { "7H", "corazones" }, { "8H", "corazones" }, { "9H", "corazones" }, { "10H", "corazones" }, { "JH", "corazones" }, { "QH", "corazones" }, { "KH", "corazones" }, { "AH", "corazones" },
        { "2S", "picas" }, { "3S", "picas" }, { "4S", "picas" }, { "5S", "picas" }, { "6S", "picas" }, { "7S", "picas" }, { "8S", "picas" }, { "9S", "picas" }, { "10S", "picas" }, { "JS", "picas" }, { "QS", "picas" }, { "KS", "picas" }, { "AS", "picas" }
    };

    private Dictionary<string, float> corazones_SideCards = new Dictionary<string, float>();
    private Dictionary<string, float> treboles_SideCards = new Dictionary<string, float>();
    private Dictionary<string, float> diamantes_SideCards = new Dictionary<string, float>();
    private Dictionary<string, float> picas_SideCards = new Dictionary<string, float>();

    private Dictionary<string, CardInfo> cardInfoDict = new Dictionary<string, CardInfo>();

    public GameObject cardPrefab;

    public DirectionalIndicator indicator;
    public GameObject FueraVista;

    private void Start()
    {
        tapices.SetActive(true);

        if (detector == null)
        {
            Debug.LogError("Detector reference not set in GameController.");
            return;
        }
    }

    private void Update()
    {
        if (indicator.ShouldShowIndicator())
        {
            Debug.Log("Fuera de cámara");
            FueraVista.SetActive(true);
        }
        else
        {
            if (FueraVista)
            {
                FueraVista.SetActive(false);
            }
            if (detector.HasResults())
            {
                var results = detector.GetResults();
                HandleResults(results);
            }
        }
    }

    private void HandleResults(IEnumerable<ResultBox> results)
    {
        foreach (var box in results)
        {
            var card = cards[box.bestClassIndex];
            positionCard(box, card);
        }
    }

    private void positionCard(ResultBox box, string card)
    {
        string category = cardCategory[card];
        float[] imageWidth;

        switch (category)
        {
            case "corazones":
                imageWidth = imageWidth_corazones;
                break;
            case "tréboles":
                imageWidth = imageWidth_treboles;
                break;
            case "diamantes":
                imageWidth = imageWidth_diamantes;
                break;
            case "picas":
                imageWidth = imageWidth_picas;
                break;
            default:
                return;
        }

        if (!cardInfoDict.ContainsKey(card))
        {
            cardInfoDict[card] = new CardInfo(box.rect.x, category, false);
        }
        else
        {
            cardInfoDict[card].MinXPosition = box.rect.x;
        }

        if (box.rect.x >= imageWidth[0] && box.rect.x <= imageWidth[1])
        {
            Dictionary<string, float> currentSideCards;
            switch (category)
            {
                case "corazones":
                    currentSideCards = corazones_SideCards;
                    break;
                case "tréboles":
                    currentSideCards = treboles_SideCards;
                    break;
                case "diamantes":
                    currentSideCards = diamantes_SideCards;
                    break;
                case "picas":
                    currentSideCards = picas_SideCards;
                    break;
                default:
                    return;
            }

            string lastCard = GetLastCard(currentSideCards);

            if (lastCard == null || GetCardValue(card) == GetCardValue(lastCard) + 1)
            {
                Debug.Log($"Carta {category} correcta: {card}");
                UpdateCardPosition(card, box.rect.x, currentSideCards, true);
                currentSideCards[card] = box.rect.x;
            }
            else
            {
                Debug.Log($"Carta {category} mal posicionada: {card}");
                UpdateCardPosition(card, box.rect.x, currentSideCards, false);
            }
        }
        else
        {
            Debug.Log($"{card} en espera");
        }

        // Print lists
        Debug.Log("Corazones: " + string.Join(", ", corazones_SideCards.Keys));
        Debug.Log("Tréboles: " + string.Join(", ", treboles_SideCards.Keys));
        Debug.Log("Diamantes: " + string.Join(", ", diamantes_SideCards.Keys));
        Debug.Log("Picas: " + string.Join(", ", picas_SideCards.Keys));
    }

    private string GetLastCard(Dictionary<string, float> sideCards)
    {
        if (sideCards.Count == 0) return null;

        string lastCard = null;
        int lastValue = -1;

        foreach (var card in sideCards.Keys)
        {
            int cardValue = GetCardValue(card);
            if (cardValue > lastValue)
            {
                lastValue = cardValue;
                lastCard = card;
            }
        }

        return lastCard;
    }

    private int GetCardValue(string card)
    {
        string value = card.Substring(0, card.Length - 1);
        switch (value)
        {
            case "A": return 1;
            case "J": return 11;
            case "Q": return 12;
            case "K": return 13;
            default: return int.Parse(value);
        }
    }

    private void UpdateCardPosition(string card, float xPosition, Dictionary<string, float> targetList, bool isCorrect)
    {
        if (!cardInfoDict.ContainsKey(card))
        {
            cardInfoDict[card] = new CardInfo(xPosition, cardCategory[card], isCorrect);
        }
        else
        {
            cardInfoDict[card].IsCorrectPosition = isCorrect;
            cardInfoDict[card].MinXPosition = xPosition;
            cardInfoDict[card].ResetTimer();
        }

        if (!isCorrect)
        {
            StartCoroutine(CardPositionCheck(card));
        }
    }

    private IEnumerator CardPositionCheck(string card)
    {
        yield return new WaitForSeconds(3);

        if (cardInfoDict.ContainsKey(card) && !cardInfoDict[card].IsCorrectPosition)
        {
            Debug.Log($"{card} está mal posicionada después de 3 segundos.");
            //TriggerCardAnimation(card);
        }
    }

    private void TriggerCardAnimation(string card)
    {
        // Crear una instancia del prefab de la carta
        GameObject cardInstance = Instantiate(cardPrefab);
        // Configurar la posición inicial y final dependiendo de la categoría
        // Ajustar la animación para mover la carta al tapiz correspondiente
        StartCoroutine(MoveCard(cardInstance));
    }

    private IEnumerator MoveCard(GameObject cardInstance)
    {
        // Aquí agregar la lógica para mover la carta a su destino final
        yield return null;
    }
}

public class CardInfo
{
    public float MinXPosition { get; set; }
    public string Category { get; set; }
    public bool IsCorrectPosition { get; set; }

    public CardInfo(float xPosition, string category, bool isCorrectPosition)
    {
        MinXPosition = xPosition;
        Category = category;
        IsCorrectPosition = isCorrectPosition;
    }

    public void ResetTimer()
    {
        // Reinicia el temporizador si es necesario
    }
}