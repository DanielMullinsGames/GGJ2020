using UnityEngine;

namespace Peppermint.DataBinding.Example
{
    /// <summary>
    /// This example demonstrates how to bind views to different classes.
    ///
    /// In this demo, three different shapes inherit from BindableObject, BindableMonoBehaviour and
    /// BindableScriptableObject respectively. They all have properties that are compatible with the
    /// view, so they can be bound to the view.
    ///
    /// You can create custom bindable base class by implementing the INotifyPropertyChanged
    /// interface.
    /// </summary>
    public class BindableClassExample : BindableMonoBehaviour
    {
        /// <summary>
        /// By default setting the data binding will not reset the view when unbinding the source.
        /// The DummyShape is used to reset the view to specified value. Notice that the nullable
        /// types are used for the ID and Color, so the UI.Text will be set to an empty string.
        /// </summary>
        public class DummyShape
        {
            [BindableProperty]
            public int? ID { get { return null; } }

            [BindableProperty]
            public Color? Color { get { return null; } }

            public string Description { get { return null; } }
        }

        public enum ShapeType
        {
            None,
            Circle,
            Rectangle,
            Triangle,
        }

        public Circle circle;
        public Rectangle rectangle;
        public Triangle triangle;
        public bool enableResetView = true;

        private DummyShape dummyShape;
        private object currentShape;

        private ShapeType currentShapeType;
        private ICommand removeSourceCommand;
        private ICommand setCircleCommand;
        private ICommand setRectangleCommand;
        private ICommand setTriangleCommand;

        private const string ShapeSourceName = "Shape";

        #region Bindable Properties

        public ShapeType CurrentShapeType
        {
            get { return currentShapeType; }
            set { SetProperty(ref currentShapeType, value, "CurrentShapeType"); }
        }

        public ICommand RemoveSourceCommand
        {
            get { return removeSourceCommand; }
            set { SetProperty(ref removeSourceCommand, value, "RemoveSourceCommand"); }
        }

        public ICommand SetCircleCommand
        {
            get { return setCircleCommand; }
            set { SetProperty(ref setCircleCommand, value, "SetCircleCommand"); }
        }

        public ICommand SetRectangleCommand
        {
            get { return setRectangleCommand; }
            set { SetProperty(ref setRectangleCommand, value, "SetRectangleCommand"); }
        }

        public ICommand SetTriangleCommand
        {
            get { return setTriangleCommand; }
            set { SetProperty(ref setTriangleCommand, value, "SetTriangleCommand"); }
        }

        #endregion

        void Start()
        {
            // create dummy shape
            dummyShape = new DummyShape();

            // create commands
            removeSourceCommand = new DelegateCommand(RemoveSource, () => currentShape != null);
            setCircleCommand = new DelegateCommand(() => SetSource(ShapeType.Circle));
            setRectangleCommand = new DelegateCommand(() => SetSource(ShapeType.Rectangle));
            setTriangleCommand = new DelegateCommand(() => SetSource(ShapeType.Triangle));

            BindingManager.Instance.AddSource(this, typeof(BindableClassExample).Name);
        }

        void OnDestroy()
        {
            BindingManager.Instance.RemoveSource(this);

            if (currentShape != null)
            {
                // remove shape
                BindingManager.Instance.RemoveSource(currentShape);
            }
        }

        public void SetSource(ShapeType shapeType)
        {
            if (currentShape != null)
            {
                BindingManager.Instance.RemoveSource(currentShape);
                currentShape = null;
            }

            // set current shape
            switch (shapeType)
            {
                case ShapeType.Circle:
                    currentShape = circle;
                    break;
                case ShapeType.Rectangle:
                    currentShape = rectangle;
                    break;
                case ShapeType.Triangle:
                    currentShape = triangle;
                    break;
                default:
                    Debug.LogError("Unknown shape type");
                    return;
            }

            CurrentShapeType = shapeType;
            BindingManager.Instance.AddSource(currentShape, ShapeSourceName);
        }

        public void RemoveSource()
        {
            if (currentShape == null)
            {
                return;
            }

            BindingManager.Instance.RemoveSource(currentShape);
            currentShape = null;
            CurrentShapeType = ShapeType.None;

            if (enableResetView)
            {
                // reset the view
                BindingManager.Instance.AddSource(dummyShape, ShapeSourceName);
                BindingManager.Instance.RemoveSource(dummyShape);
            }
        }
    }
}
