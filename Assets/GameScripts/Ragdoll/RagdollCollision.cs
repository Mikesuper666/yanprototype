using UnityEngine;

public class RagdollCollision
{
    private GameObject sender;

    private Collision collision;

    private float impactForce;
    /// <summary>
    /// GameObject recebedor de collisionInfo
    /// </summary>
    public GameObject Sender { get { return sender; } }// Gameobjet receiver of collision info

    public Collision Collision { get { return collision; } }
    /// <summary>
    /// Megnitude  de relação linear de 2 objetos colidindo
    /// </summary>
    public float ImpactForce { get { return impactForce; } }// Magnitude of relative linear velocity of the two colliding objects
    /// <summary>
    /// Cria um novo collisionInfo
    /// </summary>
    public RagdollCollision(GameObject sender, Collision collision)
    {
        this.sender = sender;
        this.collision = collision;
        this.impactForce = collision.relativeVelocity.magnitude;
    }/// Create a New collision info
}
