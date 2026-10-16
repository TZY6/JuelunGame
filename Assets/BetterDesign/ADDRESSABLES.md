# 场景 Addressables 加载
Unity 2022.3.29f1，Addressables 1.21.21，Scriptable Build Pipeline 1.21.23。

## 运行
1. 拉取 codex/better-design-sanctuary，等待 Package Manager 下载依赖并编译。
2. Tools → BetterDesign → Addressables → Open Launcher，打开 Scenes/BetterDesignLauncher.unity。
3. Play，点击“进入游戏 / 重试”。启动场景保留，通过 AssetReference 以 Additive 加载 BetterDesignDemo，并将其设为 Active Scene。
4. 游戏中点击顶部返回按钮或 F10，等待卸载后回到启动页，可以再次进入。返回不会自动存档，需在游戏中 F5 保存；重新进入是新会话，需要 F9 才恢复检查点。
5. 直接打开 BetterDesignDemo 仍可独立运行，只有启动场景持有加载流程，没有往玩法中添加全局单例。

启动页、加载状态、进度条、按钮、相机、EventSystem 和字体均已序列化在场景中，运行时不创建 UI。六个 TMP 标签使用指定的 7000msyhbdallicon SDF 主字体，沿用既有缺字回退。

## 打包与验证真实内容包
- Tools → BetterDesign → Addressables → Build Local Content：构建本地 catalog 与 bundles。
- Addressables Groups 的 Play Mode Script 默认 Use Asset Database，方便开发；这仍使用 Addressables API，但没有验证 bundle 分发。构建内容后可切换 Use Existing Build 检查真实本地内容包。
- Tools → BetterDesign → Addressables → Build Windows Demo：选择输出位置，使用 Launcher 作为唯一 Player 起始场景，自动构建 Addressables 内容。它不替换原项目 Ready/RPG1/RPG2 的 Build Settings 场景列表。
- 更改场景或被引用资源后需要重新构建本地内容。构建输出不提交 Git；本地包随 Player 构建复制到 StreamingAssets。
- Addressables、组、Schema、DataBuilder 与 DefaultObject 位于 BetterDesign/Addressables。ProjectSettings/EditorBuildSettings.asset 仅注册这份配置引用，原有场景列表保留。配置菜单会拒绝覆盖另一套现有 Addressables Settings。
- 本次是本地包加载。未配置 CDN、远程 catalog、热更新或断点下载，进度条显示 Addressables 操作进度，不是网络下载字节比例。

## 依赖与所有权
```text
BetterDesignLauncher 场景
 └─ SceneLauncher：预置 UI、按钮、进度、启动相机和 EventSystem
      └─ AddressableSceneFlow：加载/卸载状态，唯一持有场景 handle
           └─ Addressables.LoadSceneAsync(Additive)
                └─ BetterDesignDemo → DemoScene → DemoWorld
           └─ Addressables.UnloadSceneAsync
                └─ DemoScene.OnDestroy → 释放世界、订阅与背包控制器
```
- SceneLauncher 不参与伤害、背包或存档业务。DemoScene 的已有生命周期继续管理玩法系统。
- 同时最多一个加载/卸载操作；忙碌、已加载或已释放时拒绝重复命令。加载失败释放失败 handle，可以重试。
- 成功加载后保留 handle；返回时由 UnloadSceneAsync 释放场景引用，再释放卸载操作 handle，避免重复 Release。
- 加载操作不能被 CancellationToken 真正中止；拥有者销毁时标记 disposed，完成后立即卸载。DisposeAsync 允许测试等待清理，回调不再写入已销毁的 UI。
- 启动相机和 EventSystem 在切换时停用，游戏内沿用原场景相机与 EventSystem；返回时恢复启动页，避免重复输入模块。
- await 使用 Completed 回调桥接，没有 WaitForCompletion 同步阻塞，也不依赖 AsyncOperationHandle.Task。API 需从 Unity 主线程调用。
- 没有 DontDestroyOnLoad、静态 Manager 或自动生成的玩法对象。启动场景持续存在，场景作用域清晰。

## 自动回归
AddressableSceneTests 验证启动页绑定、字体、地址/组配置，以及 Play Mode 中重复进入/卸载、Active Scene、唯一 EventSystem、无效地址重试、加载中 Dispose 清理。原有战斗、朝向、背包拖拽、存档测试继续执行。测试应从 EditMode 运行；验证 Use Existing Build 前先构建内容。
