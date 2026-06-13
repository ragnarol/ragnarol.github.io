using UnityEngine;
using System.Collections;

namespace FadingSuns.World
{
    /// <summary>
    /// In-game clock. NPCs follow schedules by hour.
    /// One real second = configurable game minutes.
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        [Header("Time Settings")]
        [Tooltip("How many game minutes pass per real second")]
        public float gameMinutesPerRealSecond = 10f;

        private float totalGameMinutes = 480f; // Start at 08:00

        public int CurrentHour => Mathf.FloorToInt(totalGameMinutes / 60f) % 24;
        public int CurrentMinute => Mathf.FloorToInt(totalGameMinutes % 60f);
        public int CurrentDay => Mathf.FloorToInt(totalGameMinutes / 1440f);

        public event System.Action<int> OnHourChanged;
        public event System.Action<int> OnDayChanged;

        private int lastHour = -1;
        private int lastDay = -1;

        void Update()
        {
            totalGameMinutes += Time.deltaTime * gameMinutesPerRealSecond;

            if (CurrentHour != lastHour)
            {
                lastHour = CurrentHour;
                OnHourChanged?.Invoke(CurrentHour);
            }
            if (CurrentDay != lastDay)
            {
                lastDay = CurrentDay;
                OnDayChanged?.Invoke(CurrentDay);
            }
        }

        public string GetTimeString() => $"{CurrentHour:D2}:{CurrentMinute:D2}";

        public void AdvanceHours(int hours)
        {
            totalGameMinutes += hours * 60f;
        }

        public bool IsNight() => CurrentHour >= 21 || CurrentHour < 6;
        public bool IsDawn() => CurrentHour >= 6 && CurrentHour < 8;

        // Save/Load
        public float GetTotalMinutes() => totalGameMinutes;
        public void SetTotalMinutes(float val) => totalGameMinutes = val;
    }
}
