using UnityEngine;
using System.Collections.Generic;

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

        ///Maps a point on the Senso to the corresponding plate
        public SensoPlateSetup GetCorrespondingDirection(Vector2 point){
            List<SensoPlateSetup> candidatePlates = new List<SensoPlateSetup>();
            foreach (SensoPlateSetup plate in hardwarePlates){
                if (point.x >= plate.upperLeftCorner.x && point.y >= plate.upperLeftCorner.y
                    && point.x <= plate.lowerRightCorner.x && point.y <= plate.lowerRightCorner.y)
                {
                    candidatePlates.Add(plate);
                }
            }
            int count = candidatePlates.Count;
            if (count == 0){
                return null;
            }
            else if (candidatePlates.Count == 1){
                return candidatePlates[0];
            }
            else {
                SensoPlateSetup plate = candidatePlates[0];
                float minDist = Vector2.Distance(point, plate.Center);
                for (int i = 1; i < count; i++){
                    float dist = Vector2.Distance(point, candidatePlates[i].Center);
                    if (dist < minDist){
                        minDist = dist;
                        plate = candidatePlates[i];
                    }
                }
                return plate;
            }
        }

        public static SensoHardwareConfiguration InstantiateStandardConfiguration(){
            SensoHardwareConfiguration hconfig = ScriptableObject.CreateInstance<SensoHardwareConfiguration>();
            hconfig.upperLeftCorner = new Vector2(0f, 0f);
            hconfig.lowerRightCorner = new Vector2(3f, 3f);
            hconfig.hardwarePlates = new SensoPlateSetup[5];

            //center
            hconfig.hardwarePlates[0] = new SensoPlateSetup();
            hconfig.hardwarePlates[0].direction = Direction.Center;
            hconfig.hardwarePlates[0].upperLeftCorner = new Vector2(1f, 1f);
            hconfig.hardwarePlates[0].lowerRightCorner = new Vector2(2f, 2f);

            //up
            hconfig.hardwarePlates[1] = new SensoPlateSetup();
            hconfig.hardwarePlates[1].direction = Direction.Up;
            hconfig.hardwarePlates[1].upperLeftCorner = new Vector2(0f, 0f);
            hconfig.hardwarePlates[1].lowerRightCorner = new Vector2(3f, 1f);

            //right
            hconfig.hardwarePlates[2] = new SensoPlateSetup();
            hconfig.hardwarePlates[2].direction = Direction.Right;
            hconfig.hardwarePlates[2].upperLeftCorner = new Vector2(2f, 0f);
            hconfig.hardwarePlates[2].lowerRightCorner = new Vector2(3f, 3f);

            //down
            hconfig.hardwarePlates[3] = new SensoPlateSetup();
            hconfig.hardwarePlates[3].direction = Direction.Down;
            hconfig.hardwarePlates[3].upperLeftCorner = new Vector2(0f, 2f);
            hconfig.hardwarePlates[3].lowerRightCorner = new Vector2(3f, 3f);

            //left
            hconfig.hardwarePlates[4] = new SensoPlateSetup();
            hconfig.hardwarePlates[4].direction = Direction.Left;
            hconfig.hardwarePlates[4].upperLeftCorner = new Vector2(0f, 0f);
            hconfig.hardwarePlates[4].lowerRightCorner = new Vector2(1f, 3f);

            return hconfig;
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

        public Vector2 Center {
            get {
                return Vector2.Lerp(upperLeftCorner, lowerRightCorner, 0.5f);
            }
        }

        public Vector2 Dimensions {
            get {
                return new Vector2(lowerRightCorner.x - upperLeftCorner.x, lowerRightCorner.y - upperLeftCorner.y);
            }
        }
    }
}