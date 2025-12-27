using Godot;
using System;

public partial class Hud  : CanvasLayer
{
    // 记得重新构建项目，这样编辑器才能识别新信号

    [Signal]
    public delegate void StartGameEventHandler();

    // 显示消息
    public void ShowMessage(string text)
    {
        var message = GetNode<Label>("Message");
        message.Text = text;
        message.Show();

        GetNode<Timer>("MessageTimer").Start();
    }

    public void UpdateHealth(int health)
    {
        var healthBar = GetNode<HBoxContainer>("HealthBar");
        var hearts = healthBar.GetChildren();

        for (int i = 0; i < hearts.Count; i++)
        {
            // 如果索引小于当前血量则显示，否则隐藏
            (hearts[i] as CanvasItem).Visible = i < health;
        }
    }

    // 异步显示游戏结束画面
    async public void ShowGameOver()
    {
        ShowMessage("Game Over");

        var messageTimer = GetNode<Timer>("MessageTimer");
        await ToSignal(messageTimer, Timer.SignalName.Timeout);

        var message = GetNode<Label>("Message");
        message.Text = "Dodge the Creeps!";
        message.Show();

        await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
        GetNode<Button>("StartButton").Show();
    }

    // 更新分数显示
    public void UpdateScore(int score)
    {
        GetNode<Label>("ScoreLabel").Text = score.ToString();
    }

    public void UpdateHighScore(int score)
    {
        GetNode<Label>("HighScoreLabel").Text = $"Best: {score}";
    }

    // 当开始按钮被按下时调用
    private void OnStartButtonPressed()
    {
        GetNode<Button>("StartButton").Hide();
        EmitSignal(SignalName.StartGame);
    }

    // 当消息计时器超时时调用
    private void OnMessageTimerTimeout()
    {
        GetNode<Label>("Message").Hide();
    }
}