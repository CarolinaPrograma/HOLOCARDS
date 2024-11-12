using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Registro_UI : MonoBehaviour
{
    public GameObject menuPrincipal;
    public GameObject login_registro;

    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public void RegisterNewUser()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Por favor, introduce un correo y una contraseña válidos.";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                feedbackText.text = "Error al registrar: " + task.Exception.InnerExceptions[0].Message;
            }
            else
            {

                Firebase.Auth.AuthResult authResult = task.Result;
                FirebaseUser newUser = authResult.User;

                feedbackText.text = "Registro exitoso. Usuario: " + newUser.Email;
                Debug.Log("Usuario registrado con éxito: " + newUser.Email);

                SaveUserToDatabase(newUser);

                menuPrincipal.SetActive(true);
                login_registro.SetActive(false);
            }
        });
    }


    private void SaveUserToDatabase(FirebaseUser user)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        Dictionary<string, object> userInfo = new Dictionary<string, object>
    {
        { "email", user.Email },
        { "uid", user.UserId },
        { "role", "paciente" },  
        { "registrationDate", FieldValue.ServerTimestamp }
    };

        db.Collection("users").Document(user.UserId).SetAsync(userInfo).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Usuario guardado en la base de datos Firestore.");
            }
            else
            {
                Debug.LogError("Error guardando en Firestore: " + task.Exception);
            }
        });
    }

    public async void LoginUser()
    {
        Debug.Log("Entro a loginuser");
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Por favor, introduce un correo y una contraseña válidos.");
            feedbackText.text = "Por favor, introduce un correo y una contraseña válidos.";
            return;
        }

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            if (result.User == null)
            {
                Debug.LogError("El usuario es null, algo falló en la autenticación.");
                feedbackText.text = "Error al obtener los datos del usuario.";
                return;
            }

            FirebaseUser user = result.User;
            Debug.Log("USER: " + user.UserId);
            PlayerPrefs.SetString("patientId", user.UserId);
            PlayerPrefs.Save();

            feedbackText.text = "Inicio de sesión exitoso. Bienvenido: " + user.Email;
            Debug.Log("Inicio de sesión exitoso");

            login_registro.SetActive(false);
            menuPrincipal.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.LogError("Error en el inicio de sesión: " + e.Message);
            feedbackText.text = "Error al iniciar sesión";
        }
    }
}