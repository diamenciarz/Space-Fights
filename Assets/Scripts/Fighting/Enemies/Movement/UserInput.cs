using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class UserInput : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] List<KeyCode> keys;
    [SerializeField] List<MoveAction> actions;

    private Rigidbody2D rb2D;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            CheckIfKeyPressed(i);
        }
    }

    private void CheckIfKeyPressed(int i)
    {
        if (Input.GetKey(keys[i]))
        {
            callAction(i);
        }
    }

    private void callAction(int i)
    {
        actions[i].applyAction(rb2D);
    }

    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public void OnBeforeSerialize()
    {
        controlGradientProbabilitiesLength();
    }
    private void controlGradientProbabilitiesLength()
    {
        if (actions.Count < keys.Count)
        {
            actions.Add(null);
        }
        if (actions.Count > keys.Count)
        {
            actions.RemoveAt(actions.Count - 1);
        }
    }
    #endregion
}
