using Godot;
using System;

public partial class Player : Area2D
{
    // 玩家被击中时发出的信号
    [Signal]
    public delegate void HitEventHandler();

    [Signal]
    public delegate void HealthChangedEventHandler(int node); // 用于通知 UI 更新.

    [Export]
    public int Speed { get; set; } = 400; // 玩家移动速度（像素/秒）

    [Export]
    public PackedScene BulletScene;

    [Export]
    public int MaxHealth = 3;
    private int _currentHealth;

    public Vector2 ScreenSize; // 游戏窗口大小

    private Vector2 _direction = Vector2.Right; // 记录朝向

    public override void _Ready()
    {
        // 获取视口大小并隐藏玩家（等待游戏开始）
        ScreenSize = GetViewportRect().Size;
        Hide();
    }

    public override void _Process(double delta)
    {
        var velocity = Vector2.Zero; // 玩家的移动向量

        // 检测玩家输入，设置移动方向
        if (Input.IsActionPressed("move_right"))
        {
            velocity.X += 1;
        }

        if (Input.IsActionPressed("move_left"))
        {
            velocity.X -= 1;
        }

        if (Input.IsActionPressed("move_down"))
        {
            velocity.Y += 1;
        }

        if (Input.IsActionPressed("move_up"))
        {
            velocity.Y -= 1;
        }

        var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        if (velocity != Vector2.Zero)
        {
            _direction = velocity.Normalized();
            Position += _direction * Speed * (float)delta;
        }


        if (velocity.Length() > 0)
        {
            // 标准化速度向量并应用速度
            velocity = velocity.Normalized() * Speed;
            animatedSprite2D.Play();
        }
        else
        {
            animatedSprite2D.Stop();
        }
        
        // 更新玩家位置
        Position = new Vector2(
            x: Mathf.Clamp(Position.X, 0, ScreenSize.X), // 限制X坐标在屏幕范围内
            y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y)  // 限制Y坐标在屏幕范围内
        );
        
        // 根据移动方向播放合适的动画
        if (velocity.X != 0)
        {
            animatedSprite2D.Animation = "walk";
            animatedSprite2D.FlipV = false;
            // 水平翻转精灵以匹配移动方向
            animatedSprite2D.FlipH = velocity.X < 0;
        }
        else if (velocity.Y != 0)
        {
            animatedSprite2D.Animation = "up";
            animatedSprite2D.FlipV = velocity.Y > 0;
        }
        
        // 修正水平翻转（处理Y轴移动时的翻转）
        if (velocity.X < 0)
        {
            animatedSprite2D.FlipH = true;
        }
        else
        {
            animatedSprite2D.FlipH = false;
        }

        if (Input.IsActionJustPressed("shoot"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (BulletScene == null)
            return;

        var bullet = BulletScene.Instantiate<Bullet>();

        bullet.GlobalPosition = GlobalPosition + _direction * 20f;
        bullet.SetDirection(_direction);

        GetParent().AddChild(bullet);
    }

    public void Start(Vector2 position)
    {
        Position = position;
        _currentHealth = MaxHealth; // 重置血量
        EmitSignal(SignalName.HealthChanged, _currentHealth); // 通知初始血量
        Show();
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
    }

    private void OnBodyEntered(Node2D body)
    {
        // 如果撞到的是怪物
        if (body.IsInGroup("mobs"))
        {
            _currentHealth -= 1;
            EmitSignal(SignalName.HealthChanged, _currentHealth);

            // 简单的受伤反馈：闪烁一下
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.2f, 0.1f);
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.1f);

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 暂时移除怪物，防止连续扣血
                body.QueueFree(); 
            }
        }
    }

    private void Die()
    {
        Hide();
        EmitSignal(SignalName.Hit);
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }
}