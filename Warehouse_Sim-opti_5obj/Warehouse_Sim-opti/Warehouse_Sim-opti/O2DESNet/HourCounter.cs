﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace O2DESNet
{
    public interface IReadOnlyHourCounter
    {
        DateTime LastTime { get; }
        double LastCount { get; }
        bool Paused { get; }
        /// <summary>
        /// Total number of increment observed
        /// </summary>
        double TotalIncrement { get; }
        /// <summary>
        /// Total number of decrement observed
        /// </summary>
        double TotalDecrement { get; }
        double IncrementRate { get; }
        double DecrementRate { get; }
        /// <summary>
        /// Total number of hours since the initial time.
        /// </summary>
        double TotalHours { get; }
        double WorkingTimeRatio { get; }
        /// <summary>
        /// The cumulative count value on time in unit of hours
        /// </summary>
        double CumValue { get; }
        /// <summary>
        /// The average count on observation period
        /// </summary>
        double AverageCount { get; }
        /// <summary>
        /// Average timespan that a load stays in the activity, if it is a stationary process, 
        /// i.e., decrement rate == increment rate
        /// It is 0 at the initial status, i.e., decrement rate is NaN (no decrement observed).
        /// </summary>
        TimeSpan AverageDuration { get; }
        string LogFile { get; set; }
    }
    public interface IHourCounter : IReadOnlyHourCounter
    {
        void ObserveCount(double count, DateTime clockTime);
        void ObserveChange(double count, DateTime clockTime);
        void Pause();
        void Pause(DateTime clockTime);
        void Resume(DateTime clockTime);
    }
    public class ReadOnlyHourCounter : IReadOnlyHourCounter, IDisposable
    {
        public DateTime LastTime { get { return HourCounter.LastTime; } }

        public double LastCount { get { return HourCounter.LastCount; } }

        public bool Paused { get { return HourCounter.Paused; } }

        public double TotalIncrement { get { return HourCounter.TotalIncrement; } }

        public double TotalDecrement { get { return HourCounter.TotalDecrement; } }

        public double IncrementRate { get { return HourCounter.IncrementRate; } }

        public double DecrementRate { get { return HourCounter.DecrementRate; } }

        public double TotalHours { get { return HourCounter.TotalHours; } }

        public double WorkingTimeRatio { get { return HourCounter.WorkingTimeRatio; } }

        public double CumValue { get { return HourCounter.CumValue; } }

        public double AverageCount { get { return HourCounter.AverageCount; } }

        public TimeSpan AverageDuration { get { return HourCounter.AverageDuration; } }
        public string LogFile
        {
            get { return HourCounter.LogFile; }
            set { HourCounter.LogFile = value; }
        }

        private readonly HourCounter HourCounter;
        internal ReadOnlyHourCounter(HourCounter hourCounter)
        {
            HourCounter = hourCounter;
        }

        public void Dispose() { }
    }
    public class HourCounter : IHourCounter, IDisposable
    {
        private DateTime _initialTime;
        public DateTime LastTime { get; private set; }
        public double LastCount { get; private set; }
        /// <summary>
        /// Total number of increment observed
        /// </summary>
        public double TotalIncrement { get; private set; }
        /// <summary>
        /// Total number of decrement observed
        /// </summary>
        public double TotalDecrement { get; private set; }
        /// <summary>
        /// Total number of hours since the initial time.
        /// </summary>
        public double TotalHours { get; private set; }
        public double WorkingTimeRatio
        {
            get
            {
                if (LastTime == _initialTime) return 0;
                return TotalHours / (LastTime - _initialTime).TotalHours;
            }
        }
        /// <summary>
        /// The cumulative count value on time in unit of hours
        /// </summary>
        public double CumValue { get; private set; }
        /// <summary>
        /// The average count on observation period
        /// </summary>
        public double AverageCount { get { if (TotalHours == 0) return LastCount; return CumValue / TotalHours; } }
        /// <summary>
        /// Average timespan that a load stays in the activity, if it is a stationary process, 
        /// i.e., decrement rate == increment rate
        /// It is 0 at the initial status, i.e., decrement rate is NaN (no decrement observed).
        /// </summary>
        public TimeSpan AverageDuration
        {
            get
            {
                double hours = AverageCount / DecrementRate;
                if (double.IsNaN(hours) || double.IsInfinity(hours)) hours = 0;
                return TimeSpan.FromHours(hours);
            }
        }
        public bool Paused { get; private set; }

        #region For history keeping
        private Dictionary<DateTime, double> _history;
        public bool KeepHistory { get; private set; }
        /// <summary>
        /// Scatter points of (time in hours, count)
        /// </summary>
        public List<Tuple<double, double>> History
        {
            get
            {
                if (!KeepHistory) return null;
                return _history.OrderBy(i => i.Key).Select(i => new Tuple<double, double>((i.Key - _initialTime).TotalHours, i.Value)).ToList();
            }
        }
        #endregion

        internal HourCounter(bool keepHistory = false) { Init(DateTime.MinValue, keepHistory); }
        internal HourCounter(DateTime initialTime, bool keepHistory = false) { Init(initialTime, keepHistory); }
        private void Init(DateTime initialTime, bool keepHistory)
        {
            _initialTime = initialTime;
            LastTime = initialTime;
            LastCount = 0;
            TotalIncrement = 0;
            TotalDecrement = 0;
            TotalHours = 0;
            CumValue = 0;
            KeepHistory = keepHistory;
            if (KeepHistory) _history = new Dictionary<DateTime, double>();
        }
        public void ObserveCount(double count, DateTime clockTime)
        {
            if (clockTime < LastTime)
                throw new Exception("Time of new count cannot be earlier than current time.");
            if (!Paused)
            {
                var hours = (clockTime - LastTime).TotalHours;
                TotalHours += hours;
                CumValue += hours * LastCount;
                if (count > LastCount) TotalIncrement += count - LastCount;
                else TotalDecrement += LastCount - count;

                if (!HoursForCount.ContainsKey(LastCount)) HoursForCount.Add(LastCount, 0);
                HoursForCount[LastCount] += hours;
            }
            if (_logFile != null)
            {                
                using (var sw = new StreamWriter(_logFile, append: true))
                {
                    sw.Write("{0},{1}", TotalHours, LastCount);
                    if (Paused) sw.Write(",Paused");
                    sw.WriteLine();
                    if (count != LastCount)
                    {
                        sw.Write("{0},{1}", TotalHours, count);
                        if (Paused) sw.Write(",Paused");
                        sw.WriteLine();
                    }
                };
            }
            LastTime = clockTime;
            LastCount = count;
            if (KeepHistory) _history[clockTime] = count;
        }
        public void ObserveChange(double change, DateTime clockTime) { ObserveCount(LastCount + change, clockTime); }
        public void Pause() { Pause(LastTime); }
        public void Pause(DateTime clockTime)
        {
            if (Paused) return;
            ObserveChange(0, clockTime);
            Paused = true;
            if (_logFile != null)
            {
                using (var sw = new StreamWriter(_logFile, append: true))
                {
                    sw.WriteLine("{0},{1},Paused", TotalHours, LastCount);
                };
            }
        }
        public void Resume(DateTime clockTime)
        {
            if (!Paused) return;
            LastTime = clockTime;
            Paused = false;
            if (_logFile != null)
            {
                using (var sw = new StreamWriter(_logFile, append: true))
                {
                    sw.WriteLine("{0},{1},Paused", TotalHours, LastCount);
                    sw.WriteLine("{0},{1}", TotalHours, LastCount);
                };
            }
        }

        public double IncrementRate { get { return TotalIncrement / TotalHours; } }
        public double DecrementRate { get { return TotalDecrement / TotalHours; } }
        internal void WarmedUp(DateTime clockTime)
        {
            // all reset except the last count
            _initialTime = clockTime;
            LastTime = clockTime;
            TotalIncrement = 0;
            TotalDecrement = 0;
            TotalHours = 0;
            CumValue = 0;
            HoursForCount = new Dictionary<double, double>();
        }

        public Dictionary<double, double> HoursForCount = new Dictionary<double, double>();
        private void SortHoursForCount() { HoursForCount = HoursForCount.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value); }
        /// <summary>
        /// Get the percentile of count values on time, i.e., the count value that with x-percent of time the observation is not higher than it.
        /// </summary>
        /// <param name="ratio">values between 0 and 100</param>
        public double Percentile(double ratio)
        {
            SortHoursForCount();
            var threashold = HoursForCount.Sum(i => i.Value) * ratio / 100;
            foreach (var i in HoursForCount)
            {
                threashold -= i.Value;
                if (threashold <= 0) return i.Key;
            }
            return double.PositiveInfinity;
        }
        /// <summary>
        /// Statistics for the amount of time spent at each range of count values
        /// </summary>
        /// <param name="countInterval">width of the count value interval</param>
        /// <returns>A dictionary map from [the lowerbound value of each interval] to the array of [total hours observed], [probability], [cumulated probability]</returns>
        public Dictionary<double, double[]> Histogram(double countInterval) // interval -> { observation, probability, cumulative probability}
        {
            SortHoursForCount();
            var histogram = new Dictionary<double, double[]>();
            if (HoursForCount.Count > 0)
            {
                double countLb = 0;
                double cumHours = 0;
                foreach (var i in HoursForCount)
                {
                    if (i.Key > countLb + countInterval || i.Equals(HoursForCount.Last()))
                    {
                        if (cumHours > 0) histogram.Add(countLb, new double[] { cumHours, 0, 0 });
                        countLb += countInterval;
                        cumHours = i.Value;
                    }
                    else
                    {
                        cumHours += i.Value;
                    }
                }
            }
            var sum = histogram.Sum(h => h.Value[0]);
            double cum = 0;
            foreach (var h in histogram)
            {
                cum += h.Value[0];
                h.Value[1] = h.Value[0] / sum; // probability
                h.Value[2] = cum / sum; // cum. prob.
            }
            return histogram;
        }

        private string _logFile;
        public string LogFile
        {
            get { return _logFile; }
            set
            {
                _logFile = value;
                if (_logFile != null)
                    using (var sw = new StreamWriter(_logFile))
                    {
                        sw.WriteLine("Hours,Count,Remark");
                        sw.WriteLine("{0},{1}", TotalHours, LastCount);
                    };
            }
        }
        
        private ReadOnlyHourCounter ReadOnly { get; set; } = null;
        public ReadOnlyHourCounter AsReadOnly()
        {
            if (ReadOnly == null) ReadOnly = new ReadOnlyHourCounter(this);
            return ReadOnly;
        }

        public void Dispose() { }
    }
}
