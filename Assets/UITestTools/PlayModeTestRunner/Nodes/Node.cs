using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using PlayQ.UITestTools;
using UnityEngine;

namespace Tests.Nodes
{
    public enum TestState
    {
        Undefined,
        Passed,
        Ignored,
        Failed
    }
    
    public abstract class Node
    {
        public event Action StateUpdated;


        [JsonProperty]
        public string Name { get; private set; }

        public abstract string FullName { get; }
        
        [JsonIgnore]
        public abstract TestState State { get; }
        
        [JsonProperty]
        private bool isHided;

        [JsonIgnore]
        public bool IsHided
        {
            get 
            {
                return isHided;
            }
        }

        [JsonProperty]
        protected TestState cachedState;
        protected bool stateIsDirty = true;
        
        protected void UpdateParentTestState()
        {
            if (StateUpdated != null)
            {
                StateUpdated();
            }
            stateIsDirty = true;
            if (parent != null)
            {
                parent.UpdateParentTestState(); 
            }
        }
        
        public bool IsChildOf(Node possibleParent)
        {
            if (parent != null)
            {
                if (parent == possibleParent)
                {
                    return true;
                }
                else
                {
                    return parent.IsChildOf(possibleParent);
                }
            }
            return false;
        }
        
        protected List<MethodInfo> MethodNamesToInfos(Type type, List<string> names)
        {
            List<MethodInfo> infos = new List<MethodInfo>();
            foreach (var setupMethodName in names)
            {
                var methodInfo = type.GetMethod(setupMethodName);
                infos.Add(methodInfo);    
            }
            return infos;
        }
        protected List<string> MethodInfosToNames(List<MethodInfo> infos)
        {
            List<string> names = new List<string>();
            foreach (var info in infos)
            {
                names.Add(info.Name);
            }
            return names;
        }
        
        public void SetHided(bool isHided)
        {
            if (this.isHided != isHided)
            {
                this.isHided = isHided;
                if (parent != null)
                {
                    if (isHided)
                    {
                        parent.hidedChildren++;
                    }
                    else
                    {
                        parent.hidedChildren--;
                    }

                    if (parent.hidedChildren > parent.ChildrenCount)
                    {
                        Debug.LogError("hidedChildren can't be greater than children count for "
                                       + FullName);
                        parent.hidedChildren = parent.ChildrenCount;
                    }
                    if (parent.hidedChildren < 0)
                    {
                        Debug.LogError("hidedChildren can't be less than 0 for " + FullName);
                        parent.hidedChildren = 0;
                    }
                    
                    parent.SetHided(parent.hidedChildren == parent.ChildrenCount);    
                }    
            }
        }

        [JsonProperty]
        private int hidedChildren;
        
        
        [JsonProperty]
        private bool isOpened;

        [JsonIgnore]
        public bool IsOpened
        {
            get
            {
                return isOpened || children.Count == 0;
            }
        }
        public void SetOpened(bool isOpened)
        {
            this.isOpened = isOpened;
        }
        
        
        [JsonIgnore]
        public bool IsSemiSelected
        {
            get
            {
                return semiSelectedChildren > 0 || (ChildrenCount == 0 && isSelected);
            }
        }

        [JsonProperty]
        private bool isSelected;

        [JsonIgnore]
        public bool IsSelected
        {
            get { return isSelected; }
        }

        [JsonProperty]
        private int selectedChildren;
        [JsonProperty]
        private int semiSelectedChildren;

        private void SetSelectionToParents(bool isSelected)
        {
            if (parent != null)
            {
                if (isSelected)
                {
                    parent.selectedChildren++;
                }
                else
                {
                    parent.selectedChildren--;
                }

                if (parent.selectedChildren > parent.ChildrenCount)
                {
                    Debug.LogError("selectedChildren can't be greater than children count for "
                                   + FullName);
                    parent.selectedChildren = parent.ChildrenCount;
                }
                if (parent.selectedChildren < 0)
                {
                    Debug.LogError("selectedChildren can't be less than 0 for " + FullName);
                    parent.selectedChildren = 0;
                }

                if (parent.ChildrenCount > 0)
                {
                    if (parent.selectedChildren == parent.ChildrenCount && !parent.isSelected)
                    {
                        parent.isSelected = true;
                        parent.SetSelectionToParents(true);
                    }
                    else if(parent.isSelected)
                    {
                        parent.isSelected = false;
                        parent.SetSelectionToParents(isSelected);
                    }   
                }
            }
        }
        
        private void SetSemiSelectionToParents(bool isSelected)
        {
            if (parent != null)
            {
                bool parentOldSemiSelecton = parent.IsSemiSelected;
                if (isSelected)
                {
                    parent.semiSelectedChildren++;
                }
                else
                {
                    parent.semiSelectedChildren--;
                }

                if (parent.semiSelectedChildren > parent.ChildrenCount)
                {
                    Debug.LogError("semiSelectedChildren can't be greater than children count for "
                                   + FullName);
                    parent.semiSelectedChildren = parent.ChildrenCount;
                }
                if (parent.semiSelectedChildren < 0)
                {
                    Debug.LogError("semiSelectedChildren can't be less than 0 for " + FullName);
                    parent.semiSelectedChildren = 0;
                }

                if (parent.IsSemiSelected != parentOldSemiSelecton)
                {
                    parent.SetSemiSelectionToParents(isSelected);    
                }
            }
        }
        
        private void SetSelectedToChildren(bool isSelected)
        {
            if (this.isSelected != isSelected)
            {
                this.isSelected = isSelected;
                var childCound = children.Count;
                selectedChildren = isSelected ? childCound : 0; 
                semiSelectedChildren = isSelected ? childCound : 0;
                for (int i = 0; i < childCound; i++)
                {
                    children[i].SetSelectedToChildren(isSelected);
                }
            }
        }
       
        public void SetSelected(bool isSelected)
        {
            if (this.isSelected != isSelected)
            {
                bool oldSemiselected = IsSemiSelected;
                SetSelectedToChildren(isSelected);
                SetSelectionToParents(isSelected);

                if (oldSemiselected != IsSemiSelected)
                {
                    SetSemiSelectionToParents(isSelected);
                }
            }
        }        

        [JsonProperty]
        protected Node parent;

        [JsonIgnore]
        public Node Parent
        {
            get { return parent; }
        }

        [JsonIgnore]
        public bool IsRoot
        {
            get { return parent == null; }
        }

        public int Index { get; private set; }

        [JsonProperty]
        protected List<Node> children;
        [JsonProperty]
        protected Dictionary<string, Node> nameToChild = new Dictionary<string, Node>();

        public Node ChildByName(string name)
        {
            Node child;
            nameToChild.TryGetValue(name, out child);
            return child;
        }
        
        [JsonIgnore]
        public IEnumerable<Node> Children
        {
            get { return children; }
        }

        [JsonIgnore]
        public int ChildrenCount
        {
            get { return children.Count; }
        }

        public Node ChildAt(int index)
        {
            return children[index];
        }

        [JsonConstructor]
        protected Node()
        {
            
        }
        
        public Node(string name, Node parent)
        {
            children = new List<Node>();

            Name = name;
            this.parent = parent;

            if (parent != null)
            {
                Index = parent.Children.Count();
                parent.AddChildNode(this);
            }
        }

        public void AddChildNode(Node childNode)
        {
            children.Add(childNode);
            nameToChild[childNode.Name] = childNode;
        }

        public virtual bool Contains(string pattern)
        {
            return children.Any(item => item.Contains(pattern));
        }


        public List<T> GetChildrenOfType<T>(bool recursive = true,
            bool strictTypeComparison = false, Func<Node, bool> predicate = null)
        where T : Node
        {
            List<T> result = new List<T>();
            GetChildrenOfTypeInner(result, recursive, strictTypeComparison, predicate);
            return result;
        }

        private void GetChildrenOfTypeInner<T>(List<T> result, bool recursive = true,
            bool strictTypeComparison = false, Func<Node, bool> predicate = null)
            where T : Node
        {
            var count = children.Count;
            for (int i = 0; i < count; i++)
            {
                var child = children[i];
                var typeCheck = false;
                if (strictTypeComparison)
                {
                    if (child.GetType() == typeof(T))
                    {
                        typeCheck = true;
                    }
                }
                else
                {
                    if (child is T)
                    {
                        typeCheck = true;
                    }
                }

                if (typeCheck)
                {
                    if (predicate == null)
                    {
                        result.Add((T)child);
                    }
                    else if (predicate(child))
                    {
                        result.Add((T)child);
                    }
                }
                
                if (recursive)
                {
                    child.GetChildrenOfTypeInner(result, true, strictTypeComparison,predicate);
                }
            }
        }

        public void MergeWithRoot(Node other)
        {
            if (IsRoot && other.IsRoot && Name == other.Name)
            {
                MergeWithNode(other);
            }
        }

        private void MergeWithNode(Node other)
        {
            SetDataFromNode(other);
            foreach (var child in children)
            {
                Node otherChild;
                if (other.nameToChild.TryGetValue(child.Name, out otherChild))
                {
                    child.MergeWithNode(otherChild);
                }
            }
        }

        protected virtual void SetDataFromNode(Node other)
        {
            if (other.GetType() == GetType())
            {
                isSelected = other.isSelected;
                semiSelectedChildren = other.semiSelectedChildren;
                selectedChildren = other.selectedChildren;
                isOpened = other.isOpened;
            }
            else
            {
                Debug.LogError("Can't set data from node of type " + other.GetType() +
                               " to current node of type " + GetType());
            }
        }

        
#if UNITY_EDITOR
        
        private BaseNodeView view;
        public void SetView(BaseNodeView view)
        {
            this.view = view;
        }

        [JsonIgnore]
        public BaseNodeView View
        {
            get { return view; }
        }
#endif
    }
}