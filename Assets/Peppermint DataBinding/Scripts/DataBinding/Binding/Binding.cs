using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Basic binding for single property of object.
    /// </summary>
    public class Binding : IBinding
    {
        public enum BindingMode
        {
            OneWay = 0,
            TwoWay,
            OneWayToSource,
        }

        public enum ConversionMode
        {
            None = 0,
            Parameter,
            Automatic,
        }

        [Flags]
        public enum ControlFlags
        {
            None = 0,
            AutoMatchConverter = 1,
            ResetSourceValue = 2,
            ResetTargetValue = 4,
        }

        protected object source;
        protected string sourcePath;
        protected object target;
        protected string targetPath;
        protected string sourcePropertyName;

        protected BindingMode mode;
        protected ControlFlags flags;
        protected IValueConverter converter;

        protected PropertyAccessor sourceProperty;
        protected PropertyAccessor targetProperty;

        public bool IsBound
        {
            get { return source != null; }
        }

        public object Source
        {
            get { return source; }
        }

        public ControlFlags Flags
        {
            get { return flags; }
        }

        public Binding(string sourcePath, object target, string targetPath)
            : this(sourcePath, target, targetPath, BindingMode.OneWay, ConversionMode.Automatic, null)
        {

        }

        public Binding(string sourcePath, object target, string targetPath, BindingMode mode)
            : this(sourcePath, target, targetPath, mode, ConversionMode.Automatic, null)
        {

        }

        public Binding(string sourcePath, object target, string targetPath, BindingMode mode, ConversionMode conversionMode, IValueConverter converter)
        {
            if (target == null)
            {
                Debug.LogError("target is null");
                return;
            }

            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("sourcePath is null", target as UnityEngine.Object);
                return;
            }

            if (string.IsNullOrEmpty(targetPath))
            {
                Debug.LogError("targetPath is null", target as UnityEngine.Object);
                return;
            }

            // handle nested path
            var bindingTarget = BindingUtility.GetBindingObject(target, targetPath);
            var targetPropertyName = BindingUtility.GetPropertyName(targetPath);
            var targetType = bindingTarget.GetType();

            // get target property accessor
            var targetProperty = TypeCache.Instance.GetPropertyAccessor(targetType, targetPropertyName);
            if (targetProperty == null)
            {
                Debug.LogError(string.Format("Invalid target path {0}", targetPath), target as UnityEngine.Object);
                return;
            }

            // check conversion mode
            if (conversionMode == ConversionMode.Parameter && converter == null)
            {
                Debug.LogError("Converter is null", target as UnityEngine.Object);
                return;
            }

            // set fields
            this.mode = mode;
            this.sourcePath = sourcePath;
            this.sourcePropertyName = BindingUtility.GetPropertyName(sourcePath);
            this.target = bindingTarget;
            this.targetPath = targetPropertyName;
            this.targetProperty = targetProperty;

            // setup converter
            if (conversionMode == ConversionMode.Parameter)
            {
                // use specified converter
                this.converter = converter;
            }
            else if (conversionMode == ConversionMode.Automatic)
            {
                // set flag
                flags = ControlFlags.AutoMatchConverter;
            }
        }

        public void Bind(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source is null");
            }

            if (IsBound)
            {
                Unbind();
            }

            // handle nested property
            var bindingSource = BindingUtility.GetBindingObject(source, sourcePath);
            if (bindingSource == null)
            {
                Debug.LogError("bindingSource is null", target as UnityEngine.Object);
                return;
            }

            // get property info
            var sourceType = bindingSource.GetType();
            var sourceProperty = TypeCache.Instance.GetPropertyAccessor(sourceType, sourcePropertyName);
            if (sourceProperty == null)
            {
                Debug.LogError(string.Format("Invalid source path {0}", sourcePath), target as UnityEngine.Object);
                return;
            }

            // set source
            this.source = bindingSource;
            this.sourceProperty = sourceProperty;

            if (mode == BindingMode.OneWay || mode == BindingMode.TwoWay)
            {
                var notifyInterface = this.source as INotifyPropertyChanged;
                if (notifyInterface != null)
                {
                    // add event handler
                    notifyInterface.PropertyChanged += OnSourcePropertyChanged;
                }
            }

            if (mode == BindingMode.OneWayToSource || mode == BindingMode.TwoWay)
            {
                var notifyInterface = this.target as INotifyPropertyChanged;
                if (notifyInterface != null)
                {
                    // add event handler
                    notifyInterface.PropertyChanged += OnTargetPropertyChanged;
                }
            }

            if ((flags & ControlFlags.AutoMatchConverter) != 0)
            {
                // get converter
                converter = ValueConverterProvider.Instance.MatchConverter(sourceProperty.PropertyType, targetProperty.PropertyType);
            }

            InitValue();
        }

        public void Unbind()
        {
            if (!IsBound)
            {
                return;
            }

            if (mode == BindingMode.OneWay || mode == BindingMode.TwoWay)
            {
                var notifyInterface = source as INotifyPropertyChanged;
                if (notifyInterface != null)
                {
                    // remove event handler
                    notifyInterface.PropertyChanged -= OnSourcePropertyChanged;
                }
            }

            if (mode == BindingMode.OneWayToSource || mode == BindingMode.TwoWay)
            {
                var notifyInterface = target as INotifyPropertyChanged;
                if (notifyInterface != null)
                {
                    // remove event handler
                    notifyInterface.PropertyChanged -= OnTargetPropertyChanged;
                }
            }

            ResetValue();

            if ((flags & ControlFlags.AutoMatchConverter) != 0)
            {
                // clear converter
                converter = null;
            }

            source = null;
            sourceProperty = null;
        }

        public void UpdateTarget()
        {
            if (!IsBound)
            {
                Debug.LogWarning("Unbound");
                return;
            }

            var value = sourceProperty.GetValue(source);

            // use converter
            if (converter != null)
            {
                value = converter.Convert(value, targetProperty.PropertyType);
            }

            try
            {
                // set value to target
                targetProperty.SetValue(target, value);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Failed to set value to target. sourceType={0}, targetType={1}, exception={2}",
                    sourceProperty.PropertyType, targetProperty.PropertyType, ex.Message), target as UnityEngine.Object);
            }
        }

        public void UpdateSource()
        {
            if (!IsBound)
            {
                Debug.LogWarning("Unbound");
                return;
            }

            var value = targetProperty.GetValue(target);

            // use converter
            if (converter != null)
            {
                value = converter.ConvertBack(value, sourceProperty.PropertyType);
            }

            try
            {
                // set value to source
                sourceProperty.SetValue(source, value);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Failed to set value to source. sourceType={0}, targetType={1}, exception={2}",
                    sourceProperty.PropertyType, targetProperty.PropertyType, ex.Message), target as UnityEngine.Object);
            }
        }

        public void HandleSourcePropertyChanged(object sender, string propertyName)
        {
            OnSourcePropertyChanged(sender, propertyName);
        }

        public void SetFlags(ControlFlags value)
        {
            flags |= value;
        }

        public void ClearFlags(ControlFlags value)
        {
            flags &= (~value);
        }

        protected void InitValue()
        {
            switch (mode)
            {
                case BindingMode.OneWay:
                    UpdateTarget();
                    break;

                case BindingMode.TwoWay:
                    UpdateTarget();
                    break;

                case BindingMode.OneWayToSource:
                    UpdateSource();
                    break;

                default:
                    Debug.LogErrorFormat("Invalid mode {0}", mode);
                    break;
            }
        }

        protected void ResetValue()
        {
            if (mode == BindingMode.OneWay)
            {
                if ((flags & ControlFlags.ResetTargetValue) != 0)
                {
                    targetProperty.SetValue(target, null);
                }
            }
            else if (mode == BindingMode.TwoWay)
            {
                if ((flags & ControlFlags.ResetTargetValue) != 0)
                {
                    targetProperty.SetValue(target, null);
                }
                if ((flags & ControlFlags.ResetSourceValue) != 0)
                {
                    sourceProperty.SetValue(source, null);
                }
            }
            else if (mode == BindingMode.OneWayToSource)
            {
                if ((flags & ControlFlags.ResetSourceValue) != 0)
                {
                    sourceProperty.SetValue(source, null);
                }
            }
            else
            {
                Debug.LogErrorFormat("Invalid mode {0}", mode);
            }
        }

        protected void OnSourcePropertyChanged(object sender, string propertyName)
        {
            Assert.IsTrue(IsBound);

            if (sender != source)
            {
                Debug.LogWarningFormat("Invalid sender {0}:{1}", sender, sender.GetHashCode());
                return;
            }

            if (propertyName != null && sourcePropertyName != propertyName)
            {
                // ignore invalid source path
                return;
            }

            UpdateTarget();
        }

        protected void OnTargetPropertyChanged(object sender, string propertyName)
        {
            Assert.IsTrue(IsBound);

            if (sender != target)
            {
                Debug.LogWarningFormat("Invalid sender {0}:{1}", sender, sender.GetHashCode());
                return;
            }

            if (propertyName != null && targetPath != propertyName)
            {
                // ignore invalid target path
                return;
            }

            UpdateSource();
        }
    }
}

