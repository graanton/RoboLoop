using UnityEngine;

public abstract class ArmModuleBase : MonoBehaviour, IRobotModule
{
    protected RobotCharacter _robot;

    public virtual void OnAttach(RobotCharacter robot)
    {
        _robot = robot;
        gameObject.SetActive(true);
    }

    public virtual void OnDetach()
    {
        gameObject.SetActive(false);
    }

    public virtual void Tick() { }

    public virtual void OnPrimaryPressed() { }
    public virtual void OnPrimaryReleased() { }

    public virtual void OnSecondaryPressed() { }
    public virtual void OnSecondaryReleased() { }

    public virtual void OnUtility() { }
}