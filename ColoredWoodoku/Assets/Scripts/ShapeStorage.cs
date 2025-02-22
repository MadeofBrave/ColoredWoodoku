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

            // 1x1 kareyi sayma
            if (shape is ColorSquare)
            {
                continue;
            }

            if (!shape.IsonStartPosition() && shape.IsAnyOfShapeSquareActive())
            {
                return shape;
            }
        }

        Debug.Log("Ana şekiller yerleştirildi, yeni şekiller çağrılıyor...");
        GameEvents.RequestNewShapeMethod();

        return null;
    }

    private void RequestNewShape()
    {
        foreach (var shape in ShapeList)
        {
            if (shape is ColorSquare colorSquare)
            {
                // Eğer en son patlama olmadıysa 1x1 kareyi oluşturma
                if (GameEvents.LastExplosionColor == Shape.ShapeColor.None)
                {
                    colorSquare.gameObject.SetActive(false);
                    continue;
                }

                // 1x1 kareyi doğru renkte oluştur
                colorSquare.shapeColor = GameEvents.LastExplosionColor;
                colorSquare.SetColor(GameEvents.LastExplosionColor);
                colorSquare.gameObject.SetActive(true);
            }
            else
            {
                var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
                shape.RequestNewShape(shapeData[shapeIndex]);

                // Şekil rengi yanlış atanıyorsa düzeltelim
                shape.shapeColor = shape.GetRandomShapeColor();
                shape.SetColor(shape.shapeColor);
            }
        }
    }


    public void EnableColorSquare()
    {
        if (GameEvents.LastExplosionColor == Shape.ShapeColor.None)
        {
            Debug.Log("[ShapeStorage] Patlama yok, 1x1 kare oluşturulmayacak.");
            return;
        }

        foreach (var shape in ShapeList)
        {
            if (shape is ColorSquare colorSquare)
            {
                Debug.Log($"[ShapeStorage] 1x1 Kare Güncellendi! Yeni Renk: {GameEvents.LastExplosionColor}");

                colorSquare.shapeColor = GameEvents.LastExplosionColor;
                colorSquare.SetColor(GameEvents.LastExplosionColor);

                if (!colorSquare.gameObject.activeSelf)
                {
                    colorSquare.gameObject.SetActive(true);
                }
            }
        }
    }



}
