﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShapeStorage : MonoBehaviour
{
    public List<Shapedata> shapeData;
    public List<Shape> ShapeList;
    public ColorSquare colorSquare;
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
        if (colorSquare != null)
        {
            colorSquare.CreateShape(shapeData[6]);
            colorSquare.gameObject.SetActive(false);
        }

        foreach (var shape in ShapeList)
        {
            if (shape is HammerSquare || shape is LineHammerSquare)
            {
                shape.CreateShape(shapeData[6]);
                continue;
            }
            var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
            shape.CreateShape(shapeData[shapeIndex]);
        }
    }

    public Shape GetCurrentSelectedShape()
    {
        if (colorSquare != null && colorSquare.gameObject.activeSelf && 
            !colorSquare.IsonStartPosition() && colorSquare.IsAnyOfShapeSquareActive())
        {
            return colorSquare;
        }

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

        bool anyActiveShape = ShapeList.Any(shape => shape.gameObject.activeSelf);
        if (!anyActiveShape)
        {
            GameEvents.RequestNewShapeMethod();
        }

        return null;
    }

    private void RequestNewShape()
    {
        if (colorSquare != null)
        {
            if (GameEvents.LastExplosionColor == Shape.ShapeColor.None)
            {
                colorSquare.gameObject.SetActive(false);
            }
            else
            {
                colorSquare.gameObject.SetActive(true);
                colorSquare.shapeColor = GameEvents.LastExplosionColor;
                colorSquare.SetColor(GameEvents.LastExplosionColor);
            }
        }

        foreach (var shape in ShapeList)
        {
            if (shape is HammerSquare || shape is LineHammerSquare)
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
        if (GameEvents.LastExplosionColor == Shape.ShapeColor.None || colorSquare == null)
        {
            return;
        }

        if (!colorSquare.gameObject.activeSelf)
        {
            colorSquare.gameObject.SetActive(true);
            colorSquare.RequestNewShape(shapeData[6]);  // Eğer aktif değilse şekli yeniden oluştur
        }
        
        colorSquare.shapeColor = GameEvents.LastExplosionColor;
        colorSquare.SetColor(GameEvents.LastExplosionColor);
    }
}