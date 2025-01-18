using Godot;
using System;
public partial class AttackHitboxScript : Area3D
{
    [Export] private CollisionShape3D collisionShape;
    private Node3D hitBody;
    public void Collided(Node3D body)
    {
        hitBody = body;
        if (body.IsClass("CharacterBody3D") && body.Name != "PlayerBody3D") { /*GD.Print("prevented deletion due to self hit");*/ return; }
        Visible = false;

        CallDeferred(AttackHitboxScript.MethodName.DisableThisThing);
    }

    private void DisableThisThing() { GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true; /*GD.Print("DELETED HITBOX due to collision with " + hitBody.Name + " class: " + hitBody.GetClass().ToString());*/ }
}