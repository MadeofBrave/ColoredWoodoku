using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DropArea : MonoBehaviour, IDropHandler
{
    private Shape currentShape;
    private bool isOccupied = false;
    private Vector3 originalPosition;
    private bool isPlacedOnGrid = false;
    private List<Shape> storedShapes = new List<Shape>();

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
        
        // Position the shape in the drop area
        shape.transform.position = transform.position;
        
        // Update shape state
        shape.isInDropArea = true;
        shape.currentDropArea = this;

        // Notify other shapes about the stored shape
        GameEvents.OnShapeStoredInDropArea(shape);
        
        return true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isOccupied)
        {
            // If drop area is occupied, return the dragged shape to its original position
            Shape draggedShape = eventData.pointerDrag.GetComponent<Shape>();
            if (draggedShape != null)
            {
                draggedShape.MoveShapetoStartPosition();
            }
            return;
        }

        // Store the dropped shape
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
            // Şeklin referansını ve rengini sakla
            Shape placedShape = currentShape;
            Shape.ShapeColor originalColor = placedShape.shapeColor;
            string shapeName = placedShape.gameObject.name;
            
            Debug.Log($"[DropArea] OnShapePlacedOnGrid - Shape: {shapeName}, Color: {originalColor}");
            
            // Önce çalışacak şekilde referansı null yap
            Shape shapeToUpdate = currentShape;
            currentShape = null;
            
            // Drop area durumunu güncelle
            isPlacedOnGrid = true;
            isOccupied = false;

            // Debug için logları ekle
            Debug.Log($"[DropArea] Drop area boşaltıldı - Shape: {shapeName}");

            // En son şeklin durumunu güncelle
            if (shapeToUpdate != null)
            {
                Debug.Log($"[DropArea] Şeklin durumu güncelleniyor - Shape: {shapeName}");
                shapeToUpdate.isInDropArea = false;
                shapeToUpdate.currentDropArea = null;
            }
        }
    }

    private void OnShapeStoredInDropArea(Shape storedShape)
    {
        if (storedShape != currentShape && isOccupied)
        {
            // If another shape is stored in a different drop area, return this shape to its original position
            currentShape.RetrieveFromDropArea();
            ResetDropArea();
        }
    }

    public void RetrieveShape(Shape shape)
    {
        if (currentShape == shape)
        {
            // Şeklin referansını ve rengini sakla
            Shape shapeToReset = currentShape;
            Shape.ShapeColor originalColor = shapeToReset.shapeColor;
            string shapeName = shapeToReset.gameObject.name;
            
            Debug.Log($"[DropArea] RetrieveShape - Shape: {shapeName}, Color: {originalColor}");
            
            // Drop area'nın durumunu temizle
            isOccupied = false;
            isPlacedOnGrid = false;
            currentShape = null;

            // En son şeklin durumunu güncelle
            if (shapeToReset != null)
            {
                shapeToReset.isInDropArea = false;
                shapeToReset.currentDropArea = null;
                Debug.Log($"[DropArea] Şeklin drop area referansları temizlendi - Shape: {shapeName}");
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
            // Şeklin referansını sakla
            Shape shapeToReset = currentShape;
            
            // Drop area'nın durumunu temizle
            isOccupied = false;
            isPlacedOnGrid = false;
            currentShape = null;

            // En son şeklin durumunu güncelle (null check ekledim)
            if (shapeToReset != null)
            {
                shapeToReset.isInDropArea = false;
                shapeToReset.currentDropArea = null;
            }
            
            Debug.Log("Drop area sıfırlandı");
        }
    }
} 