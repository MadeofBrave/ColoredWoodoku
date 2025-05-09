using UnityEngine;
using UnityEngine.EventSystems;

public class RequestNewShapesButton : MonoBehaviour, IPointerClickHandler
{
    public int cost = 5;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        UpdateInteractability();
    }

    private void Update()
    {
        UpdateInteractability();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Scores.Instance.HasEnoughPoints(cost))
        {
            Scores.Instance.SpendPoints(cost);
            GameEvents.RequestNewShapeMethod();
        }
        
    }

    private void UpdateInteractability()
    {
        if (Scores.Instance != null)
        {
            bool hasEnoughPoints = Scores.Instance.HasEnoughPoints(cost);
            canvasGroup.alpha = hasEnoughPoints ? 1f : 0.3f;
            canvasGroup.blocksRaycasts = hasEnoughPoints;
        }
    }
}
