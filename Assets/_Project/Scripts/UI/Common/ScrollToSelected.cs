﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelected : MonoBehaviour
{

    public float scrollSpeed = 10f;

    ScrollRect m_ScrollRect;
    RectTransform m_RectTransform;
    RectTransform m_ContentRectTransform;
    RectTransform m_SelectedRectTransform;

    void Awake()
    {
        m_ScrollRect = GetComponent<ScrollRect>();
        m_RectTransform = GetComponent<RectTransform>();
        m_ContentRectTransform = m_ScrollRect.content;
    }

    void Update()
    {
        UpdateScrollToSelected();
    }

    void UpdateScrollToSelected()
    {

        // grab the current selected from the eventsystem
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            return;
        }
        if (selected.transform.parent != m_ContentRectTransform.transform)
        {
            return;
        }

        m_SelectedRectTransform = selected.GetComponent<RectTransform>();

        // math stuff
        //Vector3 test = m_RectTransform.InverseTransformPoint(m_RectTransform.anchoredPosition);
        Vector3 selectedDifference = m_RectTransform.localPosition - m_SelectedRectTransform.localPosition;
        float contentHeightDifference = (m_ContentRectTransform.rect.height - m_RectTransform.rect.height);

        float selectedPosition = (m_ContentRectTransform.rect.height - selectedDifference.y);
        float currentScrollRectPosition = m_ScrollRect.normalizedPosition.y * contentHeightDifference;
        float above = currentScrollRectPosition - (m_SelectedRectTransform.rect.height / 2) + m_RectTransform.rect.height;
        float below = currentScrollRectPosition + (m_SelectedRectTransform.rect.height / 2);

        // check if selected is out of bounds
        if (selectedPosition > above)
        {
            float step = selectedPosition - above;
            float newY = currentScrollRectPosition + step;
            float newNormalizedY = Mathf.Clamp(newY / contentHeightDifference, 0f, 1f);

            m_ScrollRect.normalizedPosition = Vector2.Lerp(m_ScrollRect.normalizedPosition, new Vector2(0, newNormalizedY), scrollSpeed * Time.unscaledDeltaTime);
        }
        else if (selectedPosition < below)
        {
            float step = selectedPosition - below;
            float newY = currentScrollRectPosition + step;
            float newNormalizedY = Mathf.Clamp(newY / contentHeightDifference, 0f, 1f);
            //if (newNormalizedY < 0.05f) newNormalizedY = 0f;

            m_ScrollRect.normalizedPosition = Vector2.Lerp(m_ScrollRect.normalizedPosition, new Vector2(0, newNormalizedY), scrollSpeed * Time.unscaledDeltaTime);
        }
    }
}