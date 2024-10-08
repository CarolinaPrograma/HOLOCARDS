using MixedReality.Toolkit.SpatialManipulation;
using NN;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SepararColores : MonoBehaviour
{
    [Tooltip("Reference to the Detector script.")]
    [SerializeField]
    public Detector detector;

    private string[] cards = { "10C", "10D", "10H", "10S", "2C", "2D", "2H", "2S", "3C", "3D", "3H", "3S", "4C", "4D", "4H", "4S", "5C", "5D", "5H", "5S", "6C", "6D", "6H", "6S", "7C", "7D", "7H", "7S", "8C", "8D", "8H", "8S", "9C", "9D", "9H", "9S", "AC", "AD", "AH", "AS", "JC", "JD", "JH", "JS", "KC", "KD", "KH", "KS", "QC", "QD", "QH", "QS" };
    private string[] red_cards = { "10H", "10D", "2H", "2D", "3H", "3D", "4H", "4D", "5H", "5D", "6H", "6D", "7H", "7D", "8H", "8D", "9H", "9D", "AH", "AD", "JH", "JD", "KH", "KD", "QH", "QD" };
    private string[] black_cards = { "10C", "10S", "2C", "2S", "3C", "3S", "4C", "4S", "5C", "5S", "6C", "6S", "7C", "7S", "8C", "8S", "9C", "9S", "AC", "AS", "JC", "JS", "KC", "KS", "QC", "QS" };

    // Se deja el hueco central para considerarse en espera para ofrecer ayuda
    float imageWidth_left = 120;
    float imageWidth_right = 200;

    // Lista de cartas a la izquierda
    private Dictionary<string, float> leftSideCards = new Dictionary<string, float>();
    // Lista de cartas a la derecha
    private Dictionary<string, float> rightSideCards = new Dictionary<string, float>();

    // Mantenemos un diccionario para seguir el estado de cada carta
    private Dictionary<string, CardInfo> cardInfoDict = new Dictionary<string, CardInfo>();

    // Prefab de la carta
    //public Dictionary<string, GameObject> cardPrefabs = new Dictionary<string, GameObject>();

    public GameObject cardPrefab;
    // Posiciones de los tapices
    public Transform leftTapiz;
    public Transform rightTapiz;

    public DirectionalIndicator indicator;
    public GameObject FueraVista;

    private void Start()
    {
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
        bool isRed = Array.Exists(red_cards, c => c == card);

        if (!cardInfoDict.ContainsKey(card))
        {
            cardInfoDict[card] = new CardInfo(box.rect.x, isRed, false);
        }
        else
        {
            cardInfoDict[card].MinXPosition = box.rect.x;
        }

        if (isRed)
        {
            if (box.rect.x < imageWidth_left)
            {
                Debug.Log($"Carta roja mal posicionada: {card}");
                UpdateCardPosition(card, box.rect.x, leftSideCards, rightSideCards, false);
            }
            else if (box.rect.x > imageWidth_right)
            {
                Debug.Log($"Carta roja correcta: {card}");
                UpdateCardPosition(card, box.rect.x, rightSideCards, leftSideCards, true);
            }
            else
            {
                Debug.Log($"{card} en espera");
            }
        }
        else
        {
            if (box.rect.x > imageWidth_right)
            {
                Debug.Log($"Carta negra mal posicionada: {card}");
                UpdateCardPosition(card, box.rect.x, rightSideCards, leftSideCards, false);
            }
            else if (box.rect.x < imageWidth_left)
            {
                Debug.Log($"Carta negra correcta: {card}");
                UpdateCardPosition(card, box.rect.x, leftSideCards, rightSideCards, true);
            }
            else
            {
                Debug.Log($"{card} en espera");
            }
        }

        // Print lists
        Debug.Log("Left Side Cards: " + string.Join(", ", leftSideCards.Keys));
        Debug.Log("Right Side Cards: " + string.Join(", ", rightSideCards.Keys));
        
    }

    // Función que actualiza las listas de cartas a cada lado
    private void UpdateCardPosition(string card, float xPosition, Dictionary<string, float> targetList, Dictionary<string, float> otherList, bool isCorrect)
    {
        targetList[card] = xPosition;

        if (otherList.ContainsKey(card))
        {
            otherList.Remove(card);
        }

        if (!cardInfoDict.ContainsKey(card))
        {
            cardInfoDict[card] = new CardInfo(xPosition, isCorrect, false);
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

    // Función que calcula cuándo se considera de que el usuario se ha equivocado
    private IEnumerator CardPositionCheck(string card)
    {
        yield return new WaitForSeconds(4);

        if (cardInfoDict.ContainsKey(card) && !cardInfoDict[card].IsCorrectPosition && !cardInfoDict[card].animation)
        {
            Debug.Log($"{card} está mal posicionada después de 4 segundos.");
            cardInfoDict[card].animation = true;
            TriggerCardAnimation(card);  
        }
    }

    // Animación de ayuda cuando se equivoca 
    private void TriggerCardAnimation(string card)
    {
        // Crear una instancia del prefab de la carta
        Debug.Log(leftTapiz);
        GameObject cardInstance = Instantiate(cardPrefab, leftTapiz);
        // Configurar la posición inicial y final dependiendo de si es roja o negra
        Transform targetPosition = Array.Exists(red_cards, c => c == card) ? rightTapiz : leftTapiz;

        // Ajustar la animación para mover la carta al tapiz correspondiente
        StartCoroutine(MoveCard(cardInstance, rightTapiz));
    }

    private IEnumerator MoveCard(GameObject cardInstance, Transform targetPosition)
    {
        float duration = 2.0f; // Duración de la animación
        Vector3 startPosition = cardInstance.transform.position;
        Vector3 endPosition = new Vector3(targetPosition.position.x, startPosition.y, startPosition.z);
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            cardInstance.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cardInstance.transform.position = endPosition;
        Debug.Log($"Carta {cardInstance.name} movida al {targetPosition.name}");

        // Destruir la carta una vez que ha llegado a su destino
        Destroy(cardInstance);
    }

    // Clase Carta para saber el estado de cada carta
    private class CardInfo
    {
        public float MinXPosition { get; set; }
        public bool IsCorrectPosition { get; set; }
        private float timerStart;
        // Control de animación
        public bool animation { get; set; }

        public CardInfo(float minXPosition, bool isCorrectPosition, bool animation)
        {
            MinXPosition = minXPosition;
            IsCorrectPosition = isCorrectPosition;
            ResetTimer();
            this.animation = animation;
        }

        public void ResetTimer()
        {
            timerStart = Time.time;
        }

        public bool HasTimerElapsed(float duration)
        {
            return Time.time - timerStart >= duration;
        }
    }
}




