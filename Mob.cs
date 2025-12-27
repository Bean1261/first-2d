using Godot;
using System;

public partial class Mob : RigidBody2D
{
    // 怪物准备就绪时调用
    public override void _Ready()
    {
        var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        // 获取所有动画名称并随机播放一个
        string[] mobTypes = animatedSprite2D.SpriteFrames.GetAnimationNames();
        animatedSprite2D.Play(mobTypes[GD.Randi() % mobTypes.Length]);
    }

    // 当怪物离开屏幕时调用
    private void OnVisibleOnScreenNotifier2DScreenExited()
    {
        QueueFree(); // 从内存中释放怪物节点
    }
}