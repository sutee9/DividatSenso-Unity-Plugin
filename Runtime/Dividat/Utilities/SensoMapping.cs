using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dividat
{

    [CreateAssetMenu(fileName = "SensoCoordinateMapping", menuName = "Dividat/SensoCoordinateMapping", order = 11)]
    /// <summary>SensoMapping allows to define a coordinate range to which the raw input values of the senso should be mapped to. 
    /// This helps to abstract the Senso hardware, and just think in the game's dimensions. Example: You could map the senso to a playing field
    /// with dimensions of 20 by 20, without having to think about the raw values.</summary>
    public class SensoMapping : ScriptableObject
    {
        [Header("Target Coordinate Mapping")]
        [Tooltip("Set to the coordinate which corresponds to the player standing the upper left corner on the senso. I.e. the position left of the screen at the edge of the senso that is parralel and closest to the screen.")]
        public Vector3 upperLeftCorner;
        [Tooltip("Starting from the upper left corner, indicate the vector that would lead to the upper right corner, i.e. the position right of the screen at the edge of the senso that is parralel and closest to the screen.")]
        public Vector3 xDimensionRange;
        [Tooltip("Starting from the upper left corner, indicate the vector that would lead to the Lower left corner, i.e. the position left of the screen at the edge of the senso that is parralel and farthest from the screen.")]
        public Vector3 yDimensionRange;
        [Tooltip("You can add a flat offset to the mapping coordinates that will always be added. (optional)")]
        public Vector3  offset = Vector3.zero;

        [Header("Senso Configuration to be Used")]
        [Tooltip("Leave Empty for the configuration defined in Senso Manager.")]
        public SensoHardwareConfiguration hardwareConfiguration;

        private SensoManager _sm;
        public Vector3 GetMappedCoordinates(Vector2 sensoCoordinates)
        {
            if (_sm != null){
                return GetMapped(sensoCoordinates);
            }
            else if (_sm == null && SensoManager.Instance != null){
                _sm = SensoManager.Instance;
                if (hardwareConfiguration == null)
                {
                    hardwareConfiguration = _sm.sensoHardwareConfiguration;
                }
                return GetMapped(sensoCoordinates);
            }
            else {
                return Vector3.zero + offset;
            }
        }

        private Vector3 GetMapped(Vector2 sensoCoordinates){
           // Debug.Log(sensoCoordinates);
            return upperLeftCorner + xDimensionRange * ( (sensoCoordinates.x + hardwareConfiguration.Dimensions.x/2f) / hardwareConfiguration.Dimensions.x) + yDimensionRange * ( (sensoCoordinates.y + hardwareConfiguration.Dimensions.y/2f) / hardwareConfiguration.Dimensions.y) + offset;
        }
    }
}

