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

        // Drop area'daki şekilleri kontrol et
        var dropArea = FindObjectOfType<DropArea>();
        var shapesInDropArea = dropArea != null ? dropArea.GetStoredShapes() : new List<Shape>();

        // Joker ve Color Square'i kontrol et
        if (jokerSquare != null && !shapesInDropArea.Contains(jokerSquare))
        {
            jokerSquare.gameObject.SetActive(true);
            jokerSquare.MoveShapetoStartPosition();
            jokerSquare.StartColorCycle(); 
        }

        if (colorSquare != null && !shapesInDropArea.Contains(colorSquare))
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

        // Drop area dışındaki şekilleri yenile
        int renewedShapes = 0;
        foreach (var shape in ShapeList)
        {
            // Eğer şekil drop area'da ise atla
            if (shapesInDropArea.Contains(shape))
            {
                Debug.Log($"[ShapeStorage] {shape.gameObject.name} drop area'da olduğu için yenilenmedi");
                continue;
            }

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
            renewedShapes++;
        }

        // En az bir şekil yenilendiyse başarılı
        if (renewedShapes > 0)
        {
            Debug.Log($"{renewedShapes} şekil yenilendi, yeni şekiller başarıyla yüklendi!");
        }
        else
        {
            Debug.LogWarning("Hiç şekil yenilenemedi, tüm şekiller drop area'da olabilir!");
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