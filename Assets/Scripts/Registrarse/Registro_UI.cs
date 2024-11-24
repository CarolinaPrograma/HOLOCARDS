using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class Registro_UI : MonoBehaviour
{
    private string apiKey = "AIzaSyCjV92iYQViqtOVUnK9OVdiFH0K2LwNX3c";
    public GameObject menuPrincipal;
    public GameObject login_registro;

    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;

    private static readonly HttpClient httpClient = new HttpClient();

    public async void LoginUser()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Por favor, introduce un correo y una contraseña válidos.");
            feedbackText.text = "Por favor, introduce un correo y una contraseña válidos.";
            return;
        }

        await LoginCoroutine(email, password);
    }

    private async Task LoginCoroutine(string email, string password)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
        Debug.Log(email);
        Debug.Log(password);

        FirebaseAuthRequest payload = new FirebaseAuthRequest
        {
            email = email,
            password = password,
            returnSecureToken = true
        };
        string jsonPayload = JsonUtility.ToJson(payload);
        Debug.Log("JSON Payload: " + jsonPayload);

        try
        {

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Debug.Log("Inicio de sesión exitoso: " + responseContent);

                FirebaseAuthResponse authResponse = JsonUtility.FromJson<FirebaseAuthResponse>(responseContent);

                PlayerPrefs.SetString("patientId", authResponse.localId);
                PlayerPrefs.SetString("firebaseIdToken", authResponse.idToken);
                PlayerPrefs.Save();

                feedbackText.text = "Inicio de sesión exitoso. Bienvenido: " + email;
                login_registro.SetActive(false);
                menuPrincipal.SetActive(true);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.Log("Error al iniciar sesión: " + errorContent);
                feedbackText.text = "Error al iniciar sesión. Email o contraseña incorrectas " ;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Excepción durante el inicio de sesión: " + e.Message);
            feedbackText.text = "Error al conectar con el servidor.";
        }
    }

    [System.Serializable]
    private class FirebaseAuthRequest
    {
        public string email;
        public string password;
        public bool returnSecureToken;
    }

    [System.Serializable]
    private class FirebaseAuthResponse
    {
        public string idToken;
        public string email;
        public string refreshToken;
        public string localId;
        public string displayName;
    }
}