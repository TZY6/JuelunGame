# 当前验证结果
朝向与拖拽修复基线：9b87625313e95ff6e4ed948e2caec9b0f6374bd9。Unity 2022.3.29f1 / TMP 3.0.6。

- BetterDesign.Core、Runtime、Editor、Tests 四个程序集 Roslyn 编译通过。
- 独立临时 Unity 项目中执行 BetterDesign.Tests：**64 / 64 通过，0 失败**。包含进入 Play Mode、Awake、UI Disable/Enable、存档 JSON/文件恢复及 12 项对战验收测试。
- 新增朝向、拖拽与存档格子回归；Play Mode 检查动画更新后朝向。UGUI 事件测试覆盖移动、关闭、数据变化、拖出取消；模型测试覆盖合并上限、交换及失败原子性。
- 近战、弓箭、法术分别通过整波清场模拟，敌人 AI 正常运行；远程测试允许合法消耗初始治疗补给。
- 额外进入 Play Mode，以真实 EventSystem.RaycastAll 命中第 6 格并通过 ExecuteHierarchy 投放第 1 格补给，验证 0 → 5 的模型移动及拖拽图标关闭。该探针在临时项目运行，不是操作系统鼠标回放。
- 场景有 629 个预置 GameObject（新增 1 个无射线拖拽图标），12 个格子各绑定 InventorySlotDrag。
- 场景验证 17 个角色、48 FX、64 掉落、12 格背包、49 TMP 标签、22 按钮，以及血条/攻击方向/瞄准/弓手动画/字体回退/相机/EventSystem 引用。
- 字体验证覆盖实际 TMP 字符输出，而不仅是资源引用。原 7000 主字体及目录外的字体资源、TMP 配置没有修改。
- 有图形设备的 Unity 相机渲染检查了真实 HUD、字形、血条和战区；架构面板默认隐藏。
- 交付场景使用 Git blob 校验，与独立验证项目场景一致。代码通过 GitHub API 提交；用户工作副本未编辑。

测试输入通过 IPlayerInput/IAimedPlayerInput 驱动真实场景对象与玩法模块，不宣称完成操作系统层键鼠回放或人工试玩。没有提供未测量的 FPS/GC 提升数字；原 RPG1 的目录外缺陷与整体 Player 构建未在本轮改动。

具体复现、修复和本地玩家验收路径见 [BATTLE_ACCEPTANCE.md](BATTLE_ACCEPTANCE.md)。架构审查和迁移边界见 [ARCHITECTURE.md](ARCHITECTURE.md)。

## Addressables 场景加载
基线 14d542122a408d654f53bb71beef8469bcfee839。经用户授权额外更新 Packages/manifest.json、Packages/packages-lock.json 与 ProjectSettings/EditorBuildSettings.asset 的配置引用；原 Build Settings 场景列表保留。
- Unity 2022.3.29f1 安装 Addressables 1.21.21 / Scriptable Build Pipeline 1.21.23，Runtime、Editor、Tests 编译通过，Core 不增加 Addressables 依赖。
- 完整 BetterDesign.Tests 回归 **67/67 通过**，原有 64 项战斗、朝向、拖拽、模型与存档测试继续通过。
- 本地 catalog 与 AssetBundles 构建成功，输出为 Library/com.unity.addressables/aa/Windows/settings.json。
- 默认 Use Asset Database 模式完成 AddressableSceneTests；切换 Use Existing Build 后再次执行 **3/3 通过**，真实 bundle 加载/卸载两轮、Active Scene、唯一 EventSystem、无效 key 失败重试、加载中 Dispose 清理均验证。
- 启动场景实际 Unity 相机渲染检查了文字、按钮和进度条；六个 TMP 引用指定主字体，缺字回退工作正常。
- 交付 Launcher 场景 blob 与验证文件一致：afc7333b06d1cde3dcf577a3feff4f2b75660e19。
- 所有验证在独立临时项目执行，没有编辑用户工作副本。启动页作者工具和临时测试探针不交付；场景和组配置已直接序列化。
本轮未执行完整原 RPG1 的 Player 构建、远程 CDN 或热更新验证。Windows Demo 菜单提供明确的打包入口，原项目目录外的脚本构建问题仍需独立处理。操作与生命周期说明见 ADDRESSABLES.md。
