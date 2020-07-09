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
        public Vector3 upperLeftCorner;
        public Vector3 xDimensionRange;
        public Vector3 yDimensionRange;

        [Header("Senso Configuration to be Used")]
        [Tooltip("Leave Empty for the configuration defined in Senso Manager.")]
        public SensoHardwareConfiguration hardwareConfiguration;
        public Vector3 GetMappedCoordinates(Vector2 sensoCoordinates)
        {
            if (hardwareConfiguration == null)
            {
                hardwareConfiguration = SensoManager.Instance.sensoHardwareConfiguration;
            }
            return xDimensionRange * (sensoCoordinates.x / hardwareConfiguration.Dimensions.x) + yDimensionRange * (sensoCoordinates.y / hardwareConfiguration.Dimensions.y);
        }
    }
}

