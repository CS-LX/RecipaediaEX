using System;
using Engine;
using Game;

namespace RecipaediaEX.Overlay {
    /// <summary>
    /// 长按 + 连放：连放开始后 1 秒内累计执行 40 组放置（二次缓入由慢至快）；
    /// 之后按 ramp 末端斜率继续加速（与 1s 时刻瞬时频率衔接）。
    /// </summary>
    sealed class PlacementLongPressRepeater {
        const float RampDuration = 1f;
        const int PlacementsInRampWindow = 40;

        bool m_repeatActive;
        double m_pressStartTime;
        int m_placementsDone;

        public bool RepeatActive => m_repeatActive;

        public void OnPressStart() {
            m_pressStartTime = Time.FrameStartTime;
            m_repeatActive = false;
            m_placementsDone = 0;
        }

        public void Reset() => m_repeatActive = false;

        /// <summary>按住期间按累计调度曲线重复调用 <paramref name="tryPlaceOnce"/>；返回 false 时停止连放。</summary>
        public bool UpdateWhilePressed(float holdDelaySeconds, Func<bool> tryPlaceOnce) {
            float holdDuration = (float)(Time.FrameStartTime - m_pressStartTime);
            if (holdDuration < holdDelaySeconds) return true;

            if (!m_repeatActive) {
                m_repeatActive = true;
                m_placementsDone = 0;
            }

            float repeatHold = holdDuration - holdDelaySeconds;
            int targetCount = GetTargetPlacementCount(repeatHold);
            while (m_placementsDone < targetCount) {
                if (!tryPlaceOnce()) {
                    m_repeatActive = false;
                    return false;
                }
                m_placementsDone++;
            }
            return true;
        }

        /// <summary>连放时刻 t 应完成的累计组数：前 1s 为 40·(t/1)²，之后 C¹ 延续。</summary>
        static int GetTargetPlacementCount(float repeatHoldSeconds) {
            if (repeatHoldSeconds <= 0f) return 0;

            float scheduled;
            if (repeatHoldSeconds <= RampDuration) {
                float u = repeatHoldSeconds / RampDuration;
                scheduled = PlacementsInRampWindow * u * u;
            }
            else {
                float slope = 2f * PlacementsInRampWindow / RampDuration;
                scheduled = PlacementsInRampWindow + slope * (repeatHoldSeconds - RampDuration);
            }
            return (int)Math.Floor(scheduled);
        }
    }
}
