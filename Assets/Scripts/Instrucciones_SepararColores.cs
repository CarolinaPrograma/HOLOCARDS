using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instrucciones_SepararColores : MonoBehaviour
{
    public GameObject situarTapices;
    public GameObject Instrucciones;
    public GameObject Activar_Juego;

    public GameObject tapiz; 
    private TapToPlace tapizToPlace;

    public GameObject botonConfirmar;

    public void Start()
    {
        situarTapices.SetActive(true);
        Instrucciones.SetActive(false);
        Activar_Juego.SetActive(false);

        tapizToPlace = tapiz.GetComponent<TapToPlace>();
    }

    public void SituarTapices()
    {
        tapiz.SetActive(true);
        botonConfirmar.SetActive(true);
        situarTapices.SetActive(false);
        tapizToPlace.StartPlacement();
    }

    public void ConfirmarTapicesColocados()
    {
        tapizToPlace.StopPlacement();
        Instrucciones.SetActive(true);
        botonConfirmar.SetActive(false);
    }

    public void ActivarSepararColores()
    {
        Instrucciones.SetActive(false);
        Activar_Juego.SetActive(true);
    }
}
