namespace ScreenLab.Screen
{
    public abstract class AnalyzerBase<T>
    {
        public string Name { get; init; }
        public abstract T DefaultValue { get;}
        public T Selection { get => _selection; }

        protected T _selection;

        private static Dictionary<string, Func<T, T>> _selectionOverrides = new();
        public AnalyzerBase(string name)
        {
            Name = name;
            _selection = DefaultValue;
        }
        protected AnalyzerBase(string name, T selection) : this(name)
        {
            _selection = selection;
        }

        /// <summary>
        /// Upsert an override
        /// </summary>
        public static void ProvideOverride(string name, Func<T, T> selectionOverride)
        {
                _selectionOverrides[name] = selectionOverride;
        }
        /// <summary>
        /// Upsert overrides
        /// </summary>
        public static void ProvideOverride(Dictionary<string, Func<T, T>> selectionOverrides) 
        {
            foreach(var kvp in selectionOverrides)
            {
                ProvideOverride(kvp.Key, kvp.Value);
            }
        } 
        public static void ClearOverrides() => _selectionOverrides.Clear();
        public abstract void BeginSelection(Action<T>? callback = null);
        protected void SetSelection(T selection) => _selection = OverrideSelection(Name, selection);

        protected T OverrideSelection(string name, T original)
        {
            foreach(var o in _selectionOverrides)
            {
                if (o.Key == name)
                    return o.Value(original);
            }
            return original;
        }
    }
}
