using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MixedReality.Toolkit.UX;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Linq;


public class MenuPrincipal : MonoBehaviour
{
    public GameObject gameButtonPrefab;
    public Transform buttonsContainer;

    private static readonly HttpClient client = new HttpClient();
    private const string FIREBASE_PROJECT_ID = "holocards-2b934";
    private const string FIREBASE_API_KEY = "AIzaSyCjV92iYQViqtOVUnK9OVdiFH0K2LwNX3c";
    private string BASE_URL = $"https://firestore.googleapis.com/v1/projects/{FIREBASE_PROJECT_ID}/databases/(default)/documents";

    public GameObject si_juegos;
    public GameObject no_juegos;

    public GameObject MenuPrincipal_vistas;

    public GameObject RecordarCartas_vistas;
    public RecordarCartas recordarCartas;


    public GameObject MemorizarPalabras_vistas;
    public AsociarFrases AsociarFrases;

    public GameObject SumarCartas_vistas;
    public SumarCartas_n Sumarcartas;


    async void OnEnable()
    {
        ClearButtonsContainer();
        await CheckGamesForToday();
    }

    private async Task CheckGamesForToday()
    {
        Debug.Log("Entro a CheckGamesForToday");
        string patientId = PlayerPrefs.GetString("patientId", "");
        if (string.IsNullOrEmpty(patientId))
        {
            Debug.LogError("No se encontró la ID del paciente en PlayerPrefs.");
            return;
        }
        Debug.Log("id del paciente: " + patientId);
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
        string collectionPath = $"patients/{patientId}/assignedGames";

        try
        {
            string url = $"{BASE_URL}/{collectionPath}?key={FIREBASE_API_KEY}";
            Debug.Log("url: " + url);
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log("responseBody: " + responseBody);

            var documents = JsonUtility.FromJson<ResponseBody>(responseBody);
            Debug.Log("documents: " + documents);

            if (documents != null && documents.documents != null)
            {
                var gamesForToday = new List<Document>();

                foreach (var doc in documents.documents)
                {
                    Debug.Log("Foreach de cada DOC: " + doc);
                    if (doc.fields.fecha != null &&
                    doc.fields.fecha.stringValue == todayDate &&
                    doc.fields.status != null &&
                    doc.fields.status.stringValue == "pendiente")
                    {
                        gamesForToday.Add(doc);
                    }
                }

                if (gamesForToday.Count > 0)
                {
                    Debug.Log("Se encontraron juegos para hoy.");
                    no_juegos.SetActive(false);
                    si_juegos.SetActive(true);

                    foreach (var game in gamesForToday)
                    {
                        string gameId = game.name.Split('/')[game.name.Split('/').Length - 1];
                        Debug.Log(gameId);
                        CreateGameButton(gameId, game.fields);
                    }
                }
                else
                {
                    Debug.Log("No hay juegos asignados para hoy.");
                    no_juegos.SetActive(true);
                    si_juegos.SetActive(false);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener los juegos: {ex.Message}");
        }
    }

    private void CreateGameButton(string gameId, Fields gameData)
    {
        GameObject newButton = Instantiate(gameButtonPrefab, buttonsContainer);
        string nombre;

        TMP_Text buttonText_Titulo = newButton.transform.Find("Frontplate/AnimatedContent/Text").GetComponent<TMP_Text>();
        TMP_Text buttonText_info = newButton.transform.Find("Informacion").GetComponent<TMP_Text>();
        TMP_Text buttonText_icon = newButton.transform.Find("Frontplate/AnimatedContent/Icon/UIButtonFontIcon").GetComponent<TMP_Text>();

        if (gameData.juego != null && gameData.juego.integerValue == "1")
        {
            nombre = "Recordar Cartas";
            buttonText_Titulo.text = $"{nombre}";
            buttonText_info.text = $"Cartas: {gameData.numero_cartas?.integerValue} | Tiempo: {gameData.tiempo_total?.integerValue}s | Modalidad: {gameData.tipo_cartas?.stringValue}";
            buttonText_icon.text = "\uF2F6";
        }
        else if (gameData.juego != null && gameData.juego.integerValue == "2")
        {
            nombre = "Memorizar palabras";
            buttonText_Titulo.text = $"{nombre}";
            buttonText_info.text = $"Cartas: {gameData.numero_cartas?.integerValue} | Tiempo: {gameData.tiempo_total?.integerValue}s | Modalidad: {gameData.tipo_cartas?.stringValue}";
            buttonText_icon.text = "\uF287";
        }
        else
        {
            nombre = "Sumar Cartas";
            buttonText_Titulo.text = $"{nombre}";
            buttonText_info.text = $"Parejas: {gameData.numero_cartas?.integerValue} | Tiempo: {gameData.tiempo_total?.integerValue}s | Modalidad: {gameData.tipo_cartas?.stringValue}";
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

    private void boton_juego(string buttonText_Titulo, string Id, Fields gameData)
    {
        Debug.Log("LLamo a boton_juego");
        if (buttonText_Titulo == "Recordar Cartas")
        {
            int numeroCartas = Convert.ToInt32(gameData.numero_cartas?.integerValue);
            int tiempoTotal = Convert.ToInt32(gameData.tiempo_total?.integerValue);
            int tiempoPanel = Convert.ToInt32(gameData.tiempo_panel?.integerValue);
            string tipoCartas = gameData.tipo_cartas?.stringValue;

            RecordarCartas_vistas.SetActive(true);
            recordarCartas.Recordar_Cartas(Id, numeroCartas, tiempoTotal, tipoCartas, tiempoPanel);
            MenuPrincipal_vistas.SetActive(false);
        }

        if (buttonText_Titulo == "Memorizar palabras")
        {
            int numeroCartas = Convert.ToInt32(gameData.numero_cartas?.integerValue);
            int tiempoTotal = Convert.ToInt32(gameData.tiempo_total?.integerValue);
            int tiempoPanel = Convert.ToInt32(gameData.tiempo_panel?.integerValue);
            string tipoCartas = gameData.tipo_cartas?.stringValue;

            MemorizarPalabras_vistas.SetActive(true);
            AsociarFrases.Asociar_Frases(Id, numeroCartas, tiempoTotal, tipoCartas, tiempoPanel);
            MenuPrincipal_vistas.SetActive(false);
        }

        if (buttonText_Titulo == "Sumar Cartas")
        {
            int numeroCartas = Convert.ToInt32(gameData.numero_cartas?.integerValue);
            int tiempoTotal = Convert.ToInt32(gameData.tiempo_total?.integerValue);
            string tipoCartas = gameData.tipo_cartas?.stringValue;

            SumarCartas_vistas.SetActive(true);
            Sumarcartas.Sumar_Cartas(Id, numeroCartas, tiempoTotal, tipoCartas);
            MenuPrincipal_vistas.SetActive(false);
        }
    }

    public async Task resultados_SumarCartas(string id, List<int> tiempo_suma, int tiempo_tardado, bool exito, int aciertos, int fallos)
    {
        SumarCartas_clase sumar = new SumarCartas_clase();
        sumar.fallos = fallos;
        sumar.tiempo_suma = tiempo_suma;
        sumar.tiempo_tardado = tiempo_tardado;
        sumar.exito = exito;

        string json = $@"
            {{
                ""fields"": {{
                    ""resultados"": {{
                        ""mapValue"": {{
                            ""fields"": {{
                                ""fallos"": {{ ""integerValue"": {sumar.fallos} }},
                                ""tiempo_suma"": {{
                                    ""arrayValue"": {{
                                        ""values"": [{string.Join(", ", sumar.tiempo_suma.Select(t => $"{{ \"integerValue\": {t} }}"))}]
                                    }}
                                }},
                                ""tiempo_tardado"": {{ ""integerValue"": {sumar.tiempo_tardado} }},
                                ""exito"": {{ ""booleanValue"": {sumar.exito.ToString().ToLower()} }}
                            }}
                        }}
                    }},
                    ""status"": {{ ""stringValue"": ""completado"" }}
                }}
            }}";

        await SendToFirestore(id, json);
    }


    public async Task resultados_AsociarFrases(string id, List<int> tiempo_carta, bool exito, int aciertos, int fallos, int numero_pistas)
    {
        AsociarFrases_clase asociar = new AsociarFrases_clase();
        asociar.tiempo_carta = tiempo_carta;
        asociar.fallos = fallos;
        asociar.aciertos = aciertos;
        asociar.numero_pistas = numero_pistas;
        asociar.exito = exito;

        string json = $@"
            {{
                ""fields"": {{
                    ""resultados"": {{
                        ""mapValue"": {{
                            ""fields"": {{
                                ""tiempo_carta"": {{
                                    ""arrayValue"": {{
                                        ""values"": [{string.Join(", ", asociar.tiempo_carta.Select(t => $"{{ \"integerValue\": {t} }}"))}]
                                    }}
                                }},
                                ""aciertos"": {{ ""integerValue"": {asociar.aciertos} }},
                                ""fallos"": {{ ""integerValue"": {asociar.fallos} }},
                                ""numero_pistas"": {{ ""integerValue"": {asociar.numero_pistas} }},
                                ""exito"": {{ ""booleanValue"": {asociar.exito.ToString().ToLower()} }}
                            }}
                        }}
                    }},
                    ""status"": {{ ""stringValue"": ""completado"" }}
                }}
            }}";

        await SendToFirestore(id, json);
    }

    public async Task resultados_RecordarCartas(string id, int numero_pistas, bool exito, int numero_intentos, int tiempo_tardado)
    {
        RecordarCartas_clase recordar = new RecordarCartas_clase();
        recordar.numero_pistas = numero_pistas;
        recordar.exito = exito;
        recordar.numero_intentos = numero_intentos;
        recordar.tiempo_tardado = tiempo_tardado;

        string json = $@"
            {{
                ""fields"": {{
                    ""resultados"": {{
                        ""mapValue"": {{
                            ""fields"": {{
                                ""numero_pistas"": {{ ""integerValue"": {recordar.numero_pistas} }},
                                ""exito"": {{ ""booleanValue"": {recordar.exito.ToString().ToLower()} }},
                                ""numero_intentos"": {{ ""integerValue"": {recordar.numero_intentos} }},
                                ""tiempo_tardado"": {{ ""integerValue"": {recordar.tiempo_tardado} }}
                            }}
                        }}
                    }},
                    ""status"": {{ ""stringValue"": ""completado"" }}
                }}
            }}";

        await SendToFirestore(id, json);
    }

    public async Task SendToFirestore(string id, string json)
    {
        HttpClient httpClient = new HttpClient();
        string patientId = PlayerPrefs.GetString("patientId", "");
        string url = $"https://firestore.googleapis.com/v1/projects/holocards-2b934/databases/(default)/documents/patients/{patientId}/assignedGames/{id}?updateMask.fieldPaths=resultados&updateMask.fieldPaths=status\r\n";

        try
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string token = PlayerPrefs.GetString("firebaseIdToken", "");
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(url),
                Content = content
            };

            var response = await httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Data successfully sent to Firestore");
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Error sending data: {response.ReasonPhrase}");
                Debug.LogError($"Response: {responseBody}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred: {ex.Message}");
        }
    }

    [Serializable]
    public class ResponseBody
    {
        public Document[] documents;
    }

    [Serializable]
    public class Document
    {
        public string name;
        public Fields fields;
        public string createTime;
        public string updateTime;
    }

    [Serializable]
    public class Fields
    {
        public IntegerValue numero_cartas;
        public StringValue fecha;
        public IntegerValue tiempo_panel;
        public IntegerValue tiempo_total;
        public StringValue status;
        public IntegerValue juego;
        public StringValue tipo_cartas;
        public MapValue resultados;
    }

    [Serializable]
    public class IntegerValue
    {
        public string integerValue;
    }

    [Serializable]
    public class StringValue
    {
        public string stringValue;
    }

    [Serializable]
    public class MapValue
    {
        public Fields fields;
    }

    [Serializable]
    public class RecordarCartas_clase
    {
        public int numero_pistas = 0;
        public bool exito;
        public int numero_intentos = 0;
        public int tiempo_tardado;
    }

    [Serializable]
    public class SumarCartas_clase
    {
        public List<int> tiempo_suma = new List<int>();
        public int tiempo_tardado;
        public bool exito;
        public int aciertos = 0;
        public int fallos = 0;
    }

    [Serializable]
    public class AsociarFrases_clase
    {
        public int aciertos;
        public int fallos;
        public int numero_pistas;
        public bool exito;
        public List<int> tiempo_carta = new List<int>();
    }

    public void ClearButtonsContainer()
    {
        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }
    }
}



