using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DropArea : MonoBehaviour, IDropHandler
{
    private Shape currentShape;
    private bool isOccupied = false;
    private Vector3 originalPosition;
    private bool isPlacedOnGrid = false;

    private void OnEnable()
    {
        GameEvents.ShapeStoredInDropArea += OnShapeStoredInDropArea;
    }

    private void OnDisable()
    {
        GameEvents.ShapeStoredInDropArea -= OnShapeStoredInDropArea;
    }

    public List<Shape> GetStoredShapes()
    {
        List<Shape> shapes = new List<Shape>();
        if (currentShape != null && isOccupied)
        {
            shapes.Add(currentShape);
        }
        return shapes;
    }

    public bool StoreShape(Shape shape)
    {
        if (isOccupied)
        {
            return false;
        }

        currentShape = shape;
        isOccupied = true;
        originalPosition = shape.transform.position;
        shape.transform.position = transform.position;
        shape.isInDropArea = true;
        shape.currentDropArea = this;
        GameEvents.OnShapeStoredInDropArea(shape);
        
        return true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isOccupied)
        {
            Shape draggedShape = eventData.pointerDrag.GetComponent<Shape>();
            if (draggedShape != null)
            {
                draggedShape.MoveShapetoStartPosition();
            }
            return;
        }

        Shape shape = eventData.pointerDrag.GetComponent<Shape>();
        if (shape != null)
        {
            StoreShape(shape);
        }
    }

    public void OnShapePlacedOnGrid()
    {
        if (currentShape != null)
        {
            Shape placedShape = currentShape;
            Shape.ShapeColor originalColor = placedShape.shapeColor;
            string shapeName = placedShape.gameObject.name;
            
            Shape shapeToUpdate = currentShape;
            currentShape = null;
            isPlacedOnGrid = true;
            isOccupied = false;

            if (shapeToUpdate != null)
            {
                shapeToUpdate.isInDropArea = false;
                shapeToUpdate.currentDropArea = null;
            }
        }
    }

    private void OnShapeStoredInDropArea(Shape storedShape)
    {
        if (storedShape != currentShape && isOccupied)
        {
            currentShape.RetrieveFromDropArea();
            ResetDropArea();
        }
    }

    public void RetrieveShape(Shape shape)
    {
        if (currentShape == shape)
        {
            Shape shapeToReset = currentShape;
            Shape.ShapeColor originalColor = shapeToReset.shapeColor;
            string shapeName = shapeToReset.gameObject.name;
            isOccupied = false;
            isPlacedOnGrid = false;
            currentShape = null;

            if (shapeToReset != null)
            {
                shapeToReset.isInDropArea = false;
                shapeToReset.currentDropArea = null;
            }
        }
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }

    public bool IsPlacedOnGrid()
    {
        return isPlacedOnGrid;
    }

    public void ResetDropArea()
    {
        if (currentShape != null)
        {
            Shape shapeToReset = currentShape;
            isOccupied = false;
            isPlacedOnGrid = false;
            currentShape = null;

            if (shapeToReset != null)
            {
                shapeToReset.isInDropArea = false;
                shapeToReset.currentDropArea = null;
            }
        }
    }
} 