using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSquare : Shape
{
    public ShapeStorage shapeStorage;
    private void OnEnable()
    {

        RequestNewShape(ShapeStorage.Instance.shapeData[6]);
        shapeColor = GameEvents.LastExplosionColor; // Patlayan rengi al
        SetColor(shapeColor);
        GameEvents.TriggerOneByOneBlockExplosion += HandleBlockExplosion;
    }

    private void OnDisable()
    {
        GameEvents.TriggerOneByOneBlockExplosion -= HandleBlockExplosion;
        shapeColor= ShapeColor.None;
        SetColor(shapeColor);
    }

    public void HandleBlockExplosion(Shape.ShapeColor lastExplosionColor)
    {
        if (lastExplosionColor == Shape.ShapeColor.None)
        {
            return;
        }

        Debug.Log($"HandleBlockExplosion �a�r�ld�! Yeni Renk: {lastExplosionColor}");

        shapeColor = lastExplosionColor;
        SetColor(lastExplosionColor);
        gameObject.SetActive(true);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        bool shapePlaced = CheckIfOneByOneBlockCanBePlaced();

        Debug.Log($"[ColorSquare] 1x1 Kare B�rak�ld� - Yerle�ti mi?: {shapePlaced}");

        if (shapePlaced)
        {
            Debug.Log($"[ColorSquare] 1x1 Kare yerle�tirildi! Renk: {shapeColor} -> Renk s�f�rlan�yor...");

            shapeColor = ShapeColor.None;
            SetColor(ShapeColor.None);
            gameObject.SetActive(false);
            GameEvents.SetLastExplosionColorMethod(ShapeColor.None);
        }
        else
        {
            SetColor(GameEvents.LastExplosionColor);
            MoveShapetoStartPosition();
        }
    }
}
