using TMPro;
using UnityEngine;
using System.Collections;

namespace Slax.Schedule
{
    public class UIScheduleEventObserverRenderer : MonoBehaviour
    {
        private CanvasGroup _cg;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI ID;
        public TextMeshProUGUI Date;

        private Coroutine fadeOutCoroutine;

        private void Awake()
        {
            _cg = GetComponent<CanvasGroup>();
        }

        public void ShowEvent(ScheduleEvent ev)
        {
            if (ev == null) return;
            Name.text = ev.Name;
            ID.text = $"ID: {ev.ID}";
            Date.text = $"{ev.Timestamp.Date} / {ev.Timestamp.Season.ToString()} / {ev.Timestamp.Year}";

            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }

            _cg.alpha = 1;
            fadeOutCoroutine = StartCoroutine(FadeOutAfterDelay());
        }

        private IEnumerator FadeOutAfterDelay()
        {
            yield return new WaitForSeconds(2f);

            float fadeDuration = 1f;
            float elapsedTime = 0f;
            float startAlpha = _cg.alpha;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
                _cg.alpha = newAlpha;
                yield return null;
            }

            _cg.alpha = 0f;
        }
    }
}
