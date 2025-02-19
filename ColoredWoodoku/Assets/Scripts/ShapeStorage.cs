using System.Collections.Generic;
using UnityEngine;

public class ShapeStorage : MonoBehaviour
{
    public List<Shapedata> shapeData;
    public List<Shape> ShapeList;
    public static ShapeStorage Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        GameEvents.RequestNewShape -= RequestNewShape;
    }

    private void OnEnable()
    {
        GameEvents.RequestNewShape += RequestNewShape;
    }

    void Start()
    {
        foreach (var shape in ShapeList)
        {
            if (shape is ColorSquare)
            {
                continue; 
            }
            var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
            shape.CreateShape(shapeData[shapeIndex]);

        }

    }

    public Shape GetCurrentSelectedShape()
    {   
        foreach (var shape in ShapeList)
        {
            if (!shape.gameObject.activeSelf)
            {
                continue;
            }

            if (!shape.IsonStartPosition() && shape.IsAnyOfShapeSquareActive())
            {
                if (!shape.IsAnyOfShapeSquareActive())
                {

                    if (shape is ColorSquare)
                    {
                        return null;
                    }

                    RequestNewShape();
                    return null;
                }

                return shape;
            }
        }


        if (GameEvents.LastExplosionColor != Shape.ShapeColor.None)
        {
            EnableColorSquare();
            RequestNewShape();
        }
        else
        {
            Debug.Log("Patlama yok, 1x1 Kare aktif edilmeyecek.");
        }

        return null;
    }



    private void RequestNewShape()
    {
        foreach (var shape in ShapeList)
        {
            if (shape is ColorSquare)
            {
                ColorSquare colorSquare = shape as ColorSquare;
                if (colorSquare != null)
                {
                    colorSquare.RequestNewShape(shapeData[6]);
                    colorSquare.shapeColor = GameEvents.LastExplosionColor;
                    colorSquare.gameObject.SetActive(true);
                }
            }
            else
            {
                var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);

                Shape randomShape = shape.GetComponent<Shape>(); 
                if (randomShape != null)
                {
                    shape.shapeColor = randomShape.GetRandomShapeColor(); 
                }
                else
                {
                    shape.shapeColor = Shape.ShapeColor.None; 
                }

                shape.RequestNewShape(shapeData[shapeIndex]); 
                shape.SetColor(shape.shapeColor); 
            }
        }
    }


    private void EnableColorSquare()
    {
        if (GameEvents.LastExplosionColor == Shape.ShapeColor.None) { 
        Debug.Log("patlama olmadý þekil inaktif");
        return; 
        }

        foreach (var shape in ShapeList)
        {
            if (shape is ColorSquare colorSquare)
            { 
                    colorSquare.gameObject.SetActive(true); 
                    colorSquare.RequestNewShape(shapeData[6]);
                    colorSquare.shapeColor = GameEvents.LastExplosionColor; 
                    colorSquare.CreateShape(shapeData[6]);
                
            }
        }
    }



}
