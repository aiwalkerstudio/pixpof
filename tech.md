# 技术架构文档

## 技术选型
- 游戏引擎: Godot 4.x
  * C#支持
  * 场景树系统
  * 内置物理引擎
  * 跨平台编译
  * 优秀的2D支持
- 开发语言: C# (.NET)
- 资源管理: Godot资源系统
- 构建工具: Godot CLI

## 项目结构
/
/.godot # Godot配置文件
/addons # 插件目录
/src # C#源代码
/Core # 核心游戏逻辑
/Combat # 战斗系统
/Skills # 技能系统
/Items # 物品系统
/Stats # 属性系统
/Scenes # 场景相关代码
/Main # 主场景逻辑
/Battle # 战斗场景逻辑
/UI # UI相关代码
/Components # UI组件
/HUD # 战斗界面
/Utils # 工具类
/Extensions # 扩展方法
/Helpers # 辅助函数
/assets # 游戏资源
  /i18n # 国际化资源
    /translations # 翻译文件
      /source # 源文本文件
        ui.csv # UI文本
        items.csv # 物品描述
        skills.csv # 技能描述
        dialogs.csv # 对话文本
      /compiled # 编译后的翻译文件
        zh_CN.translation # 简体中文
        en_US.translation # 英语(美国)
        ja_JP.translation # 日语
    /localized_assets # 本地化资源
      /audio # 配音文件
        /zh_CN
        /en_US
        /ja_JP
      /images # 本地化图片
        /zh_CN
        /en_US
        /ja_JP
    /tools # 本地化工具
      translation_sync.sh # 翻译同步工具
      csv_validator.py # CSV格式检查
      translation_compiler.py # 翻译文件编译
/scenes # 场景文件(.tscn)
/sprites # 精灵图
/audio # 音频文件
/themes # UI主题
/resources # Godot资源文件(.tres)
/items # 物品配置
/skills # 技能配置
/platform # 平台特定代码
/mobile # 移动端适配
/desktop # 桌面端适配
/build # 构建输出目录
/config # 配置文件目录
  pixpof.csproj # C#项目配置
  project.godot # Godot项目配置
  .gitignore # Git忽略配置
  .gitattributes # Git属性配置

## 核心系统

### 场景系统
- 主场景管理
- 场景切换
- 场景预加载
- 场景树组织

### 节点组织
- 实体节点结构
- 组件化设计
- 信号连接
- 节点缓存

### 战斗系统
- 状态机管理
- 碰撞检测
- 技能释放
- 伤害计算

### UI系统
- Control节点
- 主题系统
- 自适应布局
- 输入处理

## 技术要点

### Godot特性使用
- 信号系统(Signal)
- 资源预加载
- 场景实例化
- 物理系统
- 动画系统

### C#特性
- 异步编程
- 依赖注入
- 事件系统
- 扩展方法

### 跨平台适配
- 输入系统适配
  * 触摸输入
  * 键鼠输入
- UI自适应
- 性能优化
- 平台特定API

### 资源管理
- 资源预加载
- 动态加载
- 资源释放
- 场景缓存

## 开发规范
- C#代码规范
- 节点命名规范
- 场景组织规范
- 资源命名规范

## 调试与优化
- 内置调试工具
- 性能分析
- 内存监控
- 远程调试

## 构建发布
- 导出配置
- 平台打包
- 资源压缩
- 更新部署

## 扩展性设计
- MOD系统接口
- 配置数据序列化
- 插件系统
- 存档系统

## MOD系统设计
参考 GodotModding/godot-mod-loader 的架构设计, 使用C#重新实现核心功能

### 插件架构
/addons
/mod_core # MOD核心框架
/scripts # 核心脚本
ModManager.cs # MOD管理器
ModLoader.cs # MOD加载器
ModConfig.cs # MOD配置
/resources # 框架资源
plugin.cfg # 插件配置
/mod_sdk # MOD开发SDK
/templates # MOD模板
/scripts # 辅助脚本
/docs # 开发文档

### MOD接口设计
csharp
public interface IMod
{
string Id { get; }
string Name { get; }
string Version { get; }
string[] Dependencies { get; }
void OnLoad(); // MOD加载时调用
void OnEnable(); // MOD启用时调用
void OnDisable(); // MOD禁用时调用
void OnUnload(); // MOD卸载时调用
}

### 核心功能

#### MOD管理器
- MOD生命周期管理
  * 加载顺序控制
  * 依赖检查
  * 版本兼容性检查
- MOD配置管理
  * 配置文件读写
  * 配置UI生成
- MOD资源管理
  * 资源隔离
  * 资源热重载
  * 资源冲突处理

#### 扩展点系统
- 游戏内容扩展
  * 新物品
  * 新技能
  * 新地图
  * 新怪物
- 系统功能扩展
  * UI扩展
  * 玩法扩展
  * 规则修改
- 数据扩展
  * 配置注入
  * 数据覆盖

#### 开发工具
- MOD模板生成器
- 调试工具
- 打包工具
- 文档生成器

### MOD开发流程
1. 创建MOD项目
   - 使用模板生成器
   - 配置MOD信息
   - 设置依赖关系

2. 开发MOD内容
   - 实现IMod接口
   - 添加自定义内容
   - 使用扩展点API

3. 测试与调试
   - 本地测试
   - 热重载调试
   - 兼容性测试

4. 打包与发布
   - 资源打包
   - 生成配置
   - 发布验证

### 安全性考虑
- MOD签名验证
- 权限控制
- 资源访问限制
- 性能监控
- 错误隔离

### 最佳实践
- MOD设计规范
- 性能优化建议
- 兼容性处理
- 版本控制
- 发布流程

### 示例MOD结构
/mods
/example_mod
/scripts # MOD脚本
/resources # MOD资源
/configs # 配置文件
mod.cfg # MOD信息
manifest.json # 资源清单

### 技术实现要点
- 使用反射加载MOD程序集
- 实现资源虚拟文件系统
- 提供事件系统支持MOD间通信
- 使用配置系统管理MOD设置
- 实现热重载支持
- 提供调试和性能分析工具