using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject menuPrincipal;
    public GameObject juego1;
    public GameObject juego2;
    public GameObject juego3;

    public void ActivarSepararColores()
    {
        menuPrincipal.SetActive(false);
        juego1.SetActive(true);
        juego2.SetActive(false);
        juego3.SetActive(false);
    }

    public void ActivarAsociarFrases()
    {
        menuPrincipal.SetActive(false);
        juego1.SetActive(false);
        juego2.SetActive(true);
        juego3.SetActive(false);
    }

    public void ActivarRecordarCartas()
    {
        menuPrincipal.SetActive(false);
        juego1.SetActive(false);
        juego2.SetActive(false);
        juego3.SetActive(true);
    }

    public void VolverAlMenu()
    {
        menuPrincipal.SetActive(true);
        juego1.SetActive(false);
        juego2.SetActive(false);
        juego3.SetActive(false);
    }
}
