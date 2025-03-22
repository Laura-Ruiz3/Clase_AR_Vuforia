using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Color = UnityEngine.Color;

public class ChangeColor1 : MonoBehaviour
{
    public GameObject model;
    public Color[] colors;    
    public Material colorMaterial;
    public Texture[] texture;
    public Material textureMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorMaterial.color = colors[0];
        textureMaterial.mainTexture = texture[0];
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ChangeColor_BTN()
    {
        textureMaterial.mainTexture = texture[Random.Range(0, texture.Length)];
        colorMaterial.color = colors[Random.Range(0,colors.Length)];
    }
}
