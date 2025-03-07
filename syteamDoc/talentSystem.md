# 简化版流放之路天赋系统设计

## 1. 系统概述

### 1.1 设计目标
- 简化天赋树结构
- 保留构建多样性
- 确保选择的意义
- 降低入门门槛

### 1.2 核心特点
- 三大天赋方向
- 清晰的成长路径
- 关键天赋节点
- 灵活的重置机制

## 2. 天赋树结构

### 2.1 主要分支
- 力量路线（红色）
  * 偏向物理伤害
  * 生命值提升
  * 护甲增强
  
- 敏捷路线（绿色）
  * 攻击速度
  * 暴击属性
  * 闪避能力
  
- 智力路线（蓝色）
  * 魔法伤害
  * 魔法抗性
  * 资源回复

### 2.2 节点类型
- 小型节点
  * 提供基础属性提升
  * 每个+3%~5%提升
  
- 中型节点
  * 提供特殊效果
  * 改变某些机制
  
- 大型节点（关键天赋）
  * 提供独特效果
  * 改变构建方向

## 3. 天赋点系统

### 3.1 获取方式
- 升级获得
  * 每级1点天赋点
  * 最高60级
  * 总计60点天赋点

- 任务奖励
  * 主线任务额外获得
  * 共计20点天赋点
  * 总计可获得80点

### 3.2 重置机制
- 单点重置
  * 消耗金币
  * 费用随等级增加
  
- 全部重置
  * 通过特殊道具
  * 完全重置所有点数

## 4. 关键天赋设计

### 4.1 力量系
- 战争意志
  * 生命值越低，伤害越高
  * 最高可达50%额外伤害
  
- 不屈之躯
  * 受到致命伤害时
  * 3秒内免疫伤害
  * 60秒冷却
  
- 震地之力
  * 近战攻击附带范围伤害
  * 范围基于力量值

### 4.2 敏捷系
- 疾风步伐
  * 攻击速度提升30%
  * 移动速度提升20%
  
- 暗影打击
  * 暴击时隐身1秒
  * 下次攻击伤害翻倍
  
- 连锁反应
  * 暴击时获得额外攻速
  * 可叠加3层

### 4.3 智力系
- 元素掌控
  * 魔法伤害转化为随机元素
  * 每种元素伤害+30%
  
- 法力护盾
  * 伤害优先扣除魔法值
  * 魔法值消耗减半
  
- 奥术回响
  * 施法触发额外法术
  * 威力为原技能的30%

## 5. 天赋路径示例

### 5.1 物理战士
- 主要投资力量路线
- 关键天赋：
  * 战争意志
  * 不屈之躯
- 副要投资：
  * 敏捷树攻速节点
  * 生命值节点

### 5.2 暴击刺客
- 主要投资敏捷路线
- 关键天赋：
  * 疾风步伐
  * 暗影打击
- 副要投资：
  * 力量树生命节点
  * 暴击伤害节点

### 5.3 元素法师
- 主要投资智力路线
- 关键天赋：
  * 元素掌控
  * 奥术回响
- 副要投资：
  * 敏捷树施法速度
  * 魔法回复节点

## 6. 技术实现

### 6.1 天赋节点接口
```csharp
public interface ITalentNode
{
    string Name { get; }
    TalentType Type { get; }
    List<ITalentNode> ConnectedNodes { get; }
    bool IsActivated { get; }
    void Activate(Character character);
    void Deactivate(Character character);
    bool CanActivate(Character character);
}
```

### 6.2 天赋树管理
```csharp
public class TalentTree
{
    public int AvailablePoints { get; }
    public List<ITalentNode> ActiveNodes { get; }
    public bool CanActivateNode(ITalentNode node);
    public void ActivateNode(ITalentNode node);
    public void DeactivateNode(ITalentNode node);
    public void ResetAll();
}
```

## 7. 平衡性设计

### 7.1 点数分配
- 小型节点：1点
- 中型节点：2点
- 大型节点：3-5点
- 确保80点可以构建完整体系

### 7.2 属性平衡
- 单项属性最大提升100%
- 关键天赋效果独特但不破坏平衡
- 确保跨系天赋投资的价值

## 8. 界面设计

### 8.1 视觉呈现
- 三大方向清晰分区
- 已激活路径高亮显示
- 可激活节点特殊标记
- 未达条件节点灰显

### 8.2 操作设计
- 点击节点激活/查看
- 右键节点取消激活
- 显示连接路径预览
- 支持路径规划功能

## 9. 后续扩展

### 9.1 近期规划
- 增加特殊天赋效果
- 添加条件解锁节点
- 优化重置机制
- 添加天赋预设

### 9.2 远期规划
- 季节性特殊天赋
- 天赋树外观定制
- 成就天赋系统
- PVP平衡调整

## 10. 注意事项
- 保持选择的趣味性
- 避免出现唯一解
- 确保新手友好
- 预留扩展空间
- 注意与其他系统的配合
