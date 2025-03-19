using UnityEngine;
using UnityEngine.UI;

public class ColorSelectionPanel : MonoBehaviour
{
    [SerializeField] private Button blueButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Text blueCostText;
    [SerializeField] private Text greenCostText;
    [SerializeField] private Text yellowCostText;

    private Shape currentShape;
    private CanvasGroup canvasGroup;
    private const int COLOR_COST = 5;
    public Button rotateButton;
    private Shape selectedShape;


    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HidePanel();

        blueButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Blue));
        greenButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Green));
        yellowButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Yellow));
        rotateButton.onClick.AddListener(OnRotateButtonClick);

        blueCostText.text = COLOR_COST.ToString();
        greenCostText.text = COLOR_COST.ToString();
        yellowCostText.text = COLOR_COST.ToString();
    }

    private void OnEnable()
    {
        GameEvents.ShowColorSelectionPanel += ShowPanel;
    }

    private void OnDisable()
    {
        GameEvents.ShowColorSelectionPanel -= ShowPanel;
    }

    public void ShowPanel(Shape shape)
    {
        if (shape == null) return;

        currentShape = shape;
        SetSelectedShape(shape);
        transform.localPosition = new Vector3(0, 0, 0);
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
        bool hasEnoughPoints = Scores.Instance.HasEnoughPoints(COLOR_COST);
        blueButton.interactable = hasEnoughPoints;
        greenButton.interactable = hasEnoughPoints;
        yellowButton.interactable = hasEnoughPoints;
    }

    private void ChangeColor(Shape.ShapeColor newColor)
    {
        if (currentShape != null && currentShape.TryChangeColor(newColor))
        {
            HidePanel();
        }
    }
    public void SetSelectedShape(Shape shape)
    {
        selectedShape = shape;
    }

    private void OnRotateButtonClick()
    {
        if (selectedShape != null)
        {
            selectedShape.transform.Rotate(0, 0, 90);
        }
    }
}