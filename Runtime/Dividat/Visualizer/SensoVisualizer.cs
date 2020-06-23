using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dividat;

namespace Dividat.Visualizer { 
    public class SensoVisualizer : MonoBehaviour
    {
        [Tooltip("Configure such that Center = 0, Up = 1, Right = 2, Down = 3, Left = 4")]
        public VisualizedPlate[] visualizedPlates;
        public GameObject centerOfGravity;
        public Vector2 jumpYgroundedY = new Vector2(4f, 1.8f);
        public GameObject basePlate;

        public float basePlate_Length = 9f;
        public Vector2 topLeftCorner = new Vector2(0f, 0f);
        public Vector2 bottomRightCorner = new Vector2(3f, 3f);
    
        private bool _setupOK = false;
        private float _range_x = 1f;
        private float _range_y = 1f;

        // Start is called before the first frame update
        void Start()
        {
            if (visualizedPlates.Length != 5)
            {
                Debug.LogAssertion("[SensoVisualizer] Failed Setup.  5 VisualizedPlates must be set");
            }
            else
            {
                if (visualizedPlates[0].direction != Direction.Center || visualizedPlates[1].direction != Direction.Up
                    || visualizedPlates[2].direction != Direction.Right || visualizedPlates[3].direction != Direction.Down
                    || visualizedPlates[4].direction != Direction.Left)
                {
                    Debug.LogWarning("[SensoVisualizer] Check setup. Visualized plates might not have their directions and order setup correctly. Must be Center = 0, Up = 1, Right = 2, Down = 3, Left = 4");
                }
                else
                {
                    _setupOK = true;
                }
            }
            _range_x = SensoManager.Instance.sensoHardwareConfiguration.Dimensions.x;
            _range_y = SensoManager.Instance.sensoHardwareConfiguration.Dimensions.y;
            //Debug.Log("rangex " +  _range_x + " rangey " + _range_y);
        }

        // Update all Plates and the center of gravity
        void Update()
        {
            Vector2 cog = new Vector2();
            if (_setupOK)
            {
                for (int i=0; i < visualizedPlates.Length; i++)
                {
                    bool active = SensoManager.Instance.GetPlateActive((Direction)i);
                    //Debug.Log(i + " is " + active);
                    visualizedPlates[i].SetPlateActiveState(active);
                    
                    Plate p = SensoManager.Instance.GetPlateState((Direction)i);
                    visualizedPlates[i].SetPlateState(p);
                }
            }
            if (SensoManager.Instance.playerPresent)
            {
                centerOfGravity.SetActive(true);
                //Debug.Log("cog before=" + cog);
                cog = SensoManager.Instance.CenterOfGravity;
                //Debug.Log("cog now=" + cog);
                centerOfGravity.transform.localPosition = new Vector3(
                    ((cog.x - topLeftCorner.x) / _range_x) * basePlate_Length - basePlate_Length/2f,
                    SensoManager.Instance.jump ? jumpYgroundedY.x : jumpYgroundedY.y,
                    (-1) * (((cog.y - topLeftCorner.y) / _range_y) * basePlate_Length - basePlate_Length / 2)
                );
                //Debug.Log("cog="+ cog.x + " topLeft " + topLeftCorner.x + " range " + _range_x);// * basePlate_Length - basePlate_Length/2f);
            }
            else
            {
                centerOfGravity.SetActive(false);
            }
        }
    }
}
