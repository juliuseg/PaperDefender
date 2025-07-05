using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FadeEverythingInAndOut : MonoBehaviour
{
    public float fadeTime = 1f;
    
    private List<Image> allImages = new List<Image>();
    private List<float> originalImageAlphas = new List<float>();
    private List<TextMeshProUGUI> allTextMeshPro = new List<TextMeshProUGUI>();
    private List<float> originalTextMeshProAlphas = new List<float>();
    private List<Text> allTexts = new List<Text>();
    private List<float> originalTextAlphas = new List<float>();
    private bool isInitialized = false;
    
    // Coroutine management
    private Coroutine currentFadeCoroutine = null;

    void Start()
    {
        InitializeUIElements();
    }

    public void InitializeUIElements()
    {
        if (isInitialized) return;
        
        // Clear previous data
        allImages.Clear();
        originalImageAlphas.Clear();
        allTextMeshPro.Clear();
        originalTextMeshProAlphas.Clear();
        allTexts.Clear();
        originalTextAlphas.Clear();
        
        // Get all images from this object and its children
        Image[] images = GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            allImages.Add(image);
            originalImageAlphas.Add(image.color.a);
        }
        
        // Get all TextMeshPro components from this object and its children
        TextMeshProUGUI[] textMeshProComponents = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI textMeshPro in textMeshProComponents)
        {
            allTextMeshPro.Add(textMeshPro);
            originalTextMeshProAlphas.Add(textMeshPro.color.a);
        }
        
        // Get all regular Text components from this object and its children
        Text[] textComponents = GetComponentsInChildren<Text>(true);
        foreach (Text text in textComponents)
        {
            allTexts.Add(text);
            originalTextAlphas.Add(text.color.a);
        }
        
        isInitialized = true;
    }

    public void FadeOut()
    {
        if (!isInitialized)
            InitializeUIElements();
        
        // Stop any currently running fade coroutine
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
            
        currentFadeCoroutine = StartCoroutine(FadeOutCoroutine());
    }

    public void FadeIn()
    {
        if (!isInitialized)
            InitializeUIElements();
        
        // Stop any currently running fade coroutine
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
            
        currentFadeCoroutine = StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / fadeTime;
            
            // Fade out images
            for (int i = 0; i < allImages.Count; i++)
            {
                if (allImages[i] != null)
                {
                    Color currentColor = allImages[i].color;
                    float targetAlpha = Mathf.Lerp(originalImageAlphas[i], 0f, normalizedTime);
                    allImages[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
                }
            }
            
            // Fade out TextMeshPro components
            for (int i = 0; i < allTextMeshPro.Count; i++)
            {
                if (allTextMeshPro[i] != null)
                {
                    Color currentColor = allTextMeshPro[i].color;
                    float targetAlpha = Mathf.Lerp(originalTextMeshProAlphas[i], 0f, normalizedTime);
                    allTextMeshPro[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
                }
            }
            
            // Fade out regular Text components
            for (int i = 0; i < allTexts.Count; i++)
            {
                if (allTexts[i] != null)
                {
                    Color currentColor = allTexts[i].color;
                    float targetAlpha = Mathf.Lerp(originalTextAlphas[i], 0f, normalizedTime);
                    allTexts[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
                }
            }
            
            yield return null;
        }
        
        // Ensure all elements are fully transparent
        for (int i = 0; i < allImages.Count; i++)
        {
            if (allImages[i] != null)
            {
                Color currentColor = allImages[i].color;
                allImages[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            }
        }
        
        for (int i = 0; i < allTextMeshPro.Count; i++)
        {
            if (allTextMeshPro[i] != null)
            {
                Color currentColor = allTextMeshPro[i].color;
                allTextMeshPro[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            }
        }
        
        for (int i = 0; i < allTexts.Count; i++)
        {
            if (allTexts[i] != null)
            {
                Color currentColor = allTexts[i].color;
                allTexts[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            }
        }
        
        // Clear the coroutine reference when done
        currentFadeCoroutine = null;
    }

    IEnumerator FadeInCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / fadeTime;
            
            // Fade in images
            for (int i = 0; i < allImages.Count; i++)
            {
                if (allImages[i] != null)
                {
                    Color currentColor = allImages[i].color;
                    float targetAlpha = Mathf.Lerp(0f, originalImageAlphas[i], normalizedTime);
                    allImages[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
                }
            }
            
            // Fade in TextMeshPro components
            for (int i = 0; i < allTextMeshPro.Count; i++)
            {
                if (allTextMeshPro[i] != null)
                {
                    Color currentColor = allTextMeshPro[i].color;
                    float targetAlpha = Mathf.Lerp(0f, originalTextMeshProAlphas[i], normalizedTime);
                    allTextMeshPro[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
                }
            }
            
            // Fade in regular Text components
            for (int i = 0; i < allTexts.Count; i++)
            {
                if (allTexts[i] != null)
                {
                    Color currentColor = allTexts[i].color;
                    float targetAlpha = Mathf.Lerp(0f, originalTextAlphas[i], normalizedTime);
                    allTexts[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
                }
            }
            
            yield return null;
        }
        
        // Ensure all elements are at their original alpha values
        for (int i = 0; i < allImages.Count; i++)
        {
            if (allImages[i] != null)
            {
                Color currentColor = allImages[i].color;
                allImages[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, originalImageAlphas[i]);
            }
        }
        
        for (int i = 0; i < allTextMeshPro.Count; i++)
        {
            if (allTextMeshPro[i] != null)
            {
                Color currentColor = allTextMeshPro[i].color;
                allTextMeshPro[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, originalTextMeshProAlphas[i]);
            }
        }
        
        for (int i = 0; i < allTexts.Count; i++)
        {
            if (allTexts[i] != null)
            {
                Color currentColor = allTexts[i].color;
                allTexts[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, originalTextAlphas[i]);
            }
        }
        
        // Clear the coroutine reference when done
        currentFadeCoroutine = null;
    }
    
    void OnDestroy()
    {
        // Clean up any running coroutines when the object is destroyed
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
    }
}
