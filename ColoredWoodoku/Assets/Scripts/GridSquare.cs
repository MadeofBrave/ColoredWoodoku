using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSquare : MonoBehaviour
{
    public Image normalImage;
    public Image hooverImage;
    public Image activeImage;
    public List<Sprite> normalImages; 
    public Sprite[] colorSprites;

    public Shape.ShapeColor squareColor;
    public bool isOccupied = false;

    public bool Selected { get; set; }
    public int SquareIndex { get; set; }
    public bool SquareOccupied { get; set; }

    void Start()
    {
        SetRandomInitialColor();
        Selected = false;
        SquareOccupied = false;
       
    }

    private void SetRandomInitialColor()
    {
        int randomColorIndex = Random.Range(0, normalImages.Count); 
        normalImage.sprite = normalImages[randomColorIndex];
    }
public void PlaceShapeOnBoard(Shape.ShapeColor color)
{
    isOccupied = true; // Kare artýk dolu
    squareColor = color; // Renk atamasý yap
    SetColor(color);

    Debug.Log($"[GridSquare] Kareye yerleþtirildi! Yeni renk: {color}");
}


    private void SetColor(Shape.ShapeColor color)
    {
        int index = (int)color;
        if (index >= 0 && index < colorSprites.Length)
        {
            normalImage.sprite = colorSprites[index];
        }
        else
        {
            Debug.LogWarning("Color index out of range: " + index);
        }
    }


    public void ActivateSquare()
    {
        hooverImage.gameObject.SetActive(false);
        activeImage.gameObject.SetActive(true);
        Selected = true;
        SquareOccupied = true;
    }

    public bool IsOccupiedByColor(Shape.ShapeColor color)
    {
        return isOccupied && squareColor == color;
    }

    public void Deactivate()
    {
        activeImage.gameObject.SetActive(false);
    }

    public void ClearOccupied()
    {
        Selected = false;
        SquareOccupied = false;
        isOccupied = false;
        squareColor = default;
        if (normalImages != null && normalImages.Count > 0)
        {
            normalImage.sprite = normalImages[0]; 
        }
    }

    public bool CanWeUseTheSquare()
    {
        return !isOccupied;
    }

    public void SetImage(bool setFirstImage)
    {
        if (normalImages != null && normalImages.Count >= 2)
        {
            normalImage.sprite = setFirstImage ? normalImages[0] : normalImages[1];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!SquareOccupied)
        {
            Selected = true;
            hooverImage.gameObject.SetActive(true);
        }
        else if (collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().SetOccupied();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Selected = true;
        if (!SquareOccupied)
        {
            hooverImage.gameObject.SetActive(true);
        }
        else if (collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().SetOccupied();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!SquareOccupied)
        {
            Selected = false;
            hooverImage.gameObject.SetActive(false);
        }
        else if (collision.GetComponent<ShapeSquare>() != null)
        {
            collision.GetComponent<ShapeSquare>().UnSetOccupied();
        }
    }
}
