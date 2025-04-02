using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;

//using Unity.Android.Gradle;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using static UnityEngine.GraphicsBuffer;

public class MoveCopy : MonoBehaviour
{
    public GameObject model;
    public GameObject[] items;
    public GameObject[] itemIcons;
    public ObserverBehaviour[] ImageTargets;
    public ObserverBehaviour[] detectedMarkers;
    public ObserverBehaviour tmpTarget;
    public int currentTarget;
    public float speed = 1.0f;
    public int noTarget = 0;
    public int prevNoTarget = 0;
    public int firstTarget = 0;
    public int secondTarget = 0;
    public Button[] animal;
    private bool isMoving = false;    
    public AudioSource audioSource;
    public AudioClip[] clips;

    private const int CAMERA_MOVEMENT = 10;
    private const float DIR_THRESHOLD = 0.8f;
    private Vector2 startingPosition;

    //Estados
    Transform textBoxItem;
    Transform onlyOpt;
    Transform moreOpt;
    TMP_Text textFound;
    public bool[] itemFound;

    //Texto
    public GameObject textBox;
    public GameObject oneOpt;
    public GameObject twoOpt;
    public TMP_Text textTemplate;
    public TMP_Text[] textComponent;
    public TMP_Text[] options;
    public TMP_Text[] itemPicked;
    public float delay = 0.05f;
    private string fullText;
    private string optionTemplate;
    private bool stopTextCoroutine = false;

    //Gestos
    //List<ObserverBehaviour> availableTargets = new List<ObserverBehaviour>();
    List<int> availableTargets = new List<int>();
    //List<int> _lastVisibleTargets = new List<int>();

    //Start is called before the first frame update
    void Start()
    {
        textBoxItem = GameObject.Find("TextBox").transform;
        onlyOpt = GameObject.Find("OneOpt").transform;
        moreOpt = GameObject.Find("TwoOpt").transform;
        textTemplate.text = "";
        textBox.SetActive(false);
        oneOpt.SetActive(false);
        twoOpt.SetActive(false);

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
        foreach (Button btn in animal)
        {
            btn.gameObject.SetActive(false);
        }
        itemFound = new bool[6];
        print(textBoxItem.Find("Mission")?.GetComponent<TMP_Text>().text);
        //fullText = textBoxItem.Find("Mission")?.GetComponent<TMP_Text>().text;
        StartCoroutine(ShowText(noTarget));
    }

    void inputMovement()
    {
        //Debug.Log(Input.touchCount);

        if (Input.touchCount > 0)
        {
            Touch input = Input.GetTouch(0);

            switch (input.phase)
            {
                case TouchPhase.Began:
                    startingPosition = input.position;
                    break;
                case TouchPhase.Ended:
                    setDirection((input.position - startingPosition).normalized);
                    break;
            }
        }
    }

    void setDirection(Vector2 v2)
    {
        int tmp;
        if (availableTargets.Count == 2)
        {
            if (v2.y > DIR_THRESHOLD)
            {
                Debug.Log("Arriba");
                tmp = availableTargets[1];
                //if (availableTargets[1] != 5)
                //{                    
                //    availableTargets[1] = availableTargets[0];
                //    availableTargets[0] = tmp;
                //    Debug.Log("tmp es: " + tmp);
                //}                                    
                //if (noTarget != 5 || itemFound[5] == false)
                //    SwapElements(0,1);

                //if (noTarget != 5)
                //{
                //    if (availableTargets[1] == 5 && itemFound[5] == false)
                //    {
                //        Debug.Log("Adelante");
                //        Debug.Log("ItemFound: " + itemFound[5]);
                //        SwapElements(0, 1);
                //    }
                //}
                if (availableTargets[1] != 5)
                    SwapElements(0, 1);

                Debug.Log("Nueva lista: " + string.Join(", ", availableTargets));
                goToTarget(tmp);
            }
        }
        else if (availableTargets.Count == 3)
        {
            Debug.Log("El noTarget al deslizar es: " + noTarget);
            if (v2.x < -DIR_THRESHOLD)
            {
                //Debug.Log("Izquierda");
                tmp = availableTargets[1];
                //if (availableTargets[1] != 5)
                //{
                //    availableTargets[1] = availableTargets[0];
                //    availableTargets[0] = tmp;
                //    Debug.Log("tmp es: " + tmp);
                //}
                //if(noTarget != 5)
                //{
                //    if (itemFound[5] == true)
                //        Debug.Log("No swap");
                //    else if (availableTargets[1] == 5 && itemFound[5] == false)
                //    {
                //        Debug.Log("Izquierda");
                //        Debug.Log("ItemFound: " + itemFound[5]);
                //        SwapElements(0, 1);
                //    }                    
                //}
                if (availableTargets[1] != 5)
                    SwapElements(0, 1);

                Debug.Log("Nueva lista: " + string.Join(", ", availableTargets));
                goToTarget(tmp);
            }
            if (v2.x > DIR_THRESHOLD)
            {
                //Debug.Log("Derecha");
                tmp = availableTargets[2];
                //if (availableTargets[2] != 5)
                //{
                //    availableTargets[2] = availableTargets[0];
                //    availableTargets[0] = tmp;
                //    Debug.Log("tmp es: " + tmp);
                //}

                //if (noTarget != 5)
                //{
                //    if (availableTargets[2] == 5 && itemFound[5] == false)
                //    {
                //        Debug.Log("Derecha");
                //        Debug.Log("ItemFound: " + itemFound[5]);
                //        SwapElements(0, 2);
                //    }
                //}
                if (availableTargets[2] != 5)
                    SwapElements(0, 2);

                Debug.Log("Nueva lista: " + string.Join(", ", availableTargets));
                goToTarget(tmp);
            }
        }
    }

    //Función para mandar mover el modelo al presionar el botón
    public void goToTarget(int num)
    {
        noTarget = num;
        if (detectedMarkers == null)
        {
            detectedMarkers[0] = ImageTargets[noTarget];
            print("Marcador detectado: "+detectedMarkers[0]);
        }
        else if (detectedMarkers.Length == 1) {
            detectedMarkers[1] = ImageTargets[noTarget];
            print("Marcador detectado: " + detectedMarkers[1]);
        }
        if (noTarget != 5)
            moveToNextMarket(noTarget);
        else if (noTarget == 5 && itemFound[5] == false)
            moveToNextMarket(noTarget);
        else if (noTarget == 5 && itemFound[5] == true)
        {
            StartCoroutine(ShowText(noTarget));
        }
    }

    public void moveToNextMarket(int num)
    {
        noTarget = num;
        if (noTarget == 0 && itemFound[5] == true)
            prevNoTarget = 0;
        //Función para comprobar si el modelo está en movimiento
        if (!isMoving)
        {                        
            StartCoroutine(MoveModel());         
        }
    }

    public void moveToNextMarketAuto(int num)
    {
        noTarget = num;
        int tmp;
        //Función para comprobar si el modelo está en movimiento
        if (availableTargets.Count == 2)
        {
            if (availableTargets[0] == 5)
            {
                tmp = 0;
                availableTargets[1] = availableTargets[0];
                availableTargets[0] = tmp;
                Debug.Log("tmp es: " + tmp);
                SwapElements(0, 2);
            }
        }
        else if (availableTargets.Count == 3)
        {
            if (availableTargets[1] == 5)
            {
                tmp = availableTargets[2];
                availableTargets[2] = availableTargets[0];
                availableTargets[0] = tmp;
                Debug.Log("tmp es: " + tmp);
                SwapElements(0,2);
            }
            if (availableTargets[2] == 5)
            {
                tmp = availableTargets[1];
                availableTargets[1] = availableTargets[0];
                availableTargets[0] = tmp;
                Debug.Log("tmp es: " + tmp);
                SwapElements(0, 1);
            }
        }
        for (int i = 0; i < items.Length - 1; i++)
        {
            //itemFound [i] = false;
            print("Holi");
            items[i].SetActive(false);
        }
        for (int i = 0; i < itemIcons.Length; i++)
        {
            //itemFound [i] = false;
            print("Holi");
            itemIcons[i].SetActive(false);
        }
        for (int i = 1; i < itemFound.Length - 1; i++)
        {
            //itemFound [i] = false;
            print("Holi: "+i);
            itemFound[i] = false;
        }
        //if (availableTargets.Count == 3)
        //{
        //    if (noTarget == availableTargets[1])
        //    {
        //        SwapElements(0, availableTargets[1]);
        //    }
        //    else if (noTarget == availableTargets[2])
        //    {
        //        SwapElements(0, availableTargets[2]);
        //    }
        //}
        if (!isMoving)
        {
            StartCoroutine(MoveModel());
        }
    }

    public void detectTarget()
    {
        //availableTargets.Clear();
        //bool areEqual = availableTargets.OrderBy(x => x).SequenceEqual(_lastVisibleTargets.OrderBy(x => x));

        List<int> newVisibleTargets = new List<int>();

        //foreach (ObserverBehaviour target in ImageTargets)
        for (int i = 0; i < ImageTargets.Length; i++)
        {
            ObserverBehaviour target = ImageTargets[i];
            //if (target != null && (target.TargetStatus.Status == Status.TRACKED || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
            if (target != null && (target.TargetStatus.Status == Status.TRACKED))
            {
                for (int j = 0; j < ImageTargets.Length; j++)
                {
                    if (ImageTargets[j] != null && ImageTargets[j].TargetName == target.TargetName)
                    {
                        //availableTargets.Add(ImageTargets[i]);
                        //_lastVisibleTargets.Add(i);
                        if (!newVisibleTargets.Contains(i))
                            newVisibleTargets.Add(i);
                    }
                }
            }
            //if (target != null && (target.TargetStatus.Status == Status.NO_POSE || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
            //{
            //    for (int i = 0; i < ImageTargets.Length; i++)
            //    {
            //        if (ImageTargets[i] != null && ImageTargets[i].TargetName == target.TargetName)
            //        {
            //            //availableTargets.Add(ImageTargets[i]);
            //            _lastVisibleTargets.Remove(i);
            //        }
            //    }
            //}
        }
        //if (!availableTargets.SequenceEqual(newVisibleTargets))        

        bool check = availableTargets.OrderBy(x => x).SequenceEqual(newVisibleTargets.OrderBy(x => x));
        //Debug.Log("availableTargets: " + string.Join(", ", availableTargets));
        //Debug.Log("newTargets: " + string.Join(", ", newVisibleTargets));
        if (!check)
        {
            //availableTargets.AddRange(_lastVisibleTargets);
            //_lastVisibleTargets = new List<int>(availableTargets);
            //_lastVisibleTargets.Clear();
            availableTargets = new List<int>(newVisibleTargets);

            if (availableTargets.Count > 0)
            {
                if (availableTargets[0] == 5)
                    goToTarget(0);
            }
            if (availableTargets.Count == 2)
            {
                if (noTarget == availableTargets[1])
                {
                    //SwapElements(0, availableTargets[1]);
                    SwapElements(0, 1);
                }                
            }
            else if (availableTargets.Count == 3)
            {
                if (noTarget == availableTargets[1])
                {
                    //SwapElements(0, availableTargets[1]);
                    SwapElements(0, 1);
                }
                if (noTarget == availableTargets[2])
                {
                    //SwapElements(0, availableTargets[2]);
                    SwapElements(0, 2);
                }
            }         

            //for (int i = 0; i < ImageTargets.Length; i++)
            //{
            //    if ((ImageTargets[i] == tmpTarget))
            //    {
            //        Debug.Log("Marcador padre: " + tmpTarget);
            //        SwapElements(0, i);
            //    }
            //}
            Debug.Log("Lista: " + string.Join(", ", availableTargets));
            //Debug.Log("Visible Targets: " + string.Join(", ", availableTargets));
        }        
    }

    public void SwapElements(int indexA, int indexB)
    {
        // Verificar los índices antes de hacer el intercambio
        Debug.Log($"Intercambiando {availableTargets[indexA]} con {availableTargets[indexB]}");

        int tmp = availableTargets[indexA];
        availableTargets[indexA] = availableTargets[indexB];
        availableTargets[indexB] = tmp;

        // Mostrar la lista después del intercambio
        Debug.Log("Lista después del intercambio: " + string.Join(", ", availableTargets));

        //goToTarget(availableTargets[indexA]);
    }

    public void showOptions()
    {
        if (availableTargets.Count <= 1)
        {
            oneOpt.gameObject.SetActive(false);
            twoOpt.gameObject.SetActive(false);
        }
        if (availableTargets.Count == 2)
        {
            twoOpt.gameObject.SetActive(false);
            oneOpt.gameObject.SetActive(true);                        
            options[0].text = ImageTargets[availableTargets[1]].TargetName;
        }
        if (availableTargets.Count == 3)
        {
            oneOpt.gameObject.SetActive(false);
            twoOpt.gameObject.SetActive(true);
            options[1].text = ImageTargets[availableTargets[1]].TargetName;
            options[2].text = ImageTargets[availableTargets[2]].TargetName;
        }
    }

    //Función que detecta si un objetivo 
    //public void detectTarget()
    //{
    //    List<int> availableTargets = new List<int>();
    //    foreach (Button btn in animal)
    //    {
    //        btn.interactable = false;
    //    }
    //    availableTargets.Clear();
    //    foreach (ObserverBehaviour target in ImageTargets)
    //    {
    //        if (target != null && (target.TargetStatus.Status == Status.TRACKED || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
    //        {
    //            for (int i = 0; i < ImageTargets.Length; i++)
    //            {
    //                if (ImageTargets[i] != null && ImageTargets[i].TargetName == target.TargetName)
    //                {
    //                    animal[i].interactable = true;
    //                    availableTargets.Add(i);
    //                }
    //            }
    //        }
    //    }
    //    _lastVisibleTargets = new List<int>(availableTargets);
    //    // Iterar sobre cada marcador en ImageTargets
    //    //for (int i = 0; i < ImageTargets.Length; i++)
    //    //{
    //    //    ObserverBehaviour target = ImageTargets[i];

    //    //    // Verificar si el marcador está rastreado o extendidamente rastreado
    //    //    if (target != null && (target.TargetStatus.Status == Status.TRACKED || target.TargetStatus.Status == Status.EXTENDED_TRACKED))
    //    //    {
    //    //        // Si el marcador está visible, activar el botón correspondiente y agregarlo a la lista
    //    //        if (!availableTargets.Contains(i))
    //    //        {
    //    //            animal[i].interactable = true;
    //    //            availableTargets.Add(i);
    //    //        }
    //    //    }
    //    //    else
    //    //    {
    //    //        // Si el marcador no está visible, desactivar el botón correspondiente y eliminarlo de la lista
    //    //        if (availableTargets.Contains(i))
    //    //        {
    //    //            animal[i].interactable = false;
    //    //            availableTargets.Remove(i);
    //    //        }
    //    //    }
    //    //}
    //    Debug.Log("Lista: " + string.Join(", ", availableTargets));
    //}

    //Corrutina
    private IEnumerator MoveModel()
    {
        isMoving = true;
        stopTextCoroutine = true;
        //ObserverBehaviour target = GetNextDetectedTarget();
        ObserverBehaviour target = ImageTargets[noTarget];
        Debug.Log("El noTarget es: " + noTarget);
        if (target == null)
        {
            isMoving = false;
            yield break; //Final de corrutina
        }

        Vector3 startPosition = model.transform.position;
        //Vector3 endPosition = target.transform.position;
        Vector3 endPosition = new Vector3(target.transform.position.x, target.transform.position.y + 0.25f, target.transform.position.z);

        float journey = 0;

        textBox.SetActive(false);
        textTemplate.text = "";
        //Movimiento el modelo 3D
        if (noTarget != 5 || (noTarget == 5 && itemFound[5] == false))
        {
            while (journey <= 1f)
            {
                journey += Time.deltaTime * speed;
                model.transform.position = Vector3.Lerp(startPosition, endPosition, journey);
                yield return null;
            }
            currentTarget = (currentTarget + 1) % ImageTargets.Length;
            model.transform.SetParent(target.transform);
        }
        
        isMoving = false; //Terminó el recorrido        
        if (noTarget > 0)
        {
            if ((noTarget == 4 && itemFound[3] == true) || noTarget != 4)
                items[noTarget - 1].SetActive(true);
            if (noTarget == 5)
            {
                audioSource.clip = clips[4];
                audioSource.Play();
                Debug.Log("Perdiste");
                yield return new WaitForSeconds(3.0f);
            }                
            //if (noTarget < 4)
            //{
            //    itemIcons[noTarget - 1].SetActive(true);
            //}
        }
        //StartCoroutine(ShowIcon(noTarget));
        //prevNoTarget = noTarget;
        if (noTarget > 0 || (noTarget == 0 && prevNoTarget != 5))
            StartCoroutine(ShowText(noTarget));
    }


    private IEnumerator ShowText(int noTarget)
    {
        //prevNoTarget = 0;
        stopTextCoroutine = false;
        //audioSource.clip = clips[0];
        //audioSource.Play();
        //textBox.SetActive(true);
        //textTemplate.text = "";
        //yield return new WaitForSeconds(0.5f);
        //Debug.Log(noTarget);
        //Debug.Log(itemFound[0]);        
        yield return new WaitForSeconds(1.0f);
        int tmp = 0;
        switch (noTarget)
        {
            case 0:                
                if (itemFound[0] == false) {
                    audioSource.clip = clips[0];
                    audioSource.Play();
                    textBox.SetActive(true);
                    textTemplate.text = "";
                    yield return new WaitForSeconds(0.5f);
                    audioSource.clip = clips[1];
                    itemFound[0] = true;
                    fullText = textBoxItem.Find("Mission")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";                    
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);

                    fullText = textBoxItem.Find("Mission (1)")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    audioSource.clip = clips[1];
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);

                    fullText = textBoxItem.Find("Mission (2)")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    audioSource.clip = clips[1];
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    audioSource.clip = clips[0];
                    audioSource.Play();
                    textBox.SetActive(true);
                    textTemplate.text = "";
                    yield return new WaitForSeconds(0.5f);
                    fullText = textBoxItem.Find("MissionRead")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    audioSource.clip = clips[1];
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
                }
                break;
            case 1:
                audioSource.clip = clips[1];
                itemIcons[noTarget - 1].SetActive(true);
                if (itemFound[1] == false)
                {
                    itemFound[1] = true;
                    audioSource.clip = clips[5];
                    audioSource.Play();
                    yield return new WaitForSeconds(2.0f);                    
                    audioSource.clip = clips[0];
                    audioSource.Play();
                    textBox.SetActive(true);
                    textTemplate.text = "";
                    yield return new WaitForSeconds(0.5f);
                    fullText = textBoxItem.Find("FarmHoe")?.GetComponent<TMP_Text>().text;
                    audioSource.clip = clips[1];
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    audioSource.clip = clips[0];
                    audioSource.Play();
                    textBox.SetActive(true);
                    textTemplate.text = "";
                    yield return new WaitForSeconds(0.5f);
                    fullText = textBoxItem.Find("FarmHoeFounded")?.GetComponent<TMP_Text>().text;
                    audioSource.clip = clips[1];
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
                }
                break;
            case 2:
                if (itemFound[2] == false)
                {
                    if (itemFound[1] == false)
                    {
                        audioSource.clip = clips[0];
                        audioSource.Play();
                        textBox.SetActive(true);
                        textTemplate.text = "";
                        yield return new WaitForSeconds(0.5f);
                        fullText = textBoxItem.Find("GoldNotAchieved")?.GetComponent<TMP_Text>().text;
                        audioSource.clip = clips[1];
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            audioSource.Play();
                            yield return new WaitForSeconds(delay);
                        }
                        audioSource.clip = clips[2];
                        audioSource.Play();
                        yield return new WaitForSeconds(1.0f);
                    }
                    else 
                    {
                        itemIcons[noTarget - 1].SetActive(true);
                        itemFound[2] = true;
                        audioSource.clip = clips[5];
                        audioSource.Play();
                        yield return new WaitForSeconds(2.0f);
                        audioSource.clip = clips[0];
                        audioSource.Play();
                        textBox.SetActive(true);
                        textTemplate.text = "";
                        yield return new WaitForSeconds(0.5f);
                        fullText = textBoxItem.Find("Gold")?.GetComponent<TMP_Text>().text;
                        audioSource.clip = clips[1];
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            audioSource.Play();
                            yield return new WaitForSeconds(delay);
                        }
                        audioSource.clip = clips[2];
                        audioSource.Play();
                        yield return new WaitForSeconds(1.0f);
                    }
                }
                else
                {
                    audioSource.clip = clips[0];
                    audioSource.Play();
                    textBox.SetActive(true);
                    textTemplate.text = "";
                    yield return new WaitForSeconds(0.5f);
                    fullText = textBoxItem.Find("GoldFounded")?.GetComponent<TMP_Text>().text;
                    audioSource.clip = clips[1];
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
                }                
                break;
            case 3:
                if (itemFound[3] == false)
                {
                    if (itemFound[2] == false)
                    {
                        audioSource.clip = clips[0];
                        audioSource.Play();
                        textBox.SetActive(true);
                        textTemplate.text = "";
                        yield return new WaitForSeconds(0.5f);
                        fullText = textBoxItem.Find("WheatNotAchieved")?.GetComponent<TMP_Text>().text;
                        audioSource.clip = clips[1];
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            audioSource.Play();
                            yield return new WaitForSeconds(delay);
                        }
                        audioSource.clip = clips[2];
                        audioSource.Play();
                        yield return new WaitForSeconds(1.0f);
                    }
                    else 
                    {
                        itemIcons[noTarget - 1].SetActive(true);
                        itemFound[3] = true;
                        audioSource.clip = clips[5];
                        audioSource.Play();
                        yield return new WaitForSeconds(2.0f);
                        audioSource.clip = clips[0];
                        audioSource.Play();
                        textBox.SetActive(true);
                        textTemplate.text = "";
                        yield return new WaitForSeconds(0.5f);
                        fullText = textBoxItem.Find("Wheat")?.GetComponent<TMP_Text>().text;
                        audioSource.clip = clips[1];
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            audioSource.Play();
                            yield return new WaitForSeconds(delay);
                        }
                        audioSource.clip = clips[2];
                        audioSource.Play();
                        yield return new WaitForSeconds(1.0f);
                    }
                }
                else
                {
                    audioSource.clip = clips[0];
                    audioSource.Play();
                    textBox.SetActive(true);
                    textTemplate.text = "";
                    yield return new WaitForSeconds(0.5f);
                    fullText = textBoxItem.Find("WheatFounded")?.GetComponent<TMP_Text>().text;
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
                }                
                break;
            case 4:
                if (itemFound[4] == false)
                {
                    if (itemFound[3] == false)
                    {
                        audioSource.clip = clips[0];
                        audioSource.Play();
                        textBox.SetActive(true);
                        textTemplate.text = "";
                        yield return new WaitForSeconds(0.5f);
                        fullText = textBoxItem.Find("BlueChickenNotAchieved")?.GetComponent<TMP_Text>().text;
                        audioSource.clip = clips[1];
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            audioSource.Play();
                            yield return new WaitForSeconds(delay);
                        }
                        audioSource.clip = clips[2];
                        audioSource.Play();
                        yield return new WaitForSeconds(1.0f);
                    }
                    else 
                    {
                        itemFound[4] = true;
                        audioSource.clip = clips[5];
                        audioSource.Play();
                        yield return new WaitForSeconds(2.0f);
                        audioSource.clip = clips[0];
                        audioSource.Play();
                        textBox.SetActive(true);
                        textTemplate.text = "";
                        yield return new WaitForSeconds(0.5f);
                        fullText = textBoxItem.Find("BlueChicken")?.GetComponent<TMP_Text>().text;
                        audioSource.clip = clips[1];
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            audioSource.Play();
                            yield return new WaitForSeconds(delay);
                        }
                        audioSource.clip = clips[2];
                        audioSource.Play();
                        yield return new WaitForSeconds(1.0f);
                    }                    
                }
                else
                {
                    fullText = textBoxItem.Find("BlueChickenFounded")?.GetComponent<TMP_Text>().text;
                    audioSource.clip = clips[1];
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
                }
                break;
            case 5:
                Debug.Log("Entró al caso 5");
                if (itemFound[5] == false)
                {                  
                        itemFound[5] = true;
                        audioSource.clip = clips[0];
                        audioSource.Play();
                        textBox.SetActive(true);
                        textTemplate.text = "";
                        yield return new WaitForSeconds(0.5f);
                        fullText = textBoxItem.Find("Slime")?.GetComponent<TMP_Text>().text;
                        audioSource.clip = clips[1];
                        textTemplate.text = "";
                        foreach (char letter in fullText)
                        {
                            if (stopTextCoroutine)
                                yield break;
                            textTemplate.text += letter;
                            audioSource.Play();
                            yield return new WaitForSeconds(delay);
                        }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);                    

                    prevNoTarget = noTarget;
                    moveToNextMarketAuto(tmp);

                    //yield return new WaitForSeconds(1.0f);                    
                }
                else
                {
                    textBox.SetActive(true);
                    textTemplate.text = "";
                    yield return new WaitForSeconds(0.5f);
                    fullText = textBoxItem.Find("SlimeFounded")?.GetComponent<TMP_Text>().text;
                    audioSource.clip = clips[1];
                    textTemplate.text = "";
                    foreach (char letter in fullText)
                    {
                        if (stopTextCoroutine)
                            yield break;
                        textTemplate.text += letter;
                        audioSource.Play();
                        yield return new WaitForSeconds(delay);
                    }
                    audioSource.clip = clips[2];
                    audioSource.Play();
                    yield return new WaitForSeconds(1.0f);
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
        inputMovement();
        detectTarget();
        showOptions();
    }
}