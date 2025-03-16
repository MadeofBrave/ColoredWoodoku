using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ColorChangePanel : MonoBehaviour
{
    private const int COLOR_CHANGE_COST = 30;
    private Shape currentShape;
    private CanvasGroup canvasGroup;
    private bool isWaitingForLongPress = false;
    private float longPressTime = 1f;

    [SerializeField] private Button blueButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Text costText;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HidePanel();

        if (blueButton != null) blueButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Blue));
        if (greenButton != null) greenButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Green));
        if (yellowButton != null) yellowButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Yellow));

        if (costText != null)
        {
            costText.text = COLOR_CHANGE_COST.ToString();
        }
    }

    private void OnEnable()
    {
        GameEvents.ShowColorSelectionPanel += OnShowColorChangePanel;
    }

    private void OnDisable()
    {
        GameEvents.ShowColorSelectionPanel -= OnShowColorChangePanel;
    }

    private void OnShowColorChangePanel(Shape shape)
    {
        ShowPanel(shape, Input.mousePosition);
    }

    public void ShowPanel(Shape shape, Vector2 position)
    {
        if (shape == null || shape is LineEraser || shape is HammerSquare) return;

        currentShape = shape;
        transform.position = position;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        UpdateButtonsInteractability();
    }

    public void HidePanel()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        currentShape = null;
    }

    private void UpdateButtonsInteractability()
    {
        bool hasEnoughPoints = Scores.Instance.HasEnoughPoints(COLOR_CHANGE_COST);
        blueButton.interactable = hasEnoughPoints;
        greenButton.interactable = hasEnoughPoints;
        yellowButton.interactable = hasEnoughPoints;
    }

    private void ChangeColor(Shape.ShapeColor newColor)
    {
        if (currentShape == null || !Scores.Instance.HasEnoughPoints(COLOR_CHANGE_COST)) return;

        if (currentShape.TryChangeColor(newColor))
        {
            HidePanel();
        }
    }

    private void Update()
    {
        // Mouse sol tuşuna basılı tutulduğunda
        if (Input.GetMouseButton(0))
        {
            if (!isWaitingForLongPress)
            {
                isWaitingForLongPress = true;
                StartCoroutine(CheckLongPress());
            }
        }
        else
        {
            isWaitingForLongPress = false;
        }
    }

    private IEnumerator CheckLongPress()
    {
        float pressTime = 0f;
        Vector2 startPosition = Input.mousePosition;

        while (Input.GetMouseButton(0))
        {
            pressTime += Time.deltaTime;

            // Eğer mouse çok hareket ettiyse (sürükleme başladıysa) iptal et
            if (Vector2.Distance(startPosition, Input.mousePosition) > 30f)
            {
                yield break;
            }

            // Yeterli süre basıldıysa
            if (pressTime >= longPressTime)
            {
                // Mouse'un altındaki Shape'i bul
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null)
                {
                    Shape shape = hit.collider.GetComponent<Shape>();
                    if (shape != null && !(shape is LineEraser) && !(shape is HammerSquare))
                    {
                        GameEvents.ShowColorSelectionPanelMethod(shape);
                    }
                }
                break;
            }

            yield return null;
        }

        isWaitingForLongPress = false;
    }
} 