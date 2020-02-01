using UnityEngine;
using System.Collections;

namespace Kilt.EasyRopes2D.Examples
{
    public class FragmentContainer : MonoBehaviour
    {
        [SerializeField]
        float m_lifeTime = 0.5f;

        public float LifeTime
        {
            get
            {
                return m_lifeTime;
            }
            set
            {
                if (m_lifeTime == value)
                    return;
                m_lifeTime = value;
            }
        }

        protected void Start()
        {
            StartCoroutine(DelayedDestroy(this.gameObject, LifeTime, 0.4f, false));
        }

        protected IEnumerator DelayedDestroy(GameObject p_target, float p_lifeTime, float p_initialDelay, bool p_ignoreTimeScale)
        {
            if (p_target != null)
            {

                if (p_ignoreTimeScale)
                    yield return new WaitForSecondsRealtime(p_initialDelay);
                else
                    yield return new WaitForSeconds(p_initialDelay);
                var v_renderers = p_target.GetComponentsInChildren<Renderer>();
                var v_currentTime = p_lifeTime;
                if (v_currentTime < 0)
                    v_currentTime = 0;
                while (v_currentTime >= 0)
                {
                    foreach (var v_renderer in v_renderers)
                    {
                        var v_delta = p_lifeTime <= 0 ? 0 : v_currentTime / p_lifeTime;
                        if (v_renderer != null)
                        {
                            var v_spriteRenderer = v_renderer as SpriteRenderer;
                            if (v_spriteRenderer != null)
                            {
                                v_spriteRenderer.color = new Color(v_spriteRenderer.color.r, v_spriteRenderer.color.g, v_spriteRenderer.color.b, 1 * v_delta);
                            }
                            else
                            {
                                if (v_renderer.material.HasProperty("_Color"))
                                    v_renderer.material.color = new Color(v_renderer.material.color.r, v_renderer.material.color.g, v_renderer.material.color.b, 1 * v_delta);
                            }
                        }
                    }
                    yield return null;
                    v_currentTime -= p_ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                }
                DestroyUtils.Destroy(p_target);
            }
        }
    }
}
