using System;
using System.Collections.Generic;
using System.Linq;

namespace DeviceBox
{
    /// <summary>
    /// 運轉模式配置
    /// </summary>
    public class ModeConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; } = true;
        public List<ModeSchedule> Schedules { get; set; } = new List<ModeSchedule>();

        /// <summary>
        /// 檢查當前時間是否在此模式的排程內
        /// </summary>
        public bool IsInSchedule()
        {
            if (!Enabled) return false;
            if (Schedules == null || Schedules.Count == 0) return false;

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            var currentDay = now.DayOfWeek;

            foreach (var schedule in Schedules)
            {
                if (!schedule.Enabled) continue;

                // 檢查星期
                if (schedule.Days != null && schedule.Days.Count > 0 && !schedule.Days.Contains(currentDay))
                    continue;

                // 檢查時間
                if (schedule.StartTime <= schedule.EndTime)
                {
                    if (currentTime >= schedule.StartTime && currentTime <= schedule.EndTime)
                        return true;
                }
                else
                {
                    // 跨午夜
                    if (currentTime >= schedule.StartTime || currentTime <= schedule.EndTime)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 取得模式顯示文字
        /// </summary>
        public string GetDisplayText()
        {
            if (Schedules == null || Schedules.Count == 0)
                return "無排程";

            var enabledSchedules = Schedules.Where(s => s.Enabled).ToList();
            if (enabledSchedules.Count == 0)
                return "無排程";

            var texts = enabledSchedules.Select(s => s.GetDisplayText());
            return string.Join(", ", texts);
        }

        /// <summary>
        /// 複製模式配置
        /// </summary>
        public ModeConfig Clone()
        {
            return new ModeConfig
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Enabled = this.Enabled,
                Schedules = this.Schedules?.Select(s => s.Clone()).ToList() ?? new List<ModeSchedule>()
            };
        }
    }

    /// <summary>
    /// 模式排程
    /// </summary>
    public class ModeSchedule
    {
        public bool Enabled { get; set; } = true;
        public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(8);
        public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(17);
        public List<DayOfWeek> Days { get; set; } = new List<DayOfWeek>();

        /// <summary>
        /// 取得顯示文字
        /// </summary>
        public string GetDisplayText()
        {
            return StartTime.ToString(@"hh\:mm") + "-" + EndTime.ToString(@"hh\:mm");
        }

        /// <summary>
        /// 取得星期顯示文字
        /// </summary>
        public string GetDaysDisplayText()
        {
            if (Days == null || Days.Count == 0)
                return "每天";

            if (Days.Count == 7)
                return "每天";

            if (Days.Count == 5 && !Days.Contains(DayOfWeek.Saturday) && !Days.Contains(DayOfWeek.Sunday))
                return "平日 (週一至週五)";

            if (Days.Count == 2 && Days.Contains(DayOfWeek.Saturday) && Days.Contains(DayOfWeek.Sunday))
                return "週末";

            string[] dayNames = { "日", "一", "二", "三", "四", "五", "六" };
            var sortedDays = Days.OrderBy(d => (int)d).Select(d => "週" + dayNames[(int)d]);
            return string.Join(", ", sortedDays);
        }

        /// <summary>
        /// 複製排程
        /// </summary>
        public ModeSchedule Clone()
        {
            return new ModeSchedule
            {
                Enabled = this.Enabled,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                Days = this.Days != null ? new List<DayOfWeek>(this.Days) : new List<DayOfWeek>()
            };
        }
    }
}
