using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Shape : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public GameObject squareShapeImage;
    public Vector2 shapeSelectedScale;
    public Vector2 offset = new Vector2(0f, 700f);
    public ShapeColor lastExplosionColor { get; private set; }
    protected bool isPlaced = false;
    public Shapedata CurrentShapeData;
    public int TotalSquareNumber { get; set; }

    private Vector3 originalPosition;
    public DropArea currentDropArea;
    private Vector3 startPosition;
    private bool isDragging = false;
    public bool isInDropArea = false;
    private Vector3 dropAreaPosition;
    private bool isBeingRetrieved = false;
    public enum ShapeColor
    {
        Blue,
        Green,
        Yellow,
        Joker,
        None
    }
    public ShapeColor shapeColor;

    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite yellowSprite;

    private List<GameObject> _currentShape = new List<GameObject>();
    private Vector3 _shapeStartScale;
    private RectTransform _transform;
    private Canvas _canvas;
    public Vector3 _startPosition;

    public bool _shapeactive = true;
    private float holdTime = 0f;
    private float requiredHoldTime = 1f;
    private bool isHolding = false;
    private Vector2 holdStartPosition;

    public static Dictionary<ShapeColor, int> colorCosts = new Dictionary<ShapeColor, int>()
    {
        { ShapeColor.Blue, 5 },
        { ShapeColor.Green, 5 },
        { ShapeColor.Yellow, 5 }
    };

    public string CurrentColor { get; set; } = "blue";

    private void Start()
    {
        originalPosition = transform.position;
    }

    public virtual void Awake()
    {
        _shapeStartScale = this.GetComponent<RectTransform>().localScale;
        _transform = this.GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _startPosition = transform.localPosition;
        _shapeactive = true;

        // Ana objenin Image bileşeninin raycastTarget'ını kapat
        var mainImage = GetComponent<Image>();
        if (mainImage != null)
        {
            mainImage.raycastTarget = false;
        }

        // Tüm child objelerin Image bileşenlerinin raycastTarget'ını kapat
        var childImages = GetComponentsInChildren<Image>();
        foreach (var image in childImages)
        {
            image.raycastTarget = false;
        }

        System.Random random = new System.Random();
        SetColor(shapeColor);
    }

    public void SetColor(ShapeColor color)
    {
        Sprite newSprite = GetSprite(color);

        if (newSprite == null)
        {
            return;
        }

        foreach (Transform child in transform)
        {
            var image = child.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = newSprite;
            }

        }
    }

    private Sprite GetSprite(ShapeColor color)
    {
        switch (color)
        {
            case ShapeColor.Blue:
                return blueSprite;
            case ShapeColor.Green:
                return greenSprite;
            case ShapeColor.Yellow:
                return yellowSprite;
            default:
                Debug.LogWarning("GetSprite() called but no matching color found: " + color);
                return null;
        }
    }


    private void OnDisable()
    {
        GameEvents.MoveShapetoStartPosition -= MoveShapetoStartPosition;
        GameEvents.SetShapeInactive -= SetShapeInactive;
    }

    protected virtual void OnEnable()
    {
        SetColor(shapeColor);
        GameEvents.MoveShapetoStartPosition += MoveShapetoStartPosition;
        GameEvents.SetShapeInactive += SetShapeInactive;
    }


    public bool IsonStartPosition()
    {
        return transform.localPosition == _startPosition;
    }

    public bool IsAnyOfShapeSquareActive()
    {
        if (this is ColorSquare && !gameObject.activeSelf)
        {
            return false;
        }

        foreach (var square in _currentShape)
        {
            if (square.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }


    public void DeactivateShape()
    {
        if (isInDropArea)
        {
            return;
        }

        if (_shapeactive)
        {
            foreach (var square in _currentShape)
            {
                square?.GetComponent<ShapeSquare>().DeactivateShape();
            }
        }
        _shapeactive = false;
    }

    public void SetShapeInactive()
    {
        if (isInDropArea)
        {
            return;
        }

        if (IsonStartPosition() == false && IsAnyOfShapeSquareActive())
        {
            foreach (var square in _currentShape)
            {
                square.gameObject.SetActive(false);
            }
        }
    }
    public void ActivateShape()
    {
        if (!_shapeactive)
        {
            foreach (var square in _currentShape)
            {
                square?.GetComponent<ShapeSquare>().ActivateShape();
            }
        }
        _shapeactive = true;
    }

    public virtual void RequestNewShape(Shapedata shapedata)
    {
        _transform.localPosition = _startPosition;
        CreateShape(shapedata);
    }

    public void CreateShape(Shapedata shapeData)
    {
        CurrentShapeData = shapeData;
        TotalSquareNumber = GetNumberOfSquares(shapeData);

        while (_currentShape.Count <= TotalSquareNumber)
        {
            _currentShape.Add(Instantiate(squareShapeImage, transform) as GameObject);
        }

        foreach (var square in _currentShape)
        {
            square.gameObject.transform.position = Vector3.zero;
            square.gameObject.SetActive(false);
        }

        var squareRect = squareShapeImage.GetComponent<RectTransform>();
        var moveDistance = new Vector2(squareRect.rect.width * squareRect.localScale.x, squareRect.rect.height * squareRect.localScale.y);
        int currentIndexInList = 0;

        for (var row = 0; row < shapeData.rows; row++)
        {
            for (var column = 0; column < shapeData.columns; column++)
            {
                if (shapeData.board[row].column[column])
                {
                    _currentShape[currentIndexInList].SetActive(true);
                    _currentShape[currentIndexInList].GetComponent<RectTransform>().localPosition = new Vector2(
                    GetXPositionForShapeSquare(shapeData, column, moveDistance),
                    GetYPositionForShapeSquare(shapeData, row, moveDistance));

                    _currentShape[currentIndexInList].GetComponent<Image>().sprite = GetSprite(shapeColor);

                    currentIndexInList++;
                }
            }
        }
    }

    private float GetYPositionForShapeSquare(Shapedata shapedata, int row, Vector2 moveDistance)
    {
        float shiftOnY = 0f;
        if (shapedata.rows > 1)
        {
            if (shapedata.rows % 2 != 0)
            {
                var middleSquareIndex = (shapedata.rows - 1) / 2;
                var multiplier = (shapedata.rows - 1) / 2;

                if (row < middleSquareIndex)
                {
                    shiftOnY = moveDistance.y * 1;
                    shiftOnY *= multiplier;
                }
                else if (row > middleSquareIndex)
                {
                    shiftOnY = moveDistance.y * -1;
                    shiftOnY *= multiplier;
                }
            }
            else
            {
                var middleSquareIndex2 = (shapedata.rows == 2) ? 1 : (shapedata.rows / 2);
                var middleSquareIndex1 = (shapedata.rows == 2) ? 0 : shapedata.rows - 2;
                var mulplier = shapedata.rows / 2;

                if (row == middleSquareIndex1 || row == middleSquareIndex2)
                {
                    if (row == middleSquareIndex2)
                    {
                        shiftOnY = (moveDistance.y / 2) * -1;
                    }
                    if (row == middleSquareIndex1)
                    {
                        shiftOnY = (moveDistance.y / 2);
                    }
                }
                if (row < middleSquareIndex1 && row < middleSquareIndex2)
                {
                    shiftOnY = moveDistance.y * 1;
                    shiftOnY *= mulplier;
                }
                else if (row > middleSquareIndex1 && row > middleSquareIndex2)
                {
                    shiftOnY = moveDistance.y * -1;
                    shiftOnY *= mulplier;
                }
            }
        }
        return shiftOnY;
    }

    private float GetXPositionForShapeSquare(Shapedata shapedata, int column, Vector2 moveDistance)
    {
        float shiftOnX = 0f;

        if (shapedata.columns > 1)
        {
            if (shapedata.columns % 2 != 0)
            {
                var middleSquareIndex = (shapedata.columns - 1) / 2;
                var multiplier = (shapedata.columns - 1) / 2;
                if (column < middleSquareIndex)
                {
                    shiftOnX = moveDistance.x * -1;
                    shiftOnX *= multiplier;
                }
                else if (column > middleSquareIndex)
                {
                    shiftOnX = moveDistance.x * 1;
                    shiftOnX *= multiplier;
                }
            }
            else
            {
                var middleSquareIndex2 = (shapedata.columns == 2) ? 1 : (shapedata.columns / 2);
                var middleSquareIndex1 = (shapedata.columns == 2) ? 0 : (shapedata.columns - 1);
                var multiplier = shapedata.columns / 2;

                if (column == middleSquareIndex1 || column == middleSquareIndex2)
                {
                    if (column == middleSquareIndex2)
                    {
                        shiftOnX = moveDistance.x / 2;
                    }
                    if (column == middleSquareIndex1)
                    {
                        shiftOnX = (moveDistance.x / 2) * -1;
                    }
                }
                if (column < middleSquareIndex1 && column < middleSquareIndex2)
                {
                    shiftOnX = moveDistance.x * -1;
                    shiftOnX *= multiplier;
                }
                else if (column > middleSquareIndex1 && column > middleSquareIndex2)
                {
                    shiftOnX = moveDistance.x * 1;
                    shiftOnX *= multiplier;
                }
            }
        }
        return shiftOnX;
    }

    private int GetNumberOfSquares(Shapedata shapeData)
    {
        int number = 0;
        foreach (var rowData in shapeData.board)
        {
            foreach (var active in rowData.column)
            {
                if (active)
                    number++;
            }
        }
        return number;
    }

    public virtual void OnPointerClick(PointerEventData eventData) { }



    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (isInDropArea && currentDropArea != null)
        {
            Debug.Log($"[Shape] Drop Area'dan alınıyor: {currentDropArea.gameObject.name}");
            currentDropArea.RetrieveShape(this);
        }

        originalPosition = transform.position;
        StopAllCoroutines();
        isHolding = false;
        holdTime = 0f;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (_transform == null || _canvas == null) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );
        
        _transform.localPosition = pos + offset;
    }


    public virtual bool CheckIfOneByOneBlockCanBePlaced()
    {
        Debug.Log($"[Shape] CheckIfOneByOneBlockCanBePlaced başladı - Shape: {gameObject.name}");
        
        bool canPlaceShape = false;
        GameEvents.CheckIfShapeCanBePlacedMethod();
        canPlaceShape = true;

        if (canPlaceShape)
        {
            // Grid'e yerleştirme başarılı oldu
            _shapeactive = false;
            
            // Drop area'dan gelmediyse referansları temizle
            if (!isInDropArea)
            {
                Debug.Log("[Shape] Drop area'dan gelmediği için referanslar temizleniyor");
                if (currentDropArea != null)
                {
                    currentDropArea.ClearDropArea();
                    currentDropArea = null;
                }
                isInDropArea = false;
                isBeingRetrieved = false;
            }
            else
            {
                Debug.Log("[Shape] Drop area'dan geldiği için referanslar korunuyor");
            }
        }
        else
        {
            Debug.Log("[Shape] Shape yerleştirilemedi");
        }

        return canPlaceShape;
    }

    private void CleanupDropAreaReferences()
    {
        Debug.Log($"[Shape] Drop area referansları temizleniyor - Shape: {gameObject.name}");
        
        if (currentDropArea != null)
        {
            currentDropArea.ClearDropArea();
            currentDropArea = null;
        }
        
        isInDropArea = false;
        isBeingRetrieved = false;
        _shapeactive = false;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!(this is LineEraser) && !(this is HammerSquare))
        {
            isHolding = true;
            holdTime = 0f;
            holdStartPosition = eventData.position;
            StartCoroutine(CheckHoldTime());
        }
        startPosition = transform.position;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        isHolding = false;
        holdTime = 0f;
    }

    private bool AnyOtherActiveShapeOnBoard()
    {
        Shape[] allShapes = FindObjectsOfType<Shape>();
        foreach (var s in allShapes)
        {
            // DropArea'da olmayan, kendisi olmayan ve aktif bir shape varsa true döner
            if (!s.isInDropArea && s._shapeactive && s != this)
            {
                Debug.Log($"[Shape] Aktif şekil bulundu: {s.name}");
                return true;
            }
        }
        Debug.Log("[Shape] Sahnede başka aktif şekil bulunamadı");
        return false;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[Shape] OnEndDrag başladı - Shape: {gameObject.name}");
        
        // Drop area kontrolü
        DropArea dropArea = FindDropAreaForStorage();
        if (dropArea != null)
        {
            Debug.Log($"[Shape] Drop Area bulundu: {dropArea.gameObject.name}");
            
            // Drop area'ya yerleştirmeyi dene
            bool placed = dropArea.StoreShape(this);
            if (placed)
            {
                StoreInDropArea(dropArea);
                Debug.Log("[Shape] Şekil başarıyla drop area'ya yerleştirildi");
                return;
            }
            else
            {
                // Drop area doluysa şekil başlangıç pozisyonuna dönmeli
                Debug.Log("[Shape] Drop area dolu olduğu için yerleştirilemedi");
                MoveShapetoStartPosition();
                
                // Hiç başka şekil var mı?
                if (!AnyOtherActiveShapeOnBoard())
                {
                    Debug.Log("[Shape] Sahnede başka şekil kalmadı, yeni şekiller isteniyor");
                    GameEvents.RequestNewShapeMethod();
                }
                return;
            }
        }

        // Grid kontrolü (sadece drop area'ya yerleştirilmediyse kontrol et)
        bool shapePlaced = CheckIfOneByOneBlockCanBePlaced();
        if (shapePlaced)
        {
            if (this is ColorSquare)
            {
                GameEvents.TriggerOneByOneBlockExplosionMethod(shapeColor);
            }
            return;
        }

        // Hiçbir yere yerleştirilemezse başlangıç pozisyonuna dön
        Debug.Log("[Shape] Yerleştirilemedi, başlangıç pozisyonuna dönülüyor");
        MoveShapetoStartPosition();
    }

    private DropArea FindDropAreaForStorage()
    {
        DropArea[] allDropAreas = GameObject.FindObjectsOfType<DropArea>();
        Vector3[] corners = new Vector3[4];
        Vector3 shapePos = transform.position;

        foreach (var dropArea in allDropAreas)
        {
            if (!dropArea.CanStoreShape()) continue;

            RectTransform dropAreaRect = dropArea.GetComponent<RectTransform>();
            if (dropAreaRect != null)
            {
                dropAreaRect.GetWorldCorners(corners);
                Rect bounds = new Rect(
                    corners[0].x,
                    corners[0].y,
                    corners[2].x - corners[0].x,
                    corners[2].y - corners[0].y
                );

                if (bounds.Contains(shapePos))
                {
                    return dropArea;
                }
            }
        }

        return null;
    }

    public void StoreInDropArea(DropArea dropArea)
    {
        if (dropArea == null) return;

        Debug.Log($"[Shape] StoreInDropArea çağrıldı - Shape: {gameObject.name}");
        
        // Drop area'ya yerleştirme işlemi
        isInDropArea = true;
        currentDropArea = dropArea;
        _shapeactive = true; // Drop area'da aktif olmalı
        
        // RectTransform kullanarak merkeze yerleştir
        RectTransform dropAreaRect = dropArea.shapeHolder.GetComponent<RectTransform>();
        if (dropAreaRect != null)
        {
            Vector3[] corners = new Vector3[4];
            dropAreaRect.GetWorldCorners(corners);
            Vector3 center = (corners[0] + corners[2]) / 2;
            
            transform.position = center;
            transform.localScale = _shapeStartScale;
            
            Debug.Log($"[Shape] {gameObject.name} drop area'ya yerleştirildi - Pozisyon: {center}");
        }

        // Eğer sahnede başka aktif şekil yoksa yeni şekiller iste
        if (!AnyOtherActiveShapeOnBoard())
        {
            Debug.Log("[Shape] Sahnede başka aktif şekil kalmadı, yeni şekiller isteniyor");
            GameEvents.RequestNewShapeMethod();
        }
    }

    public void RetrieveFromDropArea()
    {
        if (!isInDropArea || currentDropArea == null) return;

        Debug.Log($"[Shape] RetrieveFromDropArea başladı - Shape: {gameObject.name}");
        
        isInDropArea = false;
        currentDropArea = null;
        _shapeactive = true; // Drop area'dan çıkarıldığında aktif olmalı
        
        Debug.Log($"[Shape] {gameObject.name} drop area'dan çıkarıldı");
    }

    public bool IsInDropArea()
    {
        return isInDropArea;
    }

    private IEnumerator CheckHoldTime()
    {
        while (isHolding)
        {
            holdTime += Time.deltaTime;

            if (Vector2.Distance(holdStartPosition, Input.mousePosition) > 30f)
            {
                isHolding = false;
                yield break;
            }

            if (holdTime >= requiredHoldTime)
            {
                if (!(this is ColorSquare))
                {
                    GameEvents.ShowColorSelectionPanelMethod(this);
                }
                isHolding = false;
                break;
            }

            yield return null;
        }
    }

    public bool TryChangeColor(ShapeColor newColor)
    {
        if (Scores.Instance.HasEnoughPoints(colorCosts[newColor]))
        {
            Scores.Instance.SpendPoints(colorCosts[newColor]);
            shapeColor = newColor;
            SetColor(newColor);
            return true;
        }
        return false;
    }

    public virtual void MoveShapetoStartPosition()
    {
        _transform.transform.localPosition = _startPosition;
    }
    public ShapeColor GetRandomShapeColor()
    {
        ShapeColor[] availableColors = new ShapeColor[]
        {
        ShapeColor.Blue,
        ShapeColor.Green,
        ShapeColor.Yellow,
        };

        ShapeColor selectedColor = availableColors[UnityEngine.Random.Range(0, availableColors.Length)];
        return selectedColor;
    }

    public void ChangeSprite(Sprite newSprite)
    {
        var image = GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.sprite = newSprite;
        }
    }
}