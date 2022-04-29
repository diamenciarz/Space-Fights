using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    #region Vectors
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
    #endregion

    #region Rotation
    /// <summary>
    /// Rotation from first position to second position. Range <0;360)
    /// </summary>
    /// <param name="firstPosition"></param>
    /// <param name="secondPosition"></param>
    /// <returns></returns>
    public static Quaternion DeltaPositionRotation(Vector3 firstPosition, Vector3 secondPosition)
    {
        Vector3 deltaPosition = DeltaPosition(firstPosition, secondPosition);
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
    #endregion

    #region Angles
    /// <summary>
    /// Returns a delta angle in degrees from vector "up" (0,1,0) to delta position between given vectors
    /// </summary>
    /// <param name="startingPosition"></param>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    public static float AngleFromUpToPosition(Vector3 startingPosition, Vector3 targetPosition)
    {
        Vector3 deltaPosition = HelperMethods.DeltaPosition(startingPosition, targetPosition);
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
    #endregion

    #region List Methods
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
    #endregion

    #region LineOfSight
    /// <summary>
    /// Checks, if the target is visible from the specified position. The default layer names are "Actors" and "Obstacles".
    /// </summary>
    /// <param name="originalPos"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool CanSeeDirectly(Vector3 originalPos, GameObject target)
    {
        if (target)
        {
            int obstacleLayerMask = LayerMask.GetMask("Actors", "Obstacles");
            Vector2 direction = target.transform.position - originalPos;
            //Debug.DrawRay(originalPos, direction, Color.red, 0.1f);

            RaycastHit2D raycastHit2D = Physics2D.Raycast(originalPos, direction, Mathf.Infinity, obstacleLayerMask);

            if (raycastHit2D)
            {
                GameObject objectHit = raycastHit2D.collider.gameObject;

                bool hitTargetDirectly = objectHit == target;
                if (hitTargetDirectly)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return false;
        }
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
        if (target != null)
        {
            int obstacleLayerMask = LayerMask.GetMask(layerNames);
            Vector2 direction = target.transform.position - originalPos;
            //Debug.DrawRay(originalPos, direction, Color.red, 0.5f);

            RaycastHit2D raycastHit2D = Physics2D.Raycast(originalPos, direction, Mathf.Infinity, obstacleLayerMask);

            if (raycastHit2D)
            {
                GameObject objectHit = raycastHit2D.collider.gameObject;

                bool hitTargetDirectly = objectHit == target;
                if (hitTargetDirectly)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            Debug.LogError("The target was null");
            return false;
        }
    }
    #endregion

    #region Object checks
    public static bool IsObjectAProjectile(GameObject collisionObject)
    {
        IDamageDealer damageReceiver = collisionObject.GetComponent<IDamageDealer>();
        if (damageReceiver != null)
        {
            return damageReceiver.IsAProjectile();
        }
        return false;
    }
    public static bool IsObjectAnEntity(GameObject collisionObject)
    {
        ListUpdater listUpdater = collisionObject.GetComponent<ListUpdater>();
        if (listUpdater)
        {
            return listUpdater.ListContains(ListUpdater.AddToLists.Entity);
        }
        return false;
    }
    public static bool IsObjectAnEntityPart(GameObject collisionObject)
    {
        UnitPart unitPart = collisionObject.GetComponent<UnitPart>();
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

        ListUpdater listUpdater = collisionObject.GetComponent<ListUpdater>();
        if (listUpdater)
        {
            return listUpdater.ListContains(ListUpdater.AddToLists.Obstacle);
        }
        return false;
    }
    public static int GetObjectTeam(GameObject collisionObject)
    {
        DamageReceiver damageReceiver = collisionObject.GetComponentInChildren<DamageReceiver>();
        if (damageReceiver)
        {
            return damageReceiver.GetTeam();
        }
        else
        {
            TeamUpdater teamUpdater = collisionObject.GetComponentInChildren<TeamUpdater>();
            if (teamUpdater)
            {
                return teamUpdater.GetTeam();
            }
        }
        return -2;
    }
    public static bool IsAnEntityPart(GameObject collisionObject)
    {
        UnitPart listUpdater = collisionObject.GetComponent<UnitPart>();
        if (listUpdater)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Break checks
    /// <summary>
    /// 
    /// </summary>
    /// <param name="collidingObject"></param>
    /// <param name="team">The team of the object that the collidingObject collides with</param>
    /// <returns></returns>
    public static List<BreakOnCollision.BreaksOn> GetCollisionProperties(GameObject collidingObject, int team)
    {
        List<BreakOnCollision.BreaksOn> collisionPropertyList = new List<BreakOnCollision.BreaksOn>();
        CheckObstacle(collidingObject, collisionPropertyList);

        bool isAlly = IsAlly(collidingObject, team);
        CheckProjectile(collidingObject, collisionPropertyList, isAlly);
        CheckActors(collidingObject, collisionPropertyList, isAlly);

        return collisionPropertyList;
    }
    private static bool IsAlly(GameObject collidingObject, int team)
    {
        int collidingObjectTeam = GetObjectTeam(collidingObject);
        if (collidingObjectTeam == -1 || team == -1)
        {
            return false;
        }
        return collidingObjectTeam == team;
    }
    private static void CheckObstacle(GameObject collisionObject, List<BreakOnCollision.BreaksOn> collisionPropertyList)
    {
        if (HelperMethods.IsAnObstacle(collisionObject))
        {
            collisionPropertyList.Add(BreakOnCollision.BreaksOn.Obstacles);
        }
    }
    private static void CheckProjectile(GameObject collisionObject, List<BreakOnCollision.BreaksOn> collisionPropertyList, bool isAlly)
    {
        IDamageDealer damageReceiver = collisionObject.GetComponent<IDamageDealer>();
        if (damageReceiver == null)
        {
            return;
        }
        if (damageReceiver.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Projectile))
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
        if (damageReceiver.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Explosion))
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
        if (damageReceiver.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Rocket))
        {
            if (isAlly)
            {
                collisionPropertyList.Add(BreakOnCollision.BreaksOn.AllyRockets);
            }
            else
            {
                collisionPropertyList.Add(BreakOnCollision.BreaksOn.EnemyRockets);
            }
        }
    }
    private static void CheckActors(GameObject collisionObject, List<BreakOnCollision.BreaksOn> collisionPropertyList, bool isAlly)
    {
        if (!(IsObjectAnEntity(collisionObject) || IsObjectAnEntityPart(collisionObject)))
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

    #endregion
}
