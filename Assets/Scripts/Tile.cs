using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    #region Header
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public int TileIndex { get; private set; }

    private bool isOn = false;
    public bool IsOn => isOn;

    private Image tileImage;
    private Color colorOff = new Color(0.376f, 0.376f, 0.376f, 1f);
    private Color colorOn = new Color(1f, 1f, 0.553f, 1f);

    
    public List<int> togglePattern = new List<int>(); // Stores toggle pattern for this tile
    #endregion

    private void Awake()
    {
        tileImage = GetComponent<Image>();
        if (tileImage == null)
        {
            Debug.LogError("Tile prefab must have an Image component!");
        }
    }

    #region Initialization
    public void Initialize(int x, int y, int index)
    {
        GridX = x;
        GridY = y;
        TileIndex = index;
        SetState(false); // Start with all tiles OFF
    }
    #endregion

    #region Tile Visual Handler
    public void SetState(bool state)
    {
        isOn = state;
        UpdateVisual();
    }

    public void Toggle()
    {
        isOn = !isOn;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (tileImage != null)
        {
            tileImage.color = isOn ? colorOn : colorOff;
        }
    }
    #endregion

    #region Click Dedection
    public void OnPointerClick(PointerEventData eventData)
    {
        SignalGridManager.Instance.OnTileClicked(this);
    }
    #endregion

    #region Tile Clicked Visual Feedback    
    public void PlayClickAnimation()
    {
        StartCoroutine(ClickAnimationCoroutine());
    }

    private IEnumerator ClickAnimationCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        float duration = 0.1f;
        float elapsed = 0f;

        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0.9f, elapsed / duration);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        elapsed = 0f;
        // Scale back up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0.9f, 1f, elapsed / duration);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        transform.localScale = originalScale;
    }
    #endregion
}
