﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

namespace GraphTheory
{
    [Serializable]
    public abstract class ANode
    {
        [SerializeField, HideInInspector]
        private string m_id = "";
        public string Id { get { return m_id; } }
        [SerializeField, HideInInspector]
        private List<OutportEdge> m_outports = new List<OutportEdge>(0);

        protected NodeCollection ParentNodeCollection { get; private set; } = null;

        public ANode()
        {
            m_id = Guid.NewGuid().ToString();
        }

        public virtual void OnNodeEnter(NodeCollection nodeCollection) { ParentNodeCollection = nodeCollection; }
        public virtual void OnNodeUpdate() { }
        public virtual void OnNodeExit() { }

        public OutportEdge GetOutportEdge(int index)
        {
            return m_outports[index];
        }

        [SerializeField, HideInInspector]
        private Vector2 m_position;
        [SerializeField, HideInInspector]
        private string m_comment;
#if UNITY_EDITOR
        public abstract string Name { get; }
        public int NumOutports { get { return m_outports.Count; } }
        public abstract List<Type> CompatibleGraphs { get; }//Returning null = compatible to all. Returning empty list = compatible to none.

        public Vector2 Position { get { return m_position; } set { m_position = value; } }
        public virtual Vector2 Size { get { return new Vector2(600, 300); } }
        public virtual Color NodeColor { get { return Color.gray; } }

        public virtual void DrawNodeView(Node nodeView)
        {
        }

        public void SanitizeNodeCopy(string newId, Vector2 position, Dictionary<string, string> oldToNewIdList)
        {
            m_id = newId;
            m_position = position;
            for(int i = m_outports.Count - 1; i >= 0 ; i--)
            {
                m_outports[i].Id = Guid.NewGuid().ToString();
                if (oldToNewIdList.TryGetValue(m_outports[i].ConnectedNodeId, out string foundId))
                {
                    m_outports[i].ConnectedNodeId = foundId;
                }
                else
                {
                    m_outports[i].SetInvalid();
                }
            }
        }

        public void CreateOutport()
        {
            m_outports.Add(new OutportEdge());
        }

        public void DestroyOutport(int index)
        {
            m_outports.RemoveAt(index);
        }

        public List<OutportEdge> GetAllEdges()
        {
            return m_outports;
        }
        
        public void AddOutportEdge(int outportIndex, OutportEdge outportEdge)
        {
            if(outportIndex > m_outports.Count - 1)
            {
                Debug.LogError("Error adding outport edge!");
                return;
            }
            m_outports[outportIndex] = outportEdge;
        }

        public void RemoveOutportEdge(int outportIndex)
        {
            if (outportIndex > m_outports.Count - 1)
            {
                Debug.LogError("Error removing outport edge!");
                return;
            }
            m_outports[outportIndex].SetInvalid();
        }
#endif
    }
}