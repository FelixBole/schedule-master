using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Slax.Schedule
{
    public class UIClock : MonoBehaviour
    {
        [SerializeField] private Image _clockArrow;
        [SerializeField] private TextMeshProUGUI _timeText;

        public void OnEnable()
        {
            TimeManager.OnDateTimeChanged += UpdateClock;
        }

        public void OnDisable()
        {
            TimeManager.OnDateTimeChanged -= UpdateClock;
        }

        private void UpdateClock(DateTime date)
        {
            float hourProgress = (float)date.Hour / 24f;
            float rotationDegrees = 90f - (hourProgress * 180f);

            if (_clockArrow)
            {
                _clockArrow.rectTransform.rotation = Quaternion.Euler(0f, 0f, rotationDegrees);
            }

            if (_timeText)
            {
                _timeText.text = date.TimeToString();
            }
        }
    }
}