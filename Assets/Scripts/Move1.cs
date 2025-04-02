using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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

    private const float DIR_THRESHOLD = 0.8f;
    private Vector2 startingPosition;

    //Estados
    Transform textBoxItem;
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
    List<int> availableTargets = new List<int>();

    //Start is called before the first frame update
    void Start()
    {
        textBoxItem = GameObject.Find("TextBox").transform;
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
        StartCoroutine(ShowText(noTarget));
    }

    void inputMovement()
    {
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
                tmp = availableTargets[1];                
                if (availableTargets[1] != 5)
                    SwapElements(0, 1);

                Debug.Log("Nueva lista: " + string.Join(", ", availableTargets));
                goToTarget(tmp);
            }
            if (v2.x > DIR_THRESHOLD)
            {
                tmp = availableTargets[2];                
                if (availableTargets[2] != 5)
                    SwapElements(0, 2);

                Debug.Log("Nueva lista: " + string.Join(", ", availableTargets));
                goToTarget(tmp);
            }
        }
    }

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
            moveToNextMarker(noTarget);
        else if (noTarget == 5 && itemFound[5] == false)
            moveToNextMarker(noTarget);
        else if (noTarget == 5 && itemFound[5] == true)
        {
            StartCoroutine(ShowText(noTarget));
        }
    }

    public void moveToNextMarker(int num)
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

    public void moveToNextMarkerAuto(int num)
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
            print("Holi");
            items[i].SetActive(false);
        }
        for (int i = 0; i < itemIcons.Length; i++)
        {
            print("Holi");
            itemIcons[i].SetActive(false);
        }
        for (int i = 1; i < itemFound.Length - 1; i++)
        {
            print("Holi: "+i);
            itemFound[i] = false;
        }
        if (!isMoving)
        {
            StartCoroutine(MoveModel());
        }
    }

    public void detectTarget()
    {
        List<int> newVisibleTargets = new List<int>();

        for (int i = 0; i < ImageTargets.Length; i++)
        {
            ObserverBehaviour target = ImageTargets[i];
            if (target != null && (target.TargetStatus.Status == Status.TRACKED))
            {
                for (int j = 0; j < ImageTargets.Length; j++)
                {
                    if (ImageTargets[j] != null && ImageTargets[j].TargetName == target.TargetName)
                    {
                        if (!newVisibleTargets.Contains(i))
                            newVisibleTargets.Add(i);
                    }
                }
            }
        }

        bool check = availableTargets.OrderBy(x => x).SequenceEqual(newVisibleTargets.OrderBy(x => x));
        if (!check)
        {
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
                    SwapElements(0, 1);
                }                
            }
            else if (availableTargets.Count == 3)
            {
                if (noTarget == availableTargets[1])
                {
                    SwapElements(0, 1);
                }
                if (noTarget == availableTargets[2])
                {
                    SwapElements(0, 2);
                }
            }         
            Debug.Log("Lista: " + string.Join(", ", availableTargets));
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

    //Corrutina
    private IEnumerator MoveModel()
    {
        isMoving = true;
        stopTextCoroutine = true;
        ObserverBehaviour target = ImageTargets[noTarget];
        Debug.Log("El noTarget es: " + noTarget);
        if (target == null)
        {
            isMoving = false;
            yield break; //Final de corrutina
        }

        Vector3 startPosition = model.transform.position;
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
        }
        if (noTarget > 0 || (noTarget == 0 && prevNoTarget != 5))
            StartCoroutine(ShowText(noTarget));
    }


    private IEnumerator ShowText(int noTarget)
    {
        stopTextCoroutine = false;
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
                    moveToNextMarkerAuto(tmp);
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

    //// Update is called once per frame
    void Update()
    {
        inputMovement();
        detectTarget();
        showOptions();
    }
}