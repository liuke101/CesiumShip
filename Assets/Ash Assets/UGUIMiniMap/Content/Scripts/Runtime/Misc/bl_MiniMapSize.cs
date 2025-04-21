using System.Collections;
using UnityEngine;

namespace Lovatto.MiniMap
{
    public sealed class bl_MiniMapSize : MonoBehaviour
    {
        private bl_MiniMap miniMap;
        private Coroutine sizeCoroutine;
        private Vector2 defaultPivot;
        private RectTransform root;
        private float defaultSizeX;
        private float nextSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="miniMap"></param>
        public void Init(bl_MiniMap miniMapOwner)
        {
            miniMap = miniMapOwner;
            root = miniMapOwner.MiniMapUI.root;
            defaultPivot = root.pivot;
            defaultSizeX = root.sizeDelta.x;
            nextSize = root.sizeDelta.x;
        }

        /// <summary>
        /// 
        /// </summary>
        public void DoTransition()
        {
            if (sizeCoroutine != null) StopCoroutine(sizeCoroutine);

            // if the transition is from fullscreen to minimap
            if (!miniMap.IsFullScreen)
            {
                DoTransitionToDefault();
                return;
            }

            switch (miniMap.fullScreenMode)
            {
                case MiniMapFullScreenMode.ScreenArea:
                    AreaScreenTransition();
                    break;
                case MiniMapFullScreenMode.ScaleToFitScreen:
                    ScaleToFitTransition();
                    break;
                case MiniMapFullScreenMode.ScaleToCoverScreen:
                    ScaleToCoverTransition();
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void DoTransitionToDefault()
        {
            sizeCoroutine = StartCoroutine(DoSizeTransition());
            IEnumerator DoSizeTransition()
            {
                miniMap.HighPrecisionMode = true;
                Vector2 posOrigin = root.anchoredPosition;
                Quaternion rotOrigin = root.localRotation;
                Vector2 sizeOrigin = root.sizeDelta;
                Vector2 originPivot = root.pivot;

                Vector2 targetPos = miniMap.MiniMapPosition;
                Quaternion targetRot = Quaternion.Euler(miniMap.MiniMapRotation);
                Vector2 targetSize = miniMap.MiniMapSize;
                nextSize = targetSize.x;
                Vector2 targetPivot = defaultPivot;

                float d = 0;
                float t;
                while (d < 1)
                {

                    d += Time.deltaTime / miniMap.sizeTransitionDuration;
                    t = miniMap.sizeTransitionCurve.Evaluate(d);

                    root.pivot = Vector2.Lerp(originPivot, targetPivot, t);
                    root.anchoredPosition = Vector2.Lerp(posOrigin, targetPos, t);
                    root.localRotation = Quaternion.Slerp(rotOrigin, targetRot, t);
                    root.sizeDelta = Vector2.Lerp(sizeOrigin, targetSize, t);

                    yield return null;
                }

                root.pivot = targetPivot;
                root.anchoredPosition = targetPos;
                root.localRotation = targetRot;
                root.sizeDelta = targetSize;
                miniMap.HighPrecisionMode = false;
                bl_MiniMapEvents.onSizeChanged?.Invoke(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float GetSizeRatio()
        {
            return nextSize / defaultSizeX;
        }

        /// <summary>
        /// 
        /// </summary>
        void ScaleToFitTransition()
        {
            sizeCoroutine = StartCoroutine(DoFitSizeTransition());
            IEnumerator DoFitSizeTransition()
            {
                miniMap.HighPrecisionMode = true;

                Vector2 posOrigin = root.anchoredPosition;
                Quaternion rotOrigin = root.localRotation;
                Vector2 sizeOrigin = root.sizeDelta;
                Vector2 originPivot = root.pivot;

                var canvasTransform = (RectTransform)miniMap.m_Canvas.transform;
                float yRatio = canvasTransform.sizeDelta.y / root.sizeDelta.y;

                Vector2 targetPos = new Vector2(canvasTransform.sizeDelta.x * 0.5f, -(canvasTransform.sizeDelta.y * 0.5f));
                Quaternion targetRot = Quaternion.Euler(miniMap.FullMapRotation);
                Vector2 targetSize = miniMap.MiniMapSize * yRatio;
                if (miniMap.fullScreenMargin > 0) targetSize -= Vector2.one * miniMap.fullScreenMargin;
                Vector2 targetPivot = Vector2.one * 0.5f;
                nextSize = targetSize.x;

                float d = 0;
                float t;
                while (d < 1)
                {

                    d += Time.deltaTime / miniMap.sizeTransitionDuration;
                    t = miniMap.sizeTransitionCurve.Evaluate(d);

                    root.pivot = Vector2.Lerp(originPivot, targetPivot, t);
                    root.anchoredPosition = Vector2.Lerp(posOrigin, targetPos, t);
                    root.localRotation = Quaternion.Slerp(rotOrigin, targetRot, t);
                    root.sizeDelta = Vector2.Lerp(sizeOrigin, targetSize, t);

                    yield return null;
                }

                root.pivot = targetPivot;
                root.anchoredPosition = targetPos;
                root.localRotation = targetRot;
                root.sizeDelta = targetSize;
                miniMap.HighPrecisionMode = false;
                bl_MiniMapEvents.onSizeChanged?.Invoke(miniMap.IsFullScreen);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ScaleToCoverTransition()
        {
            sizeCoroutine = StartCoroutine(DoCoverSizeTransition());
            IEnumerator DoCoverSizeTransition()
            {
                miniMap.HighPrecisionMode = true;

                Vector2 posOrigin = root.anchoredPosition;
                Quaternion rotOrigin = root.localRotation;
                Vector2 sizeOrigin = root.sizeDelta;
                Vector2 originPivot = root.pivot;

                var canvasTransform = (RectTransform)miniMap.m_Canvas.transform;
                float ratio = canvasTransform.sizeDelta.x / root.sizeDelta.x;

                Vector2 targetPos = new Vector2(canvasTransform.sizeDelta.x * 0.5f, -(canvasTransform.sizeDelta.y * 0.5f));
                Quaternion targetRot = Quaternion.Euler(miniMap.FullMapRotation);
                Vector2 targetSize = miniMap.MiniMapSize * ratio;
                if (miniMap.fullScreenMargin > 0) targetSize -= Vector2.one * miniMap.fullScreenMargin;
                Vector2 targetPivot = Vector2.one * 0.5f;
                nextSize = targetSize.x;

                float d = 0;
                float t;
                while (d < 1)
                {

                    d += Time.deltaTime / miniMap.sizeTransitionDuration;
                    t = miniMap.sizeTransitionCurve.Evaluate(d);

                    root.pivot = Vector2.Lerp(originPivot, targetPivot, t);
                    root.anchoredPosition = Vector2.Lerp(posOrigin, targetPos, t);
                    root.localRotation = Quaternion.Slerp(rotOrigin, targetRot, t);
                    root.sizeDelta = Vector2.Lerp(sizeOrigin, targetSize, t);

                    yield return null;
                }

                root.pivot = targetPivot;
                root.anchoredPosition = targetPos;
                root.localRotation = targetRot;
                root.sizeDelta = targetSize;
                miniMap.HighPrecisionMode = false;
                bl_MiniMapEvents.onSizeChanged?.Invoke(miniMap.IsFullScreen);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void AreaScreenTransition()
        {
            sizeCoroutine = StartCoroutine(DoSizeTransition());
            IEnumerator DoSizeTransition()
            {
                bool isFullScreen = miniMap.IsFullScreen;
                miniMap.HighPrecisionMode = true;
                Vector2 posOrigin = root.anchoredPosition;
                Quaternion rotOrigin = root.localRotation;
                Vector2 sizeOrigin = root.sizeDelta;

                Vector2 targetPos = isFullScreen ? miniMap.FullMapPosition : miniMap.MiniMapPosition;
                Quaternion targetRot = Quaternion.Euler(isFullScreen ? miniMap.FullMapRotation : miniMap.MiniMapRotation);
                Vector2 targetSize = isFullScreen ? miniMap.FullMapSize : miniMap.MiniMapSize;
                nextSize = targetSize.x;

                float d = 0;
                float t;
                while (d < 1)
                {

                    d += Time.deltaTime / miniMap.sizeTransitionDuration;
                    t = miniMap.sizeTransitionCurve.Evaluate(d);

                    root.anchoredPosition = Vector2.Lerp(posOrigin, targetPos, t);
                    root.localRotation = Quaternion.Slerp(rotOrigin, targetRot, t);
                    root.sizeDelta = Vector2.Lerp(sizeOrigin, targetSize, t);

                    yield return null;
                }

                root.anchoredPosition = targetPos;
                root.localRotation = targetRot;
                root.sizeDelta = targetSize;
                miniMap.HighPrecisionMode = false;
                bl_MiniMapEvents.onSizeChanged?.Invoke(isFullScreen);
            }
        }
    }
}