using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kilt.EasyRopes2D
{

    public enum RopeBreakActionEnum { Nothing, DestroySelf }
    public enum JointColliderEnum { Circle, Box }
    public enum EditorRopeStyleEnum { Gizmos, NodeObjects }
    public enum TensionHelperAddOptionEnum { AddInFirstAndLastNode, AddInPluggedObjects }
    public enum RigidbodyCollisionDetectionTypeEnum { Discrete, Continuous }

    [SelectionBase, ExecuteInEditMode]
    public class Rope2D : MonoBehaviour
    {
        #region Events

        public System.Action<Rope2D> OnRopeDestroyed;
        public System.Action<Rope2D> OnRopeCreated;
        public System.Action<Rope2D, int> OnRopeBreak; //Parameter = Node Index
        public System.Action<Rope2D, int> OnRopeCut; //Parameter = Node Index
        public System.Action<Rope2D, float> OnRopeTensioned; // Float Value will be A deltaTensionedValue(value 1 == Rope Will Break)

        #endregion

        #region Static Properties

        public static Rope2D[] s_allRopesInScene = new Rope2D[0];

        public static Rope2D[] AllRopesInScene
        {
            get
            {
                return s_allRopesInScene;
            }
        }

        #endregion

        #region Private Variables

        [SerializeField]
        GameObject m_objectA = null;
        [SerializeField]
        GameObject m_objectB = null;
        [SerializeField]
        int m_amountOfNodes = 0;
        [SerializeField]
        bool m_autoCalculateAmountOfNodes = true; //Used to recalc amount of nodes to point A to B (dont tensionate rope in creation when this variable is TRUE)

        [SerializeField]
        GameObject m_customNodePrefab = null;
        [SerializeField]
        float m_nodeMass = 1;
        [SerializeField]
        float m_nodeDistanceOffSet = 0;
        [SerializeField]
        Sprite m_nodeSprite = null;
        [SerializeField]
        Material m_nodeMaterial = null;
        [SerializeField]
        Vector2 m_nodeSpriteGlobalSize = new Vector2(0.001f, 0.0001f);
        [SerializeField]
        Vector2 m_nodeLocalScale = new Vector2(1, 1);
        [SerializeField]
        JointColliderEnum m_jointCollider = JointColliderEnum.Circle;

        //Cut
        [SerializeField]
        bool m_userCanCutTheRope = true;

        //Breakable Node
        [SerializeField]
        bool m_ropeCanBreak = true;
        [SerializeField]
        float m_breakAngle = 120;
        [SerializeField]
        float m_ropeMaxSizeMultiplier = 1.5f;
        [SerializeField]
        GameObject m_breakEffect = null;

        //Renderer
        [SerializeField]
        string m_ropeSortingLayerName = "Default";
        [SerializeField]
        int m_ropeDepth = 0;
        [SerializeField]
        Material m_ropeMaterial = null;
        [SerializeField]
        bool m_useLineRenderer = false;
        [SerializeField]
        AnimationCurve m_lineRendererCurveWidth = AnimationCurve.Linear(0, 1, 1, 1);
        [SerializeField]
        int m_lineRendererEndCapVertices = 0;
        [SerializeField]
        int m_lineRendererCornerVertices = 0;

        //Spring Properties
        [SerializeField]
        bool m_isSpringNode = true;
        [SerializeField]
        float m_springFrequency = 20.0f;
        [SerializeField, Range(0,1), Tooltip("Damping value will stabilize spring rope over time")]
        float m_springDampingValue = 0f;

        //Tension Properties
        [SerializeField]
        TensionHelperAddOptionEnum m_tensionHelperAddOption = TensionHelperAddOptionEnum.AddInPluggedObjects;
        [SerializeField, Range(0, 0.99f), Tooltip("Rope will be considered tensioned if CurrentRopeSize is greater than DefaultRopeSize * (1 + m_tensionNormalizedTolerance)")]
        float m_tensionNormalizedTolerance = 0.05f;
        [SerializeField]
        Color m_nonTensionedColor = Color.white;
        [SerializeField]
        Color m_tensionedColor = Color.red;
        [SerializeField]
        bool m_useTensionColors = true;
        [SerializeField, Tooltip("(EXPERIMENTAL) Use this method if you want to stabilize the rope when when not tensioned")]
        bool m_maxDistanceOnlyMode = false;

        //Destroy Properties
        [SerializeField]
        RopeBreakActionEnum m_ropeBreakAction = RopeBreakActionEnum.DestroySelf;
        [SerializeField]
        float m_destroyTime = 1.2f;
        [SerializeField]
        bool m_lowMassVariationWhenBroken = true;

        //Misc
        [SerializeField, Tooltip("When this mode is enable, the first and last node will be plugged by the center of mass instead of the borders increasing the rope stability.Disable this property when your PlugguedObject is smaller then one node of this rope or when using Spring Node")]
        bool m_stableFirstAndLastNodes = false;
        [SerializeField]
        EditorRopeStyleEnum m_editorRopeStyle = EditorRopeStyleEnum.Gizmos;

        //Extra Physics
        [SerializeField]
        float m_nodeGravityScale = 1.0f;
        [SerializeField]
        float m_nodeAngularDrag = 0.25f;
        [SerializeField]
        float m_nodeLinearDrag = 0.01f;
        [SerializeField]
        RigidbodyCollisionDetectionTypeEnum m_nodeCollisionDetectionType = RigidbodyCollisionDetectionTypeEnum.Discrete;

        //Aux Variables
        float _nonTensionedRopeSize = 0f;

        [SerializeField, HideInInspector]
        bool _needUpdateLineRenderer = true;
        [SerializeField, HideInInspector]
        bool _needUpdateRope = true;
        [SerializeField, HideInInspector]
        List<GameObject> m_nodes = new List<GameObject>();
        [SerializeField, HideInInspector]
        List<GameObject> m_chunks = new List<GameObject>();

        #endregion

        #region Protected Properties

        public bool NeedUpdateRope
        {
            get
            {
                return _needUpdateRope;
            }
            protected set
            {
                if (_needUpdateRope == value)
                    return;
                _needUpdateRope = value;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        protected bool NeedUpdateLineRenderer
        {
            get
            {
                return _needUpdateLineRenderer;
            }
            set
            {
                if (_needUpdateLineRenderer == value)
                    return;
                _needUpdateLineRenderer = value;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        #endregion

        #region Public Properties

        //Destroy Properties
        public RopeBreakActionEnum RopeBreakAction {
            get { return m_ropeBreakAction; }
            set
            {
                if (m_ropeBreakAction == value)
                    return;
                RecordObject();
                m_ropeBreakAction = value;
            }
        }
        public float DestroyTime {
            get { return m_destroyTime; }
            set
            {
                if (m_destroyTime == value)
                    return;
                RecordObject();
                m_destroyTime = value;
            }
        }
        public bool LowMassVariationWhenBroken {
            get { return m_lowMassVariationWhenBroken; }
            set
            {
                if (m_lowMassVariationWhenBroken == value)
                    return;
                RecordObject();
                m_lowMassVariationWhenBroken = value;
            }
        }

        //LineRendere Extras
        public AnimationCurve LineRendererCurveWidth
        {
            get
            {
                if (m_lineRendererCurveWidth == null)
                    m_lineRendererCurveWidth = new AnimationCurve();
                return m_lineRendererCurveWidth;
            }
            set
            {
                if (m_lineRendererCurveWidth == value)
                    return;
                RecordObject();
                m_lineRendererCurveWidth = value;
                NeedUpdateRope = true;
            }
        }

        public int LineRendererEndCapVertices
        {
            get { return m_lineRendererEndCapVertices; }
            set
            {
                if (m_lineRendererEndCapVertices == value)
                    return;
                RecordObject();
                m_lineRendererEndCapVertices = value;
                NeedUpdateRope = true;
            }
        }

        public int LineRendererCornerVertices
        {
            get { return m_lineRendererCornerVertices; }
            set
            {
                if (m_lineRendererCornerVertices == value)
                    return;
                RecordObject();
                m_lineRendererCornerVertices = value;
                NeedUpdateRope = true;
            }
        }

        //Main Properties
        public virtual bool MaxDistanceOnlyMode
        {
            get { return m_maxDistanceOnlyMode; }
            set
            {
                if (m_maxDistanceOnlyMode == value)
                    return;
                RecordObject();
                m_maxDistanceOnlyMode = value;
                SetNodesMaxDistanceOnly(m_maxDistanceOnlyMode);
                NeedUpdateRope = true;
            }
        }

        public virtual GameObject ObjectA
        {
            get { return m_objectA; }
            set
            {
                if (m_objectA == value)
                    return;
                RecordObject();
                m_objectA = value;
                NeedUpdateRope = true;
            }
        }

        public virtual GameObject ObjectB
        {
            get { return m_objectB; }
            set
            {
                if (m_objectB == value)
                    return;
                RecordObject();
                m_objectB = value; NeedUpdateRope = true;
            }
        }

        public int AmountOfNodes
        {
            get { return m_amountOfNodes; }
            set
            {
                if (m_amountOfNodes == value)
                    return;
                RecordObject();
                m_amountOfNodes = value;
                NeedUpdateRope = true;
            }
        }

        public bool AutoCalculateAmountOfNodes
        {
            get { return m_autoCalculateAmountOfNodes; }
            set
            {
                if (m_autoCalculateAmountOfNodes == value)
                    return;
                RecordObject();
                m_autoCalculateAmountOfNodes = value;
                NeedUpdateRope = true;
            }
        }

        public GameObject CustomNodePrefab
        {
            get { return m_customNodePrefab; }
            set
            {
                if (m_customNodePrefab == value)
                    return;
                RecordObject();
                m_customNodePrefab = value;
                NeedUpdateRope = true;
            }
        }

        public RigidbodyCollisionDetectionTypeEnum NodeCollisionDetectionType
        {
            get { return m_nodeCollisionDetectionType; }
            set
            {
                if (m_nodeCollisionDetectionType == value)
                    return;
                RecordObject();
                m_nodeCollisionDetectionType = value;
                NeedUpdateRope = true;
            }
        }

        public float NodeMass
        {
            get { return m_nodeMass; }
            set
            {
                float v_value = Mathf.Max(0.001f, value);
                if (m_nodeMass == v_value)
                    return;
                RecordObject();
                m_nodeMass = v_value;
                NeedUpdateRope = true;
            }
        }

        public Sprite NodeSprite
        {
            get
            {
                return m_nodeSprite;
            }
            set
            {
                if (m_nodeSprite == value)
                    return;
                RecordObject();
                m_nodeSprite = value;
                UpdateNodeBounds();
                NeedUpdateRope = true;
            }
        }

        public Material RopeMaterial
        {
            get { return m_ropeMaterial; }
            set
            {
                if (m_ropeMaterial == value)
                    return;
                RecordObject();
                m_ropeMaterial = value;
            }
        }

        public Vector2 NodeSpriteGlobalSize
        {
            get { return m_nodeSpriteGlobalSize; }
            set
            {
                Vector2 v_value = new Vector2(Mathf.Max(0.001f, value.x), Mathf.Max(0.001f, value.y));
                if (m_nodeSpriteGlobalSize == v_value)
                    return;
                RecordObject();
                //Can Only Set this Value when Sprite is Null
                if (NodeSprite == null)
                {
                    m_nodeSpriteGlobalSize = v_value;
                    UpdateNodeBounds();
                    NeedUpdateRope = true;
                }
            }
        }

        public Vector2 NodeLocalScale
        {
            get { return m_nodeLocalScale; }
            set
            {
                Vector2 v_value = new Vector2(Mathf.Max(0.01f, value.x), Mathf.Max(0.01f, value.y));
                if (m_nodeLocalScale == v_value)
                    return;
                RecordObject();
                m_nodeLocalScale = v_value;
                UpdateNodeBounds();
                NeedUpdateRope = true;
            }
        }

        public float NodeDistanceOffSet
        {
            get { return m_nodeDistanceOffSet; }
            set
            {
                if (m_nodeDistanceOffSet == value)
                    return;
                RecordObject();
                m_nodeDistanceOffSet = value;
                UpdateNodeBounds();
                NeedUpdateRope = true;
            }
        }

        public JointColliderEnum JointCollider
        {
            get { return m_jointCollider; }
            set
            {
                if (m_jointCollider == value)
                    return;
                RecordObject();
                m_jointCollider = value;
                NeedUpdateRope = true;
            }
        }

        //Renderer
        public string RopeSortingLayerName
        {
            get { return m_ropeSortingLayerName; }
            set
            {
                if (m_ropeSortingLayerName == value)
                    return;
                RecordObject();
                m_ropeSortingLayerName = value;
                UpdateRopeSorting();
                UpdateNodesSorting();
            }
        }

        public int RopeDepth
        {
            get { return m_ropeDepth; }
            set
            {
                if (m_ropeDepth == value)
                    return;
                RecordObject();
                m_ropeDepth = value;
                UpdateRopeSorting();
                UpdateNodesSorting();
            }
        }

        public Material NodeMaterial
        {
            get { return m_nodeMaterial; }
            set
            {
                if (m_nodeMaterial == value)
                    return;
                RecordObject();
                m_nodeMaterial = value;
            }
        }

        public bool UseLineRenderer
        {
            get { return m_useLineRenderer; }
            set
            {
                if (m_useLineRenderer == value)
                    return;
                RecordObject();
                m_useLineRenderer = value;
                NeedUpdateLineRenderer = true;
                UpdateLineRendererVisibility();
            }
        }

        //Cut
        public bool UserCanCutTheRope
        {
            get { return m_userCanCutTheRope; }
            set
            {
                if (m_userCanCutTheRope == value)
                    return;
                RecordObject();
                m_userCanCutTheRope = value;
            }
        }

        //Breakable Node
        public bool RopeCanBreak
        {
            get { return m_ropeCanBreak; }
            set
            {
                if (m_ropeCanBreak == value)
                    return;
                RecordObject();
                m_ropeCanBreak = value;
            }
        }

        public float BreakAngle
        {
            get { return m_breakAngle; }
            set
            {
                if (m_breakAngle == value)
                    return;
                RecordObject();
                m_breakAngle = value;
                NeedUpdateLineRenderer = true;
            }
        }

        public float RopeMaxSizeMultiplier
        {
            get { return m_ropeMaxSizeMultiplier; }
            set
            {
                if (m_ropeMaxSizeMultiplier == value)
                    return;
                RecordObject();
                m_ropeMaxSizeMultiplier = value;
            }
        }

        public GameObject BreakEffect
        {
            get { return m_breakEffect; }
            set
            {
                if (m_breakEffect == value)
                    return;
                RecordObject();
                m_breakEffect = value;
                NeedUpdateLineRenderer = true;
            }
        }

        //Tensioned Properties
        public TensionHelperAddOptionEnum TensionHelperAddOption
        {
            get { return m_tensionHelperAddOption; }
            set
            {
                if (m_tensionHelperAddOption == value)
                    return;
                RecordObject();
                m_tensionHelperAddOption = value;
                DestroyTensionHelper();
                if (Application.isPlaying)
                    CheckTension();
            }
        }

        public float TensionNormalizedTolerance
        {
            get { return m_tensionNormalizedTolerance; }
            set
            {
                if (m_tensionNormalizedTolerance == value)
                    return;
                RecordObject();
                m_tensionNormalizedTolerance = value;
            }
        }

        public Color NonTensionedColor
        {
            get { return m_nonTensionedColor; }
            set
            {
                if (m_nonTensionedColor == value)
                    return;
                RecordObject();
                m_nonTensionedColor = value;
            }
        }

        public Color TensionedColor
        {
            get { return m_tensionedColor; }
            set
            {
                if (m_tensionedColor == value)
                    return;
                RecordObject();
                m_tensionedColor = value;
            }
        }

        public bool UseTensionColors
        {
            get { return m_useTensionColors; }
            set
            {
                if (m_useTensionColors == value)
                    return;
                RecordObject();
                m_useTensionColors = value;
            }
        }

        //Spring Properties
        public bool IsSpringNode
        {
            get { return m_isSpringNode; }
            set
            {
                if (m_isSpringNode == value)
                    return;
                RecordObject();
                m_isSpringNode = value;
                NeedUpdateRope = true;
            }
        }

        public float SpringFrequency
        {
            get { return m_springFrequency; }
            set
            {
                if (m_springFrequency == value)
                    return;
                RecordObject();
                m_springFrequency = value;
                NeedUpdateRope = true;
            }
        }

        public float SpringDampingValue
        {
            get { return m_springDampingValue; }
            set
            {
                if (m_springDampingValue == value)
                    return;
                RecordObject();
                m_springDampingValue = value;
                NeedUpdateRope = true;
            }
        }

        //Extra Physics
        public float NodeGravityScale
        {
            get { return m_nodeGravityScale; }
            set
            {
                if (m_nodeGravityScale == value)
                    return;
                RecordObject();
                m_nodeGravityScale = value;
                NeedUpdateRope = true;
            }
        }

        public float NodeAngularDrag
        {
            get { return m_nodeAngularDrag; }
            set
            {
                if (m_nodeAngularDrag == value)
                    return;
                RecordObject();
                m_nodeAngularDrag = value;
                NeedUpdateRope = true;
            }
        }

        public float NodeLinearDrag
        {
            get { return m_nodeLinearDrag; }
            set
            {
                if (m_nodeLinearDrag == value)
                    return;
                RecordObject();
                m_nodeLinearDrag = value;
                NeedUpdateRope = true;
            }
        }

        //Misc
        public bool StableFirstAndLastNodes
        {
            get { return m_stableFirstAndLastNodes; }
            set
            {
                if (m_stableFirstAndLastNodes == value)
                    return;
                RecordObject();
                m_stableFirstAndLastNodes = value;
                NeedUpdateRope = true;
            }
        }

        public EditorRopeStyleEnum EditorRopeStyle
        {
            get { return m_editorRopeStyle; }
            set
            {
                if (m_editorRopeStyle == value)
                    return;
                RecordObject();
                m_editorRopeStyle = value;
                if (!Application.isPlaying && Application.isEditor)
                    NeedUpdateRope = true;
            }
        }

        //Other
        public List<GameObject> Nodes
        {
            get
            {
                if (m_nodes == null)
                    m_nodes = new List<GameObject>();
                return m_nodes;
            }
            protected set
            {
                if (m_nodes == value)
                    return;
                RecordObject();
                m_nodes = value;
            }
        }

        public List<GameObject> Chunks
        {
            get
            {
                if (m_chunks == null)
                    m_chunks = new List<GameObject>();
                return m_chunks;
            }
            protected set
            {
                if (m_chunks == value)
                    return;
                RecordObject();
                m_chunks = value;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void OnDrawGizmos()
        {
            DrawDebugGizmos();
            UpdateEditorNodes();
        }

        protected virtual void OnEnable()
        {
            if (!RopeInternalUtils.IsPrefab(gameObject))
                RegisterRopeInScene(this);
        }

        protected virtual void OnDisable()
        {
            if (!RopeInternalUtils.IsPrefab(gameObject))
                UnregisterRopeFromScene(this);
            DestroyTensionHelper();
        }

        protected virtual void Start()
        {
            CorrectValues();
            if (Application.isPlaying)
            {
                if (Application.isEditor)
                    UpdateRopePosition();
                if (NeedUpdateRope)
                    CreateRope();
                else
                    CreateRope(false);
            }
        }

        protected virtual void Update()
        {
            if (Application.isPlaying)
            {
                if (Application.isEditor)
                    CorrectValues();
                if (NeedUpdateRope)
                    CreateRope();
                else if (NeedUpdateLineRenderer)
                    UpdateRenderersVisibility();
                CheckTension();
                if (UseLineRenderer)
                    UpdateLineRenderersNodePosition();
                CheckIfRopeNeedBreak();
                CheckIfConnectedObjectsExists();
            }
            else if (Application.isEditor)
                UpdateEditorNodes();
        }

        protected virtual void LateUpdate()
        {
            if (Application.isPlaying)
            {
                if (Chunks.Count <= 0 && Nodes.Count <= 0 && RopeBreakAction == RopeBreakActionEnum.DestroySelf)
                    DestroyUtils.DestroyImmediate(this.gameObject);
            }
        }

        #endregion

        #region Debug Draw

        public virtual void UpdateEditorNodes()
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying && !_isFirstOnDrawGizmosCall)
            {
                if ((Nodes.Count > 0 || Chunks.Count > 0) && (EditorRopeStyle == EditorRopeStyleEnum.Gizmos || (ObjectA == null || ObjectB == null)))
                    ClearNodes();
                if (EditorRopeStyle == EditorRopeStyleEnum.NodeObjects)
                {
                    bool v_ropeRefreshed = false;
                    int v_fullAmountOfNodes = AmountOfNodes + 2;
                    if (v_fullAmountOfNodes != Nodes.Count)
                    {
                        CreateRope(false);
                        v_ropeRefreshed = true;
                    }
                    else if (NeedUpdateRope)
                    {
                        CreateRope(true);
                        v_ropeRefreshed = true;
                    }
                    CorrectNodeDistanceAndRotation(true, true);
                    if (UseLineRenderer)
                        UpdateLineRenderersNodePosition();
                    if (v_ropeRefreshed && SceneView.currentDrawingSceneView != null)
                        SceneView.currentDrawingSceneView.Repaint();
                }
            }
#endif
        }

#if UNITY_EDITOR
        bool _isFirstOnDrawGizmosCall = true;
#endif
        public virtual void DrawDebugGizmos()
        {
#if UNITY_EDITOR
            try
            {
                if (_isFirstOnDrawGizmosCall)
                {
                    _isFirstOnDrawGizmosCall = false;
                    UpdateNodeBounds();
                }
                if (Application.isEditor && !Application.isPlaying)
                {
                    CorrectValues();
                    UpdateRopePosition();
                }

                if (ObjectA != null && ObjectB != null)
                {
                    //Recalc Nodes And Gizmo Color in Editor Mode
                    if (!Application.isPlaying && Application.isEditor)
                    {
                        if (AutoCalculateAmountOfNodes)
                            RecalcAmountOfNodes();
                        _nonTensionedRopeSize = GetNonTensionedRopeSize();
                    }
                    Color v_gizmosColor = Color.Lerp(Color.white, Color.red, GetTensionDelta());

                    Matrix4x4 v_rotationMatrix = Matrix4x4.identity;

                    //Draw Rectangle
                    Vector2 v_inicialPosition = ObjectA.transform.position;
                    Vector2 v_finalPosition = ObjectB.transform.position;
                    Vector2 v_middlePosition = new Vector2((v_inicialPosition.x + v_finalPosition.x) / 2.0f, (v_inicialPosition.y + v_finalPosition.y) / 2.0f);
                    Vector2 v_direction = RopeInternalUtils.GetVectorDirection(v_inicialPosition, v_finalPosition);

                    float v_distance = Mathf.Abs(Vector2.Distance(v_inicialPosition, v_finalPosition));
                    float v_angle = Vector2.Angle(new Vector2(1, 0), v_direction);
                    v_angle = v_finalPosition.y < v_inicialPosition.y ? -v_angle : v_angle;

                    Quaternion v_quaternion = Quaternion.identity;
                    v_quaternion.eulerAngles = new Vector3(0, 0, v_angle);
                    v_rotationMatrix = Matrix4x4.TRS(v_middlePosition, v_quaternion, new Vector3(v_distance, GetNodeSize().y, 0.003f));
                    Gizmos.matrix = v_rotationMatrix;
                    Gizmos.color = v_gizmosColor;
                    Gizmos.DrawWireCube(Vector2.zero, Vector2.one);

                    //Only draw this gizmos when in GameMode option
                    if (EditorRopeStyle == EditorRopeStyleEnum.Gizmos || Application.isPlaying)
                    {
                        int v_fullAmountOfNodes = AmountOfNodes + 2;
                        float v_percent = v_distance != 0 ? GetNodeDistance(false) / 2f / v_distance : 0;
                        Vector3 v_offset = StableFirstAndLastNodes ? Vector3.zero : (Vector3)(v_finalPosition - v_inicialPosition) * v_percent;
                        for (int i = 0; i < v_fullAmountOfNodes; i++)
                        {
                            Vector3 v_position = ObjectA.transform.position + v_offset + ((ObjectB.transform.position - v_offset) - (ObjectA.transform.position + v_offset)) * Mathf.Max(0, i) / Mathf.Max(1, (AmountOfNodes + 1));
                            v_rotationMatrix = Matrix4x4.TRS(v_position, v_quaternion, new Vector3(GetNodeSize().x / 2.0f, GetNodeSize().y / 2.0f, 0.0001f));
                            Gizmos.matrix = v_rotationMatrix;
                            Gizmos.color = v_gizmosColor;
                            if (JointCollider == JointColliderEnum.Circle)
                                Gizmos.DrawWireSphere(Vector2.zero, 1f);
                            else if (JointCollider == JointColliderEnum.Box)
                            {
                                Gizmos.DrawWireCube(Vector2.zero, new Vector2(2, 1.6f));
                                Gizmos.DrawWireCube(Vector2.zero, new Vector2(2, 0.001f));
                            }
                        }
                    }
                }
            }
            catch { }
#endif
        }

        #endregion

        #region Tension Functions

        // Return Values:
        // value = 0 : Non Tensioned, 
        // 0 < value < 1 : Tensioned, 
        // value = 1 : Rope Will Break
        public float GetTensionDelta()
        {
            float v_lerpDelta = 0;
            float v_currentRopeSize = GetCurrentRopeSize();
            float v_maxRopeSize = RopeMaxSizeMultiplier * _nonTensionedRopeSize;

            if (v_maxRopeSize < v_currentRopeSize)
                v_lerpDelta = 1;
            else if (v_currentRopeSize > _nonTensionedRopeSize)
            {
                v_currentRopeSize -= _nonTensionedRopeSize;
                v_maxRopeSize -= _nonTensionedRopeSize;
                v_lerpDelta = Mathf.Clamp(v_maxRopeSize != 0 ? Mathf.Abs(v_currentRopeSize) / Mathf.Abs(v_maxRopeSize) : 1, 0, 1);
            }
            return v_lerpDelta;
        }

        public bool IsRopeTensioned()
        {
            return GetTensionDelta() > m_tensionNormalizedTolerance;
        }

        protected void SetTensionColor(float p_lerpDelta)
        {
            if (UseTensionColors)
            {
                p_lerpDelta = !IsRopeBroken() ? p_lerpDelta : 0;
                Color v_finalColor = Color.Lerp(NonTensionedColor, TensionedColor, p_lerpDelta);
                if (UseLineRenderer)
                {
                    for (int i = 0; i < Chunks.Count; i++)
                    {
                        LineRenderer v_lineRenderer = GetLineRendererComponentByChunk(i);
                        if (v_lineRenderer != null && v_lineRenderer.material != null)
                        {
                            if (v_lineRenderer.material.HasProperty("_Color"))
                                v_lineRenderer.material.SetColor("_Color", v_finalColor);
                            else if (v_lineRenderer.material.HasProperty("_Tint"))
                                v_lineRenderer.material.SetColor("_Tint", v_finalColor);
                        }
                    }
                }
                else
                {
                    foreach (GameObject v_nodes in Nodes)
                    {
                        if (v_nodes != null && v_nodes.GetComponent<Renderer>() is SpriteRenderer)
                            (v_nodes.GetComponent<Renderer>() as SpriteRenderer).color = v_finalColor;
                    }
                }
            }

        }

        //float _lastTensionHelperCheckTimeDelta = 0;
        protected void CheckTension()
        {
            if (ObjectA != null && ObjectB != null && Application.isPlaying)
            {
                //_lastTensionHelperCheckTimeDelta += Time.deltaTime;
                InsertTensionHelper();
                float v_distance = Vector3.Distance(ObjectA.transform.position, ObjectB.transform.position);
                float v_ropeTensionDelta = GetTensionDelta();
                bool v_isRopeTensioned = v_ropeTensionDelta > TensionNormalizedTolerance;
                if (v_ropeTensionDelta > TensionNormalizedTolerance && !IsRopeBroken())
                {
                    if (v_distance > _nonTensionedRopeSize * (1.1 + TensionNormalizedTolerance))
                    {
                        bool v_correctFirstAndLast = (TensionHelperAddOption == TensionHelperAddOptionEnum.AddInPluggedObjects);
                        CorrectNodeDistanceAndRotation(v_correctFirstAndLast);
                    }
                    if (OnRopeTensioned != null)
                        OnRopeTensioned(this, v_ropeTensionDelta);
                }

                var v_maxCheckTime = 0.2f;
                if (_tensionHelper != null && 
                    _tensionHelper.enabled != v_isRopeTensioned
                   /*((_tensionHelper.enabled != v_isRopeTensioned &&_lastTensionHelperCheckTimeDelta > v_maxCheckTime) || 
                   (v_distance < 0.95f * _nonTensionedRopeSize && _tensionHelper.enabled))*/
                   )
                {
                    _tensionHelper.enabled = v_isRopeTensioned;
                    if (!_tensionHelper.enabled)
                        ResetDeltaTimeToCheckIfRopeNeedBreak(v_maxCheckTime); //Prevent rope to break when returning
                }
                //if (_lastTensionHelperCheckTimeDelta > v_maxCheckTime)
                //    _lastTensionHelperCheckTimeDelta = 0;
                SetTensionColor(v_ropeTensionDelta);
            }
        }

        /* Tension Helper is a Spring or Distance Joint that plug first and Last node, this is used when rope is tensioned(cause we must correct position of each node).
         * After correct node positions all rope lose energy, so tensionHelper simulates this tension */
        AnchoredJoint2D _tensionHelper = null;
        protected void InsertTensionHelper()
        {
            if (!IsRopeBroken() && Application.isPlaying)
            {
                if (_tensionHelper == null)
                {
                    float v_distanceCorrector = 1.05f;
                    if (TensionHelperAddOption == TensionHelperAddOptionEnum.AddInPluggedObjects)
                    {
                        v_distanceCorrector = IsSpringNode ? 1.05f : 1.05f; //Distance corrector is different to Spring and Distance Joints in this mode
                        float v_frequencyCorrector = 0.3f; //Reduce force generated by TensionHelper in this mode
                        _tensionHelper = PlugDistanceOrSpringJoint(ObjectB, ObjectA, _nonTensionedRopeSize * v_distanceCorrector, IsSpringNode);
                        if (_tensionHelper is SpringJoint2D)
                            ((SpringJoint2D)_tensionHelper).frequency *= v_frequencyCorrector;
                    }
                    else //Old Method
                        _tensionHelper = PlugDistanceOrSpringJoint(Nodes.GetLast(), Nodes.GetFirst(), _nonTensionedRopeSize * v_distanceCorrector, IsSpringNode);
                    _tensionHelper.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                }
            }
            else
                DestroyTensionHelper();
        }

        protected void DestroyTensionHelper()
        {
            if (_tensionHelper != null)
            {
                DestroyUtils.Destroy(_tensionHelper);
                _tensionHelper = null;
            }
        }

        //AnchoredJoints distort positions when rope is too much tensioned, so we must correct this values
        protected void CorrectNodeDistanceAndRotation(bool p_correctFirstAndLast, bool p_force = false)
        {
            if ((!IsRopeBroken() || p_force) && ObjectA != null && ObjectB != null)
            {
                float v_angle = Vector2.Angle(ObjectA.transform.position, ObjectB.transform.position);
                float v_distance = Mathf.Abs(Vector2.Distance(ObjectB.transform.position, ObjectA.transform.position));
                float v_percent = v_distance != 0 ? GetNodeDistance(false) / 2f / v_distance : 0;
                Vector3 v_offset = StableFirstAndLastNodes ? Vector3.zero : (ObjectB.transform.position - ObjectA.transform.position) * v_percent;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    GameObject v_object = Nodes[i];
                    if (v_object != null)
                    {
                        //Correct Rotation
                        Vector3 v_direction = RopeInternalUtils.GetVectorDirection(ObjectA.transform.position, ObjectB.transform.position);
                        v_angle = Vector3.Angle(new Vector2(1, 0), v_direction);
                        if (ObjectB.transform.position.y < ObjectA.transform.position.y)
                            v_angle = -v_angle;
                        v_angle += 180;
                        v_object.transform.rotation = Quaternion.Euler(new Vector3(0, 0, v_angle));
                        //Correct Distance
                        if (i == 0)
                        {
                            if (ObjectA != null && p_correctFirstAndLast)
                            {
                                Vector3 v_transformPosition = v_object.transform.position;
                                v_transformPosition.x = ObjectA.transform.position.x;
                                v_transformPosition.y = ObjectA.transform.position.y;
                                v_object.transform.position = v_transformPosition + v_offset;
                            }

                        }
                        else if (i == Nodes.Count - 1)
                        {
                            if (ObjectB != null && p_correctFirstAndLast)
                            {
                                Vector3 v_transformPosition = v_object.transform.position;
                                v_transformPosition.x = ObjectB.transform.position.x;
                                v_transformPosition.y = ObjectB.transform.position.y;
                                v_object.transform.position = v_transformPosition - v_offset;
                            }
                        }
                        else
                        {
                            Vector3 v_position = ObjectA.transform.position + v_offset + ((ObjectB.transform.position - v_offset) - (ObjectA.transform.position + v_offset)) * Mathf.Max(0, i) / Mathf.Max(1, (AmountOfNodes + 1));
                            v_position.z = v_object.transform.position.z;
                            v_object.transform.position = v_position;

                            Rigidbody2D v_body = GetRigidBody2DFromObject(v_object);
                            if (v_body != null)
                            {
                                v_body.velocity = Vector2.zero;
                                v_body.angularVelocity = 0;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Create Node Functions

        public virtual void CreateRope(bool p_clearNodes = true)
        {
            DestroyTensionHelper();
            if (ObjectA != null && ObjectB != null && AmountOfNodes > 0)
            {
                UpdateNodeBounds();
                _ropeIsBroken = false;
                UpdateRopePosition();
                RecalcAmountOfNodes();
                //In Play mode we want to remove this property after create because this rope, now, have fixed size
                if (Application.isPlaying)
                    m_autoCalculateAmountOfNodes = false;

                _nonTensionedRopeSize = ((AmountOfNodes + 1) * GetNodeDistance(false)) + GetNodeSize().x;
                NeedUpdateRope = false;
                Quaternion v_rotation = Quaternion.LookRotation(ObjectA.transform.position - ObjectB.transform.position, Vector3.up);
                v_rotation.x = 0;
                v_rotation.y = 0;

                //True amount of nodes include first and last nodes
                int v_trueAmount = AmountOfNodes + 2;
                //Create Main Chunk
                GameObject v_chunk = CreateChunk("Chunk" + 1);
                v_chunk.transform.SetParent(null, false);
                if (p_clearNodes)
                {
                    if (Chunks.Count > 0 || Nodes.Count > 0)
                        DestroyRope();
                    else
                        ClearNodes();
                }
                else
                {
                    //Remove useless nodes 
                    while (Nodes.Count > v_trueAmount && Nodes.Count > 0)
                    {
                        GameObject v_object = Nodes[Nodes.Count - 1];
                        DestroyUtils.DestroyImmediate(v_object);
                        Nodes.RemoveAt(Nodes.Count - 1);
                    }
                    //Change parent to main chunk before ClearChunks
                    foreach (GameObject v_node in Nodes)
                    {
                        if (v_node != null)
                            v_node.transform.parent = v_chunk.transform;
                    }
                    ClearChunks();
                }
                v_chunk.transform.SetParent(this.transform, false);
                Chunks.Add(v_chunk);

                //Mathf.Max(0,(Nodes.Count-1))
                for (int i = 0; i < v_trueAmount; i++)
                {
                    GameObject v_newNode = null;
                    GameObject v_modelNode = i < Nodes.Count && Nodes[i] != null ? Nodes[i] : CustomNodePrefab;
                    //Last Node is First Node too
                    if (v_trueAmount == 1)
                    {
                        Vector2 v_position = ObjectA.transform.position + (ObjectB.transform.position - ObjectA.transform.position);
                        v_newNode = CreateLastNode(v_modelNode, v_position, v_rotation, ObjectA, v_chunk, NodeSprite);
                    }
                    else
                    {
                        GameObject v_connectedNode = Nodes.Count > i - 1 && i - 1 >= 0 ? Nodes[i - 1] : null;
                        //First Node
                        if (i == 0)
                        {
                            Vector2 v_position = ObjectA.transform.position;
                            v_newNode = CreateFirstNode(v_modelNode, v_position, v_rotation, v_chunk, NodeSprite);
                        }
                        //Last Node
                        else if (i == v_trueAmount - 1)
                        {
                            Vector2 v_position = ObjectA.transform.position + (ObjectB.transform.position - ObjectA.transform.position);
                            v_newNode = CreateLastNode(v_modelNode, v_position, v_rotation, v_connectedNode, v_chunk, NodeSprite);
                        }
                        //Other Node
                        else
                        {
                            Vector2 v_position = ObjectA.transform.position + (ObjectB.transform.position - ObjectA.transform.position) * Mathf.Max(0, i - 1) / Mathf.Max(1, (AmountOfNodes - 1));
                            v_newNode = CreateNode(v_modelNode, v_position, v_rotation, v_connectedNode, v_chunk, NodeSprite, "Node" + (i + 1), NodeMass, true, IsSpringNode, true);
                        }
                    }
                    if (v_newNode != null && !Nodes.Contains(v_newNode))
                    {
                        if (i < Nodes.Count)
                        {
                            if (Nodes[i] != null && Nodes[i] != v_newNode)
                                DestroyUtils.DestroyImmediate(Nodes[i]);
                            Nodes[i] = v_newNode;
                        }
                        else
                            Nodes.Add(v_newNode);
                    }
                }
                CorrectNodeDistanceAndRotation(true, true);
                UpdateNodesSorting();
                //LineRenderer
                if (UseLineRenderer)
                    StartLineRenderersInitialValues();
                //Plug Script
                AddAttachedScriptInPlugguedObjects();
                //Activate Plugged Nodes
                ActivatePluggedNodes();
                //Set CollideConnected false to all nodes
                ApplyIgnoreRopeCollisionToAllNodes();
                if (Application.isPlaying)
                {
                    //Call Event
                    CheckTension();
                    if (OnRopeCreated != null)
                        OnRopeCreated(this);
                    SendMessageToAllIndirectObjects("IndirectRopeCreated");
                }
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        protected virtual void ApplyIgnoreRopeCollisionToAllNodes()
        {
            if (Application.isPlaying)
            {
                foreach (GameObject v_nodeObject in Nodes)
                {
                    if (v_nodeObject != null)
                    {
                        Node2D v_node = v_nodeObject.GetComponent<Node2D>();
                        if (v_node != null)
                        {
                            v_node.ApplyIgnoreRopeCollision(true);
                        }
                    }
                }
            }
        }

        public virtual void AddAttachedScriptInPlugguedObjects()
        {
            GameObject v_attachedA = GetAttachedObjectA();
            GameObject v_attachedB = GetAttachedObjectB();
            if (v_attachedA != null)
            {
                RopesAttached v_attachedScript = v_attachedA.GetComponent<RopesAttached>();
                if (v_attachedScript == null)
                    v_attachedScript = v_attachedA.AddComponent<RopesAttached>();
                v_attachedScript.AddPluggedRopeInList(this);
            }
            if (v_attachedB != null)
            {
                RopesAttached v_attachedScript = v_attachedB.GetComponent<RopesAttached>();
                if (v_attachedScript == null)
                    v_attachedScript = v_attachedB.AddComponent<RopesAttached>();
                v_attachedScript.AddPluggedRopeInList(this);
            }
        }

        public virtual GameObject CreateNode(GameObject p_modelNode, Vector2 p_position, Quaternion p_rotation, GameObject p_connectedNode, GameObject p_chunk, Sprite p_sprite = null, string p_name = "Node", float p_nodeMass = 0.05f, bool p_generateHingeJoint = true, bool p_isSpringJoint = true, bool p_generateDistanceOrSpringJoint = true)
        {
            if (p_chunk == null)
                p_chunk = this.gameObject;
            //GameObject
            GameObject v_newNode = p_modelNode != null ? (RopeInternalUtils.IsPrefab(p_modelNode) ? GameObject.Instantiate(p_modelNode) : p_modelNode) : new GameObject(p_name);
            v_newNode.name = p_name;
            v_newNode.layer = p_chunk.layer;
            v_newNode.transform.parent = p_chunk.transform;
            v_newNode.transform.position = p_position;
            v_newNode.transform.localScale = new Vector3(NodeLocalScale.x, NodeLocalScale.y, 1);
            v_newNode.transform.rotation = p_rotation;

            //Node2D
            Node2D v_node2DComponent = p_modelNode == null ? null : v_newNode.GetNonMarkedComponentInChildren<Node2D>();
            if (v_node2DComponent == null)
                v_node2DComponent = v_newNode.AddComponent<Node2D>();

            //SpriteRenderer
            SpriteRenderer v_renderer = p_modelNode == null ? null : v_newNode.GetNonMarkedComponentInChildren<SpriteRenderer>();
            if (v_renderer == null)
            {
                v_renderer = v_newNode.AddComponent<SpriteRenderer>();
                v_renderer.sprite = p_sprite;
                if (NodeMaterial != null)
                    v_renderer.sharedMaterial = NodeMaterial;
            }
            v_renderer.enabled = UseLineRenderer ? false : true;

            //Collider
            if (p_modelNode == null || v_newNode.GetNonMarkedComponentInChildren<Collider2D>() == null)
            {
                if (JointCollider == JointColliderEnum.Circle)
                {
                    CircleCollider2D v_collider = v_newNode.AddComponent<CircleCollider2D>();
                    v_collider.radius = GetNodeDistance() / 2;//(GetNodeSize().x/2f) *(1/Mathf.Max(0.0001f, v_newNode.transform.lossyScale.x));
                }
                else if (JointCollider == JointColliderEnum.Box)
                {
                    BoxCollider2D v_collider = v_newNode.AddComponent<BoxCollider2D>();
                    v_collider.size = new Vector2(GetNodeDistance(), GetNodeDistance() / 2);
                    v_collider.offset = new Vector2(0, 0);
                }
            }
            //Destroy all previous Joints
            if (p_modelNode != null)
            {
                AnchoredJoint2D[] v_joints = v_newNode.GetNonMarkedComponentsInChildren<AnchoredJoint2D>(true, true);
                foreach (AnchoredJoint2D v_joint in v_joints)
                {
                    DestroyImmediate(v_joint);
                }
            }
            //Destroy all Previous Rigidbodys2D
            if (p_modelNode != null)
            {
                Rigidbody2D[] v_bodys = v_newNode.GetNonMarkedComponentsInChildren<Rigidbody2D>(true, true);
                foreach (Rigidbody2D v_body in v_bodys)
                {
                    DestroyImmediate(v_body);
                }
            }
            //HingeJoint
            if (p_generateHingeJoint)
            {
                PlugHingeJoint(v_newNode, p_connectedNode);
            }

            //Distance Or Spring Joint
            if (p_generateDistanceOrSpringJoint)
                PlugDistanceOrSpringJoint(p_connectedNode, v_newNode, GetNodeDistance(false), (p_isSpringJoint && !m_maxDistanceOnlyMode));

            //RigidBody
            Rigidbody2D v_rigidBody = v_newNode.GetNonMarkedComponentInChildren<Rigidbody2D>();
            if (v_rigidBody == null)
                v_rigidBody = v_newNode.AddComponent<Rigidbody2D>();
            if (v_rigidBody != null)
            {
                v_rigidBody.mass = p_nodeMass;
                v_rigidBody.gravityScale = NodeGravityScale;
                v_rigidBody.angularDrag = NodeAngularDrag;
                v_rigidBody.drag = NodeLinearDrag;
                v_rigidBody.collisionDetectionMode = NodeCollisionDetectionType == RigidbodyCollisionDetectionTypeEnum.Continuous ? CollisionDetectionMode2D.Continuous : CollisionDetectionMode2D.Discrete;
            }
            return v_newNode;
        }

        public virtual GameObject CreateFirstNode(GameObject p_modelNode, Vector2 p_position, Quaternion p_rotation, GameObject p_chunk, Sprite p_sprite = null)
        {
            //GameObject
            GameObject v_firstNode = CreateNode(p_modelNode, p_position, p_rotation, ObjectA, p_chunk, p_sprite, "First", NodeMass, false, false, StableFirstAndLastNodes);
            DistanceJoint2D v_joint = v_firstNode.GetComponent<DistanceJoint2D>();
            if (!StableFirstAndLastNodes)
                PlugHingeJoint(v_firstNode, ObjectA, true, true);
            else if (v_joint != null && ObjectA != null)
            {
#if UNITY_5_3_OR_NEWER
                v_joint.autoConfigureDistance = false;
                v_joint.autoConfigureDistance = false;
#endif
                v_joint.distance = 0.01f;
            }
            return v_firstNode;
        }

        public virtual GameObject CreateLastNode(GameObject p_modelNode, Vector2 p_position, Quaternion p_rotation, GameObject p_previusNode, GameObject p_chunk, Sprite p_sprite = null)
        {
            //GameObject
            GameObject v_lastNode = CreateNode(p_modelNode, p_position, p_rotation, p_previusNode, p_chunk, p_sprite, "Last", NodeMass, true, false);
            DistanceJoint2D v_joint = v_lastNode.GetComponent<DistanceJoint2D>();
            if (v_joint != null)
            {
#if UNITY_5_3_OR_NEWER
                v_joint.autoConfigureConnectedAnchor = false;
                v_joint.autoConfigureDistance = false;
                v_joint.maxDistanceOnly = m_maxDistanceOnlyMode;
#endif
                v_joint.distance = 0.01f;
                v_joint.anchor = new Vector2((GetNodeSize().x / 2f) * (1 / Mathf.Max(0.0001f, v_lastNode.transform.lossyScale.x)), 0);
                v_joint.connectedAnchor = new Vector2((-GetNodeSize().x / 2f) * (1 / Mathf.Max(0.0001f, v_joint.connectedBody != null ? v_joint.connectedBody.transform.lossyScale.x : 0.0001f)), 0);
            }
            if (!StableFirstAndLastNodes)
            {
                HingeJoint2D v_newHingeJoint = PlugHingeJoint(v_lastNode, ObjectB, true, true);
                v_newHingeJoint.anchor = new Vector2(-v_newHingeJoint.anchor.x, v_newHingeJoint.anchor.y);
            }
            else
            {
                DistanceJoint2D v_newJoint = v_lastNode.AddComponent<DistanceJoint2D>();
                if (v_newJoint != null && ObjectB != null)
                {
#if UNITY_5_3_OR_NEWER
                    v_joint.autoConfigureConnectedAnchor = false;
                    v_joint.autoConfigureDistance = false;
#endif
                    v_newJoint.distance = 0.01f;
                    v_newJoint.connectedBody = GetRigidBody2DFromObject(ObjectB);
                    //If Point B is a Tack, use it position to set Parent Rigidbody anchor
                    if (v_newJoint.connectedBody != null)
                        v_newJoint.connectedAnchor = v_newJoint.connectedBody.transform.InverseTransformPoint(ObjectB.transform.position);

                }
            }
            return v_lastNode;
        }

        #endregion

        #region Breakable Functions

        //Used To Break First and Last Nodes if Connected objects Removed from game
        protected bool BreakIfHaveNullNode()
        {
            if (!IsRopeBroken())
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i] == null)
                    {
                        BreakNode(i);
                        return true;
                    }
                }
            }
            return false;
        }

        //Used To Break First and Last Nodes if Connected objects Removed from game
        protected void CheckIfConnectedObjectsExists()
        {
            if (!IsRopeBroken())
            {
                if (ObjectA == null && Nodes[0] != null)
                    BreakNode(Nodes[0]);
                else if (ObjectB == null && Nodes[Nodes.Count - 1] != null)
                    BreakNode(Nodes[Nodes.Count - 1]);
            }
        }

        protected List<GameObject> GetConnectedObjects(GameObject p_object)
        {
            List<GameObject> v_return = new List<GameObject>();
            if (p_object != null /*&& !p_object.name.Equals("Last") && !p_object.name.Equals("First")*/)
            {
                Rigidbody2D v_objectABody = GetRigidBody2DFromObject(ObjectA);
                Rigidbody2D v_objectBBody = GetRigidBody2DFromObject(ObjectB);
                HingeJoint2D[] v_hingeJoints = p_object.GetComponents<HingeJoint2D>();
                foreach (HingeJoint2D v_joint in v_hingeJoints)
                {
                    if (v_joint != null && v_joint.connectedBody != null && v_joint.connectedBody != v_objectABody && v_joint.connectedBody != v_objectBBody)
                    {
                        v_return.AddChecking(v_joint.connectedBody.gameObject);
                    }
                }
            }
            return v_return;
        }

        protected bool RopeExceedMaxSize()
        {
            if (ObjectA != null && ObjectB != null)
            {
                if (!IsRopeBroken())
                {
                    float v_distance = Vector3.Distance(ObjectA.transform.position, ObjectB.transform.position);
                    float v_nonTensionedRopeSize = GetNonTensionedRopeSize();
                    if (v_distance > (v_nonTensionedRopeSize * RopeMaxSizeMultiplier))
                        return true;
                }
                else
                {
                    foreach (GameObject v_chunk in Chunks)
                    {
                        bool v_chunkExceedLimit = ChunkExceedMaxSize(v_chunk);
                        if (v_chunkExceedLimit)
                            return true;
                    }
                }
            }
            return false;
        }

        protected bool ChunkExceedMaxSize(GameObject p_chunk)
        {
            if (p_chunk != null)
            {
                int v_firstIndex = -1;
                int v_lastIndex = -1;
                GetFirstAndLastNodeIndexInChunk(out v_firstIndex, out v_lastIndex, p_chunk);
                if (v_firstIndex >= 0 && v_firstIndex < Nodes.Count &&
                   v_lastIndex >= 0 && v_lastIndex < Nodes.Count)
                {
                    if (Nodes[v_firstIndex] != null && Nodes[v_lastIndex] != null)
                    {
                        float v_nonTensionedChunkSize = GetNonTensionedChunkSize(p_chunk);
                        float v_distance = Vector3.Distance(Nodes[v_firstIndex].transform.position, Nodes[v_lastIndex].transform.position);
                        if (v_distance > v_nonTensionedChunkSize * RopeMaxSizeMultiplier)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected bool NodeNeedBreak(GameObject p_object)
        {
            if (p_object != null && RopeCanBreak)
            {
                List<GameObject> v_connectedObjects = GetConnectedObjects(p_object);
                foreach (GameObject v_connectedObject in v_connectedObjects)
                {
                    float v_angle = Mathf.Abs(Vector3.Angle(p_object.transform.up, v_connectedObject.transform.up) % 360); //check angle between this chain and connected chain
                    if (v_angle > BreakAngle)//if angle is more than breakAngle variable, break chain
                        return true;
                }
            }
            return false;
        }

        float _maxTimeToCheckRope = 0.02f; //0.6f;
        float _deltaTimeToCheckRope = 0.02f; //0.6f;
        protected void ResetDeltaTimeToCheckIfRopeNeedBreak(float p_extraTime = 0.0f)
        {
            _deltaTimeToCheckRope = Mathf.Max(0, _maxTimeToCheckRope + p_extraTime);
        }

        protected virtual void CheckIfRopeNeedBreak()
        {
            bool v_canBreakInThisCycle = true;
            if (!IsRopeBroken())
            {
                if (_deltaTimeToCheckRope > 0)
                {
                    _deltaTimeToCheckRope = Mathf.Max(0, _deltaTimeToCheckRope - Time.deltaTime);
                }
                else
                {
                    v_canBreakInThisCycle = !BreakIfHaveNullNode();
                    _deltaTimeToCheckRope = _maxTimeToCheckRope;
                    if (RopeCanBreak && v_canBreakInThisCycle)
                    {
                        //Prevent to break in same point everytime
                        List<GameObject> v_shuffledNodes = Nodes.CloneList();
                        v_shuffledNodes.Shuffle();

                        List<GameObject> v_shuffledChunks = Chunks.CloneList();
                        v_shuffledChunks.Shuffle();

                        if (IsRopeTensioned())
                        {
                            foreach (GameObject v_chunk in v_shuffledChunks)
                            {
                                int v_firstIndex = -1;
                                int v_lastIndex = -1;
                                GetFirstAndLastNodeIndexInChunk(out v_firstIndex, out v_lastIndex, v_chunk);
                                if (v_firstIndex >= 0 && v_firstIndex < Nodes.Count &&
                                   v_lastIndex >= 0 && v_lastIndex < Nodes.Count)
                                {
                                    if (Nodes[v_firstIndex] != null && Nodes[v_lastIndex] != null)
                                    {
                                        if (ChunkExceedMaxSize(v_chunk))
                                        {
                                            BreakNode(v_shuffledNodes[Random.Range(v_firstIndex, v_lastIndex + 1)]);
                                            v_canBreakInThisCycle = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        //This Part Check Angle beetween nodes, so rope dont need to be tensioned
                        if (v_canBreakInThisCycle)
                        {
                            for (int i = 0; i < v_shuffledNodes.Count; i++)
                            {
                                GameObject v_node = v_shuffledNodes[i];
                                if (v_node != null)
                                {
                                    if (NodeNeedBreak(v_node))
                                    {
                                        BreakNode(v_node);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //This mode dont accept null as Node
        protected void BreakNode(GameObject p_node)
        {
            if (p_node != null && p_node.transform.parent != null)
            {
                BreakNode(Nodes.IndexOf(p_node));
            }
        }

        //This mode accept null as Node. Index must be valid!
        protected virtual void BreakNode(int p_nodeIndex)
        {
            if (p_nodeIndex >= 0 && p_nodeIndex < Nodes.Count)
            {
                GameObject v_node = Nodes[p_nodeIndex];
                _ropeIsBroken = true;
                //Add Post Nodes in Other Chunk
                GameObject v_chunk = GetChunkByNodeIndex(p_nodeIndex);
                int v_nodeIndex = p_nodeIndex;
                int v_firstIndex = -1;
                int v_lastIndex = -1;
                GetFirstAndLastNodeIndexInChunk(out v_firstIndex, out v_lastIndex, v_chunk);

                if (v_firstIndex >= 0 && v_firstIndex < Nodes.Count && v_lastIndex >= 0 && v_lastIndex < Nodes.Count)
                {
                    //Create New Chunk
                    if (v_lastIndex + 1 > v_nodeIndex + 1)
                    {
                        GameObject v_newChunk = CreateChunk("Chunk" + (Chunks.Count + 1));
                        Chunks.Add(v_newChunk);
                        for (int i = v_nodeIndex + 1; i < v_lastIndex + 1; i++)
                        {
                            if (Nodes[i] != null)
                            {
                                Vector3 v_vectorScale = Nodes[i].transform.localScale;
                                Nodes[i].transform.parent = v_newChunk.transform;
                                Nodes[i].transform.localScale = v_vectorScale;
                            }
                        }
                        Chunks.Add(v_newChunk);
                    }
                }

                //Destroy And Emit Effect
                if (LowMassVariationWhenBroken)
                    SetNodesToLowMassVariation();
                StartBreakEffects(v_node);
                if (Nodes.Count > v_nodeIndex && v_nodeIndex >= 0)
                    Nodes[v_nodeIndex] = null;
                SetNodesMaxDistanceOnly(false);
                DestroyUtils.DestroyImmediate(v_node);
                //Force Recreate LineRenderer Components
                StartLineRenderersInitialValues();
                StartRopeBreakAction();
                DestroyTensionHelper();
                if (OnRopeBreak != null)
                    OnRopeBreak(this, v_nodeIndex);
                SendMessageToAllIndirectObjects("IndirectRopeBroke");
            }
        }

        //Used After Break
        protected virtual void SetNodesToLowMassVariation()
        {
            foreach (GameObject v_node in Nodes)
            {
                if (v_node != null)
                {
                    Rigidbody2D v_body = GetRigidBody2DFromObject(v_node, false);
                    if (v_body != null)
                    {
                        RopeInternalUtils.TryClearRigidBody2D(v_body);
                        v_body.mass = Mathf.Max(0.001f, (NodeMass / 100.0f));
                    }
                }
            }
        }

        protected virtual void SetNodesMaxDistanceOnly(bool p_active)
        {
            foreach (GameObject v_node in Nodes)
            {
                if (v_node != null)
                {
                    var v_joints = GetComponentsInChildren<DistanceJoint2D>();
                    foreach (DistanceJoint2D v_joint in v_joints)
                    {
                        if(v_joint != null)
                            v_joint.maxDistanceOnly = p_active;
                    }
                }
            }
        }

        protected virtual void StartRopeBreakAction()
        {
            if (RopeBreakAction == RopeBreakActionEnum.DestroySelf)
            {
                StartCoroutine(DelayedDestroy(0.4f));
            }
        }

        protected IEnumerator DelayedDestroy(float p_initialDelay)
        {
            if (gameObject != null)
            {
                yield return new WaitForSeconds(p_initialDelay);
                var v_renderers = gameObject.GetComponentsInChildren<Renderer>();
                var v_currentTime = DestroyTime;
                if (v_currentTime < 0)
                    v_currentTime = 0;
                while (v_currentTime >= 0)
                {
                    foreach (var v_renderer in v_renderers)
                    {
                        var v_delta = DestroyTime <= 0 ? 0 : v_currentTime / DestroyTime;
                        if (v_renderer != null)
                        {
                            var v_spriteRenderer = v_renderer as SpriteRenderer;
                            if (v_spriteRenderer != null)
                            {
                                v_spriteRenderer.color = new Color(v_spriteRenderer.color.r, v_spriteRenderer.color.g, v_spriteRenderer.color.b, 1 * v_delta);
                            }
                            else
                            {
                                if(v_renderer.material.HasProperty("_Color"))
                                    v_renderer.material.color = new Color(v_renderer.material.color.r, v_renderer.material.color.g, v_renderer.material.color.b, 1 * v_delta);
                            }
                        }
                    }
                    yield return null;
                    v_currentTime -= Time.deltaTime;
                }
                RemoveObject();
            }
        }

        protected virtual void StartBreakEffects(GameObject p_node)
        {
            if (BreakEffect != null && p_node != null)
            {
                GameObject v_effectObject = GameObject.Instantiate(BreakEffect) as GameObject;
                v_effectObject.transform.position = p_node.transform.position;
                v_effectObject.transform.rotation = p_node.transform.rotation;
                v_effectObject.transform.parent = this.transform.parent;
                //Force Scale to original one
                v_effectObject.transform.localScale = BreakEffect.transform.localScale;
            }
        }

        #endregion

        #region Cut Functions

        //CutNode is Called By User(And Need UserCanCutTheRope == true), BreakNode is Called by self rope when RopeCanBreak == true.
        public virtual bool CutNode(GameObject p_node, bool p_force = false)
        {
            if (p_node != null && !IsRopeBroken() && (UserCanCutTheRope || p_force))
            {
                int v_nodeIndex = Nodes.IndexOf(p_node);
                BreakNode(p_node);
                if (OnRopeCut != null)
                    OnRopeCut(this, v_nodeIndex);
                return true;
            }
            return false;
        }

        #endregion

        #region Line Renderer Functions

        protected void UpdateRenderersVisibility()
        {
            NeedUpdateLineRenderer = false;
            //Update Node Visibility
            for (int i = 0; i < Nodes.Count; i++)
            {
                GameObject v_object = Nodes[i];
                if (v_object != null)
                {
                    SpriteRenderer v_renderer = v_object.GetComponent<SpriteRenderer>();
                    if (v_renderer != null)
                        v_renderer.enabled = !m_useLineRenderer ? true : false;
                }
            }
            StartLineRenderersInitialValues();
        }

        protected void StartLineRenderersInitialValues()
        {
            NeedUpdateRope = false;
            if (UseLineRenderer)
            {
                UpdateNodeBounds();
                Vector2 v_size = GetNodeSize();
                for (int i = 0; i < Chunks.Count; i++)
                {
                    LineRenderer v_lineRenderer = GetLineRendererComponentByChunk(i);
                    if (v_lineRenderer != null)
                    {
                        int v_vertexCount = GetNodesCountInChunk(i);
#if UNITY_5_5
                        v_lineRenderer.numPositions = v_vertexCount;
                        v_lineRenderer.widthCurve = m_lineRendererCurveWidth;
                        v_lineRenderer.widthMultiplier = Mathf.Max(0.0001f, v_size.y);
                        v_lineRenderer.numCapVertices = m_lineRendererEndCapVertices;
                        v_lineRenderer.numCornerVertices = m_lineRendererEndCapVertices;
#elif UNITY_5_6_OR_NEWER
                        v_lineRenderer.positionCount = v_vertexCount;
                        v_lineRenderer.widthCurve = m_lineRendererCurveWidth;
                        v_lineRenderer.widthMultiplier = Mathf.Max(0.0001f, v_size.y);
                        v_lineRenderer.numCapVertices = m_lineRendererEndCapVertices;
                        v_lineRenderer.numCornerVertices = m_lineRendererEndCapVertices;
#else
                        v_lineRenderer.SetVertexCount(v_vertexCount);
                        v_lineRenderer.SetWidth(Mathf.Max(0.0001f, v_size.y), Mathf.Max(0.0001f, v_size.y));
#endif

#if UNITY_5_5
                        v_lineRenderer.textureMode = LineTextureMode.Tile;
                        v_lineRenderer.sharedMaterial = RopeMaterial;
                        if (Application.isPlaying)
                            v_lineRenderer.material = new Material(RopeMaterial);
#elif UNITY_5_6_OR_NEWER
                        v_lineRenderer.textureMode = LineTextureMode.RepeatPerSegment;
                        v_lineRenderer.sharedMaterial = RopeMaterial;
                        if (Application.isPlaying)
                            v_lineRenderer.material = new Material(RopeMaterial);
#else
                        Material v_clonedMaterial = new Material(RopeMaterial);
                        v_clonedMaterial.mainTextureScale = new Vector2(v_clonedMaterial.mainTextureScale.x * v_vertexCount, v_clonedMaterial.mainTextureScale.y);
                        v_lineRenderer.sharedMaterial = v_clonedMaterial;
                        if (Application.isPlaying)
                            v_lineRenderer.material = v_clonedMaterial;
#endif
                    }
                }
                UpdateRopeSorting();
                UpdateLineRenderersNodePosition();
            }
        }

        protected void UpdateLineRenderersNodePosition()
        {
            if (UseLineRenderer)
            {
                foreach (GameObject v_chunk in Chunks)
                {
                    try
                    {
                        int v_firstIndex = -1;
                        int v_lastIndex = -1;
                        GetFirstAndLastNodeIndexInChunk(out v_firstIndex, out v_lastIndex, v_chunk);
                        LineRenderer v_linerenderer = GetLineRendererComponentByChunk(v_chunk);
                        if (v_firstIndex >= 0 && v_firstIndex < Nodes.Count && v_lastIndex >= 0 && v_lastIndex < Nodes.Count)
                        {
                            for (int i = v_firstIndex; i < v_lastIndex + 1; i++)
                            {
                                GameObject v_object = Nodes[i];
                                //Prevent Bugs in Line Renderer Setting the First and Last points to ObjectA and B
                                if (v_object != null /*&& !IsRopeBroken()*/)
                                {
                                    v_object = (i == 0 && ObjectA != null) ? ObjectA : ((i == Nodes.Count - 1 && ObjectB != null) ? ObjectB : Nodes[i]);
                                }
                                if (v_object != null && v_linerenderer != null)
                                {
                                    v_linerenderer.SetPosition(i - v_firstIndex, new Vector3(v_object.transform.position.x, v_object.transform.position.y, transform.position.z + 0.001f));
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        protected void UpdateLineRendererVisibility()
        {
            foreach (GameObject v_chunk in Chunks)
            {
                LineRenderer v_linerenderer = GetLineRendererComponentByChunk(v_chunk);
                if (v_linerenderer != null)
                    v_linerenderer.enabled = UseLineRenderer && Application.isPlaying ? true : false;
            }

        }

#endregion

#region Chunk Functions

        public virtual GameObject CreateChunk(string p_name)
        {
            GameObject v_chunk = new GameObject(p_name);
            v_chunk.transform.parent = this.transform;
            v_chunk.transform.position = Vector3.zero;
            v_chunk.transform.localScale = Vector3.one;
            v_chunk.layer = this.gameObject.layer;
            return v_chunk;
        }

        public GameObject GetChunkByNodeIndex(int p_index)
        {
            GameObject v_chunkReturn = null;
            if (p_index >= 0 && p_index < Nodes.Count)
            {
                for (int i = 0; i < Chunks.Count; i++)
                {
                    GameObject v_chunk = Chunks[i];
                    if (v_chunk != null)
                    {
                        int v_first = -1;
                        int v_last = -1;
                        GetFirstAndLastNodeIndexInChunk(out v_first, out v_last, v_chunk);
                        if (p_index >= v_first && p_index <= v_last)
                        {
                            v_chunkReturn = v_chunk;
                            break;
                        }
                    }
                }
            }
            return v_chunkReturn;
        }

        public void GetFirstAndLastNodeIndexInChunk(out int p_firstIndex, out int p_lastIndex, GameObject p_chunk)
        {
            p_firstIndex = -1;
            p_lastIndex = -1;
            if (p_chunk != null)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i] != null && Nodes[i].transform.parent == p_chunk.transform)
                    {
                        if (p_firstIndex == -1)
                            p_firstIndex = i;
                        p_lastIndex = i;
                    }
                }
            }
        }

        public LineRenderer GetLineRendererComponentByChunk(GameObject p_chunk)
        {
            if (p_chunk != null)
            {
                LineRenderer v_lineRenderer = p_chunk.GetComponent<LineRenderer>();
                if (v_lineRenderer == null)
                {
                    v_lineRenderer = p_chunk.AddComponent<LineRenderer>();
                }
                return v_lineRenderer;
            }
            return null;
        }

        public LineRenderer GetLineRendererComponentByChunk(int p_chunkIndex)
        {
            GameObject v_chunk = null;
            if (p_chunkIndex >= 0 && p_chunkIndex < Chunks.Count)
                v_chunk = Chunks[p_chunkIndex];
            return GetLineRendererComponentByChunk(v_chunk);
        }

        public int GetNodesCountInChunk(int p_chunkIndex)
        {
            GameObject v_chunk = null;
            if (p_chunkIndex >= 0 && p_chunkIndex < Chunks.Count)
                v_chunk = Chunks[p_chunkIndex];
            return GetNodesCountInChunk(v_chunk);
        }

        public int GetNodesCountInChunk(GameObject p_chunk)
        {
            int v_counter = 0;
            if (p_chunk != null)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i] != null && Nodes[i].transform.parent == p_chunk.transform)
                        v_counter++;
                }
            }
            return v_counter;
        }

#endregion

#region Others

        public virtual AnchoredJoint2D PlugDistanceOrSpringJoint(GameObject p_node1, GameObject p_node2, float p_distance, bool p_isSpringJoint, bool p_forceGenerateRigidbody = true)
        {
            AnchoredJoint2D v_anchoredJoint = null;
            Rigidbody2D v_body1 = GetRigidBody2DFromObject(p_node1);
            Rigidbody2D v_body2 = GetRigidBody2DFromObject(p_node2);
            if (p_forceGenerateRigidbody)
            {
                if (p_node1 != null && v_body1 == null)
                    v_body1 = p_node1.AddComponent<Rigidbody2D>();
                if (p_node2 != null && v_body2 == null)
                    v_body2 = p_node2.AddComponent<Rigidbody2D>();
            }
            if (v_body1 != null && v_body2 != null && v_body1 != v_body2)
            {
                v_anchoredJoint = p_isSpringJoint ? (AnchoredJoint2D)(v_body2.gameObject.AddComponent<SpringJoint2D>()) : (AnchoredJoint2D)(v_body2.gameObject.AddComponent<DistanceJoint2D>());
#if UNITY_5_3_OR_NEWER
                v_anchoredJoint.autoConfigureConnectedAnchor = false;
#endif
                v_anchoredJoint.connectedBody = v_body1;

                v_anchoredJoint.connectedAnchor = v_body1.transform.InverseTransformPoint(p_node1.transform.position);
                v_anchoredJoint.anchor = v_body2.transform.InverseTransformPoint(p_node2.transform.position);


                SpringJoint2D v_springJoint = v_anchoredJoint as SpringJoint2D;
                DistanceJoint2D v_distanceJoint = v_anchoredJoint as DistanceJoint2D;
                if (v_springJoint != null)
                {
#if UNITY_5_3_OR_NEWER
                    v_springJoint.autoConfigureConnectedAnchor = false;
                    v_springJoint.autoConfigureDistance = false;
                    v_springJoint.dampingRatio = SpringDampingValue;
#endif
                    v_springJoint.distance = p_distance;
                    v_springJoint.frequency = SpringFrequency;
                }
                if (v_distanceJoint != null)
                {
#if UNITY_5_3_OR_NEWER
                    v_distanceJoint.autoConfigureConnectedAnchor = false;
                    v_distanceJoint.autoConfigureDistance = false;
                    v_distanceJoint.maxDistanceOnly = m_maxDistanceOnlyMode;
#endif
                    v_distanceJoint.distance = p_distance;
                }
            }
            return v_anchoredJoint;
        }

        public virtual HingeJoint2D PlugHingeJoint(GameObject p_nodeOwner, GameObject p_connectedObject, bool p_forceGenerateNew = true, bool p_plugInCenter = false, bool p_forceGenerateRigidbody = true /*, bool p_useFixedAngle = false*/)
        {
            HingeJoint2D v_hingeJoint = null;
            Rigidbody2D v_nodeOwnerBody = GetRigidBody2DFromObject(p_nodeOwner);
            Rigidbody2D v_connectedObjectBody = GetRigidBody2DFromObject(p_connectedObject);
            if (p_forceGenerateRigidbody)
            {
                if (p_nodeOwner != null && v_nodeOwnerBody == null)
                    v_nodeOwnerBody = p_nodeOwner.AddComponent<Rigidbody2D>();
                if (p_connectedObject != null && v_connectedObjectBody == null)
                    v_connectedObjectBody = p_connectedObject.AddComponent<Rigidbody2D>();
            }
            if (v_nodeOwnerBody != null && v_connectedObjectBody != null)
            {
                v_hingeJoint = v_nodeOwnerBody.GetNonMarkedComponent<HingeJoint2D>();
                if (p_forceGenerateNew || v_hingeJoint == null)
                    v_hingeJoint = v_nodeOwnerBody.gameObject.AddComponent<HingeJoint2D>();
#if UNITY_5_3_OR_NEWER
                v_hingeJoint.autoConfigureConnectedAnchor = false;
#endif
                v_hingeJoint.anchor = new Vector2((GetNodeSize().x / 2f) * (1 / Mathf.Max(0.0001f, p_nodeOwner.transform.lossyScale.x)) + NodeDistanceOffSet, 0);
                Rigidbody2D v_connectedBody = GetRigidBody2DFromObject(p_connectedObject);
                Vector2 v_connectedAnchor = new Vector2((-GetNodeSize().x / 2f) * (1 / Mathf.Max(0.0001f, p_connectedObject.transform.lossyScale.x)) - NodeDistanceOffSet, 0);

                if (p_plugInCenter)
                    v_connectedAnchor = Vector2.zero;
                //Convert to real connectedBody Coordinates
                if (v_connectedBody != null)
                    v_connectedAnchor = v_connectedBody.transform.InverseTransformPoint(p_connectedObject.transform.TransformPoint(v_connectedAnchor));

                v_hingeJoint.connectedAnchor = v_connectedAnchor;
                v_hingeJoint.connectedBody = v_connectedBody;
                v_hingeJoint.enableCollision = false;
            }
            return v_hingeJoint;
        }

        public List<GameObject> GetPluggedObjectsInRope()
        {
            List<GameObject> v_list = new List<GameObject>();
            v_list.AddChecking(GetAttachedObjectA());
            v_list.AddChecking(GetAttachedObjectB());
            return v_list;
        }

        public void SendMessageToAllIndirectObjects(string p_functionToCall = "IndirectRopeDestroyed")
        {
            if (Application.isPlaying)
            {
                //Send Message To All Indirect Objects
                List<RopesAttached> v_attachedComponents = new List<RopesAttached>();
                List<GameObject> v_pluggedObjets = GetPluggedObjectsInRope();
                foreach (var v_pluggedObject in v_pluggedObjets)
                {
                    if (v_pluggedObject != null)
                    {
                        var v_ropesAtacchedComponent = v_pluggedObject.GetComponentInChildren<RopesAttached>();
                        if (v_ropesAtacchedComponent != null && !v_attachedComponents.Contains(v_ropesAtacchedComponent))
                            v_attachedComponents.Add(v_ropesAtacchedComponent);
                    }
                }
                List<GameObject> v_indirectObjects = new List<GameObject>();
                foreach (RopesAttached v_component in v_attachedComponents)
                {
                    v_indirectObjects.MergeList(v_component.GetAllIndirectObjects(true));
                }
                foreach (GameObject v_object in v_indirectObjects)
                {
                    if (v_object != null)
                        v_object.SendMessage(p_functionToCall, this, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public void DestroyRope()
        {
            ClearNodes();
            DestroyTensionHelper();
            if (OnRopeDestroyed != null)
                OnRopeDestroyed(this);
            SendMessageToAllIndirectObjects();
        }

        public void RemoveObject()
        {
            DestroyRope();
            this.gameObject.SetActive(false);
            DestroyUtils.DestroyImmediate(this.gameObject);
        }

#endregion

#region Mass Calculator

        public float GetRopeMass(bool p_multiplyByGravityScale = true)
        {
            float v_ropeMass = 0;
            foreach (GameObject v_chunk in Chunks)
                v_ropeMass += GetChunkMass(v_chunk, p_multiplyByGravityScale);
            return v_ropeMass;
        }

        public float GetMassAttachedToObject(GameObject p_object, bool p_multiplyByGravityScale = true)
        {
            float v_mass = 0;
            if (p_object != null)
            {
                if (Nodes.Count > 0)
                {
                    GameObject v_firstNode = Nodes[0];
                    GameObject v_lastNode = Nodes[Nodes.Count - 1];

                    Rigidbody2D v_rigidBodyObject = GetRigidBody2DFromObject(p_object);
                    Rigidbody2D v_objectARigidBody = GetRigidBody2DFromObject(ObjectA);
                    Rigidbody2D v_objectBRigidBody = GetRigidBody2DFromObject(ObjectB);

                    if (v_rigidBodyObject != null)
                    {
                        if (v_rigidBodyObject == v_objectARigidBody)
                        {
                            if (v_firstNode != null && v_firstNode.transform.parent != null)
                                v_mass = GetChunkMass(v_firstNode.transform.parent.gameObject, p_multiplyByGravityScale);
                        }
                        else if (v_rigidBodyObject == v_objectBRigidBody)
                        {
                            if (v_lastNode != null && v_lastNode.transform.parent != null)
                                v_mass = GetChunkMass(v_lastNode.transform.parent.gameObject, p_multiplyByGravityScale);
                        }
                    }
                }
            }
            return v_mass;
        }

        public float GetChunkMass(GameObject p_chunk, bool p_multiplyByGravityScale = true)
        {
            float v_chunkMass = 0f;
            if (p_chunk != null)
            {
                int v_firstIndex = -1;
                int v_lastIndex = -1;
                GetFirstAndLastNodeIndexInChunk(out v_firstIndex, out v_lastIndex, p_chunk);
                if (v_firstIndex >= 0 && v_firstIndex < Nodes.Count && v_lastIndex >= 0 && v_lastIndex < Nodes.Count)
                {
                    for (int i = v_firstIndex; i < v_lastIndex + 1; i++)
                    {
                        GameObject v_node = Nodes[i];
                        v_chunkMass += RopeInternalUtils.GetObjectMass(v_node, true, p_multiplyByGravityScale);
                    }
                }
            }
            return v_chunkMass;
        }

#endregion

#region Helper Functions

        public void MarkToRecreateRope()
        {
            NeedUpdateRope = true;
        }

        public float GetCurrentRopeSize()
        {
            float v_distance = 0;
            if (ObjectA != null && ObjectB != null)
                v_distance = Vector2.Distance(ObjectA.transform.position, ObjectB.transform.position);
            return v_distance;
        }

        public float GetNonTensionedRopeSize()
        {
            float v_nonTensionedRopeSize = ((AmountOfNodes + 1) * GetNodeDistance(false)) + GetNodeSize().x;
            return v_nonTensionedRopeSize;
        }

        public float GetNonTensionedChunkSize(GameObject p_chunk)
        {
            float v_nonTensionedChunkSize = 0;
            if (p_chunk != null)
            {
                int v_firstIndex = -1;
                int v_lastIndex = -1;
                GetFirstAndLastNodeIndexInChunk(out v_firstIndex, out v_lastIndex, p_chunk);
                if (v_firstIndex >= 0 && v_firstIndex < Nodes.Count &&
                   v_lastIndex >= 0 && v_lastIndex < Nodes.Count)
                {
                    int v_chunkNodesCount = GetNodesCountInChunk(p_chunk);
                    float v_offsetDistance = 0;
                    if (Nodes.GetLast() != null && Nodes.GetLast().transform.parent == p_chunk)
                    {
                        v_offsetDistance += GetNodeSize().x / 2.0f;
                        v_chunkNodesCount -= 1;
                    }
                    if (Nodes.GetFirst() != null && Nodes.GetFirst().transform.parent == p_chunk)
                    {
                        v_offsetDistance += GetNodeSize().x / 2.0f;
                        v_chunkNodesCount -= 1;
                    }
                    v_nonTensionedChunkSize = ((v_chunkNodesCount + 1) * GetNodeDistance(false)) + v_offsetDistance;
                }
            }
            return v_nonTensionedChunkSize;
        }

        public virtual void RecalcAmountOfNodes()
        {
            int v_amountOfNodes = AmountOfNodes;
            if (AutoCalculateAmountOfNodes)
            {
                float v_distance = Vector3.Distance(ObjectA.transform.position, ObjectB.transform.position);
                v_amountOfNodes = Mathf.CeilToInt(v_distance / GetNodeDistance(false)) - 2; // We must remove First and Last Node
                if (StableFirstAndLastNodes)
                    v_amountOfNodes += 1; //First and last node was plugged in center, so we must remove only one node
            }
            AmountOfNodes = Mathf.Clamp(v_amountOfNodes, 1, 100);
        }

        private void ActivatePluggedNodes()
        {
            if (ObjectA != null)
            {
                ObjectA.SetActive(true);
                GameObject v_affectedObjectA = GetAttachedObjectA();
                if (v_affectedObjectA != null)
                    v_affectedObjectA.SetActive(true);
            }
            if (ObjectB != null)
            {
                ObjectB.SetActive(true);
                GameObject v_affectedObjectB = GetAttachedObjectB();
                if (v_affectedObjectB != null)
                    v_affectedObjectB.SetActive(true);
            }
        }

        public GameObject GetAttachedObjectA()
        {
            Rigidbody2D v_body = GetRigidBody2DFromObject(ObjectA);
            if (v_body != null)
                return v_body.gameObject;
            return null;
        }

        public GameObject GetAttachedObjectB()
        {
            Rigidbody2D v_body = GetRigidBody2DFromObject(ObjectB);
            if (v_body != null)
                return v_body.gameObject;
            return null;
        }

        public Rigidbody2D GetRigidBody2DFromObject(GameObject p_object, bool p_searchInParent = true)
        {
            Rigidbody2D v_rigidBody = null;
            if (p_object != null)
            {
                v_rigidBody = p_object.GetComponent<Rigidbody2D>();
                Transform v_currentTransform = p_object.transform;
                while (v_currentTransform.parent != null && v_rigidBody == null && p_searchInParent)
                {
                    v_currentTransform = v_currentTransform.parent;
                    v_rigidBody = v_currentTransform.GetComponent<Rigidbody2D>();

                }
            }
            return v_rigidBody;
        }

        bool _ropeIsBroken = true;
        float _maxTimeToUpdateIsRopeIsBroken = 0.1f;
        float _timeToUpdateIsRopeIsBroken = 0f;
        //Check if rope is intact or not
        public bool IsRopeBroken()
        {
            if (_timeToUpdateIsRopeIsBroken > 0)
                _timeToUpdateIsRopeIsBroken = Mathf.Max(0, _timeToUpdateIsRopeIsBroken - Time.deltaTime);
            if (Nodes.Count <= 0)
                _ropeIsBroken = true;
            else if (!_ropeIsBroken && _timeToUpdateIsRopeIsBroken <= 0)
            {
                _timeToUpdateIsRopeIsBroken = _maxTimeToUpdateIsRopeIsBroken;
                if (Chunks.Count != 1)
                    _ropeIsBroken = true;
                if (!_ropeIsBroken)
                    _ropeIsBroken = AnyNodeIsBroken();
            }
            return _ropeIsBroken;
        }

        public bool AnyNodeIsBroken()
        {
            foreach (GameObject v_object in Nodes)
            {
                if (v_object == null)
                    return true;
            }
            return false;
        }

        protected virtual void CorrectValues()
        {
        }

        protected virtual void UpdateRopePosition()
        {
            if (ObjectA != null && ObjectB != null)
            {
                Vector3 v_position = ObjectA.transform.position + (ObjectB.transform.position - ObjectA.transform.position) / 2.0f;
                transform.position = v_position;
            }
        }

        protected virtual void UpdateRopeSorting()
        {
            for (int i = 0; i < Chunks.Count; i++)
            {
                LineRenderer v_lineRenderer = GetLineRendererComponentByChunk(i);
                if (v_lineRenderer != null)
                {
                    Renderer v_renderer = v_lineRenderer.GetComponent<Renderer>();
                    if (v_renderer != null)
                    {
                        try
                        {
                            v_renderer.sortingOrder = RopeDepth;
                            v_renderer.sortingLayerName = RopeSortingLayerName;
                        }
                        catch
                        {
                            m_ropeSortingLayerName = "Default";
                        }
                    }
                }
            }
        }

        protected virtual void UpdateNodesSorting()
        {
            foreach (GameObject v_object in Nodes)
            {
                if (v_object != null)
                {
                    Renderer v_renderer = v_object.GetComponent<Renderer>();
                    if (v_renderer != null)
                    {
                        try
                        {
                            v_renderer.sortingOrder = RopeDepth;
                            v_renderer.sortingLayerName = RopeSortingLayerName;
                        }
                        catch
                        {
                            m_ropeSortingLayerName = "Default";
                        }
                    }
                }
            }
        }

        float _nodeDistance = 0;
        float _nodeDistanceInGlobalScale = 0;
        public float GetNodeDistance(bool p_inLocalScale = true)
        {
            return p_inLocalScale ? _nodeDistance : _nodeDistanceInGlobalScale;
        }

        Vector2 _nodeSize = new Vector2(0.0001f, 0.0001f);
        public Vector2 GetNodeSize()
        {
            if (_nodeSize.x <= 0 || _nodeSize.y <= 0)
                _nodeSize = new Vector2(Mathf.Max(0.0001f, _nodeSize.x), Mathf.Max(0.0001f, _nodeSize.y));
            return _nodeSize;
        }

        public void UpdateNodeBounds()
        {
            GameObject v_boundsObject = new GameObject("TempObject");
            v_boundsObject.hideFlags = HideFlags.HideAndDontSave;
            v_boundsObject.SetActive(false);
            if (NodeSprite != null)
            {
                SpriteRenderer v_renderer = v_boundsObject.AddComponent<SpriteRenderer>();
                v_renderer.sprite = NodeSprite;
                v_boundsObject.transform.localScale = Vector3.one;
                m_nodeSpriteGlobalSize = v_renderer.bounds.size;
                //Set Local scale and get _nodeSize in LocalScale
                if ((Application.isPlaying && !Application.isEditor) || (Application.isEditor && !RopeInternalUtils.IsPrefab(this.gameObject)))
                    v_boundsObject.transform.parent = this.transform;
                v_boundsObject.transform.localScale = NodeLocalScale;
                _nodeSize = v_renderer.bounds.size;
            }
            else
            {
                //use Global Size To Recalc This values;
                if ((Application.isPlaying && !Application.isEditor) || (Application.isEditor && !RopeInternalUtils.IsPrefab(this.gameObject)))
                    v_boundsObject.transform.parent = this.transform;
                v_boundsObject.transform.localScale = NodeLocalScale;
                _nodeSize = new Vector2(NodeSpriteGlobalSize.x * v_boundsObject.transform.lossyScale.x, NodeSpriteGlobalSize.y * v_boundsObject.transform.lossyScale.y);
            }
            _nodeSize = new Vector2(_nodeSize.x * 0.97f, _nodeSize.y);
            _nodeDistance = ((_nodeSize.x / 2f) * (1 / Mathf.Max(0.0001f, v_boundsObject.transform.lossyScale.x)) + NodeDistanceOffSet) * 2;

            //Calc Size in Local Scale to make this the smallest possible value to Distance
            float _nodeSizeXinLocalScale = _nodeSize.x / Mathf.Max(0.0001f, v_boundsObject.transform.lossyScale.x * 0.97f);
            //_nodeDistance = Mathf.Max(_nodeSizeXinLocalScale, _nodeDistance);
            _nodeDistance = Mathf.Max(Mathf.Min(1.4f, _nodeSizeXinLocalScale), _nodeDistance);
            _nodeDistanceInGlobalScale = _nodeDistance * Mathf.Max(0.0001f, v_boundsObject.transform.lossyScale.x);

            DestroyUtils.DestroyImmediate(v_boundsObject);
        }

        public void ClearChunks()
        {
            for (int i = 0; i < Chunks.Count; i++)
            {
                GameObject v_chunk = Chunks[i];
                LineRenderer v_renderer = GetLineRendererComponentByChunk(i);
                if (v_renderer != null)
                {
                    if (Application.isEditor)
                    {
#if !UNITY_5_5_OR_NEWER
                        if (v_renderer.sharedMaterial != null)
                            Renderer.DestroyImmediate(v_renderer.sharedMaterial, false);
#endif
                    }
                    else
                    {
                        if (v_renderer.material != null)
                            Renderer.Destroy(v_renderer.material);
                    }
                }
                if (v_chunk != null)
                {
                    if (Application.isEditor && !Application.isPlaying)
                        GameObject.DestroyImmediate(v_chunk);
                    else
                        GameObject.Destroy(v_chunk);
                }
            }
            foreach (Transform v_transform in transform)
            {
                if (v_transform != null)
                {
                    if (Application.isEditor && !Application.isPlaying)
                        GameObject.DestroyImmediate(v_transform.gameObject);
                    else
                        GameObject.Destroy(v_transform.gameObject);
                }
            }
            Chunks.Clear();
        }

        public void ClearNodes()
        {
            ClearChunks();
            Nodes.Clear();
        }

#endregion

#region Static Functions

        public static void RegisterRopeInScene(Rope2D p_rope)
        {
            List<Rope2D> v_ropeList = new List<Rope2D>(s_allRopesInScene);
            v_ropeList.AddChecking(p_rope);
            s_allRopesInScene = v_ropeList.ToArray();
        }

        public static void UnregisterRopeFromScene(Rope2D p_rope)
        {
            List<Rope2D> v_ropeList = new List<Rope2D>(s_allRopesInScene);
            v_ropeList.RemoveChecking(p_rope);
            s_allRopesInScene = v_ropeList.ToArray();
        }

#if UNITY_EDITOR

        [MenuItem("KILT/Helper/Ropes/Clean Useless Ropes")]
        public static void CleanUselessRopes()
        {
            if (s_allRopesInScene != null)
            {
                foreach (Rope2D v_rope in s_allRopesInScene)
                {
                    if (v_rope != null && v_rope.GetPluggedObjectsInRope().Count < 2)
                    {
                        DestroyUtils.Destroy(v_rope.gameObject);
                    }
                }
            }
        }
        //Dont Need to Call CleanUselessRopes by your own hands, do it via this functions
        [UnityEditor.Callbacks.PostProcessScene]
        private static void PostProcessSceneFunctions()
        {
            if (!Application.isPlaying)
                CleanUselessRopes();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void PostProcessReloadScriptsFunctions()
        {
            CleanUselessRopes();
        }

#endif

#endregion

#region Internal Editor Functions

        protected void RecordObject()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Rope Modifications");
#endif
        }

#endregion
    }
}

