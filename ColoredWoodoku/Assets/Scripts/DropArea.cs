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
    public Transform shapeHolder;
    public Vector2 colliderSize = new Vector2(100f, 100f); 
    private Image dropAreaImage;
    private RectTransform rectTransform;

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

        // Drop Area'nın kendi Image bileşenini al
        dropAreaImage = GetComponent<Image>();
        if (dropAreaImage != null)
        {
            dropAreaImage.raycastTarget = true; // UI etkileşimi için true yapıyoruz
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
            shapeHolder = transform;
        }

        rectTransform = GetComponent<RectTransform>();

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
        
        if (shape != null && shape == currentShape)
        {
            Debug.Log($"[DropArea] Shape trigger alanından çıktı, temizleniyor: {shape.gameObject.name}");
            currentShape = null;
            shape.isInDropArea = false;
            shape.currentDropArea = null;
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
            Gizmos.DrawSphere(shapeHolder.position, 5f);
        }
    }
#endif

    public bool CanStoreShape()
    {
        return currentShape == null;
    }

    public bool StoreShape(Shape shape)
    {
        if (!CanStoreShape())
        {
            Debug.Log($"[DropArea] Drop area dolu, yerleştirme başarısız: {shape.gameObject.name}");
            return false;
        }

        Debug.Log($"[DropArea] StoreShape çağrıldı - Shape: {shape.gameObject.name}");
        
        // Shape'i drop area'ya yerleştir
        currentShape = shape;
        shape.isInDropArea = true;
        shape.currentDropArea = this;

        // Shape'i drop area'nın merkezine yerleştir
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Vector3 center = (corners[0] + corners[2]) / 2;
        shape.transform.position = center;
        
        Debug.Log($"[DropArea] Shape başarıyla yerleştirildi: {shape.gameObject.name}");
        return true;
    }

    public void RetrieveShape(Shape shape)
    {
        if (currentShape != shape)
        {
            Debug.Log($"[DropArea] Bu shape drop area'da değil: {shape.gameObject.name}");
            return;
        }

        Debug.Log($"[DropArea] RetrieveShape çağrıldı - Shape: {shape.gameObject.name}");
        
        // Shape'i drop area'dan çıkar
        shape.RetrieveFromDropArea();
        ClearDropArea();
    }

    public void ClearDropArea()
    {
        if (currentShape != null)
        {
            Debug.Log($"[DropArea] Drop area temizleniyor, mevcut shape: {currentShape.gameObject.name}");
            currentShape = null;
        }
    }

    public bool HasShape()
    {
        return currentShape != null;
    }

    public Shape GetCurrentShape()
    {
        return currentShape;
    }
}
