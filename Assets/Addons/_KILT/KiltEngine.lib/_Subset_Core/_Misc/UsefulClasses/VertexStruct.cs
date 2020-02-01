using UnityEngine;
using System.Collections;

namespace Kilt
{
    [System.Serializable]
    public struct VertexStruct
    {
        #region Private Variables

        [SerializeField]
        Vector3 m_position;
        [SerializeField]
        Vector2 m_uv;
        [SerializeField]
        Vector2 m_uv1;
        [SerializeField]
        Color m_color;

        #endregion

        #region Public Properties

        public Vector3 Position
        {
            get
            {
                return m_position;
            }
            set
            {
                if (m_position == value)
                    return;
                m_position = value;
            }
        }

        public Vector2 UV
        {
            get
            {
                return m_uv;
            }
            set
            {
                if (m_uv == value)
                    return;
                m_uv = value;
            }
        }

        public Vector2 UV1
        {
            get
            {
                return m_uv1;
            }
            set
            {
                if (m_uv1 == value)
                    return;
                m_uv1 = value;
            }
        }

        public Color Color
        {
            get
            {
                return m_color;
            }
            set
            {
                if (m_color == value)
                    return;
                m_color = value;
            }
        }

        #endregion

        #region Constructors

        public VertexStruct(Vector3 p_position, Vector2 p_uv, Vector2 p_uv1, Color p_color)
        {
            m_position = p_position;
            m_uv = p_uv;
            m_uv1 = p_uv1;
            m_color = p_color;
        }
        public VertexStruct(Vector3 p_position, Vector2 p_uv, Color p_color)
        {
            m_position = p_position;
            m_uv = p_uv;
            m_uv1 = Vector2.zero;
            m_color = p_color;
        }

        #endregion
    }
}
