using System.Numerics;

namespace CombinableProgress
{
    /// <summary>
    /// Combines different <see cref="Progress{T}"/> objects into one.
    /// </summary>
    /// <remarks>
    /// Length can not overflow largest possible value of <see cref="{T}"/>
    /// </remarks>
    /// <typeparam name="T">T hast to be a number</typeparam>
    public class CombinableProgress<T> : Progress<T> where T : INumber<T>
    {
        private readonly List<Progress<T>> _progressors = new();
        private readonly List<T> _values = new();
        private readonly object _lock = new();

        /// <summary>
        /// Read-only property describing how many <see cref="Progress{T}"/> are in this <see cref="CombinableProgress{T}"/>.
        /// </summary>
        public int Count => _progressors.Count;

        /// <summary>
        /// Initializes the <see cref="CombinableProgress{T}"/>.
        /// </summary>
        public CombinableProgress() { }

        /// <summary>Initializes the <see cref="CombinableProgress{T}"/> with the specified callback.</summary>
        /// <param name="handler">
        /// A handler to invoke for each reported progress value.  This handler will be invoked
        /// in addition to any delegates registered with the <see cref="ProgressChanged"/> event.
        /// Depending on the <see cref="System.Threading.SynchronizationContext"/> instance captured by
        /// the <see cref="Progress{T}"/> at construction, it's possible that this handler instance
        /// could be invoked concurrently with itself.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="handler"/> is null.</exception>
        public CombinableProgress(Action<T> handler) : base(handler) { }

        /// <summary>
        /// Tries to attach an element to the progress.
        /// </summary>
        /// <param name="progress"><see cref="Progress{T}"/> to attach</param>
        /// <returns>bool which indicates success</returns>
        public bool TryAttach(Progress<T> progress)
        {
            if (T.CreateTruncating(_progressors.Count) != T.CreateSaturating(_progressors.Count))
                return false;
            lock (_lock)
            {
                if (_progressors.Contains(progress))
                    return true;
                _progressors.Add(progress);
                _values.Add(T.Zero);
            }
            progress.ProgressChanged += OnProgressChanged;
            return true;
        }

        /// <summary>
        /// Try's to remove a progress if it was attached in available 
        /// </summary>
        /// <param name="progress"><see cref="Progress{T}"/> to attach</param>
        /// <returns>bool which indicates success</returns>
        public bool TryRemove(Progress<T> progress)
        {
            lock (_lock)
            {
                int index = _progressors.FindIndex(x => x == progress);
                if (index == -1)
                    return false;
                _progressors.RemoveAt(index);
                _values.RemoveAt(index);
            }
            progress.ProgressChanged -= OnProgressChanged;
            return true;
        }

        /// <summary>
        /// Removes all attatched <see cref="Progress{T}"/> objects
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                foreach (Progress<T> progress in _progressors)
                    progress.ProgressChanged -= OnProgressChanged;
                _progressors.Clear();
                _values.Clear();
            }
        }

        /// <summary>
        /// Attach an element to the progress. If this does not work an overflow exception is thrown.
        /// </summary>
        /// <param name="progress"><see cref="Progress{T}"/> to attach</param>
        /// <exception cref="OverflowException">Throws Exception when the length is bigger than the largest possible value of <see cref="T"/></exception>
        public void Attach(Progress<T> progress)
        {
            if (!TryAttach(progress))
                throw new OverflowException($"The length of {GetType()} would exceed the largest possible value of {T.One.GetType()}");
        }

        /// <summary>Reports a progress change to all progressors attached.</summary>
        /// <param name="value">The value of the updated progress.</param>
        public void ReportToAll(T value)
        {
            lock (_lock)
                _progressors.ForEach(x => ((IProgress<T>)x).Report(value));
        }

        /// <summary>
        /// Will be called if one Progress changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProgressChanged(object? sender, T e)
        {
            T average = T.Zero;
            T remain = T.Zero;

            lock (_lock)
            {
                T n = T.CreateSaturating(_progressors.Count);
                for (int i = 0; i < _progressors.Count; i++)
                {
                    if (ReferenceEquals(_progressors[i], sender))
                        _values[i] = e;
                    average += _values[i] / n;
                    remain += _values[i] % n;
                    average += remain / n;
                    remain %= n;
                }
            }
            OnReport(average);
        }
    }
}
