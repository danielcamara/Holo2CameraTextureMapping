//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HUX.Collections
{
    /// <summary>
    /// A utility that stores transform information for objects in a collection
    /// This info can then be used for non-destructive realtime manipulation
    /// Or to restore an earlier configuration gathered on startup
    /// </summary>
    public class ObjectCollectionDynamic : MonoBehaviour
    {
        public enum BehaviorEnum
        {
            StoreEveryTime,
            StoreOnceOnStartup,
            StoreManually,
        }

        /// <summary>
        /// Extends collection node class to include stored local position / rotation data
        /// </summary>
        [Serializable]
        public class CollectionNodeDynamic : ObjectCollection.CollectionNode
        {
            public CollectionNodeDynamic(ObjectCollection.CollectionNode node)
            {
                this.transform = node.transform;
                this.Name = node.Name;
                this.Offset = node.Offset;
                this.Radius = node.Radius;

                this.localPositionOnStartup = transform.localPosition;
                this.localEulerAnglesOnStartup = transform.localEulerAngles;
            }

            public CollectionNodeDynamic (Transform nodeTransform)
            {
                this.transform = nodeTransform;
                this.Name = nodeTransform.name;
                this.Offset = Vector2.zero;
                this.Radius = 0f;

                this.localPositionOnStartup = transform.localPosition;
                this.localEulerAnglesOnStartup = transform.localEulerAngles;
            }

            public Vector3 localPositionOnStartup;
            public Vector3 localEulerAnglesOnStartup;
        }

        /// <summary>
        /// When to gather the dynamic node information
        /// </summary>
        public BehaviorEnum Behavior = BehaviorEnum.StoreEveryTime;

        /// <summary>
        /// List of dynamic nodes in the collection
        /// </summary>
        public List<CollectionNodeDynamic> DynamicNodeList;

        [SerializeField]
        private ObjectCollection collection;

        private bool updatedOnce = false;

        /// <summary>
        /// Sets each node in the collection to its last stored arrangement
        /// Automatically prunes destroyed or removed nodes
        /// Does not handle nodes added since last stored arrangement
        /// </summary>
        public virtual void RestoreArrangement()
        {
            if (DynamicNodeList == null)
                return;

            for (int i = DynamicNodeList.Count - 1; i >= 0; i--)
            {
                if (DynamicNodeList[i].transform == null || DynamicNodeList[i].transform.parent != collection.transform)
                {
                    DynamicNodeList.RemoveAt(i);
                } else
                {
                    DynamicNodeList[i].transform.localPosition = DynamicNodeList[i].localPositionOnStartup;
                    DynamicNodeList[i].transform.localEulerAngles = DynamicNodeList[i].localEulerAnglesOnStartup;
                }
            }
        }

        /// <summary>
        /// Manually store collection arrangement
        /// </summary>
        public void StoreArrangement ()
        {
            if (collection == null)
                return;

            DynamicNodeList = new List<CollectionNodeDynamic>();
            if (collection.NodeList == null || collection.NodeList.Count != transform.childCount)
            {
                foreach (Transform child in transform)
                {
                    DynamicNodeList.Add(new CollectionNodeDynamic(child));
                }
            }
            else
            {
                foreach (ObjectCollection.CollectionNode node in collection.NodeList)
                {
                    DynamicNodeList.Add(new CollectionNodeDynamic(node));
                }
            }
        }

        void Awake()
        {
            if (collection == null)
            {
                collection = gameObject.GetComponent<ObjectCollection>();
            }
        }

        void Start()
        {
            switch (Behavior)
            {
                case BehaviorEnum.StoreManually:
                default:
                    // Don't do anything
                    break;

                case BehaviorEnum.StoreEveryTime:
                    // Subscribe to the collection's update events
                    collection.OnCollectionUpdated += OnCollectionUpdated;
                    break;

                case BehaviorEnum.StoreOnceOnStartup:
                    // If we're only supposed to update once
                    // Gather our list of nodes now
                    StoreArrangement();
                    break;
            }
        }

        void OnCollectionUpdated (ObjectCollection collection)
        {
            StoreArrangement();
        }
    }
}
