using System;
using System.Collections.Generic;
using UnityEngine;
using static HelperMethods.LineOfSightUtils;
using static HelperMethods.VectorUtils;
using static TeamUpdater;

public static class StaticDataHolder
{
    public static Dictionary<ObjectTypes, List<GameObject>> listDictionary = new Dictionary<ObjectTypes, List<GameObject>>();

    public static List<float> soundDurationList = new List<float>();
    public static int soundLimit = 10;

    public enum ObjectTypes
    {
        Obstacle,
        Projectile,
        Entity
    }
    private static bool wasInstantiated = false;


    public static class ListModification
    {
        public static void AddObject(ObjectTypes objectType, GameObject addObject)
        {
            ListContents.Generic.InstantiateAllLists();

            List<GameObject> objectList;
            listDictionary.TryGetValue(objectType, out objectList);
            objectList.Add(addObject);
        }
        public static void RemoveObject(ObjectTypes objectType, GameObject removeObject)
        {
            ListContents.Generic.InstantiateAllLists();

            List<GameObject> objectList;
            listDictionary.TryGetValue(objectType, out objectList);
            if (objectList.Contains(removeObject))
            {
                objectList.Remove(removeObject);
            }
        }
        public static List<GameObject> SubtractAllies(List<GameObject> inputList, Team myTeam)
        {
            for (int i = inputList.Count - 1; i >= 0; i--)
            {
                ITeamable teamable = inputList[i].GetComponent<ITeamable>();
                if (teamable != null)
                {
                    if (teamable.GetTeam().IsAlly(myTeam))
                    {
                        inputList.Remove(inputList[i]);
                    }
                }
            }
            return inputList;
        }
        public static List<GameObject> SubtractMeAndEnemies(List<GameObject> inputList, Team myTeam, GameObject gameObjectToIgnore)
        {
            for (int i = inputList.Count - 1; i >= 0; i--)
            {
                ITeamable teamable = inputList[i].GetComponent<ITeamable>();
                if (teamable == null)
                {
                    continue;
                }
                if (teamable.GetTeam().IsEnemy(myTeam))
                {
                    inputList.Remove(inputList[i]);
                }
                //Remove itself from ally list
                if (inputList[i] == gameObjectToIgnore)
                {
                    inputList.Remove(inputList[i]);
                }
            }
            return inputList;
        }
        public static List<GameObject> SubtractNeutrals(List<GameObject> inputList, Team myTeam)
        {
            for (int i = inputList.Count - 1; i >= 0; i--)
            {
                ITeamable teamable = inputList[i].GetComponent<ITeamable>();
                if (teamable == null)
                {
                    continue;
                }
                if (teamable.GetTeam().IsNeutral(myTeam))
                {
                    inputList.Remove(inputList[i]);
                }
            }
            return inputList;
        }
        public static List<GameObject> SubtractNeutralsAndAllies(List<GameObject> inputList, Team myTeam)
        {
            return SubtractAllies(SubtractNeutrals(inputList, myTeam), myTeam);
        }
        public static List<GameObject> DeleteObstacles(List<GameObject> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (HelperMethods.ObjectUtils.IsAnObstacle(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
            return list;
        }
        public static List<GameObject> GetObjectsInCone(List<GameObject> targetList, Cone cone)
        {
            List<GameObject> objectsInCone = new List<GameObject>();
            foreach (GameObject target in targetList)
            {
                if (IsPositionInCone(target.transform.position, cone))
                {
                    objectsInCone.Add(target);
                }
            }
            return objectsInCone;
        }
        public static List<GameObject> GetObjectsInRange(List<GameObject> targetList, Vector2 position, float range)
        {
            List<GameObject> objectsInCone = new List<GameObject>();
            foreach (GameObject target in targetList)
            {
                if (IsPositionInRange(target.transform.position, position, range))
                {
                    objectsInCone.Add(target);
                }
            }
            return objectsInCone;
        }
    }
    public static class ListContents
    {
        public static class Enemies
        {
            public static GameObject GetClosestEnemyInSightAngleWise(Vector3 positionVector, Team myTeam, float direction)
            {
                return Generic.GetClosestObjectInSightAngleWise(GetEnemyList(myTeam), positionVector, direction);
            }
            public static GameObject GetClosestEnemyAngleWise(Vector3 positionVector, Team myTeam, float direction)
            {
                return Generic.GetClosestObjectAngleWise(GetEnemyList(myTeam), positionVector, direction);
            }
            public static GameObject GetClosestEnemyInSight(Vector3 positionVector, Team myTeam)
            {
                return Generic.GetClosestObjectInSight(GetEnemyList(myTeam), positionVector);
            }
            public static GameObject GetClosestEnemy(Vector3 positionVector, Team myTeam)
            {
                return Generic.GetClosestObject(GetEnemyList(myTeam), positionVector);
            }
            public static List<GameObject> GetEnemyList(Team myTeam)
            {
                return ListModification.SubtractNeutrals(ListModification.SubtractAllies(Generic.GetEntityList(), myTeam), myTeam);
            }
        }
        public static class Allies
        {
            public static GameObject GetClosestAllyInSightAngleWise(Vector3 positionVector, Team myTeam, GameObject gameObjectToIgnore, float direction)
            {
                return Generic.GetClosestObjectInSightAngleWise(GetAllyList(myTeam, gameObjectToIgnore), positionVector, direction);
            }
            public static GameObject GetClosestAllyAngleWise(Vector3 positionVector, Team myTeam, GameObject gameObjectToIgnore, float direction)
            {
                return Generic.GetClosestObjectAngleWise(GetAllyList(myTeam, gameObjectToIgnore), positionVector, direction);
            }
            public static GameObject GetClosestAllyInSight(Vector3 positionVector, Team myTeam, GameObject gameObjectToIgnore)
            {
                return Generic.GetClosestObjectInSight(GetAllyList(myTeam, gameObjectToIgnore), positionVector);
            }
            public static GameObject GetClosestAlly(Vector3 positionVector, Team myTeam, GameObject gameObjectToIgnore)
            {
                return Generic.GetClosestObject(GetAllyList(myTeam, gameObjectToIgnore), positionVector);
            }
            public static List<GameObject> GetAllyList(Team myTeam, GameObject gameObjectToIgnore)
            {
                return ListModification.SubtractNeutrals(ListModification.SubtractMeAndEnemies(Generic.GetEntityList(), myTeam, gameObjectToIgnore), myTeam);
            }
        }
        public static class Generic
        {
            public static bool ListContainsNonObstacle(List<GameObject> list)
            {
                foreach (var item in list)
                {
                    if (!HelperMethods.ObjectUtils.IsAnObstacle(item))
                    {
                        return true;
                    }
                }
                return false;
            }
            public static List<GameObject> GetEntityList()
            {
                List<GameObject> entityList;
                listDictionary.TryGetValue(ObjectTypes.Entity, out entityList);
                return HelperMethods.ListUtils.CloneList(entityList);
            }
            public static GameObject GetClosestObject(List<GameObject> possibleTargetList, Vector3 positionVector)
            {
                if (possibleTargetList.Count == 0)
                {
                    return null;
                }
                GameObject currentNearestTarget = null;
                foreach (GameObject target in possibleTargetList)
                {
                    if (currentNearestTarget == null)
                    {
                        currentNearestTarget = target;
                        continue;
                    }
                    bool currentTargetIsCloser = Distance(positionVector, target.transform.position) < HelperMethods.VectorUtils.Distance(positionVector, currentNearestTarget.transform.position);
                    if (currentTargetIsCloser)
                    {
                        currentNearestTarget = target;
                    }
                }
                return currentNearestTarget;
            }
            public static GameObject GetClosestObjectInSight(List<GameObject> possibleTargetList, Vector3 positionVector)
            {
                if (possibleTargetList.Count == 0)
                {
                    return null;
                }
                GameObject currentNearestTarget = null;
                foreach (GameObject target in possibleTargetList)
                {
                    if (!CanSeeDirectly(positionVector, target))
                    {
                        continue;
                    }
                    if (currentNearestTarget == null)
                    {
                        currentNearestTarget = target;
                        continue;
                    }
                    bool currentTargetIsCloser = Distance(positionVector, target.transform.position) < Distance(positionVector, currentNearestTarget.transform.position);
                    if (currentTargetIsCloser)
                    {
                        currentNearestTarget = target;
                    }
                }
                return currentNearestTarget;
            }
            public static GameObject GetClosestObjectInSight(List<GameObject> possibleTargetList, Vector3 positionVector, LayerNames[] layers)
            {
                if (possibleTargetList.Count == 0)
                {
                    return null;
                }
                GameObject currentNearestTarget = null;
                foreach (GameObject target in possibleTargetList)
                {
                    if (!CanSeeDirectly(positionVector, target, layers))
                    {
                        continue;
                    }
                    if (currentNearestTarget == null)
                    {
                        currentNearestTarget = target;
                        continue;
                    }
                    bool currentTargetIsCloser = Distance(positionVector, target.transform.position) < Distance(positionVector, currentNearestTarget.transform.position);
                    if (currentTargetIsCloser)
                    {
                        currentNearestTarget = target;
                    }
                }
                return currentNearestTarget;
            }
            public static GameObject GetClosestObjectAngleWise(List<GameObject> targetList, Vector3 middlePosition, float middleAngle)
            {
                if (targetList.Count == 0)
                {
                    return null;
                }
                GameObject currentClosestTarget = null;
                foreach (GameObject target in targetList)
                {
                    if (currentClosestTarget == null)
                    {
                        currentClosestTarget = target;
                        continue;
                    }
                    if (IsFirstCloserToMiddleThanSecond(target, currentClosestTarget, middlePosition, middleAngle))
                    {
                        currentClosestTarget = target;
                    }
                }
                return currentClosestTarget;
            }
            public static GameObject GetClosestObjectInSightAngleWise(List<GameObject> targetList, Vector3 middlePosition, float middleAngle)
            {
                if (targetList.Count == 0)
                {
                    return null;
                }
                GameObject currentClosestTarget = null;
                foreach (GameObject target in targetList)
                {
                    if (!CanSeeDirectly(middlePosition, target))
                    {
                        continue;
                    }
                    if (currentClosestTarget == null)
                    {
                        currentClosestTarget = target;
                        continue;
                    }
                    if (IsFirstCloserToMiddleThanSecond(target, currentClosestTarget, middlePosition, middleAngle))
                    {
                        currentClosestTarget = target;
                    }
                }
                return currentClosestTarget;
            }
            public static GameObject GetClosestObjectInSightAngleWise(List<GameObject> targetList, Vector3 middlePosition, float middleAngle, LayerNames[] layers)
            {
                if (targetList.Count == 0)
                {
                    return null;
                }
                GameObject currentClosestTarget = null;
                foreach (GameObject target in targetList)
                {
                    if (!CanSeeDirectly(middlePosition, target, layers))
                    {
                        continue;
                    }
                    if (currentClosestTarget == null)
                    {
                        currentClosestTarget = target;
                        continue;
                    }
                    if (IsFirstCloserToMiddleThanSecond(target, currentClosestTarget, middlePosition, middleAngle))
                    {
                        currentClosestTarget = target;
                    }
                }
                return currentClosestTarget;
            }
            public static bool IsFirstCloserToMiddleThanSecond(GameObject first, GameObject second, Vector3 middlePosition, float middleAngle = 0)
            {
                float zAngleFromMiddleToCurrentClosestEnemy = CountAngleFromMiddleToPosition(middlePosition, second.transform.position, middleAngle);
                float zAngleFromMiddleToItem = CountAngleFromMiddleToPosition(middlePosition, first.transform.position, middleAngle);
                //If the found target is closer to the middle (angle wise) than the current closest target, make is the closest target
                bool isCloserAngleWise = Mathf.Abs(zAngleFromMiddleToCurrentClosestEnemy) > Mathf.Abs(zAngleFromMiddleToItem);
                return isCloserAngleWise;
            }
            public static List<GameObject> GetObjectList(ObjectTypes objectType)
            {
                InstantiateAllLists();

                List<GameObject> objectList;
                listDictionary.TryGetValue(objectType, out objectList);
                return HelperMethods.ListUtils.CloneList(objectList);
            }
            public static List<GameObject> GetObjectList(ObjectTypes[] objectType)
            {
                InstantiateAllLists();
                List<GameObject> allObjectList = new List<GameObject>();
                foreach (var type in objectType)
                {
                    List<GameObject> objectList;
                    listDictionary.TryGetValue(type, out objectList);
                    allObjectList.AddRange(objectList);
                }
                return HelperMethods.ListUtils.CloneList(allObjectList);
            }
            #region Helper methods
            public static void InstantiateAllLists()
            {
                if (wasInstantiated)
                {
                    return;
                }
                wasInstantiated = true;
                foreach (var type in Enum.GetValues(typeof(ObjectTypes)))
                {
                    ObjectTypes objectType = (ObjectTypes)type;
                    CreateList(objectType);
                }
            }
            private static void CreateList(ObjectTypes objectType)
            {
                listDictionary.Add(objectType, new List<GameObject>());
            }
            private static float CountAngleFromMiddleToPosition(Vector3 middlePosition, Vector3 targetPosition, float middleAngle)
            {
                float angleFromZeroToItem = HelperMethods.RotationUtils.DeltaPositionRotation(middlePosition, targetPosition).eulerAngles.z;
                float angleFromGunToItem = Mathf.DeltaAngle(middleAngle, angleFromZeroToItem);

                return angleFromGunToItem;
            }
            #endregion
        }
    }
    public static class Sounds
    {
        #region Sound List
        public static void AddSoundDuration(float duration)
        {
            soundDurationList.Add(Time.time + duration);
        }
        public static int GetSoundCount()
        {
            RemoveOldSoundsFromList();
            return soundDurationList.Count;
        }
        private static void RemoveOldSoundsFromList()
        {
            for (int i = soundDurationList.Count - 1; i >= 0; i--)
            {
                if (soundDurationList[i] < Time.time)
                {
                    soundDurationList.RemoveAt(i);
                }
            }
        }
        public static int GetSoundLimit()
        {
            return soundLimit;
        }
        #endregion

        /// <summary>
        /// Plays a sound, if there is space in the list of sounds. 
        /// MaxSoundCount is currently from 1-10. 
        /// The higher the count, the higher the priority, so more sounds can play in total.
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="soundPosition"></param>
        /// <param name="volume"></param>
        public static void PlaySound(AudioClip sound, Vector3 soundPosition, float volume, int maxSoundCount)
        {
            if (sound == null)
            {
                return;
            }
            if (GetSoundCount() <= (GetSoundLimit() - GetSoundCount() + maxSoundCount))
            {
                AudioSource.PlayClipAtPoint(sound, soundPosition, volume);
                AddSoundDuration(sound.length);
            }
        }
        /// <summary>
        /// Plays a sound, if there is space in the list of sounds. 
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="soundPosition"></param>
        /// <param name="volume"></param>
        public static void PlaySound(AudioClip sound, Vector3 soundPosition, float volume)
        {
            if (sound == null)
            {
                return;
            }
            if (GetSoundCount() <= GetSoundLimit())
            {
                AudioSource.PlayClipAtPoint(sound, soundPosition, volume);
                AddSoundDuration(sound.length);
            }
        }
    }
}
