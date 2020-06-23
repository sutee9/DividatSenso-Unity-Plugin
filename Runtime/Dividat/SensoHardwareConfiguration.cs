using Dividat;
using UnityEngine;

namespace Dividat {

    [CreateAssetMenu(fileName = "SensoHardwareConfiguration", menuName = "Dividat/SensoHardwareConfiguration", order = 10)]
    public class SensoHardwareConfiguration : ScriptableObject {
        [Tooltip("Specify the hardware plates. Use the following indexes for the plates. Center = 0, Up = 1, Right = 2, Down = 3, Left = 4")]
        public SensoPlateSetup[] hardwarePlates = new SensoPlateSetup[5];
        [Tooltip("Coordinates of the upper left corner of the Senso. Usually this should be (0,0). Imagine standing just behind the senso. Up refers to the end where the TV would be placed.")]
        public Vector2 upperLeftCorner = new Vector2(0f, 0f);
        [Tooltip("Coordinates of the lower right corner of the Senso. Usually this should be (3, 3). Imagine standing just behind the senso. Up refers to the end where the TV would be placed.")]
        public Vector2 lowerRightCorner = new Vector2(3f, 3f);

        public Vector2 Dimensions {
            get {
                return new Vector2(lowerRightCorner.x - upperLeftCorner.x, lowerRightCorner.y - upperLeftCorner.y);
            }
        }
    }
    [System.Serializable]
    public class SensoPlateSetup {
        public Direction direction = Direction.Center;

        [Header("Hardware Coordinates ")]
        [Tooltip("Coordinates of the upper left corner of this plate. If the plate does not have a rectangle shape (such as the up, right, down, left plates), create an imaginary rectangle which would fit the plate tightly. (Imagine standing just behind the senso. Up refers to the end where the TV would be placed)")]
        public Vector2 upperLeftCorner = new Vector2(1f, 1f);
        [Tooltip("Coordinates of the lower right corner. If the plate does not have a rectangle shape (such as the up, right, down, left plates), create an imaginary rectangle which would fit the plate tightly.")]
        public Vector2 lowerRightCorner = new Vector2(2f, 2f);

        public Vector2 Dimensions {
            get {
                return new Vector2(lowerRightCorner.x - upperLeftCorner.x, lowerRightCorner.y - upperLeftCorner.y);
            }
        }
    }
}