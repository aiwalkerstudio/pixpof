# 简化版流放之路技能系统设计

## 1. 系统概述

### 1.1 设计目标
- 简化流放之路的技能系统，保留核心玩法
- 实现基础的技能组合机制
- 保持系统的可扩展性
- 确保游戏平衡性

### 1.2 核心特点
- 主动技能与辅助技能的配合
- 简化版技能宝石系统
- 基础的技能成长体系
- 清晰的技能效果机制

## 2. 技能分类

### 2.1 主动技能
- 直接伤害类
  * 单体打击
  * 范围攻击
  * 持续伤害
- 控制类
  * 减速
  * 眩晕
  * 击退
- 增益类
  * 临时增强
  * 护盾
  * 治疗

### 2.2 辅助技能
- 光环效果
  * 属性提升
  * 伤害增强
  * 防御强化
- 被动效果
  * 持续回复
  * 伤害反弹
  * 特殊触发

## 3. 技能属性

### 3.1 基础属性
- 技能等级
- 基础伤害/效果
- 冷却时间
- 施法消耗
- 施法距离
- 影响范围
- 持续时间

### 3.2 特殊属性
- 暴击率
- 暴击伤害
- 效果触发几率
- 连锁次数
- 穿透次数

## 4. 技能强化系统

### 4.1 技能宝石
- 增伤宝石
  * 提升技能基础伤害
  * 增加暴击属性
- 效果宝石
  * 添加元素属性
  * 改变技能效果
- 机制宝石
  * 修改技能机制
  * 增加特殊效果

### 4.2 技能升级
- 等级提升效果
  * 伤害提升
  * 效果增强
  * 冷却缩减
- 解锁特效
  * 视觉效果
  * 打击感增强

## 5. 技能效果系统

### 5.1 基础效果
- 伤害类型
  * 物理伤害
  * 魔法伤害
- 状态效果
  * 减速
  * 眩晕
  * 流血
  * 中毒

### 5.2 效果叠加规则
- 同类效果
  * 刷新持续时间
  * 叠加层数
- 不同效果
  * 独立计算
  * 互相影响

## 6. 技能组合机制

### 6.1 技能联动
- 技能衔接
  * 连招系统
  * 技能打断
- 效果叠加
  * 多重增益
  * 伤害加成

### 6.2 技能修饰
- 属性修饰
  * 伤害类型转换
  * 效果范围改变
- 机制修饰
  * 触发方式改变
  * 作用机制变化

## 7. 平衡性设计

### 7.1 消耗机制
- 魔法消耗
- 冷却时间
- 施法前摇
- 硬直时间

### 7.2 限制机制
- 技能等级上限
- 装备要求
- 属性需求
- 技能槽位

## 8. 技术实现要点

### 8.1 核心接口
```csharp
public interface ISkill
{
string Name { get; }
int Level { get; }
float Cooldown { get; }
float ManaCost { get; }
void Cast(Vector3 position);
void LevelUp();
bool CanCast();
}
public interface IActiveSkill : ISkill
{
float Damage { get; }
float Range { get; }
float Duration { get; }
}
public interface ISupportSkill : ISkill
{
float EffectValue { get; }
bool IsAura { get; }
void ApplyEffect(Character target);
}
```

### 8.2 效果系统
```csharp
public interface ISkillEffect
{
string EffectType { get; }
float Value { get; }
float Duration { get; }
void Apply(Character target);
void Remove(Character target);
void Update(float deltaTime);
}
```

## 9. 后续扩展方向

### 9.1 近期扩展
- 技能特效系统
- 技能音效系统
- 技能动画系统
- 技能命中判定

### 9.2 远期规划
- 技能天赋树
- 技能成就系统
- 技能自定义系统
- PVP平衡调整

## 10. 系统简化说明

### 10.1 核心简化
- 取消技能石系统
  * 所有技能默认可用
  * 重点转向技能组合玩法
  * 降低获取门槛，提升可玩性

- 固定技能链接配置
  * 两个6连主技能组
  * 三个4连副技能组
  * 每组包含一个主动技能和多个辅助技能
  * 主技能组（6连）
    - 1个主动技能槽位
    - 5个辅助技能槽位
    - 适合构建核心输出技能
  * 副技能组（4连）
    - 1个主动技能槽位
    - 3个辅助技能槽位
    - 适合构建功能性技能

- 取消技能升级机制
  * 技能效果固定
  * 通过不同组合实现变化
  * 降低养成复杂度

### 10.2 平衡性调整
- 主技能组（6连）
  * 较高的基础伤害/效果
  * 较长的冷却时间
  * 较高的资源消耗
  * 适合作为核心输出技能

- 副技能组（4连）
  * 中等的基础伤害/效果
  * 较短的冷却时间
  * 较低的资源消耗
  * 适合作为辅助或过渡技能

### 10.3 技能组合示例
1. 输出型配置
   - 6连主技能1：范围攻击 + 5个增伤辅助
   - 6连主技能2：单体攻击 + 5个暴击辅助
   - 4连副技能1：位移技能 + 3个冷却缩减
   - 4连副技能2：控制技能 + 3个效果增强
   - 4连副技能3：防御技能 + 3个持续时间

2. 生存型配置
   - 6连主技能1：范围控制 + 5个效果增强
   - 6连主技能2：护盾技能 + 5个防御增强
   - 4连副技能1：治疗技能 + 3个效果提升
   - 4连副技能2：位移技能 + 3个冷却缩减
   - 4连副技能3：减速技能 + 3个范围增加

### 10.4 技术实现调整
- 移除技能石相关接口
- 简化技能属性系统
- 重点实现技能组合逻辑
- 优化技能槽位管理

### 10.5 优势
- 降低上手难度
- 保留组合深度
- 更容易平衡
- 实现成本降低
- 便于后期扩展

### 10.6 注意事项
- 确保基础技能足够有趣
- 平衡各技能组合的强度
- 提供足够的组合空间
- 避免出现单一最优解
- 保持技能特色
  
## 11 快速切换系统
- 技能预设方案（最多保存3套）
- 一键切换技能组合
- 场景自动切换预设