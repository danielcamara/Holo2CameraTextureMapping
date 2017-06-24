﻿using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;
using HoloToolkit.Unity;
using System.Collections.Generic;
using System;
using System.Collections;

namespace HoloCameraTextureMapping
{
    public class TextureMappingManager : Singleton<TextureMappingManager>
    {
        public GameObject SpatialMapping;
        public Material TextureMappingMaterial;

        public List<GameObject> SampleObjects;

        public event Action SpatialMappingCreated = delegate { };
        private bool scanComplete = false;

        private new void Awake()
        {
            base.Awake();
            var spatialMappingSources = SpatialMapping.GetComponents<SpatialMappingSource>();
            foreach (var source in spatialMappingSources)
            {
                source.SurfaceAdded += SpatialMappingSource_SurfaceAdded;
                source.SurfaceUpdated += SpatialMappingSource_SurfaceUpdated;
            }

        }

        private void OnTextureUpdate()
        {
            var imageTextureMappingList = SpatialMapping.GetComponentsInChildren<ImageTextureMapping>();
            foreach (var imageTextureMapping in imageTextureMappingList)
            {
                imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixList, TakePicture.Instance.projectionMatrixList, TakePicture.Instance.textureArray);
            }

            foreach (var obj in SampleObjects)
            {
                imageTextureMappingList = obj.GetComponentsInChildren<ImageTextureMapping>();
                foreach (var imageTextureMapping in imageTextureMappingList)
                {
                    imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixList, TakePicture.Instance.projectionMatrixList, TakePicture.Instance.textureArray);
                }
            }

        }

        void Start()
        {
            /*
            var spatialMappingManager = SpatialMappingManager.Instance;
            if (spatialMappingManager)
            {
                spatialMappingManager.SetSurfaceMaterial(TextureMappingMaterial);
            }
            */

            // Use Spatial Understanding
            //SpatialUnderstanding.Instance.ScanStateChanged += Instance_ScanStateChanged;
            //SpatialUnderstanding.Instance.OnScanDone += Instance_ScanStateChanged;
            //SpatialUnderstanding.Instance.RequestBeginScanning();

            TakePicture.Instance.OnTextureUpdated += OnTextureUpdate;

            //StartCoroutine(FinishScanning());
            //StartCoroutine(UpdateTexture());
        }

        private void Update()
        {
            /*
            if (scanComplete)
            {
                Debug.Log("scan completed!!");
                SpatialUnderstanding.Instance.RequestFinishScan();
                OnTextureUpdate();
                scanComplete = false;
            }
            */
        }

        private IEnumerator FinishScanning()
        {
            yield return new WaitForSeconds(15);
            //SpatialUnderstanding.Instance.RequestFinishScan();
            Debug.Log("finish!");
            OnTextureUpdate();
        }

        private IEnumerator UpdateTexture()
        {
            yield return new WaitForSeconds(5);
            while (true)
            {
                OnTextureUpdate();
                yield return new WaitForSeconds(10);
            }
        }

        public void StartTextureMapping()
        {
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial = TextureMappingMaterial;
        }

        private void SpatialMappingSource_SurfaceAdded(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e)
        {
            ApplyTextureMapping(e.Data.Object);
            Debug.Log("added");
            SpatialMappingCreated();
        }

        private void SpatialMappingSource_SurfaceUpdated(object sender, DataEventArgs<SpatialMappingSource.SurfaceUpdate> e)
        {
            ApplyTextureMapping(e.Data.New.Object);
            Debug.Log("updated");
        }

        private void ApplyTextureMapping(GameObject obj)
        {
            var imageTextureMapping = obj.GetComponent<ImageTextureMapping>();
            if (imageTextureMapping == null)
            {
                imageTextureMapping = obj.AddComponent<ImageTextureMapping>();
            }

            imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixList, TakePicture.Instance.projectionMatrixList, TakePicture.Instance.textureArray);
        }
    }
}