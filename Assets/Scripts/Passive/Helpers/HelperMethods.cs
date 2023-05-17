using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static HelperMethods.LineOfSightUtils;
using static StaticDataHolder;
using static TeamUpdater;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class HelperMethods
{
    public class VectorUtils
    {
        /// <summary>
        /// Get distance between objects in 2D. Z position values are ignored
        /// </summary>
        /// <param name="firstObject"></param>
        /// <param name="secondObject"></param>
        /// <returns></returns>
        public static float Distance(GameObject firstObject, GameObject secondObject)
        {
            return DeltaPosition(firstObject, secondObject).magnitude;
        }
        /// <summary>
        /// Get distance between positions in 2D. Z position values are ignored
        /// </summary>
        /// <param name="firstPosition"></param>
        /// <param name="secondPosition"></param>
        /// <returns></returns>
        public static float Distance(Vector3 firstPosition, Vector3 secondPosition)
        {
            return DeltaPosition(firstPosition, secondPosition).magnitude;
        }
        /// <summary>
        /// Get distance between positions in 2D.
        /// </summary>
        /// <param name="firstPosition"></param>
        /// <param name="secondPosition"></param>
        /// <returns></returns>
        public static float Distance(Vector2 firstPosition, Vector2 secondPosition)
        {
            return DeltaPosition(firstPosition, secondPosition).magnitude;
        }
        /// <summary>
        /// Get delta position in 2D. Z position values are ignored. Starts from the first object. 
        /// </summary>
        /// <param name="startingObject"></param>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        public static Vector3 DeltaPosition(GameObject startingObject, GameObject targetObject)
        {
            Vector3 myPositionVector = startingObject.transform.position;
            myPositionVector.z = 0;
            Vector3 targetPosition = targetObject.transform.position;
            targetPosition.z = 0;

            return (targetPosition - myPositionVector);
        }
        /// <summary>
        /// Get delta position in 2D. Z position values are ignored. Starts from the first position. 
        /// </summary>
        /// <param name="startingVector"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public static Vector3 DeltaPosition(Vector3 startingVector, Vector3 targetPosition)
        {
            startingVector.z = 0;
            targetPosition.z = 0;

            return (targetPosition - startingVector);
        }
        /// <summary>
        /// Get delta position in 2D. Starts from the first position. 
        /// </summary>
        /// <param name="startingVector"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public static Vector2 DeltaPosition(Vector2 startingVector, Vector2 targetPosition)
        {
            return (targetPosition - startingVector);
        }
        /// <summary>
        /// Returns a vector of specified magnitude and direction in 2D. 
        /// Direction given in degrees.
        /// </summary>
        /// <param name="firstPosition"></param>
        /// <param name="secondPosition"></param>
        /// <returns></returns>
        public static Vector3 DirectionVector(float magnitude, float direction)
        {
            Vector3 returnVector = magnitude * DirectionVectorNormalized(direction);
            return returnVector;

        }
        /// <summary>
        /// Returns a vector of specified direction in 2D. 
        /// Direction given in degrees.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Vector3 DirectionVectorNormalized(float direction)
        {
            float xStepMove = -Mathf.Sin(Mathf.Deg2Rad * direction);
            float yStepMove = Mathf.Cos(Mathf.Deg2Rad * direction);
            Vector3 returnVector = new Vector3(xStepMove, yStepMove, 0);
            return returnVector.normalized;
        }
        /// <summary>
        /// Returns the direction of the vector given in degrees in range <0;360>
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float VectorDirection(Vector2 vector)
        {
            return AngleUtils.ClampAngle360(Vector2.SignedAngle(Vector2.up, vector));
        }
        /// <summary>
        /// Projects the vector on the left onto the vector on the right
        /// </summary>
        public static Vector2 ProjectVector(Vector2 project, Vector2 onto)
        {
            float length = Vector2.Dot(project, onto.normalized);
            return onto.normalized * length;
        }
        /// <summary>
        /// Get translated mouse position in 2D. Returns the position, as if mouse cursor was actually placed in that position on the map (Screen to world point)
        /// </summary>
        /// <param name="zCoordinate"></param> 
        /// <returns></returns>
        public static Vector2 TranslatedMousePosition()
        {
            Vector3 returnVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return returnVector;

        }
        /// <summary>
        /// Get translated mouse position in 2D. Returns the position, as if mouse cursor was actually placed in that position on the map (Screen to world point)
        /// </summary>
        /// <param name="zCoordinate"></param> 
        /// <returns></returns>
        public static Vector3 TranslatedMousePosition(float zCoordinate)
        {
            Vector3 returnVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            returnVector.z = zCoordinate;
            return returnVector;

        }
        /// <summary>
        /// Get translated mouse position in 2D. Returns the position, as if mouse cursor was actually placed in that position on the map (Screen to world point)
        /// </summary>
        /// <param name="zCoordinateVector"></param>
        /// <returns></returns>
        public static Vector3 TranslatedMousePosition(Vector3 zCoordinateVector)
        {
            Vector3 returnVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            returnVector.z = zCoordinateVector.z;
            return returnVector;

        }
        /// <summary>
        /// Returns true if the distance between two positions is smaller or equal to range
        /// </summary>
        /// <returns></returns>
        public static bool IsPositionInRange(Vector2 position, Vector2 from, float range)
        {
            return range == 0 || Distance(position, from) <= range;
        }
        /// <summary>
        /// Returns true if the distance between two positions is smaller or equal to range
        /// </summary>
        /// <returns></returns>
        public static bool IsPositionInRange(GameObject from, GameObject to, float range)
        {
            return range == 0 || Distance(from, to) <= range;
        }
        /// <summary>
        /// Returns true if the target position is inside a given Cone
        /// </summary>
        /// <returns></returns>
        public static bool IsPositionInCone(Vector3 targetPosition, Cone cone)
        {
            if (IsPositionInRange(cone.coneOrigin, targetPosition, cone.range))
            {
                float angleFromZeroToTarget = AngleUtils.AngleFromUpToPosition(cone.coneOrigin, targetPosition);
                float angleFromMiddleToTarget = Mathf.DeltaAngle(cone.direction, angleFromZeroToTarget);

                bool isInCone = angleFromMiddleToTarget > -cone.rightAngle && angleFromMiddleToTarget < cone.leftAngle;
                if (isInCone)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsFirstCloserToMiddleThanSecond(GameObject first, GameObject second, Vector3 middlePosition, float middleAngle = 0)
        {
            float zAngleFromMiddleToCurrentClosestEnemy = CountAngleFromMiddleToPosition(middlePosition, second.transform.position, middleAngle);
            float zAngleFromMiddleToItem = CountAngleFromMiddleToPosition(middlePosition, first.transform.position, middleAngle);
            //If the found target is closer to the middle (angle wise) than the current closest target, make is the closest target
            bool isCloserAngleWise = Mathf.Abs(zAngleFromMiddleToCurrentClosestEnemy) > Mathf.Abs(zAngleFromMiddleToItem);
            return isCloserAngleWise;
        }
        public static bool IsFirstCloserThanSecond(Vector2 position, GameObject first, GameObject second)
        {
            return Distance(position, (Vector2)first.transform.position) < Distance(position, (Vector2)second.transform.position);
        }
        public static bool IsFirstCloserThanSecond(Vector2 position, Vector2 first, Vector2 second)
        {
            return Distance(position, first) < Distance(position, second);
        }
        /// <summary>
        /// Rotate vector by angle in degrees clockwise
        /// </summary>
        public static Vector2 RotateVector(Vector3 vector, float angle)
        {
            float angleInRadians = -angle * Mathf.Deg2Rad;
            float newX = (vector.x * Mathf.Cos(angleInRadians)) + (vector.y * Mathf.Sin(angleInRadians));
            float newY = (vector.x * Mathf.Sin(angleInRadians)) + (vector.y * Mathf.Cos(angleInRadians));
            return new Vector2(newX, newY);
        }
        #region Helper methods
        private static float CountAngleFromMiddleToPosition(Vector3 middlePosition, Vector3 targetPosition, float middleAngle)
        {
            float angleFromZeroToItem = HelperMethods.RotationUtils.DeltaPositionRotation(middlePosition, targetPosition).eulerAngles.z;
            float angleFromGunToItem = Mathf.DeltaAngle(middleAngle, angleFromZeroToItem);

            return angleFromGunToItem;
        }
        #endregion
        public class Cone
        {
            public Cone(Vector2 coneOrigin, float direction, float leftAngle, float rightAngle, float range)
            {
                this.coneOrigin = coneOrigin;
                this.direction = direction;
                this.leftAngle = leftAngle;
                this.rightAngle = rightAngle;
                this.range = range;
            }
            public Vector2 coneOrigin;
            public float direction;
            public float leftAngle;
            public float rightAngle;
            public float range;
        }
    }
    public class RotationUtils
    {
        /// <summary>
        /// Rotation from first position to second position. Range <0;360)
        /// </summary>
        /// <param name="firstPosition"></param>
        /// <param name="secondPosition"></param>
        /// <returns></returns>
        public static Quaternion DeltaPositionRotation(Vector3 firstPosition, Vector3 secondPosition)
        {
            Vector3 deltaPosition = VectorUtils.DeltaPosition(firstPosition, secondPosition);
            if (deltaPosition.x == 0)
            {
                if (deltaPosition.y >= 0)
                {
                    return Quaternion.Euler(0, 0, 90);
                }
                else
                {
                    return Quaternion.Euler(0, 0, -90);
                }
            }

            float zRotation = Mathf.Rad2Deg * Mathf.Atan2(deltaPosition.y, deltaPosition.x);
            return Quaternion.Euler(0, 0, zRotation);
        }
        /// <summary>
        /// Returns a random rotation in the specified range. The middle point it up. That corresponds to the vector (0,1,0)
        /// </summary>
        /// <param name="leftSpread"></param>
        /// <param name="rightSpread"></param>
        /// <returns></returns>
        public static Quaternion RandomRotationInRange(float leftSpread, float rightSpread)
        {
            Quaternion returnRotation = Quaternion.Euler(0, 0, Random.Range(-rightSpread, leftSpread));
            return returnRotation;
        }
    }
    public class AngleUtils
    {
        /// <summary>
        /// Returns a delta angle in degrees from vector "up" (0,1,0) to delta position between given vectors
        /// </summary>
        /// <param name="startingPosition"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public static float AngleFromUpToPosition(Vector3 startingPosition, Vector3 targetPosition)
        {
            Vector3 deltaPosition = VectorUtils.DeltaPosition(startingPosition, targetPosition);
            float deltaAngle = Vector3.SignedAngle(Vector3.up, deltaPosition, Vector3.forward);
            return deltaAngle;
        }
        /// <summary>
        /// Returns a delta angle in degrees from vector "up" (0,1,0) to given vector
        /// </summary>
        /// <param name="startingPosition"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public static float AngleFromUpToPosition(Vector3 deltaPosition)
        {
            float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, deltaPosition, Vector3.forward);
            return angleFromZeroToItem;
        }
        public static float ClampAngle360(float rotation)
        {
            float clamped = rotation % 360;
            if (clamped < 0)
            {
                return clamped + 360;
            }
            return clamped;
        }
        /// <summary>
        /// Clamps angle between <-180;180>
        /// </summary>
        /// <returns></returns>
        public static float ClampAngle180(float rotation)
        {
            float clamped = rotation % 360;
            if (clamped > 180)
            {
                return clamped - 360;
            }
            if (clamped < -180)
            {
                return clamped + 360;
            }
            return clamped;
        }
    }
    public class ListUtils
    {
        public static int GetWeightedIndex(List<int> probabilities)
        {
            int chance = Random.Range(0, CountChanceSum(probabilities));
            int sum = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                sum += probabilities[i];
                if (chance < sum)
                {
                    return i;
                }
            }
            return probabilities.Count - 1;
        }
        public static int CountChanceSum(List<int> probabilities)
        {
            int sum = 0;
            foreach (int chance in probabilities)
            {
                sum += chance;
            }
            return sum;
        }
        public static int GetIndexFromChance(int chance, List<int> probabilities)
        {
            int sum = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                sum += probabilities[i];
                if (chance < sum)
                {
                    return i;
                }
            }
            return probabilities.Count - 1;
        }
        public static List<GameObject> CloneList(List<GameObject> inputList)
        {
            List<GameObject> returnList = new List<GameObject>(inputList.Count);
            inputList.ForEach((item) => returnList.Add(item));
            return returnList;
        }
        public static List<float> CloneList(List<float> inputList)
        {
            List<float> returnList = new List<float>(inputList.Count);
            inputList.ForEach((item) => returnList.Add(item));
            return returnList;
        }
        public static List<int> CloneList(List<int> inputList)
        {
            List<int> returnList = new List<int>(inputList.Count);
            inputList.ForEach((item) => returnList.Add(item));
            return returnList;
        }
        public static List<Vector3> CloneList(List<Vector3> inputList)
        {
            List<Vector3> returnList = new List<Vector3>(inputList.Count);
            inputList.ForEach((item) => returnList.Add(item));
            return returnList;
        }
        public static List<Vector2> CloneList(List<Vector2> inputList)
        {
            List<Vector2> returnList = new List<Vector2>(inputList.Count);
            inputList.ForEach((item) => returnList.Add(item));
            return returnList;
        }
    }
    public class LineOfSightUtils
    {
        public enum LayerNames
        {
            Indestructibles,
            Team1Projectiles,
            Team2Projectiles,
            Team3Projectiles,
            EnemyToAllProjectiles,
            NeutralProjectiles,
            Team1Actors,
            Team2Actors,
            Team3Actors,
            EnemyToAllActors,
            NeutralActors
        }

        public static LayerNames NumberToLayerName(int number)
        {
            switch (number)
            {
                case 6:
                    return LayerNames.Indestructibles;
                case 9:
                    return LayerNames.Team1Projectiles;
                case 10:
                    return LayerNames.Team2Projectiles;
                case 11:
                    return LayerNames.Team3Projectiles;
                case 12:
                    return LayerNames.EnemyToAllProjectiles;
                case 13:
                    return LayerNames.NeutralProjectiles;
                case 14:
                    return LayerNames.Team1Actors;
                case 15:
                    return LayerNames.Team2Actors;
                case 16:
                    return LayerNames.Team3Actors;
                case 17:
                    return LayerNames.EnemyToAllActors;
                case 18:
                    return LayerNames.NeutralActors;
                default:
                    return LayerNames.Indestructibles;
            }
        }
        public static int LayerNameToNumber(LayerNames layerName)
        {
            switch (layerName)
            {
                case LayerNames.Indestructibles:
                    return 6;
                case LayerNames.Team1Projectiles:
                    return 9;
                case LayerNames.Team2Projectiles:
                    return 10;
                case LayerNames.Team3Projectiles:
                    return 11;
                case LayerNames.EnemyToAllProjectiles:
                    return 12;
                case LayerNames.NeutralProjectiles:
                    return 13;
                case LayerNames.Team1Actors:
                    return 14;
                case LayerNames.Team2Actors:
                    return 15;
                case LayerNames.Team3Actors:
                    return 16;
                case LayerNames.EnemyToAllActors:
                    return 17;
                case LayerNames.NeutralActors:
                    return 18;
                default:
                    return 6;
            }
        }
        #region Can see directly
        /// <summary>
        /// Checks, if the target is visible from the specified position. The default layer names are "Actors", "Obstacles".
        /// </summary>
        /// <param name="originalPos"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Vector2 GetRaycastHitPositionIgnoreEverything(Vector3 originalPos, GameObject target)
        {
            ObjectTypes[] objectTypes = {ObjectTypes.Entity, ObjectTypes.Indestructible};
            return GetRaycastHitPositionIgnoreEverything(originalPos, target, objectTypes);
        }
        public static Vector2 GetRaycastHitPositionIgnoreEverything(Vector3 originalPos, GameObject target, ObjectTypes[] objectTypes)
        {
            Vector2 direction = target.transform.position - originalPos;
            //Debug.DrawLine(target.transform.position, originalPos, Color.red);
            LayerNames[] layerNames = ObjectUtils.GetLayers(objectTypes).ToArray();

            // Check if is not already inside of the object
            Collider2D[] collidersInside = GetOverlappingCollidersWithCircle(originalPos, 0.01f, objectTypes);
            foreach (Collider2D collider in collidersInside)
            {
                if (collider.gameObject == target)
                {
                    return target.transform.position;
                }
            }

            RaycastHit2D[] raycastHits2DWithProjectiles = Physics2D.RaycastAll(originalPos, direction, direction.magnitude, GetLayerMask(layerNames));
            RaycastHit2D[] raycastHits2DNoProjectiles = Physics2D.RaycastAll(originalPos, direction, direction.magnitude, GetLayerMaskWithoutProjectiles(layerNames));

            foreach (RaycastHit2D hit in raycastHits2DWithProjectiles)
            {
                if(hit.collider.gameObject == target)
                {
                    return hit.point;
                }
            }
            foreach (RaycastHit2D hit in raycastHits2DNoProjectiles)
            {
                if (hit.collider.gameObject == target)
                {
                    return hit.point;
                }
            }
            Debug.LogError("This hit should be guaranteed");
            return Vector2.zero;
        }
        public static Vector2 GetRaycastHitPosition(Vector3 originalPos, GameObject target)
        {
            return GetRaycastHitPosition(originalPos, target, GetDefaultLayerNames());
        }
        public static Vector2 GetRaycastHitPosition(Vector3 originalPos, GameObject target, LayerNames[] layers)
        {
            Vector2 direction = target.transform.position - originalPos;
            //Debug.DrawLine(target.transform.position, originalPos, Color.red);

            RaycastHit2D raycastHit2DWithProjectiles = Physics2D.Raycast(originalPos, direction, direction.magnitude, GetLayerMask(layers));
            RaycastHit2D raycastHit2DNoProjectiles = Physics2D.Raycast(originalPos, direction, direction.magnitude, GetLayerMaskWithoutProjectiles(layers));
            if (!raycastHit2DNoProjectiles && !raycastHit2DWithProjectiles)
            {
                return Vector2.zero;
            }
            if (!raycastHit2DNoProjectiles)
            {

                return raycastHit2DWithProjectiles.point;
            }
            return raycastHit2DNoProjectiles.point;
        }
        public static bool CanSeeDirectly(Vector3 originalPos, GameObject target)
        {
            return CanSeeDirectly(originalPos, target, GetDefaultLayerNames());
        }
        public static bool CanSeeDirectly(Vector3 originalPos, GameObject target, LayerNames[] layers)
        {
            Vector2 direction = target.transform.position - originalPos;
            //Debug.DrawLine(target.transform.position, originalPos, Color.red);

            RaycastHit2D raycastHit2DWithProjectiles = Physics2D.Raycast(originalPos, direction, direction.magnitude, GetLayerMask(layers));
            RaycastHit2D raycastHit2DNoProjectiles = Physics2D.Raycast(originalPos, direction, direction.magnitude, GetLayerMaskWithoutProjectiles(layers));
            if (!raycastHit2DNoProjectiles && !raycastHit2DWithProjectiles)
            {
                return false;
            }
            if (!raycastHit2DNoProjectiles)
            {
                return raycastHit2DWithProjectiles.collider.gameObject == target;
            }
            return raycastHit2DNoProjectiles.collider.gameObject == target;
        }
        /// <summary>
        /// Checks, if the target position is visible from the specified position. The default layer names are "Actors" and "Obstacles".
        /// </summary>
        /// <param name="originalPos"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public static Collider2D[] GetOverlappingCollidersWithCircle(Vector2 circleMiddle, float range, ObjectTypes[] objectTypes)
        {
            int layerMask = GetLayerMask(ObjectUtils.GetLayers(objectTypes).ToArray());
            Collider2D[] collidersHit = Physics2D.OverlapCircleAll(circleMiddle, range, layerMask);
            return collidersHit;
        }
        public static bool CanSeeDirectly(Vector3 originalPos, Vector3 targetPosition)
        {
            return CanSeeDirectly(originalPos, targetPosition, GetDefaultLayerNames());
        }
        public static bool CanSeeDirectly(Vector3 originalPos, Vector3 targetPosition, LayerNames[] layers)
        {
            Vector2 direction = targetPosition - originalPos;
            //Debug.DrawRay(originalPos, direction, Color.red, 0.5f);

            RaycastHit2D raycastHit2DNoProjectiles = Physics2D.Raycast(originalPos, direction, direction.magnitude, GetLayerMaskWithoutProjectiles(layers));

            return !raycastHit2DNoProjectiles;
        }

        #region Helper methods
        private static LayerNames[] GetDefaultLayerNames()
        {
            LayerNames[] layers = { LayerNames.Team1Actors, LayerNames.Team2Actors, LayerNames.Team3Actors, LayerNames.EnemyToAllActors, LayerNames.Indestructibles };
            return layers;
        }
        private static int GetLayerMask(LayerNames[] layers)
        {
            string[] names = new string[layers.Length];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = layers[i].ToString();
            }
            return LayerMask.GetMask(names);
        }
        private static int GetLayerMaskWithoutProjectiles(LayerNames[] layers)
        {
            string[] names = new string[layers.Length];
            for (int i = 0; i < names.Length; i++)
            {
                bool isProjectile = layers[i] == LayerNames.EnemyToAllProjectiles || layers[i] == LayerNames.NeutralProjectiles || layers[i] == LayerNames.Team1Projectiles || layers[i] == LayerNames.Team2Projectiles || layers[i] == LayerNames.Team3Projectiles;
                if (isProjectile)
                {
                    continue;
                }
                names[i] = layers[i].ToString();
            }
            return LayerMask.GetMask(names);
        }
        /// <summary>
        /// Checks, if the target position is visible from the specified position.
        /// </summary>
        /// <param name="originalPos"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        #endregion

        public static bool CanSeeDirectly(Vector3 originalPos, Vector3 targetPosition, string[] layerNames)
        {
            int obstacleLayerMask = LayerMask.GetMask(layerNames);
            Vector2 direction = targetPosition - originalPos;
            //Debug.DrawRay(originalPos, direction, Color.red, 0.5f);

            RaycastHit2D raycastHit2D = Physics2D.Raycast(originalPos, direction, direction.magnitude, obstacleLayerMask);
            bool somethingIsInTheWay = raycastHit2D;
            if (somethingIsInTheWay)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Checks, if the target is visible from the specified position.
        /// </summary>
        /// <param name="originalPos"></param>
        /// <param name="target"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static bool CanSeeDirectly(Vector3 originalPos, GameObject target, string[] layerNames)
        {
            if (target == null)
            {
                Debug.LogError("The target was null");
                return false;
            }
            int obstacleLayerMask = LayerMask.GetMask(layerNames);
            Vector2 direction = target.transform.position - originalPos;

            RaycastHit2D raycastHit2D = Physics2D.Raycast(originalPos, direction, Mathf.Infinity, obstacleLayerMask);

            if (!raycastHit2D)
            {
                return false;
            }

            GameObject objectHit = raycastHit2D.collider.gameObject;
            bool hitTargetDirectly = objectHit == target;
            if (hitTargetDirectly)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Edge distance
        public static float EdgeDistance(GameObject from, GameObject to)
        {
            return EdgeDeltaPosition(from, to).magnitude;
        }
        public static Vector2 EdgeDeltaPosition(GameObject from, GameObject to)
        {
            Vector2 fromPosition = from.transform.position;
            Vector2 toPosition = to.transform.position;

            Collider2D toCollider = to.GetComponent<Collider2D>();
            Collider2D fromCollider = from.GetComponent<Collider2D>();
            Vector2 closestFromPoint = toCollider.ClosestPoint(fromPosition);
            Vector2 closestToPoint = fromCollider.ClosestPoint(toPosition);

            return closestFromPoint - closestToPoint;
        }
        #endregion
    }
    /// <summary>
    /// Type checks
    /// </summary>
    public class ObjectUtils
    {
        public static bool IsObjectAProjectile(GameObject collisionObject)
        {
            IDamageDealer damageReceiver = collisionObject.GetComponent<IDamageDealer>();
            if (damageReceiver != null)
            {
                return damageReceiver.GetDamageInstance().isAProjectile;
            }
            return false;
        }
        public static bool IsObjectAnEntity(GameObject collisionObject)
        {
            ListUpdater listUpdater = collisionObject.GetComponent<ListUpdater>();
            if (listUpdater)
            {
                return listUpdater.ListContains(StaticDataHolder.ObjectTypes.Entity);
            }
            return false;
        }
        public static bool IsObjectAnEntityPart(GameObject collisionObject)
        {
            DamageReceiver unitPart = collisionObject.GetComponent<DamageReceiver>();
            if (unitPart)
            {
                return true;
            }
            return false;
        }
        public static bool IsAnObstacle(GameObject collisionObject)
        {
            if (collisionObject.tag == "Obstacle")
            {
                return true;
            }
            // Is indestructible
            bool objectIsOnObstacleLayerMask = collisionObject.layer == 6;
            if (objectIsOnObstacleLayerMask)
            {
                return true;
            }
            ListUpdater listUpdater = collisionObject.GetComponent<ListUpdater>();
            if (listUpdater)
            {
                return listUpdater.ListContains(StaticDataHolder.ObjectTypes.Obstacle);
            }
            return false;
        }
        public static GameObject FindObjectWithName(string path, string name)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] info = dir.GetFiles("*.*");
            foreach (FileInfo f in info)
            {
                if (f.Extension == ".prefab")
                {
                    string fileName = f.Name;
                    string fileExtension = f.Extension;
                    fileName = fileName.Replace(fileExtension, "");

                    if (fileName == name)
                    {
                        string filePath = path + fileName + fileExtension;
                        return AssetDatabase.LoadMainAssetAtPath(filePath) as GameObject;
                    }
                }
            }
            return null;
        }
        public static List<LayerNames> GetLayers(ObjectTypes[] targetTypes)
        {
            List<LayerNames> layers = new List<LayerNames>();
            RemoveRepeatedTargetTypes(ref targetTypes);
            foreach (var type in targetTypes)
            {
                if (type == ObjectTypes.Entity || type == ObjectTypes.Obstacle)
                {
                    if (!layers.Contains(LayerNames.Team1Actors))
                    {
                        layers.Add(LayerNames.Team1Actors);
                        layers.Add(LayerNames.Team2Actors);
                        layers.Add(LayerNames.Team3Actors);
                        layers.Add(LayerNames.EnemyToAllActors);
                    }
                }
                if (type == ObjectTypes.Indestructible)
                {
                    if (!layers.Contains(LayerNames.Indestructibles))
                    {
                        layers.Add(LayerNames.Indestructibles);
                    }
                }
                if (type == ObjectTypes.Projectile)
                {
                    if (!layers.Contains(LayerNames.Team1Projectiles))
                    {
                        layers.Add(LayerNames.Team1Projectiles);
                        layers.Add(LayerNames.Team2Projectiles);
                        layers.Add(LayerNames.Team3Projectiles);
                        layers.Add(LayerNames.EnemyToAllProjectiles);
                    }
                }
            }
            return layers;
        }
        /// <summary>
        /// This edits the original array and removes repeated values from it
        /// </summary>
        /// <param name="targetTypes"></param>
        private static void RemoveRepeatedTargetTypes(ref StaticDataHolder.ObjectTypes[] targetTypes)
        {
            List<StaticDataHolder.ObjectTypes> nonRepeatedTargetTypes = new List<StaticDataHolder.ObjectTypes>();
            foreach (var type in targetTypes)
            {
                if (!nonRepeatedTargetTypes.Contains(type))
                {
                    nonRepeatedTargetTypes.Add(type);
                }
            }
            targetTypes = nonRepeatedTargetTypes.ToArray();
        }
    }
    /// <summary>
    /// Break checks
    /// </summary>
    public class CollisionUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collidingObject"></param>
        /// <param name="team">The team of the object that the collidingObject collides with</param>
        /// <returns></returns>
        public static List<BreakOnCollision.BreaksOn> GetCollisionProperties(GameObject collidingObject, Team team)
        {
            List<BreakOnCollision.BreaksOn> collisionPropertyList = new List<BreakOnCollision.BreaksOn>();
            CheckObstacle(collidingObject, collisionPropertyList);

            bool isAlly = IsAlly(collidingObject, team);
            CheckDamageDealers(collidingObject, collisionPropertyList, isAlly);
            CheckActors(collidingObject, collisionPropertyList, isAlly);

            return collisionPropertyList;
        }
        private static void CheckObstacle(GameObject collisionObject, List<BreakOnCollision.BreaksOn> collisionPropertyList)
        {
            if (ObjectUtils.IsAnObstacle(collisionObject))
            {
                collisionPropertyList.Add(BreakOnCollision.BreaksOn.Obstacles);
            }
        }
        private static bool IsAlly(GameObject collidingObject, Team team)
        {
            ITeamable teamable = collidingObject.GetComponent<ITeamable>();
            if (teamable == null)
            {
                return false;
            }
            Team objectTeam = teamable.GetTeam();
            return objectTeam.IsAlly(team);
        }
        private static void CheckDamageDealers(GameObject collisionObject, List<BreakOnCollision.BreaksOn> collisionPropertyList, bool isAlly)
        {
            IDamageDealer damageReceiver = collisionObject.GetComponent<IDamageDealer>();
            if (damageReceiver == null)
            {
                return;
            }
            if (ObjectUtils.IsObjectAProjectile(collisionObject))
            {
                if (isAlly)
                {
                    collisionPropertyList.Add(BreakOnCollision.BreaksOn.AllyProjectiles);
                }
                else
                {
                    collisionPropertyList.Add(BreakOnCollision.BreaksOn.EnemyProjectiles);
                }
            }
            if (damageReceiver.DamageCategoryContains(DamageInstance.TypeOfDamage.Explosion))
            {
                if (isAlly)
                {
                    collisionPropertyList.Add(BreakOnCollision.BreaksOn.AllyExplosions);
                }
                else
                {
                    collisionPropertyList.Add(BreakOnCollision.BreaksOn.EnemyExplosions);
                }
            }
        }
        private static void CheckActors(GameObject collisionObject, List<BreakOnCollision.BreaksOn> collisionPropertyList, bool isAlly)
        {
            if (!(ObjectUtils.IsObjectAnEntity(collisionObject) || ObjectUtils.IsObjectAnEntityPart(collisionObject)))
            {
                return;
            }

            if (isAlly)
            {
                collisionPropertyList.Add(BreakOnCollision.BreaksOn.AllyParts);
            }
            else
            {
                collisionPropertyList.Add(BreakOnCollision.BreaksOn.EnemyParts);
            }
        }

        #region OnDestroy
        /// <summary>
        /// Returns true, if this method has found and called at least one trigger
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static bool DoDestroyActions(GameObject gameObject, TriggerOnDeath.DestroyCause destroyCause)
        {
            TriggerOnDeath[] onDeathTriggers = gameObject.GetComponentsInChildren<TriggerOnDeath>();
            if (onDeathTriggers.Length == 0)
            {
                return false;
            }

            foreach (TriggerOnDeath trigger in onDeathTriggers)
            {
                trigger.CallDestroyAction(destroyCause);
            }
            return true;
        }
        #endregion

    }

    public class InputUtils
    {
        public static bool IsAnyInputKeyPressed()
        {
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        }

        public static bool IsMouseLeftClicked()
        {
            return Input.GetMouseButton(0);
        }
        public static bool IsMouseRightClicked()
        {
            return Input.GetMouseButton(1);
        }
    }
}
