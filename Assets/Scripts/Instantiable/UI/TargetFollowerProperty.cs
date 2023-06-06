using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollowerProperty : MonoBehaviour
{
    public EntityCreator.ObjectFollowers targetIcon;
    public bool followMouseCursor;
    public StaticDataHolder.ObjectTypes[] targetTypesToFollow;
}
