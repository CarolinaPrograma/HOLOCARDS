using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instrucciones : MonoBehaviour
{
    public GameObject explicacion;
    public GameObject empezar_juego;

    void Start()
    {
        explicacion.SetActive(true);
    }

    public void Jugar()
    {
        explicacion.SetActive(false);
        empezar_juego.SetActive(true);
    }
}
