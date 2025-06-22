using NWH.Common.SceneManagement;
using NWH.Common.Vehicles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class CompassBar : MonoBehaviour
{
    //Camera
    public Transform MainCamera;

    // 固定的世界北方向，默认为Z轴正方向
    public Vector3 fixedWorldNorth = Vector3.forward;

    private float groupWidth = 0f;

    public Row CardinalDirectionRow;
    public Row DirectionDegreeRow;

    public Color32 NorthColor = Color.yellow;
    [Range(20f, 100f)]
    public float DistanceBetweenTwoRow = 45f;

    public RectTransform CompassParent;
    private RectTransform background;

    private RectTransform group0RectTransform;
    private RectTransform group1RectTransform;

    private Vector3 lastCompassParentPosition;

    private void Start()
    {
        background = CompassParent.parent.GetComponent<RectTransform>();
        SetupCompass();

        if (VehicleChanger.Instance != null)
        {
            VehicleChanger.Instance.onVehicleChanged.AddListener(OnVehicleChanged);

            // Set initial camera for the active vehicle
            if (VehicleChanger.Instance.vehicles.Count > VehicleChanger.Instance.activeVehicleIndex && VehicleChanger.Instance.activeVehicleIndex >= 0)
            {
                Vehicle activeVehicle = VehicleChanger.Instance.vehicles[VehicleChanger.Instance.activeVehicleIndex];
                if (activeVehicle != null)
                {
                    OnVehicleChanged(activeVehicle);
                }
            }
        }
    }

    private void SetupCompass()
    {

        List<Transform> _instantiatedRows = new List<Transform>();

        for (int i = 0; i < 24; i++)
        {
            int _degree = 15 * (i + 1);

            bool isCardinalDirection =
                _degree == 90 ||
                _degree == 180 ||
                 _degree == 270 ||
                  _degree == 360;

            Row _obj = isCardinalDirection ? (CardinalDirectionRow) : (DirectionDegreeRow);
            Row _row = Instantiate<Row>(_obj, _obj.transform.parent);
            Vector2 v = _row.GetComponent<RectTransform>().anchoredPosition;
            v.x = i * DistanceBetweenTwoRow;
            _row.GetComponent<RectTransform>().anchoredPosition = v;



            if (_degree == 360)
            {//Cardinal Direction - N
                _row.LineImage.color = NorthColor;
                _row.DirectionText.text = "N";
            }
            else if (_degree == 90)
            {//Cardinal Direction - E
                _row.DirectionText.text = "E";
            }
            else if (_degree == 180)
            {//Cardinal Direction - S
                _row.DirectionText.text = "S";
            }
            else if (_degree == 270)
            {//Cardinal Direction - W
                _row.DirectionText.text = "W";
            }
            else
            {//Direction Degree
                _row.DirectionText.text = _degree.ToString();
            }
            _row.gameObject.SetActive(true);
            _instantiatedRows.Add(_row.transform);
        }

        Destroy(CardinalDirectionRow.gameObject);
        Destroy(DirectionDegreeRow.gameObject);

        GameObject _group0 = new GameObject("Group0", typeof(RectTransform));
        _group0.transform.SetParent(CompassParent);
        _group0.transform.localScale = Vector3.one;
        _group0.transform.localRotation = Quaternion.identity;

        Vector2 _sizeDelta = _group0.GetComponent<RectTransform>().sizeDelta;
        _sizeDelta.x = 23f * DistanceBetweenTwoRow;
        _sizeDelta.y = _group0.transform.parent.parent.GetComponent<RectTransform>().sizeDelta.y;
        _group0.GetComponent<RectTransform>().sizeDelta = _sizeDelta;
        groupWidth = _sizeDelta.x;

        Vector2 _anchoredPosition = _group0.GetComponent<RectTransform>().anchoredPosition;
        _anchoredPosition.y = 0f;
        _anchoredPosition.x = _sizeDelta.x / 2f;
        _group0.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;

        for (int i = _instantiatedRows.Count - 1; i >= 0; i--)
        {
            _instantiatedRows[i].SetParent(_group0.transform);
        }

        GameObject _group1 = Instantiate(_group0, _group0.transform.parent);
        _group1.name = "Group1";
        _anchoredPosition = _group1.GetComponent<RectTransform>().anchoredPosition;
        _anchoredPosition.x = (_anchoredPosition.x * 3f) + DistanceBetweenTwoRow;
        _group1.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;

        //Set in the midst groups in parent
        float _subtractionAmount = _group0.GetComponent<RectTransform>().sizeDelta.x;
        _anchoredPosition = _group1.GetComponent<RectTransform>().anchoredPosition;
        _anchoredPosition.x = _anchoredPosition.x - _subtractionAmount;
        _group1.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;
        _anchoredPosition = _group0.GetComponent<RectTransform>().anchoredPosition;
        _anchoredPosition.x = _anchoredPosition.x - _subtractionAmount;
        _group0.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;

        group0RectTransform = _group0.GetComponent<RectTransform>();
        group1RectTransform = _group1.GetComponent<RectTransform>();

    }

    private void Update()
    {
        // 计算相机forward在水平面上的投影
        Vector3 cameraForwardHorizontal = MainCamera.transform.forward;
        cameraForwardHorizontal.y = 0;
        cameraForwardHorizontal.Normalize();

        // 计算相机forward与fixedWorldNorth的带符号夹角（绕Y轴）
        float currentHeadingDegrees = Vector3.SignedAngle(fixedWorldNorth, cameraForwardHorizontal, Vector3.up);

        // 计算每度对应的像素
        float pixelsPerDegree = DistanceBetweenTwoRow / 15.0f;

        // 计算目标UI偏移
        float targetAnchoredX = -currentHeadingDegrees * pixelsPerDegree;

        // 设置CompassParent的anchoredPosition.x
        Vector2 anchoredPosition = CompassParent.anchoredPosition;
        anchoredPosition.x = targetAnchoredX;
        CompassParent.anchoredPosition = anchoredPosition;

        ShiftingRows();
    }

    private void ShiftingRows()
    {
        if (lastCompassParentPosition.x == CompassParent.position.x)
            return;

        bool isSlidingToLeft = lastCompassParentPosition.x > CompassParent.position.x;
       
        float _dis0 = Vector2.Distance(background.position, group0RectTransform.position);
        float _dis1 = Vector2.Distance(background.position, group1RectTransform.position);
        if (_dis0 >= groupWidth)
        {
            if (group0RectTransform.position.x < background.position.x && isSlidingToLeft)
            {//Solda. Sağ tarafa ışınla
                Vector3 v = group0RectTransform.anchoredPosition;
                v.x = group1RectTransform.anchoredPosition.x + groupWidth + DistanceBetweenTwoRow;
                group0RectTransform.anchoredPosition = v;
            }
            else if (group0RectTransform.position.x > background.position.x && !isSlidingToLeft)
            {//Sağda. Sol tarafa ışınla
                Vector3 v = group0RectTransform.anchoredPosition;
                v.x = group1RectTransform.anchoredPosition.x - groupWidth - DistanceBetweenTwoRow;
                group0RectTransform.anchoredPosition = v;
            }
        }
        else if (_dis1 >= groupWidth)
        {
            if (group1RectTransform.position.x < background.position.x && isSlidingToLeft)
            {//Solda. Sağ tarafa ışınla
                Vector3 v = group1RectTransform.anchoredPosition;
                v.x = group0RectTransform.anchoredPosition.x + groupWidth + DistanceBetweenTwoRow;
                group1RectTransform.anchoredPosition = v;
            }
            else if (group1RectTransform.position.x > background.position.x && !isSlidingToLeft)
            {//Sağda. Sol tarafa ışınla
                Vector3 v = group1RectTransform.anchoredPosition;
                v.x = group0RectTransform.anchoredPosition.x - groupWidth - DistanceBetweenTwoRow;
                group1RectTransform.anchoredPosition = v;
            }
        }

        lastCompassParentPosition = CompassParent.position;

    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (VehicleChanger.Instance != null)
        {
            VehicleChanger.Instance.onVehicleChanged.RemoveListener(OnVehicleChanged);
        }
    }

    public void OnVehicleChanged(Vehicle newVehicle)
    {
        if (newVehicle != null)
        {
            // Find the camera associated with the vehicle.
            // A common approach is to find a Camera component in the children of the vehicle's transform.
            Camera[] vehicleCamera = newVehicle.GetComponentsInChildren<Camera>();
            
            if (vehicleCamera.Length > 0)
            {
                MainCamera = vehicleCamera.FirstOrDefault(cam => cam.name == "CameraDriver").transform;
            }
            else
            {
                Debug.LogWarning("No camera found on the new vehicle.", newVehicle);
            }
        }
    }
}