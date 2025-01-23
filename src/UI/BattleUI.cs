using Godot;
using Game;  // 添加这行来引用 Game 命名空间

public partial class BattleUI : Control
{
    private Game.Player _player;  // 使用完整的命名空间路径

    public void Initialize(Game.Player player)  // 修改参数类型
    {
        _player = player;
        // ... 其他代码保持不变 ...
    }

    // ... 其他代码保持不变 ...
} 