using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dividat;

namespace Dividat.Visualizer { 
    public class VisualizedPlate : MonoBehaviour
    {
        public GameObject positionIndicator;
        public GameObject plate;
        public Direction direction;
        public Vector2 visualizedPlateDimensions = new Vector2(1f, 1f);
        public Vector2 upYdownY = new Vector2(0.09f, 0.06f);

        private Vector2 _topLeftCorner = new Vector2(0, 0);
        private Vector2 _botRightCorner = new Vector2(0, 0);

        private bool _active;
        private float _range_x = 1f;
        private float _range_y = 1f;

        private void Start()
        {
            SensoPlateSetup plateConfig = SensoManager.Instance.sensoHardwareConfiguration.hardwarePlates[(int)direction];
            _range_x = plateConfig.Dimensions.x;
            _range_y = plateConfig.Dimensions.y;
            //Debug.Log(direction + " rangeX " + _range_x);

            _topLeftCorner = plateConfig.upperLeftCorner;
            _botRightCorner = plateConfig.lowerRightCorner;
            //Debug.Log(direction + " topLeft="+_topLeftCorner);
        }

        public void SetPlateState(Plate p)
        {
            positionIndicator.transform.localPosition = new Vector3(
                ((p.x - _topLeftCorner.x) / _range_x) * visualizedPlateDimensions.x - visualizedPlateDimensions.x / 2,
                positionIndicator.transform.localPosition.y,
                (-1) * (((p.y - _topLeftCorner.y) / _range_y) * visualizedPlateDimensions.y - visualizedPlateDimensions.y / 2 )
            );
        }

        public void SetPlateActiveState(bool active)
        {
            positionIndicator.SetActive(active);
            plate.transform.localPosition = new Vector3(plate.transform.localPosition.x, active ? upYdownY.y : upYdownY.x, plate.transform.localPosition.z);
            
        }


    }
}
