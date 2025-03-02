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
            if (shape is HammerSquare)
            {
                shape.CreateShape(shapeData[6]); 
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
                return shape;
            }
        }
        GameEvents.RequestNewShapeMethod();
        return null;
    }


    private void RequestNewShape()
    {
        foreach (var shape in ShapeList)
        {
            if (shape is ColorSquare colorSquare)
            {
                if (GameEvents.LastExplosionColor == Shape.ShapeColor.None)
                {
                    colorSquare.gameObject.SetActive(false);
                    continue;
                }

                colorSquare.gameObject.SetActive(true);
            }
            else if (shape is HammerSquare)
            {
                shape.RequestNewShape(shapeData[6]); 
            }
            else
            {
                var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
                shape.RequestNewShape(shapeData[shapeIndex]);
                shape.shapeColor = shape.GetRandomShapeColor();
                shape.SetColor(shape.shapeColor);
            }
        }
    }


    public void EnableColorSquare()
    {

        if (GameEvents.LastExplosionColor == Shape.ShapeColor.None)
        {
            return;
        }

        foreach (var shape in ShapeList)
        {
            if (shape is ColorSquare colorSquare)
            {

                colorSquare.shapeColor = GameEvents.LastExplosionColor;

                if (!colorSquare.gameObject.activeSelf)
                {
                    colorSquare.gameObject.SetActive(true);
                }
            }
        }
    }



}
