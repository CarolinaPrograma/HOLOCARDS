using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class instrucciones_recordar : MonoBehaviour
{
    public GameObject ins_1;
    public GameObject ins_2;
    public GameObject ins_3;
    public GameObject instrucciones;

    public GameObject panel;
    public GameObject boton_show_panel;

    public RecordarCartas recordarCartas;

    public GameObject MenuPrincipal_vista;
    public GameObject RecordarCartas_vista;

    void Start()
    {
        ins_1.SetActive(true);
        ins_2.SetActive(false);
        ins_3.SetActive(false);
    }

    public void ins_sig_2()
    {
        ins_1.SetActive(false);
        ins_2.SetActive(true);
        ins_3.SetActive(false);
    }

    public void ins_jug_3()
    {
        ins_1.SetActive(false);
        ins_2.SetActive(false);
        ins_3.SetActive(true);
    }

    public void empezar_juego()
    {
        instrucciones.SetActive(false);
        recordarCartas.EmpezarJuego();
    }

    public void volver_menuprincipal()
    {
        MenuPrincipal_vista.SetActive(true);
        RecordarCartas_vista.SetActive(false);

    }
    public void showPanel()
    {
        if (panel.activeSelf)
        {
            panel.SetActive(false);
            boton_show_panel.SetActive(true);
        }
        else
        {
            panel.SetActive(true);
            boton_show_panel.SetActive(false);
        }

    }
}
