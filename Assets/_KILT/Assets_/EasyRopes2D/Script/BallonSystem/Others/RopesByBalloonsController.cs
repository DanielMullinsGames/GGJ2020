using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kilt.EasyRopes2D
{
    public class RopesByBalloonsController : MonoBehaviour
    {

        #region Singleton

        private static RopesByBalloonsController m_instance = null;

        public static RopesByBalloonsController Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = GameObject.FindObjectOfType(typeof(RopesByBalloonsController)) as RopesByBalloonsController;
                    if (m_instance == null)
                    {
                        RopesByBalloonsController v_object = new GameObject("RopesByBalloonsController").AddComponent<RopesByBalloonsController>();
                        m_instance = v_object;
                    }
                }

                return m_instance;
            }
            protected set
            {
                m_instance = value;
            }
        }

        #endregion

        #region Private Variables

        List<RopesByBallons> _ropesByBallonsList = new List<RopesByBallons>();
        protected bool _needRemoveNulls = false;

        #endregion

        #region Public Variables

        public List<RopesByBallons> RopesByBallonsList
        {
            get
            {
                if (_ropesByBallonsList == null)
                    _ropesByBallonsList = new List<RopesByBallons>();
                return _ropesByBallonsList;
            }
            protected set
            {
                _ropesByBallonsList = value;
            }
        }

        #endregion

        #region Unity Functions

        protected virtual void LateUpdate()
        {
            RemoveNullRefs();
        }

        #endregion

        #region Helper Functions

        protected virtual RopesByBallons GetRopeByBallonStructureInternal(Rope2D p_rope, bool p_registerNewIfDontFind = true)
        {
            RopesByBallons v_found = null;
            if (p_rope != null)
            {
                foreach (RopesByBallons v_struct in RopesByBallonsList)
                {
                    if (v_struct != null && v_struct.Rope != null)
                    {
                        if (v_struct.Rope == p_rope)
                        {
                            v_found = v_struct;
                            break;
                        }
                    }
                    else
                        _needRemoveNulls = true;
                }
            }
            if (p_registerNewIfDontFind && v_found == null)
            {
                v_found = new RopesByBallons(p_rope);
                RopesByBallonsList.Add(v_found);
            }
            return v_found;
        }

        protected virtual List<RopesByBallons> GetAllStructsWithBallonInternal(BalloonProperty p_ballon)
        {
            List<RopesByBallons> v_finalList = new List<RopesByBallons>();
            if (p_ballon != null)
            {
                foreach (RopesByBallons v_struct in RopesByBallonsList)
                {
                    if (v_struct != null && v_struct.Rope != null)
                    {
                        if (v_struct.DirectBallons.Contains(p_ballon))
                            v_finalList.Add(v_struct);
                        else if (v_struct.IndirectBallons.Contains(p_ballon))
                            v_finalList.Add(v_struct);
                    }
                    else
                        _needRemoveNulls = true;
                }
            }
            return v_finalList;
        }

        protected virtual void RemoveNullRefs(bool p_force = false)
        {
            if (_needRemoveNulls || p_force)
            {
                _needRemoveNulls = false;
                List<RopesByBallons> p_newList = new List<RopesByBallons>();
                foreach (RopesByBallons v_struct in RopesByBallonsList)
                {
                    if (v_struct != null && v_struct.Rope != null)
                        p_newList.Add(v_struct);
                }
                RopesByBallonsList = p_newList;
            }
        }

        #endregion

        #region Static Functions

        public static bool InstanceExists()
        {
            return GetInstance(false) == null ? false : true;
        }

        public static RopesByBalloonsController GetInstance(bool p_canCreateANewOne = false)
        {
            RopesByBalloonsController v_instance = null;
            if (p_canCreateANewOne)
                v_instance = Instance;
            else
            {
                if (m_instance == null)
                    m_instance = GameObject.FindObjectOfType(typeof(RopesByBalloonsController)) as RopesByBalloonsController;
                v_instance = m_instance;
            }
            return v_instance;
        }

        public static RopesByBallons GetRopeByBallonStructure(Rope2D p_rope, bool p_registerNewIfDontFind = true, bool p_canCreateNewControllerIfDontExists = true)
        {
            RopesByBalloonsController v_instance = GetInstance(p_canCreateNewControllerIfDontExists);
            RopesByBallons v_return = null;
            if (v_instance != null)
                v_return = v_instance.GetRopeByBallonStructureInternal(p_rope, p_registerNewIfDontFind);
            return v_return;
        }

        public static List<RopesByBallons> GetAllStructsWithBallon(BalloonProperty p_ballon, bool p_canCreateNewControllerIfDontExists = true)
        {
            RopesByBalloonsController v_instance = GetInstance(p_canCreateNewControllerIfDontExists);
            List<RopesByBallons> v_returnList = new List<RopesByBallons>();
            if (v_instance != null)
                v_returnList = v_instance.GetAllStructsWithBallonInternal(p_ballon);
            return v_returnList;
        }

        #endregion
    }

    #region Helper Classes

    [System.Serializable]
    public class RopesByBallons
    {
        #region Private Variables

        [SerializeField]
        Rope2D m_rope;
        [SerializeField]
        List<BalloonProperty> m_directBallons = new List<BalloonProperty>();
        [SerializeField]
        List<BalloonProperty> m_indirectBallons = new List<BalloonProperty>();

        #endregion

        #region Public Properties

        public Rope2D Rope { get { return m_rope; } set { m_rope = value; } }
        public List<BalloonProperty> DirectBallons
        {
            get
            {
                if (m_directBallons == null)
                    m_directBallons = new List<BalloonProperty>();
                return m_directBallons;
            }
            set { m_directBallons = value; }
        }
        public List<BalloonProperty> IndirectBallons
        {
            get
            {
                if (m_indirectBallons == null)
                    m_indirectBallons = new List<BalloonProperty>();
                return m_indirectBallons;
            }
            set { m_indirectBallons = value; }
        }

        #endregion

        #region Constructor

        public RopesByBallons()
        {
        }

        public RopesByBallons(Rope2D p_rope)
        {
            m_rope = p_rope;
        }

        #endregion
    }

    #endregion
}
