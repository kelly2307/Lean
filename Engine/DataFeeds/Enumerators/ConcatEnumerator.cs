/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
*/

using System.Collections;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Util;

namespace QuantConnect.Lean.Engine.DataFeeds.Enumerators
{
    /// <summary>
    /// 
    /// </summary>
    public class ConcatEnumerator : IEnumerator<BaseData>
    {
        private readonly IEnumerator<BaseData> _firstEnumerator;
        private readonly IEnumerator<BaseData> _secondEnumerator;
        private readonly bool _skipDuplicateEndTimes;

        /// <summary>
        /// 
        /// </summary>
        public BaseData Current { get; set; }
        object IEnumerator.Current => Current;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstEnumerator"></param>
        /// <param name="secondEnumerator"></param>
        /// <param name="skipDuplicateEndTimes"></param>
        public ConcatEnumerator(IEnumerator<BaseData> firstEnumerator,
            IEnumerator<BaseData> secondEnumerator,
            bool skipDuplicateEndTimes)
        {
            _firstEnumerator = firstEnumerator;
            _secondEnumerator = secondEnumerator;
            _skipDuplicateEndTimes = skipDuplicateEndTimes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            while (_firstEnumerator.MoveNext())
            {
                Current = _firstEnumerator.Current;
                return true;
            }

            var lastEndTime = Current?.EndTime;

            while (_secondEnumerator.MoveNext())
            {
                if (_skipDuplicateEndTimes
                    && lastEndTime.HasValue
                    && _secondEnumerator.Current != null
                    && _secondEnumerator.Current.EndTime <= lastEndTime)
                {
                    continue;
                }

                Current = _secondEnumerator.Current;
                return true;
            }

            Current = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _firstEnumerator.DisposeSafely();
            _secondEnumerator.DisposeSafely();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            _firstEnumerator.Reset();
            _secondEnumerator.Reset();
        }
    }
}
