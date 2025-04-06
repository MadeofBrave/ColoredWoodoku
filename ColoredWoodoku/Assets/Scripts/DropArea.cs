using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour
{
    public static DropArea Instance;
    private BoxCollider2D areaCollider;
    private Shape currentShape;
    public GameObject shapeHolder;
    public Vector2 colliderSize = new Vector2(100f, 100f); 
    private Image dropAreaImage;
    private RectTransform rectTransform;
    private bool isAvailable = true;
    private bool isPlacingShape = false;

    // Shape listesi ve data
    public List<Shape> storedShapes = new List<Shape>();
    private const int maxStoredShapes = 1; // Sabit 1 şekil tutacak
    public Shapedata CurrentShapeData { get; private set; }

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

        rectTransform = GetComponent<RectTransform>();
        storedShapes = new List<Shape>();
        CurrentShapeData = null;

        Debug.Log($"[DropArea] Başlatıldı: {gameObject.name}, Konum: {transform.position}, Collider Size: {areaCollider.size}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Shape shape = other.GetComponent<Shape>();
        if (shape != null)
        {
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[DropArea] OnTriggerExit2D - Çıkan nesne: {other.gameObject.name}");
        
        Shape shape = other.GetComponent<Shape>();
        if (shape == null) return;

        // Şekil grid'e yerleştirildiğinde (!_shapeactive) veya drop area'dan alındığında (!isInDropArea) temizle
        if (storedShapes.Contains(shape) && (!shape._shapeactive || !shape.isInDropArea))
        {
            Debug.Log($"[DropArea] Şekil grid'e yerleştirildi veya alındı, temizleniyor: {shape.name}");
            storedShapes.Remove(shape);
            if (currentShape == shape)
            {
                currentShape = null;
                CurrentShapeData = null;
            }
            UpdateStoredShapes();
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
        Debug.Log("[DropArea] UpdateStoredShapes başladı");
        
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
            CurrentShapeData = currentShape.CurrentShapeData;
            isAvailable = false;
            Debug.Log($"[DropArea] Drop area güncellendi - Mevcut şekil: {currentShape.name}");
        }
        else
        {
            currentShape = null;
            CurrentShapeData = null;
            isAvailable = true;
            Debug.Log("[DropArea] Drop area boş");
        }
    }

    public bool HasShape()
    {
        // Sadece storedShapes listesini kontrol et
        return storedShapes.Count > 0 && storedShapes[0] != null;
    }

    public bool StoreShape(Shape shape)
    {
        Debug.Log($"[DropArea] StoreShape çağrıldı - Shape: {shape.name}, Stored shapes count: {storedShapes.Count}");
        
        // Önce mevcut durumu güncelle
        UpdateStoredShapes();
        
        // Drop area'da herhangi bir şekil var mı kontrol et
        if (storedShapes.Count > 0)
        {
            Debug.Log($"[DropArea] Drop area dolu (mevcut şekil: {storedShapes[0].name}), yeni şekil reddediliyor: {shape.name}");
            shape.MoveShapetoStartPosition();
            return false;
        }
        
        // Drop area tamamen boş, yeni şekil eklenebilir
        Debug.Log($"[DropArea] Drop area boş, yeni şekil yerleştiriliyor: {shape.name}");
        storedShapes.Add(shape);
        currentShape = shape;
        CurrentShapeData = shape.CurrentShapeData;
        isAvailable = false;
        
        Debug.Log($"[DropArea] Şekil başarıyla eklendi: {shape.name}");
        return true;
    }

    private IEnumerator ResetPlacingFlag()
    {
        // Bir frame bekle
        yield return new WaitForEndOfFrame();
        isPlacingShape = false;
        Debug.Log("[DropArea] Yerleştirme işlemi tamamlandı, isPlacingShape sıfırlandı");
    }

    public void RetrieveShape(Shape shape)
    {
        Debug.Log($"[DropArea] RetrieveShape çağrıldı - Shape: {shape.name}");
        if (storedShapes.Contains(shape))
        {
            Debug.Log($"[DropArea] Shape oyuncu tarafından alınıyor: {shape.name}");
            shape.RetrieveFromDropArea();
            storedShapes.Remove(shape);
            CurrentShapeData = null;
            UpdateStoredShapes();
        }
    }

    public void ClearDropArea()
    {
        Debug.Log($"[DropArea] Drop area temizleniyor - Stored shapes count: {storedShapes.Count}");
        storedShapes.Clear();
        currentShape = null;
        CurrentShapeData = null;
        isAvailable = true;
        UpdateStoredShapes(); // Temizlik sonrası listeyi güncelle
    }

    public bool IsShapeStored(Shape shape)
    {
        UpdateStoredShapes(); // Kontrol öncesi listeyi güncelle
        return storedShapes.Contains(shape);
    }

    public List<Shape> GetStoredShapes()
    {
        UpdateStoredShapes(); // Liste alınmadan önce güncelle
        return storedShapes;
    }

    public bool CanStoreShape()
    {
        UpdateStoredShapes(); // Kontrol öncesi listeyi güncelle
        return !HasShape() && storedShapes.Count < maxStoredShapes;
    }
}
