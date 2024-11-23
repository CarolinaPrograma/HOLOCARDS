using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEditor;
using System.Threading.Tasks;
using System.Linq;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject gameButtonPrefab; // Prefab del botón que representa cada juego
    public Transform buttonsContainer;  // Contenedor de los botones

    private FirebaseFirestore firestore;

    public GameObject si_juegos;
    public GameObject no_juegos;



    public GameObject MenuPrincipal_vistas;

    public GameObject RecordarCartas_vistas;
    public RecordarCartas recordarCartas;


    public GameObject MemorizarPalabras_vistas;
    public AsociarFrases AsociarFrases;

    public GameObject SumarCartas_vistas;
    public SumarCartas_n Sumarcartas;


    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        CheckGamesForToday();
    }


    private void CheckGamesForToday()
    {
        string patientId = PlayerPrefs.GetString("patientId", "");
        if (string.IsNullOrEmpty(patientId))
        {
            Debug.LogError("No se encontró la ID del paciente en PlayerPrefs.");
            return;
        }

        CollectionReference gamesRef = firestore.Collection("patients").Document(patientId).Collection("assignedGames");

        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
        gamesRef.WhereEqualTo("fecha", todayDate).WhereEqualTo("status", "pendiente").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                QuerySnapshot snapshot = task.Result;

                if (snapshot.Count > 0)
                {
                    Debug.Log("Se encontraron juegos para hoy.");
                    no_juegos.SetActive(false);
                    si_juegos.SetActive(true);

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        string gameId = document.Id;
                        Debug.Log(gameId);
                        Dictionary<string, object> gameData = document.ToDictionary();
                        CreateGameButton(gameId, gameData);
                    }
                }
                else
                {
                    Debug.Log("No hay juegos asignados para hoy.");
                    no_juegos.SetActive(true);
                    si_juegos.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Error al obtener los juegos: " + task.Exception);
            }
        });
    }


    private void CreateGameButton(string gameId, Dictionary<string, object> gameData)
    {

        GameObject newButton = Instantiate(gameButtonPrefab, buttonsContainer);
        string nombre;

        TMP_Text buttonText_Titulo = newButton.transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TMP_Text>();
        TMP_Text buttonText_info = newButton.transform.Find("Informacion").GetComponent<TMP_Text>();
        TMP_Text buttonText_icon = newButton.transform.Find("Frontplate/AnimatedContent/Icon/UIButtonFontIcon").GetComponent<TMP_Text>();

        if ($"{gameData["juego"]}" == "1")
        {
            nombre = "Recordar Cartas";
            buttonText_Titulo.text = $"{nombre}";
            buttonText_info.text = $"Cartas: {gameData["numero_cartas"]} | Tiempo: {gameData["tiempo_total"]}s | Modalidad: {gameData["tipo_cartas"]}";
            buttonText_icon.text = "\uF2F6";
        }
        else if ($"{gameData["juego"]}" == "2")
        {
            nombre = "Memorizar palabras";
            buttonText_Titulo.text = $"{nombre}";
            buttonText_info.text = $"Cartas: {gameData["numero_cartas"]} | Tiempo: {gameData["tiempo_total"]}s | Modalidad: {gameData["tipo_cartas"]}";
            buttonText_icon.text = "\uF287";
        }
        else
        {
            nombre = "Sumar Cartas";
            buttonText_Titulo.text = $"{nombre}";
            buttonText_info.text = $"Parejas: {gameData["numero_cartas"]} | Tiempo: {gameData["tiempo_total"]}s | Modalidad: {gameData["tipo_cartas"]}";
            buttonText_icon.text = "\uF10A";
        }

        PressableButton interactable = newButton.GetComponent<PressableButton>();
        if (interactable == null)
        {
            Debug.LogError("Interactable component is missing on the button prefab.");
            return;
        }

        // Add the onClick event
        interactable.OnClicked.AddListener(() => boton_juego(nombre, gameId, gameData)); 
    }

    private void boton_juego(string buttonText_Titulo, string Id, Dictionary<string, object> gameData)
    {
        Debug.Log("LLamo a boton_juego");
        if (buttonText_Titulo == "Recordar Cartas")
        {
            int numeroCartas = Convert.ToInt32(gameData["numero_cartas"]);
            int tiempoTotal = Convert.ToInt32(gameData["tiempo_total"]);
            int tiempoPanel = Convert.ToInt32(gameData["tiempo_panel"]);
            string tipoCartas = gameData["tipo_cartas"].ToString();

            RecordarCartas_vistas.SetActive(true);
            recordarCartas.Recordar_Cartas(Id, numeroCartas, tiempoTotal, tipoCartas, tiempoPanel);
            MenuPrincipal_vistas.SetActive(false);
        }

        if (buttonText_Titulo == "Memorizar palabras")
        {
            int numeroCartas = Convert.ToInt32(gameData["numero_cartas"]);
            int tiempoTotal = Convert.ToInt32(gameData["tiempo_total"]);
            int tiempoPanel = Convert.ToInt32(gameData["tiempo_panel"]);
            string tipoCartas = gameData["tipo_cartas"].ToString();

            MemorizarPalabras_vistas.SetActive(true);
            AsociarFrases.Asociar_Frases(Id, numeroCartas, tiempoTotal, tipoCartas, tiempoPanel);
            MenuPrincipal_vistas.SetActive(false);
        }

        if (buttonText_Titulo == "Sumar Cartas")
        {
            int numeroCartas = Convert.ToInt32(gameData["numero_cartas"]);
            int tiempoTotal = Convert.ToInt32(gameData["tiempo_total"]);
            string tipoCartas = gameData["tipo_cartas"].ToString();

            SumarCartas_vistas.SetActive(true);
            Sumarcartas.Sumar_Cartas(Id, numeroCartas, tiempoTotal, tipoCartas);
            MenuPrincipal_vistas.SetActive(false);
        }

    }

    public async Task resultados_SumarCartas(string id, List<int> tiempo_suma, int tiempo_tardado, bool exito, int aciertos, int fallos)
    {
        Dictionary<string, object> resultados = new Dictionary<string, object>
    {
        { "tiempoTotal", tiempo_tardado },
        { "tiemposSumas", tiempo_suma },
        { "exito", exito },
        { "aciertos", aciertos },
        { "fallos", fallos },
        { "fecha", Timestamp.GetCurrentTimestamp() }
    };

        Debug.Log("Diccionario de resultados a subir: " + string.Join(", ", resultados.Select(kv => kv.Key + ": " + kv.Value)));
        await SubirResultadosAFirebase(id, resultados);
    }

    public async Task resultados_RecordarCartas(string id, int numero_pistas, bool exito, int numero_intentos, int tiempo_tardado)
    {
        Dictionary<string, object> resultados = new Dictionary<string, object>
    {
        { "numero_pistas", numero_pistas },
        { "exito", exito },
        { "numero_intentos", numero_intentos },
        { "tiempo_tardado", tiempo_tardado },
        { "fecha", Timestamp.GetCurrentTimestamp() }
    };

        Debug.Log("Diccionario de resultados a subir: " + string.Join(", ", resultados.Select(kv => kv.Key + ": " + kv.Value)));
        await SubirResultadosAFirebase(id, resultados);
    }

    public async Task resultados_AsociarFrases(string id, List<int> tiempo_carta, bool exito, int aciertos, int fallos, int numero_pistas)
    {
        Dictionary<string, object> resultados = new Dictionary<string, object>
    {
        { "tiempo_carta", tiempo_carta },
        { "exito", exito },
        { "aciertos", aciertos },
        { "fallos", fallos },
        { "numero_pistas", numero_pistas },
        { "fecha", Timestamp.GetCurrentTimestamp() }
    };

        Debug.Log("Diccionario de resultados a subir: " + string.Join(", ", resultados.Select(kv => kv.Key + ": " + kv.Value)));
        await SubirResultadosAFirebase(id, resultados);
    }

    public async Task SubirResultadosAFirebase(string juegoId, Dictionary<string, object> resultados)
    {
        string patientId = PlayerPrefs.GetString("patientId", "");

        DocumentReference docRef = firestore.Collection("patients").Document(patientId)
                                       .Collection("assignedGames").Document(juegoId);

        Dictionary<string, object> juegoData = new Dictionary<string, object>
        {
            { "resultados", resultados },
            { "status", "completado" }
        };

        try
        {
            await docRef.SetAsync(juegoData, SetOptions.MergeAll);
            Debug.Log("Parámetros y resultados subidos correctamente a Firebase.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error al subir los datos del juego: " + e);
        }
    }
}