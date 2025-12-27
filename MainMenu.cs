using Godot;
using System;

public partial class MainMenu : CanvasLayer
{
    public override void _Ready()
    {
        // 连接按钮信号
        GetNode<Button>("MarginContainer/VBoxContainer/StartButton").Pressed += OnStartPressed;
        GetNode<Button>("MarginContainer/VBoxContainer/SettingsButton").Pressed += OnSettingsPressed;
        GetNode<Button>("MarginContainer/VBoxContainer/QuitButton").Pressed += OnQuitPressed;
    }

    private void OnStartPressed()
    {
        // 切换到 Main.tscn 场景
        GetTree().ChangeSceneToFile("res://Main.tscn");
    }

    private void OnSettingsPressed()
    {
        // 弹出设置窗口 (假设你做了一个 Popup 或另一个 Panel)
        GD.Print("打开设置菜单");
    }

    private void OnQuitPressed()
    {
        // 退出游戏
        GetTree().Quit();
    }
}