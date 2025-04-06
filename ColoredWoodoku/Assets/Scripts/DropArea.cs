using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour
{
    public static DropArea Instance { get; private set; }
    private BoxCollider2D areaCollider;
    private Shape currentShape;
    public GameObject shapeHolder;
    public Vector2 colliderSize = new Vector2(100f, 100f);
    private bool isAvailable = true;
    private readonly object _lock = new object();

    // Shape listesi ve data
    public List<Shape> storedShapes = new List<Shape>();
    private const int maxStoredShapes = 1;
    public Shapedata CurrentShapeData { get; private set; }

    // Events
    public delegate void ShapeStoredHandler(Shape shape);
    public event ShapeStoredHandler OnShapeStored;
    public event ShapeStoredHandler OnShapeRetrieved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            areaCollider = GetComponent<BoxCollider2D>();
            if (areaCollider == null)
            {
                areaCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            areaCollider.isTrigger = true;
            areaCollider.size = colliderSize;

            if (shapeHolder == null)
            {
                shapeHolder = transform.gameObject;
            }

            storedShapes = new List<Shape>();
            CurrentShapeData = null;

            Debug.Log($"[DropArea] Başlatıldı: {gameObject.name}, Konum: {transform.position}, Collider Size: {areaCollider.size}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DropArea] InitializeComponents error: {ex.Message}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        
        Shape shape = other.GetComponent<Shape>();
        if (shape != null)
        {
            GameEvents.OnShapeEnteredDropArea(shape);
            shape.isInDropArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;

        Debug.Log($"[DropArea] OnTriggerExit2D - Çıkan nesne: {other.gameObject.name}");
        
        Shape shape = other.GetComponent<Shape>();
        if (shape == null) return;

        lock (_lock)
        {
            if (storedShapes.Contains(shape))
            {
                Debug.Log($"[DropArea] Şekil drop area'dan çıktı: {shape.name}");
                storedShapes.Remove(shape);
                if (currentShape == shape)
                {
                    currentShape = null;
                    CurrentShapeData = null;
                }
                shape.isInDropArea = false;
                GameEvents.OnShapeLeftDropArea(shape);
                UpdateStoredShapes();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (areaCollider != null)
        {
            Vector3 center = transform.position + (Vector3)areaCollider.offset;
            Gizmos.DrawWireCube(center, areaCollider.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(colliderSize.x, colliderSize.y, 0f));
        }

        if (shapeHolder != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(shapeHolder.transform.position, 5f);
        }
    }
#endif

    public void UpdateStoredShapes()
    {
        if (storedShapes == null) return;

        Debug.Log("[DropArea] UpdateStoredShapes başladı");
        
        lock (_lock)
        {
            // Geçersiz şekilleri temizle
            for (int i = storedShapes.Count - 1; i >= 0; i--)
            {
                Shape shape = storedShapes[i];
                if (shape == null || (!shape.isInDropArea && !shape._shapeactive))
                {
                    Debug.Log($"[DropArea] Geçersiz şekil temizleniyor: {(shape != null ? shape.name : "null")}");
                    storedShapes.RemoveAt(i);
                    if (currentShape == shape)
                    {
                        currentShape = null;
                        CurrentShapeData = null;
                    }
                }
            }
            
            // CurrentShapeData'yı güncelle
            if (storedShapes.Count > 0)
            {
                currentShape = storedShapes[0];
                CurrentShapeData = currentShape?.CurrentShapeData;
                isAvailable = false;
                Debug.Log($"[DropArea] Drop area güncellendi - Mevcut şekil: {currentShape?.name ?? "null"}");
            }
            else
            {
                currentShape = null;
                CurrentShapeData = null;
                isAvailable = true;
                Debug.Log("[DropArea] Drop area boş");
            }
        }
    }

    public bool HasShape()
    {
        if (storedShapes == null) return false;
        return storedShapes.Count > 0 && storedShapes[0] != null;
    }

    public bool StoreShape(Shape shape)
    {
        if (shape == null)
        {
            Debug.LogError("[DropArea] StoreShape: shape is null");
            return false;
        }

        Debug.Log($"[DropArea] StoreShape çağrıldı - Shape: {shape.name}, Stored shapes count: {storedShapes?.Count ?? 0}");
        
        try
        {
            lock (_lock)
            {
                // Drop area dolu mu kontrol et
                if (storedShapes.Count >= maxStoredShapes)
                {
                    // Eğer drop area doluysa, mevcut şekli çıkar
                    if (storedShapes.Count > 0)
                    {
                        Shape oldShape = storedShapes[0];
                        storedShapes.RemoveAt(0);
                        oldShape.RetrieveFromDropArea();
                        GameEvents.OnShapeLeftDropArea(oldShape);
                    }
                }
                
                // Yeni şekli ekle
                Debug.Log($"[DropArea] Yeni şekil yerleştiriliyor: {shape.name}");
                storedShapes.Add(shape);
                currentShape = shape;
                CurrentShapeData = shape.CurrentShapeData;
                isAvailable = false;
                shape.isInDropArea = true;
                
                GameEvents.OnShapeStoredInDropArea(shape);
                OnShapeStored?.Invoke(shape);
                
                Debug.Log($"[DropArea] Şekil başarıyla eklendi: {shape.name}");
                return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DropArea] StoreShape error: {ex.Message}");
            return false;
        }
    }

    public void RetrieveShape(Shape shape)
    {
        if (shape == null)
        {
            Debug.LogError("[DropArea] RetrieveShape: shape is null");
            return;
        }

        Debug.Log($"[DropArea] RetrieveShape çağrıldı - Shape: {shape.name}");
        
        try
        {
            lock (_lock)
            {
                if (storedShapes.Contains(shape))
                {
                    Debug.Log($"[DropArea] Shape oyuncu tarafından alınıyor: {shape.name}");
                    shape.RetrieveFromDropArea();
                    shape.isInDropArea = false;  // Şeklin drop area'dan çıktığını işaretle
                    storedShapes.Remove(shape);
                    CurrentShapeData = null;
                    UpdateStoredShapes();
                    
                    OnShapeRetrieved?.Invoke(shape);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DropArea] RetrieveShape error: {ex.Message}");
        }
    }

    public void ClearDropArea()
    {
        Debug.Log($"[DropArea] Drop area temizleniyor - Stored shapes count: {storedShapes?.Count ?? 0}");
        
        try
        {
            lock (_lock)
            {
                storedShapes.Clear();
                currentShape = null;
                CurrentShapeData = null;
                isAvailable = true;
                UpdateStoredShapes();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DropArea] ClearDropArea error: {ex.Message}");
        }
    }

    public bool IsShapeStored(Shape shape)
    {
        if (shape == null || storedShapes == null) return false;
        
        lock (_lock)
        {
            return storedShapes.Contains(shape);
        }
    }

    public List<Shape> GetStoredShapes()
    {
        if (storedShapes == null) return new List<Shape>();
        
        lock (_lock)
        {
            return new List<Shape>(storedShapes);
        }
    }

    public bool CanStoreShape()
    {
        if (storedShapes == null) return false;
        
        lock (_lock)
        {
            return !HasShape() && storedShapes.Count < maxStoredShapes;
        }
    }
}
