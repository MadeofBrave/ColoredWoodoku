using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

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

    protected List<GameObject> _currentShape = new List<GameObject>();
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
        GameEvents.ShapeStoredInDropArea -= OnOtherShapeStoredInDropArea;
    }

    protected virtual void OnEnable()
    {
        SetColor(shapeColor);
        GameEvents.MoveShapetoStartPosition += MoveShapetoStartPosition;
        GameEvents.SetShapeInactive += SetShapeInactive;
        GameEvents.ShapeStoredInDropArea += OnOtherShapeStoredInDropArea;
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


    public virtual void DeactivateShape()
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
    public virtual void ActivateShape()
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


    public virtual void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        string shapeName = gameObject.name;
        ShapeColor originalColor = shapeColor;

        Debug.Log($"[Shape] OnEndDrag başladı - Shape: {shapeName}, Color: {originalColor}");

        // Drop area üzerinde mi kontrol et
        var dropArea = FindObjectOfType<DropArea>();
        bool isOverDropArea = dropArea != null && IsOverDropArea(dropArea);

        if (isOverDropArea)
        {
            Debug.Log($"[Shape] Drop area üzerinde - Shape: {shapeName}");
            
            // DropArea'ya yerleştirmeyi dene
            bool isStored = dropArea.StoreShape(this);
            
            if (isStored)
            {
                // Başarılıysa pozisyonu ayarla
                isInDropArea = true;
                currentDropArea = dropArea;
                Debug.Log($"[Shape] Şekil drop area'ya yerleştirildi - Shape: {shapeName}, Color: {originalColor}");
            }
            else
            {
                // Başarısızsa eski pozisyona dön
                Debug.Log($"[Shape] Drop area'ya yerleştirilemedi, başlangıca dönüyor - Shape: {shapeName}");
                MoveShapetoStartPosition();
            }
        }
        else
        {
            // Drop area'dan gelen bilgileri sakla
            bool wasInDropArea = isInDropArea;
            DropArea originalDropArea = currentDropArea;
            
            Debug.Log($"[Shape] Grid'e yerleştirme deneniyor - Shape: {shapeName}, Color: {originalColor}, isInDropArea: {wasInDropArea}");
            
            // Grid'e yerleştirme olayını çağır
            GameEvents.CheckIfShapeCanBePlacedMethod();
            
            // Yerleştirme başarılı oldu mu kontrol et
            if (_shapeactive)
            {
                // Yerleştirilemedi
                Debug.Log($"[Shape] Grid'e yerleştirilemedi - Shape: {shapeName}");
                MoveShapetoStartPosition();
                return;
            }
            
            Debug.Log($"[Shape] Grid'e yerleştirildi - Shape: {shapeName}, Color: {originalColor}");
            
            // Drop area temizleme işlemini sadece bu şekil drop area'dan geldiyse yap
            if (wasInDropArea && originalDropArea != null)
            {
                // Önce şeklin drop area referanslarını temizle
                Debug.Log($"[Shape] Drop area referansları temizleniyor - Shape: {shapeName}");
                isInDropArea = false;
                currentDropArea = null;
                
                // Sonra drop area'yı temizle
                originalDropArea.OnShapePlacedOnGrid();
            }
            
            // Şekil işlemleri
            if (this is ColorSquare)
            {
                GameEvents.TriggerOneByOneBlockExplosionMethod(originalColor);
            }
            else
            {
                // Şekli görünmez yap
                foreach (var square in _currentShape)
                {
                    if (square != null)
                    {
                        square.SetActive(false);
                    }
                }
            }
            
            // Şekli deaktive et
            gameObject.SetActive(false);
        }

        // Yerleştirilebilir şekil kontrolü
        bool anyPlaceableShapes = ShapeStorage.Instance.ShapeList.Any(shape => 
            shape.gameObject.activeSelf && shape.IsonStartPosition());

        if (!anyPlaceableShapes)
        {
            Debug.Log($"[Shape] Yerleştirilebilir şekil kalmadı, yeni şekiller talep ediliyor");
            GameEvents.RequestNewShapeMethod();
        }
    }
    
    // CheckIfOneByOneBlockCanBePlaced metodunu yeni metodla değiştirdim
    public virtual bool CheckIfShapeCanBePlacedOnGrid()
    {
        // Grid'e yerleştirme kontrolü için event'i tetikle
        GameEvents.CheckIfShapeCanBePlacedMethod();
        
        // Eğer şekil hala aktifse, yerleştirilememiş demektir
        return !_shapeactive;
    }

    // Eski metod adını koruyalım, ama yeni metodu çağıralım
    public virtual bool CheckIfOneByOneBlockCanBePlaced()
    {
        string shapeName = gameObject.name;
        ShapeColor originalColor = shapeColor;
        
        Debug.Log($"[Shape] CheckIfOneByOneBlockCanBePlaced başladı - Shape: {shapeName}, Color: {originalColor}");
        
        // Yeni metodu çağır
        bool canPlaceShape = CheckIfShapeCanBePlacedOnGrid();
        
        if (canPlaceShape)
        {
            Debug.Log($"[Shape] Grid'e yerleştirildi - Shape: {shapeName}, Color: {originalColor}");
        }
        else
        {
            Debug.Log($"[Shape] Shape yerleştirilemedi - Shape: {shapeName}");
        }
        
        return canPlaceShape;
    }

    private bool IsOverDropArea(DropArea dropArea)
    {
        if (dropArea == null) return false;

        var dropAreaRect = dropArea.GetComponent<RectTransform>();
        var shapeRect = GetComponent<RectTransform>();

        // Pozisyonları ekran koordinatlarına çevir
        Vector3[] dropCorners = new Vector3[4];
        Vector3[] shapeCorners = new Vector3[4];
        dropAreaRect.GetWorldCorners(dropCorners);
        shapeRect.GetWorldCorners(shapeCorners);

        // Drop area'nın sınırlarını kontrol et
        Rect dropRect = new Rect(dropCorners[0].x, dropCorners[0].y,
                                dropCorners[2].x - dropCorners[0].x,
                                dropCorners[2].y - dropCorners[0].y);

        // Şeklin merkez noktasını al
        Vector3 shapeCenter = shapeCorners[0] + (shapeCorners[2] - shapeCorners[0]) * 0.5f;

        return dropRect.Contains(shapeCenter);
    }

    public void MoveShapetoStartPosition()
    {
        Debug.Log("[Shape] MoveShapetoStartPosition çağrıldı - isInDropArea: " + isInDropArea);
        
        if (!isInDropArea)
        {
            transform.localPosition = _startPosition;
            _shapeactive = true;
            
            // Eğer bir drop area referansı varsa temizle
            if (currentDropArea != null)
            {
                currentDropArea = null;
            }
        }
    }

    public void RetrieveFromDropArea()
    {
        Debug.Log("[Shape] RetrieveFromDropArea çağrıldı");
        isInDropArea = false;
        currentDropArea = null;
        MoveShapetoStartPosition();
    }

    private void OnOtherShapeStoredInDropArea(Shape storedShape)
    {
        if (storedShape != this && isInDropArea)
        {
            Debug.Log($"[Shape] Başka bir şekil drop area'ya yerleştirildi, bu şekil çıkartılıyor: {gameObject.name}");
            isInDropArea = false;
            currentDropArea = null;
            MoveShapetoStartPosition();
        }
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

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!isDragging)
        {
            isHolding = true;
            holdStartPosition = eventData.position;
            holdTime = 0f;
            StartCoroutine(CheckHoldTime());
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        holdTime = 0f;
        StopAllCoroutines();
    }
}