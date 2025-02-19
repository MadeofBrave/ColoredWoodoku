using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSquare : Shape
{
    public ShapeStorage shapeStorage;

    public override void Awake()
    {
        base.Awake();
        SetColor(Shape.ShapeColor.None);
        gameObject.SetActive(false);
    }


    private void OnEnable()
    {
        GameEvents.TriggerOneByOneBlockExplosion += HandleBlockExplosion;
    }

    private void OnDisable()
    {
        GameEvents.TriggerOneByOneBlockExplosion -= HandleBlockExplosion;
    }

    public void HandleBlockExplosion(Shape.ShapeColor lastExplosionColor)
    {

        if (lastExplosionColor == Shape.ShapeColor.None)
        {
            return;
        }
        else
        {
            shapeColor = lastExplosionColor;
            SetColor(lastExplosionColor);
            gameObject.SetActive(true);
        }
    }


    public override void OnEndDrag(PointerEventData eventData)
    {
        bool shapePlaced = CheckIfOneByOneBlockCanBePlaced();

        if (shapePlaced)
        {
            shapeColor = ShapeColor.None;
            SetColor(GameEvents.LastExplosionColor);
            gameObject.SetActive(false);
        }
        else
        {   
            SetColor(GameEvents.LastExplosionColor);
            MoveShapetoStartPosition();
        }
    }
}
