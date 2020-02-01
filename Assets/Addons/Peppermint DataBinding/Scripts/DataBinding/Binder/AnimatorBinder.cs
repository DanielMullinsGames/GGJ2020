using UnityEngine;
using UnityEngine.Assertions;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// Bind the specified parameter of Animator to a property.
    ///
    /// It supports bool, int, float and trigger parameter types. Parameter name is the associated
    /// parameter name in animator. If the parameter type is trigger, the source type is
    /// AnimatorTrigger.
    ///
    /// Animator will reset its parameters after deactivated, the AnimatorBinder will restore the
    /// parameter in OnEnable method. Trigger is also restore if the event is called within the max
    /// period.
    /// </summary>
    [Binder]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Peppermint/Data Binding/Binder/Component/Animator Binder")]
    public class AnimatorBinder : MonoBehaviour
    {
        public enum ParameterType
        {
            Bool,
            Int,
            Float,
            Trigger,
        }

        public string path;
        public ParameterType parameterType;
        public string parameterName;

        private Animator animator;
        private Binding binding;
        private IDataContext dataContext;
        private bool pendingTriggerFlag;
        private int lastFrameCount;

        private AnimatorTrigger triggerValue;
        private bool boolValue;
        private int intValue;
        private float floatValue;

        private static bool enableDebug = false;
        public const int MaxRestoreFrameCount = 1;

        public bool BoolValue
        {
            set
            {
                boolValue = value;

                SetParameter();
            }
        }

        public int IntValue
        {
            set
            {
                intValue = value;

                SetParameter();
            }
        }

        public float FloatValue
        {
            set
            {
                floatValue = value;

                SetParameter();
            }
        }

        public AnimatorTrigger TriggerValue
        {
            set
            {
                if (triggerValue != null)
                {
                    // remove from current
                    triggerValue.SetEvent -= SetAnimatorTrigger;
                    triggerValue.ResetEvent -= ResetAnimatorTrigger;
                }

                // save it
                triggerValue = value;

                if (triggerValue != null)
                {
                    // add new listener
                    triggerValue.SetEvent += SetAnimatorTrigger;
                    triggerValue.ResetEvent += ResetAnimatorTrigger;
                }
            }
        }

        void Start()
        {
            // get animator
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Require Animator Component", gameObject);
                return;
            }

            CreateBinding();
        }

        void OnDestroy()
        {
            BindingUtility.RemoveBinding(binding, dataContext);
        }

        void OnEnable()
        {
            if (binding != null && binding.IsBound)
            {
                RestoreParameter();
            }
        }

        private void CreateBinding()
        {
            switch (parameterType)
            {
                case ParameterType.Bool:
                    binding = new Binding(path, this, "BoolValue");
                    break;

                case ParameterType.Int:
                    binding = new Binding(path, this, "IntValue");
                    break;

                case ParameterType.Float:
                    binding = new Binding(path, this, "FloatValue");
                    break;

                case ParameterType.Trigger:
                    binding = new Binding(path, this, "TriggerValue", Binding.BindingMode.OneWay, Binding.ConversionMode.None, null);
                    binding.SetFlags(Binding.ControlFlags.ResetTargetValue);
                    break;

                default:
                    Debug.LogError(string.Format("Unknown parameter type {0}", parameterType), gameObject);
                    break;
            }

            BindingUtility.AddBinding(binding, transform, out dataContext);
        }

        private void RestoreParameter()
        {
            if (parameterType == ParameterType.Trigger)
            {
                // check flag first
                if (pendingTriggerFlag)
                {
                    // check elapsed time
                    var validTime = (Time.frameCount - lastFrameCount) <= MaxRestoreFrameCount;

                    if (validTime)
                    {
                        animator.SetTrigger(parameterName);
                    }

                    if (enableDebug)
                    {
                        Debug.Log(string.Format("Restore trigger, sourceName={0}, validTime={1}", parameterName, validTime), gameObject);
                    }
                }

                pendingTriggerFlag = false;
            }
            else
            {
                if (enableDebug)
                {
                    Debug.Log(string.Format("Restore parameter, type={0}, sourceName={1}", parameterType, parameterName), gameObject);
                }

                SetParameter();
            }
        }

        private void SetParameter()
        {
            if (!animator.isInitialized)
            {
                if (enableDebug)
                {
                    Debug.LogWarning("SetParameter on uninitialized animator", gameObject);
                }

                return;
            }

            switch (parameterType)
            {
                case ParameterType.Bool:
                    animator.SetBool(parameterName, boolValue);
                    break;
                case ParameterType.Int:
                    animator.SetInteger(parameterName, intValue);
                    break;
                case ParameterType.Float:
                    animator.SetFloat(parameterName, floatValue);
                    break;
                default:
                    break;
            }
        }

        private void SetAnimatorTrigger()
        {
            Assert.IsTrue(parameterType == ParameterType.Trigger);

            // reset pending flag
            pendingTriggerFlag = false;

            if (!animator.isInitialized)
            {
                if (enableDebug)
                {
                    Debug.LogWarning(string.Format("SetAnimatorTrigger {0} on uninitialized animator", parameterName), gameObject);
                }

                // save frame count
                lastFrameCount = Time.frameCount;

                // mark flag
                pendingTriggerFlag = true;

                return;
            }

            animator.SetTrigger(parameterName);
        }

        private void ResetAnimatorTrigger()
        {
            Assert.IsTrue(parameterType == ParameterType.Trigger);

            // reset pending flag
            pendingTriggerFlag = false;

            if (!animator.isInitialized)
            {
                if (enableDebug)
                {
                    Debug.LogWarning(string.Format("ResetAnimatorTrigger {0} on uninitialized animator", parameterName), gameObject);
                }

                return;
            }

            animator.ResetTrigger(parameterName);
        }

    }
}
