using UnityEngine;

namespace Lovatto.MiniMap
{
    public sealed class bl_MiniMapPlane : bl_MiniMapPlaneBase
    {
        public GameObject mapPlane;
        public GameObject gridPlane;
        public Material planeMaterial, planeMobileMaterial;

        private Transform m_Transform;
        private bl_MiniMap m_Minimap;
        private Vector3 currentPosition;
        private Vector3 worldPosition;
        private float defaultYCameraPosition;

        /// <summary>
        /// 
        /// </summary>
        public override void Setup(bl_MiniMap minimap)
        {
            m_Transform = transform;
            m_Minimap = minimap;
            //Get Position reference from world space rect.
            Vector3 pos = minimap.mapBounds.position;
            //Get Size reference from world space rect.
            Vector3 size = minimap.mapBounds.size;
            if (minimap.renderType == MiniMapRenderType.Picture)
            {
                //Apply material with map texture.
                if (minimap.mapRender.IsSingle())
                {
                    PlaneRender.material = CreateMaterial(minimap.mapRender.GetSingle());
                }
            }
            //Set position
            m_Transform.localPosition = pos;
            //Set Correct size.
            m_Transform.localScale = (new Vector3(size.x, 10, size.y) / 10);

            m_Transform.rotation = minimap.mapBounds.rotation;
            var eulers = m_Transform.eulerAngles;
            eulers.x -= 90;
            m_Transform.eulerAngles = eulers;

            worldPosition = minimap.mapBounds.position;
            //Apply MiniMap Layer
            mapPlane.layer = minimap.MiniMapLayer;
            gridPlane.layer = minimap.MiniMapLayer;
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            gameObject.name = $"Minimap Plane ({minimap.gameObject.name})";
            mapPlane.SetActive(minimap.renderType == MiniMapRenderType.Picture);
            gridPlane.SetActive(minimap.ShowAreaGrid);
            if (minimap.ShowAreaGrid)
            {
                var mat = gridPlane.GetComponent<Renderer>().material;
                mat.SetTextureScale("_MainTex", Vector2.one * minimap.AreasSize);
                mat.SetColor("_Color", new Color(1, 1, 1, minimap.gridOpacity));
            }

            if (minimap.renderType == MiniMapRenderType.Picture)
            {
                //Apply material with map texture.
                if (!minimap.mapRender.IsSingle())
                {
                    SetupMultiRender();
                }
            }

            Invoke(nameof(DelayPositionInvoke), 1);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupMultiRender()
        {
            var mapRender = m_Minimap.mapRender;
            var planes = new GameObject[mapRender.GetNumberOfDivisions() * mapRender.GetNumberOfDivisions()];

            float size = PlaneRender.transform.localScale.x;
            Vector3 originPos = PlaneRender.transform.localPosition;
            float div = (float)mapRender.renderDivisions;
            float divSize = size / div;
            float halfDiv = div / 2;

            for (int i = 0; i < planes.Length; i++)
            {
                planes[i] = Instantiate(PlaneRender.gameObject);
                planes[i].transform.parent = PlaneRender.transform.parent;
                planes[i].transform.localRotation = PlaneRender.transform.localRotation;
                planes[i].transform.localScale = Vector3.one * divSize;
                planes[i].GetComponent<Renderer>().material = CreateMaterial(mapRender.GetSnapshot(i));
            }

            divSize = ((size * 10) / div) * 0.5f;

            Vector3 divPos = originPos;
            divPos.x -= ((divSize * 2) * halfDiv) - divSize;
            divPos.z -= ((divSize * 2) * halfDiv) - divSize;
            float initX = divPos.x;

            int pId = 0;
            for (int i = 0; i < div; i++)
            {
                for (int x = 0; x < div; x++)
                {
                    planes[pId].transform.localPosition = divPos;
                    divPos.x += (divSize * 2);
                    pId++;
                }
                divPos.z += (divSize * 2);
                divPos.x = initX;
            }

            foreach (var p in planes)
            {
                p.transform.parent = PlaneRender.transform;
            }

            PlaneRender.enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnUpdate()
        {
            currentPosition = m_Transform.localPosition;
            //Get Position reference from world space rect.
            float ydif = defaultYCameraPosition - m_Minimap.miniMapCamera.transform.position.y;
            currentPosition.y = currentPosition.y - ydif;
            m_Transform.position = currentPosition;
        }

        void DelayPositionInvoke() { defaultYCameraPosition = m_Minimap.miniMapCamera.transform.position.y; }

        /// <summary>
        /// 
        /// </summary>
        public override void SetMapTexture(Texture2D newTexture)
        {
            PlaneRender.material.mainTexture = newTexture;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public override void SetGridSize(float size)
        {
            gridPlane.GetComponent<Renderer>().material.SetTextureScale("_MainTex", Vector2.one * size);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActiveGrid(bool active)
        {
            if (gridPlane == null) return;
            gridPlane.SetActive(active);
        }

        /// <summary>
        /// Create Material for Minimap image in plane.
        /// you can edit and add your own shader.
        /// </summary>
        /// <returns></returns>
        public Material CreateMaterial(Texture2D texture)
        {
            Material mat = new Material(m_Minimap.isMobile ? planeMobileMaterial : planeMaterial);

            mat.mainTexture = texture;
            mat.SetFloat("_Power", m_Minimap.planeSaturation);
            return mat;
        }

        public Renderer PlaneRender
        {
            get { return mapPlane.GetComponent<Renderer>(); }
        }
    }
}