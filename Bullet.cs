using Godot;

public partial class Bullet : Area2D
{
    [Export]
    public float Speed = 600f;

    private Vector2 _velocity;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += _velocity * (float)delta;
    }

    public void SetDirection(Vector2 direction)
    {
        _velocity = direction.Normalized() * Speed;
        Rotation = direction.Angle();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("mobs"))
            return;

        body.QueueFree();   // 只杀怪物
        QueueFree();        // 子弹销毁
    }
}
