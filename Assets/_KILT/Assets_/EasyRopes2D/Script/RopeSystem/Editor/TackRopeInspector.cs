using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Kilt;
using Kilt.EasyRopes2D;
using Kilt.Extensions;

namespace KiltEditor.EasyRopes2D
{
    [CustomEditor(typeof(TackRope))]
    public class TackRopeInspector : Rope2DInspector
    {
        TackRope m_controllerTack;

        protected override void DrawAttachedObjects()
        {
            if (m_controllerTack == null)
                m_controllerTack = target as TackRope;

            //Attached Objects Properties
            Color v_oldColor = GUI.backgroundColor;
            DrawTitleText("Attached Objects Properties");
            GUILayout.Label("Attached Object A");
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GameObject v_attachedObjectA = EditorGUILayout.ObjectField(m_controllerTack.GetAttachedObjectA(), typeof(GameObject), true) as GameObject;
            if (v_attachedObjectA != m_controllerTack.GetAttachedObjectA())
                m_controllerTack.ObjectA = v_attachedObjectA;
            if (v_attachedObjectA != null)
            {
                TacksInventory v_tackController = v_attachedObjectA.GetNonMarkedComponentInChildren<TacksInventory>();
                if (v_tackController != null)
                {
                    v_tackController.FillWithChildrenTacks(true);
                    GUI.backgroundColor = m_controllerTack.TackAIndex >= 0 && m_controllerTack.TackAIndex < v_tackController.TacksInObject.Count ? Color.green : Color.red;
                    m_controllerTack.TackAIndex = EditorGUILayout.Popup(m_controllerTack.TackAIndex, v_tackController.TacksInObject.GetStringList().ToArray());
                    GUI.backgroundColor = v_oldColor;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Attached Object B");
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GameObject v_attachedObjectB = EditorGUILayout.ObjectField(m_controllerTack.GetAttachedObjectB(), typeof(GameObject), true) as GameObject;
            if (v_attachedObjectB != m_controllerTack.GetAttachedObjectB())
                m_controllerTack.ObjectB = v_attachedObjectB;
            if (v_attachedObjectB != null)
            {
                TacksInventory v_tackController = v_attachedObjectB.GetNonMarkedComponentInChildren<TacksInventory>();
                if (v_tackController != null)
                {
                    v_tackController.FillWithChildrenTacks(true);
                    GUI.backgroundColor = m_controllerTack.TackBIndex >= 0 && m_controllerTack.TackBIndex < v_tackController.TacksInObject.Count ? Color.green : Color.red;
                    m_controllerTack.TackBIndex = EditorGUILayout.Popup(m_controllerTack.TackBIndex, v_tackController.TacksInObject.GetStringList().ToArray());
                    GUI.backgroundColor = v_oldColor;
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }
    }
}
