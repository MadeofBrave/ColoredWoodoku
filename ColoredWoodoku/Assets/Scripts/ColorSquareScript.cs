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
        shapeColor = GameEvents.LastExplosionColor; 
        GameEvents.TriggerOneByOneBlockExplosion += HandleBlockExplosion;
    }

    private void OnDisable()
    {
        GameEvents.TriggerOneByOneBlockExplosion -= HandleBlockExplosion;
        shapeColor = ShapeColor.None;
    }

    public void HandleBlockExplosion(Shape.ShapeColor lastExplosionColor)
    {
        if (lastExplosionColor == Shape.ShapeColor.None)
        {
            return;
        }
        gameObject.SetActive(true);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        bool shapePlaced = CheckIfOneByOneBlockCanBePlaced();


        if (shapePlaced)
        {
            gameObject.SetActive(false);
            GameEvents.SetLastExplosionColorMethod(ShapeColor.None);
        }
        else
        {
            MoveShapetoStartPosition();
        }
    }
}