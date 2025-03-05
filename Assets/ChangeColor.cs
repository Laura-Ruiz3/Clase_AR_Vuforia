using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Color = UnityEngine.Color;

public class ChangeColor1 : MonoBehaviour
{
    public GameObject model;
    //public Color color;
    public Color[] colors = { Color.cyan, Color.green, Color.red, Color.yellow, Color.gray };
    public Material colorMaterial;
    public int i = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorMaterial.color = colors[i];
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ChangeColor_BTN()
    {
        //model.GetComponent<Renderer>().material.color = color;
        //colorMaterial.color = color;
        if (i < 4)
            i++;
        else
            i = 0;
        colorMaterial.color = colors[i];
    }
}
