namespace ScreenLab.Screen
{
    public abstract class CaptureBase<T>
    {
        protected readonly string _storagePath;
        protected readonly string LOCAL_GROUP_NAME;
        public const string GLOBAL_GROUP_NAME = "Global";
        public abstract T DefaultValue { get; }
        protected abstract string Title { get; }

        // Original local/global values are saved to file
        protected Dictionary<string, T> _localOriginalValues = new();
        protected Dictionary<string, T> _globalOriginalValues = new();
        // Overriden local/global values are retrieved publicly for processing
        private Dictionary<string, T> _localOverridenValues = new();
        private Dictionary<string, T> _globalOverridenValues = new();
        public Dictionary<string, T> LocalValues { 
            get => new(_localOverridenValues); 
        }
        public Dictionary<string, T> GlobalValues {
            get => new(_globalOverridenValues);
        }

        protected CaptureBase(string localName, string storagePath, params string[] localValueKeys)
        {
            this.LOCAL_GROUP_NAME = localName;
            _storagePath = storagePath;
            if(Path.GetDirectoryName(_storagePath) is string directory)
                Directory.CreateDirectory(directory);

            _localOriginalValues = LoadFromFile(LOCAL_GROUP_NAME, storagePath);
            _globalOriginalValues = LoadFromFile(GLOBAL_GROUP_NAME, storagePath);

            foreach (var key in localValueKeys)
            {
                if (!_localOriginalValues.ContainsKey(key))
                    _localOriginalValues[key] = DefaultValue;
            }

            _localOverridenValues = OverrideLoadedValues(_localOriginalValues);
            _globalOverridenValues = OverrideLoadedValues(_globalOriginalValues);
        }

        protected abstract Dictionary<string, T> DeserializeValues(string content);
        protected abstract string SerializeValues(Dictionary<string, T> values);
        protected virtual Dictionary<string, T> LoadFromFile(string localGroupName, string filePath)
        {
            if (!File.Exists(filePath))
                return new();

            try
            {
                var allValues = DeserializeValues(File.ReadAllText(filePath));
                return allValues
                    .Where(kvp => kvp.Key.StartsWith($"{localGroupName}_"))
                    .ToDictionary(kvp => kvp.Key.Replace($"{localGroupName}_", ""), kvp => kvp.Value);
            }
            catch
            {
                return new();
            }
        }
        protected virtual Dictionary<string, T> OverrideLoadedValues(Dictionary<string, T> original) => original;

        public void SaveValues()
        {
            string filePath = _storagePath;

            var allValues = new Dictionary<string, T>();

            // Load existing data (if any)
            if (File.Exists(filePath))
            {
                try
                {
                    allValues = DeserializeValues(File.ReadAllText(filePath));
                }
                catch { allValues = new(); }
            }

            // Remove current local/global values
            var keysToRemove = allValues.Keys
                .Where(k => k.StartsWith($"{LOCAL_GROUP_NAME}_") || k.StartsWith($"{GLOBAL_GROUP_NAME}_"))
                .ToList();
            foreach (var key in keysToRemove)
                allValues.Remove(key);

            // Add current local/global original values
            foreach (var entry in _localOriginalValues)
                allValues[$"{LOCAL_GROUP_NAME}_{entry.Key}"] = entry.Value;

            foreach (var entry in _globalOriginalValues)
                allValues[$"{GLOBAL_GROUP_NAME}_{entry.Key}"] = entry.Value;

            File.WriteAllText(filePath, SerializeValues(allValues));
        }
        // Adds or overwrites existing value
        public void StoreLocalValue(string name, T value)
        {
            _localOriginalValues[name] = value;
            _localOverridenValues = OverrideLoadedValues(_localOriginalValues);
        }

        // Adds or overwrites existing value
        public void StoreGlobalValue(string name, T value)
        {
            _globalOriginalValues[name] = value;
            _globalOverridenValues = OverrideLoadedValues(_globalOriginalValues);
        }
        public bool TryGetGlobalValue(string name, out T value)
        {
            if (_globalOverridenValues.TryGetValue(name, out value) && !value.Equals(DefaultValue))
                return true;
            else
                return false;
        }
        public bool TryGetLocalValue(string name, out T value)
        {
            if (_localOverridenValues.TryGetValue(name, out value) && !value.Equals(DefaultValue))
                return true;
            else
                return false;
        }
    }
}
