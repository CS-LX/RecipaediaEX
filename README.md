# RecipaediaEX

RecipaediaEX 是 Survivalcraft 的配方与图鉴扩展框架。它把配方定义、配方加载、配方匹配、图鉴展示和合成助手接入拆开，让内容模组可以在不改 REX 核心的前提下扩展自己的配方体系。

## 你想做什么？

| 读者 | 入口 | 内容 |
|------|------|------|
| 玩家 | [用户文档](docs/user/README.md) | 图鉴搜索、合成助手、`+` 自动摆放 |
| 依赖模组作者 | [API 文档](docs/api/README.md) | 配方、加载器、图鉴 UI、事件 / 拦截、合成助手接入 |
| REX 贡献者 | [内部开发文档](docs/internal/README.md) | 架构、策划、路线图、设计决策 |
| 维护者 | [发布与运维](docs/release/README.md) | 更新日志、发版 SOP、CI / 打包 |

完整文档索引见 [docs/README.md](docs/README.md)。

## 当前版本

| 项 | 值 |
|----|----|
| 模组版本 | `2.0.0.0`（以 `modinfo.json` 为准） |
| 目标框架 | `net10.0` |
| 游戏 API | `SurvivalcraftAPI.Engine`、`SurvivalcraftAPI.EntitySystem`、`SurvivalcraftAPI.Survivalcraft` |
| 其它依赖 | `ZLinq` |

## 核心能力

- 自定义任意 `IRecipe` 配方类型。
- 通过 `IRecipesLoader` / `IDynamicRecipeLoader` 接入静态或动态配方来源。
- 使用 `RecipaediaEXManager` 做统一配方匹配。
- 通过 `IRecipaediaItem`、分类 Provider 和 `RecipeDescriptor` 接入图鉴展示。
- 通过 EventBus / InterceptBus 订阅或拦截框架行为。
- 通过 Overlay Host / Placement Target 为机器接入同屏合成助手和 `+` 自动摆放。

## 快速入口

- 从零接入配方：读 [配方与加载器](docs/api/recipes-and-loaders.md)。
- 接入图鉴条目和配方页：读 [图鉴 UI 扩展](docs/api/recipaedia-ui.md)。
- 订阅事件或拦截行为：读 [事件与拦截总线](docs/api/events-and-intercepts.md)。
- 给机器接入合成助手：读 [合成助手接入](docs/api/crafting-overlay-integration.md)。
- 查看完整旧版 API 汇总：读 [API 使用文档](docs/API使用文档.md)。

## 构建

本机调试和发版流程见 [发布与运维文档](docs/release/README.md)。最短路径：

1. 复制 `tools/pack.config.example.json` 为 `tools/pack.config.json`，填写 `ModsFolder`。
2. 执行 `dotnet build RecipaediaEX.csproj -c Release`。
3. 构建脚本会生成并部署 `RecipaediaEX.scmod`。

版本号以 `modinfo.json` 的 `Version` 为唯一真相源。