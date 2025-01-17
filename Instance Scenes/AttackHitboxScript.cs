using Godot;
using System;
public partial class AttackHitboxScript : Area3D
{
    [Export] private CollisionShape3D collisionShape;

    public void Collided(Node3D body)
    {
        if (body.IsClass("CharachterBody3D") && body.Name != "PlayerBody3D") { GD.Print("prevented deletion due to self hit"); return; }
        Visible = false;

        CallDeferred(AttackHitboxScript.MethodName.DisableThisThing);
    }

    private void DisableThisThing() { GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true; GD.Print("DELETED HITBOX"); }
}