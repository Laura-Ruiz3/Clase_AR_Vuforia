using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using static UnityEngine.GraphicsBuffer;

public class Move1 : MonoBehaviour
{
    public GameObject model;
    public GameObject[] items;
    public ObserverBehaviour[] ImageTargets;
    public int currentTarget;
    public float speed = 1.0f;
    public int noTarget = 0;
    public Button[] animal;
    private bool isMoving = false;
    public GameObject textBox;

    //Texto
    public TMP_Text textTemplate;
    public TMP_Text[] textComponent;
    public float delay = 0.05f;
    public float delay1 = 5.0f;
    private string fullText;
    private bool stopTextCoroutine = false;

    //Start is called before the first frame update
    void Start()
    {        
        textTemplate.text = "";
        textBox.SetActive(false);
        foreach (TMP_Text text in textComponent)
        {
            text.gameObject.SetActive(false);
        }
        foreach (GameObject item in items)
        {
            item.SetActive(false);
        }
        StartCoroutine(ShowText(noTarget));
    }

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
        stopTextCoroutine = true;
        ObserverBehaviour target = GetNextDetectedTarget();
        if (target == null)
        {
            isMoving = false;
            yield break; //Final de corrutina
        }

        Vector3 startPosition = model.transform.position;
        Vector3 endPosition = target.transform.position;

        float journey = 0;

        textBox.SetActive(false);
        textTemplate.text = "";
        //Movimiento el modelo 3D
        while (journey <= 1f)
        {            
            journey += Time.deltaTime * speed;
            model.transform.position = Vector3.Lerp(startPosition, endPosition, journey);
            yield return null;
        }

        currentTarget = (currentTarget + 1) % ImageTargets.Length;
        isMoving = false; //Terminó el recorrido        
        if(noTarget > 0)
            items[noTarget-1].SetActive(true);
        StartCoroutine(ShowText(noTarget));
    }


    private IEnumerator ShowText(int noTarget)
    {
        stopTextCoroutine = false;
        textBox.SetActive(true);
        textTemplate.text = "";
        yield return new WaitForSeconds(0.5f);
        Debug.Log(noTarget);
        if (noTarget == 0)
        {
            yield return new WaitForSeconds(delay1);
            textComponent[0].enabled = true;
            fullText = textComponent[0].text;
            textTemplate.text = "";
            foreach (char letter in fullText)
            {
                if (stopTextCoroutine)
                    yield break;
                Debug.Log("En función: " + textTemplate.text);
                textTemplate.text += letter;
                yield return new WaitForSeconds(delay);
            }

            yield return new WaitForSeconds(1.0f);

            fullText = textComponent[1].text;
            textTemplate.text = "";
            foreach (char letter in fullText)
            {
                if (stopTextCoroutine)
                    yield break;
                Debug.Log("En función: " + textTemplate.text);
                textTemplate.text += letter;
                yield return new WaitForSeconds(delay);
            }
        }
        else
        {
            fullText = textComponent[noTarget + 1].text;
            foreach (char letter in fullText)
            {
                if (stopTextCoroutine)
                    yield break;
                Debug.Log("En función: " + textTemplate.text);
                textTemplate.text += letter;
                yield return new WaitForSeconds(delay);
            }
        }
        yield return new WaitForSeconds(1.0f);
        textBox.SetActive(false);
        textTemplate.text = "";
    }

    //private IEnumerator ShowText(int noTarget)
    //{
    //    textBox.SetActive(true);
    //    textTemplate.text = "";

    //    yield return null;

    //    yield return new WaitForSeconds(0.5f);

    //    // Validar que noTarget + 1 esté dentro de los límites de textComponent
    //    if (noTarget == 0)
    //    {
    //        yield return new WaitForSeconds(delay1);

    //        textComponent[0].enabled = true;
    //        fullText = textComponent[0].text;
    //        textTemplate.text = "";

    //        foreach (char letter in fullText)
    //        {
    //            textTemplate.text += letter;
    //            yield return new WaitForSeconds(delay);
    //        }

    //        yield return new WaitForSeconds(delay1);
    //        textTemplate.text = "";

    //        fullText = textComponent[1].text;
    //        while (isMoving != false)
    //        {
    //            foreach (char letter in fullText)
    //            {
    //                textTemplate.text += letter;
    //                yield return new WaitForSeconds(delay);
    //            }
    //        }

    //        textTemplate.text = "";
    //    }
    //    else
    //    {
    //        foreach (Button btn in animal)
    //        {
    //            btn.interactable = false;
    //        }

    //        // Validar que noTarget + 1 no exceda los límites de textComponent
    //        if (noTarget + 1 < textComponent.Length)
    //        {
    //            fullText = textComponent[noTarget + 1].text;
    //            textTemplate.text = "";
    //            while (isMoving != false)
    //            {
    //                foreach (char letter in fullText)
    //                {
    //                    textTemplate.text += letter;
    //                    yield return new WaitForSeconds(delay);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogWarning("Índice fuera de rango en textComponent");
    //        }
    //    }

    //    yield return new WaitForSeconds(delay1);
    //    textBox.SetActive(false);
    //    textTemplate.text = "";
    //}

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