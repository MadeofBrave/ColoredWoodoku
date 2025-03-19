using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShapeStorage : MonoBehaviour
{
    public List<Shapedata> shapeData;
    public List<Shape> ShapeList;
    public ColorSquare colorSquare;
    public JokerSquare jokerSquare; // Add reference to JokerSquare
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

        if (jokerSquare != null) // Initialize JokerSquare
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
        if (colorSquare != null && colorSquare.gameObject.activeSelf &&
            !colorSquare.IsonStartPosition() && colorSquare.IsAnyOfShapeSquareActive())
        {
            return colorSquare;
        }

        if (jokerSquare != null && jokerSquare.gameObject.activeSelf &&
            !jokerSquare.IsonStartPosition() && jokerSquare.IsAnyOfShapeSquareActive())
        {
            return jokerSquare;
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

    public void RequestNewShape()
    {
        Debug.Log("Yeni şekil talep ediliyor...");

        // JokerSquare düzgün sıfırlanıyor mu?
        if (jokerSquare != null)
        {
            jokerSquare.gameObject.SetActive(true);
            jokerSquare.MoveShapetoStartPosition();
            jokerSquare.StartColorCycle(); 
        }

        // ColorSquare düzgün sıfırlanıyor mu?
        if (colorSquare != null)
        {
            if (GameEvents.LastExplosionColor == Shape.ShapeColor.None)
            {
                colorSquare.gameObject.SetActive(false);
                Debug.Log("ColorSquare kapatıldı.");
            }
            else
            {
                colorSquare.gameObject.SetActive(true);
                colorSquare.shapeColor = GameEvents.LastExplosionColor;
                colorSquare.SetColor(GameEvents.LastExplosionColor);
                Debug.Log("ColorSquare etkinleştirildi ve rengi ayarlandı.");
            }
        }

        foreach (var shape in ShapeList)
        {
            if (shape is HammerSquare || shape is LineEraser)
            {
                shape.RequestNewShape(shapeData[6]);
            }
            else
            {
                var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
                shape.RequestNewShape(shapeData[shapeIndex]);

                shape.shapeColor = shape.GetRandomShapeColor();
                shape.SetColor(shape.shapeColor);
                shape.gameObject.SetActive(true); 
            }
        }

        bool anyActiveShape = ShapeList.Any(shape => shape.gameObject.activeSelf);
        if (!anyActiveShape)
        {
            Debug.LogWarning("Tüm şekiller kapalı, yeni şekil çağırma başarısız!");
        }
        else
        {
            Debug.Log("Yeni şekiller başarıyla yüklendi!");
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

    public void EnableJokerSquare() // Add method to enable JokerSquare
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