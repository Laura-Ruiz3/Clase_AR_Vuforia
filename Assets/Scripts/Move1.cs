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
    public GameObject[] itemIcons;
    public ObserverBehaviour[] ImageTargets;
    public int currentTarget;
    public float speed = 1.0f;
    public int noTarget = 0;
    public Button[] animal;
    private bool isMoving = false;
    public GameObject textBox;

    //Estados
    Transform textBoxItem;
    TMP_Text textFound;
    private bool[] itemFound;

    //Texto
    public TMP_Text textTemplate;
    public TMP_Text[] textComponent;
    public TMP_Text[] itemPicked;
    public float delay = 0.05f;
    private string fullText;
    private bool stopTextCoroutine = false;

    //Start is called before the first frame update
    void Start()
    {        
        textBoxItem = GameObject.Find("TextBox").transform;
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
        foreach (GameObject item in itemIcons)
        {
            item.SetActive(false);
        }
        itemFound = new bool[5];
        print(textBoxItem.Find("Mission")?.GetComponent<TMP_Text>().text);
        //fullText = textBoxItem.Find("Mission")?.GetComponent<TMP_Text>().text;
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
        if (noTarget > 0)
        {
            items[noTarget - 1].SetActive(true);
            //if (noTarget < 4)
            //{
            //    itemIcons[noTarget - 1].SetActive(true);
            //}
        }
        //StartCoroutine(ShowIcon(noTarget));
        StartCoroutine(ShowText(noTarget));
    }


    private IEnumerator ShowText(int noTarget)
    {        
        stopTextCoroutine = false;
        textBox.SetActive(true);
        textTemplate.text = "";
        yield return new WaitForSeconds(0.5f);
        Debug.Log(noTarget);
        Debug.Log(itemFound[0]);
        switch (noTarget)
        {
            case 0:
                if (itemFound[0] == false) {
                    itemFound[0] = true;
                    fullText = textBoxItem.Find("Mission")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }

                    yield return new WaitForSeconds(1.0f);

                    fullText = textBoxItem.Find("Mission (1)")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }

                    yield return new WaitForSeconds(1.0f);

                    fullText = textBoxItem.Find("Mission (2)")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }
                }
                else
                {
                    fullText = textBoxItem.Find("MissionRead")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }
                }
                break;
            case 1:
                itemIcons[noTarget - 1].SetActive(true);
                if (itemFound[1] == false)
                {
                    itemFound[1] = true;
                    fullText = textBoxItem.Find("FarmHoe")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }

                    yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    fullText = textBoxItem.Find("FarmHoeFounded")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }
                }
                break;
            case 2:
                if (itemFound[2] == false)
                {
                    if (itemFound[1] == false)
                    {
                        fullText = textBoxItem.Find("GoldNotAchieved")?.GetComponent<TMP_Text>().text;
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            yield return new WaitForSeconds(delay);
                        }
                    }
                    else {
                        itemIcons[noTarget - 1].SetActive(true);
                        itemFound[2] = true;
                        fullText = textBoxItem.Find("Gold")?.GetComponent<TMP_Text>().text;
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            yield return new WaitForSeconds(delay);
                        }

                        yield return new WaitForSeconds(1.0f);
                    }
                }
                else
                {
                    fullText = textBoxItem.Find("GoldFounded")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }
                }
                break;
            case 3:
                if (itemFound[3] == false)
                {
                    if (itemFound[2] == false)
                    {
                        fullText = textBoxItem.Find("WheatNotAchieved")?.GetComponent<TMP_Text>().text;
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            yield return new WaitForSeconds(delay);
                        }
                    }
                    else {
                        itemIcons[noTarget - 1].SetActive(true);
                        itemFound[3] = true;
                        fullText = textBoxItem.Find("Wheat")?.GetComponent<TMP_Text>().text;
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            yield return new WaitForSeconds(delay);
                        }

                        yield return new WaitForSeconds(1.0f);
                    }
                }
                else
                {
                    fullText = textBoxItem.Find("WheatFounded")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }
                }
                break;
            case 4:
                if (itemFound[4] == false)
                {
                    if (itemFound[3] == false)
                    {
                        fullText = textBoxItem.Find("BlueChickenNotAchieved")?.GetComponent<TMP_Text>().text;
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            yield return new WaitForSeconds(delay);
                        }
                    }
                    else {
                        itemFound[4] = true;
                        fullText = textBoxItem.Find("BlueChicken")?.GetComponent<TMP_Text>().text;
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            yield return new WaitForSeconds(delay);
                        }
                    }

                    yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    fullText = textBoxItem.Find("BlueChickenFounded")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }
                }
                break;
            default:
                fullText = textBoxItem.Find("Bug")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    yield return new WaitForSeconds(delay);
                }
                break;
        }
        yield return new WaitForSeconds(1.0f);
        textBox.SetActive(false);
        textTemplate.text = "";        
    }

    //private IEnumerator ShowIcon(int noTarget)
    //{
    //    stopTextCoroutine = true;
    //    yield return new WaitForSeconds(0.5f);
    //    if (noTarget > 0 && noTarget < 3)
    //    {
    //        itemIcons[noTarget - 1].SetActive(true);
    //    }
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