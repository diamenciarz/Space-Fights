using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Single Shot", menuName = "Shots/Single Shot")]
public class SingleShotScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Each shot")]
    public bool spreadProjectilesEvenly;
    public float spreadDegrees;
    public float leftBulletSpread;
    public float rightBulletSpread;
    public List<EntityCreator.Projectiles> projectilesToCreateList;
    [Header("Sound")]
    public AudioClip[] shotSounds;
    public float shotSoundVolume;

    public void OnAfterDeserialize()
    {

    }
    public void OnBeforeSerialize()
    {

    }
}
