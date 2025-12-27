using Godot;
using System;
using System.Text.Json;

public partial class Main : Node
{
    [Export]
    public PackedScene MobScene { get; set; } // 怪物场景的引用

    private int _score; // 游戏分数

    private int _highScore = 0;

    private const string SavePath = "user://highscore.json"; // user:// 是 Godot

    public override void _Ready()
    {
        LoadHighScore(); // 游戏启动时加载
        var hud = GetNode<Hud>("HUD"); 
        hud.UpdateHighScore(_highScore);
        NewGame(); // 游戏开始时初始化
    }
    
    // 保存最高分
    private void SaveHighScore()
    {
        var data = new GameData { HighScore = _highScore };
        string jsonString = JsonSerializer.Serialize(data);

        // 使用 Godot 的 FileAccess 打开文件
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(jsonString);
            GD.Print("最高分已保存: " + _highScore);
            // 保存成功后，更新 UI 显示
            GetNode<Hud>("HUD").UpdateHighScore(_highScore);
        }
    }

    // 读取最高分
    private void LoadHighScore()
    {
        if (!FileAccess.FileExists(SavePath))
        {
            _highScore = 0;
            return;
        }

        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        string jsonString = file.GetAsText();
        
        try 
        {
            var data = JsonSerializer.Deserialize<GameData>(jsonString);
            _highScore = data?.HighScore ?? 0;
            GD.Print("读取到最高分: " + _highScore);
        }
        catch (Exception e)
        {
            GD.Print("读取存档失败: " + e.Message);
            _highScore = 0;
        }
    }

    // 游戏结束时调用
    public void GameOver()
    {
        GetNode<Timer>("MobTimer").Stop(); // 停止生成怪物
        GetNode<Timer>("ScoreTimer").Stop(); // 停止计分
        // 逻辑：如果当前分数超过最高分，则更新并保存
        if (_score > _highScore)
        {
            _highScore = _score;
            SaveHighScore();
            GD.Print("打破纪录！新最高分: " + _highScore);
        }
        GetNode<Hud>("HUD").ShowGameOver(); // 显示游戏结束界面
        GetNode<AudioStreamPlayer>("Music").Stop(); // 停止背景音乐
        GetNode<AudioStreamPlayer>("DeathSound").Play(); // 播放死亡音效
    }

    // 开始新游戏
    public void NewGame()
    {
        _score = 0; // 重置分数

        var player = GetNode<Player>("Player");
        var startPosition = GetNode<Marker2D>("StartPosition");
        player.Start(startPosition.Position); // 将玩家移动到起始位置

        GetNode<Timer>("StartTimer").Start(); // 启动开始计时器
        var hud = GetNode<Hud>("HUD");
        hud.UpdateScore(_score); // 更新分数显示
        hud.ShowMessage("Get Ready!"); // 显示准备信息
        GetTree().CallGroup("mobs", Node.MethodName.QueueFree); // 清除所有怪物
        GetNode<AudioStreamPlayer>("Music").Play(); // 播放背景音乐
    }

    // 分数计时器超时时调用（每秒增加分数）
    private void OnScoreTimerTimeout()
    {
        _score++;
        GetNode<Hud>("HUD").UpdateScore(_score); // 更新分数显示
    }

    // 开始计时器超时时调用
    private void OnStartTimerTimeout()
    {
        GetNode<Timer>("MobTimer").Start(); // 开始生成怪物
        GetNode<Timer>("ScoreTimer").Start(); // 开始计分
    }

    // 怪物生成计时器超时时调用
    private void OnMobTimerTimeout()
    {
        // 创建一个新的怪物实例
        Mob mob = MobScene.Instantiate<Mob>();

        // 选择路径上的随机位置
        var mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");
        mobSpawnLocation.ProgressRatio = GD.Randf(); // 设置随机进度比例

        // 设置怪物方向垂直于路径方向
        float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;

        // 设置怪物位置到随机位置
        mob.Position = mobSpawnLocation.Position;

        // 为方向添加一些随机性
        direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
        mob.Rotation = direction;

        // 设置速度
        var velocity = new Vector2((float)GD.RandRange(150.0, 250.0), 0);
        mob.LinearVelocity = velocity.Rotated(direction);

        // 将怪物添加到主场景中
        AddChild(mob);
    }
}

// 用于 JSON 序列化的数据类
public class GameData
{
    public int HighScore { get; set; }
}