using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Slax.Schedule
{
    public class UITimeManager : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI _timeText;
        [SerializeField] protected TextMeshProUGUI _dateText;
        [SerializeField] protected TextMeshProUGUI _yearText;

        [Header("Images")]
        [SerializeField] protected Image _timeOfDayImage;
        [SerializeField] protected Sprite _morningSprite;
        [SerializeField] protected Sprite _afternoonSprite;
        [SerializeField] protected Sprite _nightSprite;

        private void OnEnable()
        {
            TimeManager.OnDateTimeChanged += UpdateTime;
        }

        private void OnDisable()
        {
            TimeManager.OnDateTimeChanged -= UpdateTime;
        }

        protected virtual void UpdateTime(DateTime date)
        {
            if (_timeText) _timeText.text = date.TimeToString();
            if (_dateText) _dateText.text = date.DateToString() + " / " + date.Season.ToString();
            if (_yearText) _yearText.text = "Year " + date.Year.ToString();

            UpdateTimeOfDaySprite(date);
        }

        protected virtual void UpdateTimeOfDaySprite(DateTime date)
        {
            if (date.IsMorning && _timeOfDayImage.sprite != _morningSprite)
                _timeOfDayImage.sprite = _morningSprite;
            else if (date.IsAfternoon && _timeOfDayImage.sprite != _afternoonSprite)
                _timeOfDayImage.sprite = _afternoonSprite;
            else if (date.IsNight && _timeOfDayImage.sprite != _nightSprite)
                _timeOfDayImage.sprite = _nightSprite;
        }
    }
}
