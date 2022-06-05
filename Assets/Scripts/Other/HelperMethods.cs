using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static TeamUpdater;

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
        private static int CountChanceSum(List<int> probabilities)
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
    }
    public class LineOfSightUtils
    {
        /// <summary>
        /// Checks, if the target is visible from the specified position. The default layer names are "Actors" and "Obstacles".
        /// </summary>
        /// <param name="originalPos"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CanSeeDirectly(Vector3 originalPos, GameObject target)
        {
            if (target == null)
            {
                return false;
            }
            int obstacleLayerMask = LayerMask.GetMask("Actors", "Obstacles", "Projectiles");
            Vector2 direction = target.transform.position - originalPos;
            Debug.DrawRay(originalPos, direction, Color.red, 0.1f);

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
        /// <summary>
        /// Checks, if the target position is visible from the specified position. The default layer names are "Actors" and "Obstacles".
        /// </summary>
        /// <param name="originalPos"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public static bool CanSeeDirectly(Vector3 originalPos, Vector3 targetPosition)
        {
            int obstacleLayerMask = LayerMask.GetMask("Actors", "Obstacles");
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
        /// Checks, if the target position is visible from the specified position.
        /// </summary>
        /// <param name="originalPos"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
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
            if (damageReceiver.DamageCategoryContains(OnCollisionDamage.TypeOfDamage.Explosion))
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
        public static bool DoDestroyActions(GameObject gameObject)
        {
            TriggerOnDeath[] onDeathTriggers = gameObject.GetComponentsInChildren<TriggerOnDeath>();
            if (onDeathTriggers.Length == 0)
            {
                return false;
            }

            foreach (TriggerOnDeath trigger in onDeathTriggers)
            {
                trigger.DoDestroyAction();
            }
            return true;
        }
        #endregion

    }
}
