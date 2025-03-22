using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using static UnityEngine.GraphicsBuffer;

public class Move1 : MonoBehaviour
{
    public GameObject model; 
    public ObserverBehaviour[] ImageTargets;
    public int currentTarget;
    public float speed = 1.0f;
    public int noTarget;
    public Button[] animal;
    private bool isMoving = false;

    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/

    //Función para mandar mover el modelo al presionar el botón
    public void goToTarget(int num)
    {
        noTarget = num;
        moveToNextMarket();
    }

    public void moveToNextMarket()
    {
        //Función para comprobar si el modelo está en movimiento
        if (!isMoving)
        {
            StartCoroutine(MoveModel());         
        }
    }

    //Función que detecta si un objetivo 
    public void detectTarget()
    {
        foreach (Button btn in animal)
        {
            btn.interactable = false;
        }
        foreach (ObserverBehaviour target in ImageTargets)
        {
            if (target != null && (target.TargetStatus.Status == Status.TRACKED || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
            {
                for (int i = 0; i < ImageTargets.Length; i++)
                {
                    if (ImageTargets[i] != null && ImageTargets[i].TargetName == target.TargetName)
                    {
                        animal[i].interactable = true;
                    }
                }
            }
        }
    }

    //Corrutina
    private IEnumerator MoveModel()
    {
        isMoving = true;
        ObserverBehaviour target = GetNextDetectedTarget();
        if (target == null)
        {
            isMoving = false;
            yield break; //Final de corrutina
        }

        Vector3 startPosition = model.transform.position;
        Vector3 endPosition = target.transform.position;

        float journey = 0;

        //Movimiento el modelo 3D
        while (journey <= 1f)
        {
            journey += Time.deltaTime * speed;
            model.transform.position = Vector3.Lerp(startPosition, endPosition, journey);
            yield return null;
        }

        currentTarget = (currentTarget + 1) % ImageTargets.Length;
        isMoving = false; //Terminó el recorrido

    }

    //Objetivo al que va a llegar
    //private ObserverBehaviour GetNextDetectedTarget()
    //{
    //    foreach (ObserverBehaviour target in ImageTargets)
    //    {
    //        if (target != null && (target.TargetStatus.Status == Status.TRACKED || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
    //        {
    //            return target; //Regresa el objetivo encontrado
    //        }
    //    }
    //    return null;
    //}

    private ObserverBehaviour GetNextDetectedTarget()
    {
        foreach (ObserverBehaviour target in ImageTargets)
        {
            if (target != null && (target.TargetStatus.Status == Status.TRACKED || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
            {                
                return ImageTargets[noTarget]; //Regresa el objetivo asociado al número entero
            }
        }
        return null;
    }

    //// Update is called once per frame
    void Update()
    {
        detectTarget();
    }
}