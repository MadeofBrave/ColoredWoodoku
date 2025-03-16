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

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HidePanel();

        // Butonlara tıklama olaylarını ekle
        blueButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Blue));
        greenButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Green));
        yellowButton.onClick.AddListener(() => ChangeColor(Shape.ShapeColor.Yellow));

        // Maliyet textlerini ayarla
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
}