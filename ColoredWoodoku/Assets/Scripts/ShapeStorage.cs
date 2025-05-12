using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShapeStorage : MonoBehaviour
{
    public List<Shapedata> shapeData;
    public List<Shape> ShapeList;
    public ColorSquare colorSquare;
    public JokerSquare jokerSquare;
    public static ShapeStorage Instance { get; private set; }

    private void Awake()
    {
        RequestNewShape();
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

        if (jokerSquare != null) 
        {
            jokerSquare.CreateShape(shapeData[6]);
            jokerSquare.gameObject.SetActive(false);
        }

        foreach (var shape in ShapeList)
        {
            if (shape is HammerSquare || shape is LineEraser)
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
        foreach (var shape in ShapeList)
        {
            if (shape.isInDropArea)
            {
                continue;
            }
            if (shape.gameObject.activeSelf && !shape.IsonStartPosition() && shape.IsAnyOfShapeSquareActive())
            {
                return shape;
            }
        }
        if (colorSquare != null && colorSquare.gameObject.activeSelf && 
            !colorSquare.IsonStartPosition() && colorSquare.IsAnyOfShapeSquareActive() && 
            !colorSquare.isInDropArea)
        {
            return colorSquare;
        }

        if (jokerSquare != null && jokerSquare.gameObject.activeSelf && 
            !jokerSquare.IsonStartPosition() && jokerSquare.IsAnyOfShapeSquareActive() && 
            !jokerSquare.isInDropArea)
        {
            return jokerSquare;
        }

        bool anyActiveShape = ShapeList.Any(shape => shape.gameObject.activeSelf && !shape.isInDropArea);
        if (!anyActiveShape)
        {
            if (GameNetworkManager.Instance != null)
            {
                GameNetworkManager.Instance.LocalPlayerFinishedPlacingShapes();
            }
            else
            {
                GameEvents.RequestNewShapeMethod();
            }
        }

        return null;
    }

    public void RequestNewShape()
    {
        var dropArea = FindObjectOfType<DropArea>();
        var shapesInDropArea = dropArea != null ? dropArea.GetStoredShapes() : new List<Shape>();
        RefreshSpecialShapes(shapesInDropArea);
        RefreshNormalShapes(shapesInDropArea);
    }

    private void RefreshSpecialShapes(List<Shape> shapesInDropArea)
    {
        if (jokerSquare != null && !shapesInDropArea.Contains(jokerSquare))
        {
            jokerSquare.gameObject.SetActive(true);
            jokerSquare.MoveShapetoStartPosition();
            jokerSquare.StartColorCycle(); 
        }

        if (colorSquare != null && !shapesInDropArea.Contains(colorSquare))
        {
            if (GameNetworkManager.Instance != null)
            {
                GameNetworkManager.Instance.ApplySyncedExplosionColor();
            }
            
            if (GameEvents.LastExplosionColor == Shape.ShapeColor.None)
            {
                colorSquare.gameObject.SetActive(false);
            }
            else
            {
                colorSquare.gameObject.SetActive(true);
                colorSquare.MoveShapetoStartPosition();
                colorSquare.shapeColor = GameEvents.LastExplosionColor;
                colorSquare.SetColor(GameEvents.LastExplosionColor);
            }
        }
    }

    private void RefreshNormalShapes(List<Shape> shapesInDropArea)
    {
        if (GameNetworkManager.Instance != null && !GameNetworkManager.Instance.AreShapesReadyToUse())
        {
            return;
        }
        
        int renewedShapes = 0;
        
        foreach (var shape in ShapeList)
        {
            if (shapesInDropArea.Contains(shape))
            {
                continue;
            }

            if (shape is HammerSquare || shape is LineEraser)
            {
                shape.RequestNewShape(shapeData[6]);
            }
            else
            {
                int shapeIndex;
                Shape.ShapeColor shapeColor;
                
                if (GameNetworkManager.Instance != null)
                {
                    shapeIndex = GameNetworkManager.Instance.GetSyncedShapeIndex(renewedShapes);
                    shapeColor = GameNetworkManager.Instance.GetSyncedShapeColor(renewedShapes);
                }
                else
                {
                    shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
                    shapeColor = shape.GetRandomShapeColor();
                }
                
                shape.RequestNewShape(shapeData[shapeIndex]);
                shape.shapeColor = shapeColor;
                shape.SetColor(shapeColor);
                shape.gameObject.SetActive(true);
            }
            
            renewedShapes++;
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
            colorSquare.RequestNewShape(shapeData[6]);
        }

        colorSquare.shapeColor = GameEvents.LastExplosionColor;
        colorSquare.SetColor(GameEvents.LastExplosionColor);
    }

    public void EnableJokerSquare()
    {
        if (GameEvents.LastExplosionColor == Shape.ShapeColor.None || jokerSquare == null)
        {
            return;
        }

        if (!jokerSquare.gameObject.activeSelf)
        {
            jokerSquare.gameObject.SetActive(true);
            jokerSquare.RequestNewShape(shapeData[6]);
        }

        jokerSquare.shapeColor = GameEvents.LastExplosionColor;
        jokerSquare.SetColor(GameEvents.LastExplosionColor);
    }
}