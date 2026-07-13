# 发版流程入口

RecipaediaEX 的具体发版步骤继续维护在 [RELEASE.md](../RELEASE.md)。

## 范围

本入口用于说明发版 SOP 的职责边界：

- 修改 `modinfo.json` 版本。
- 更新 `docs/CHANGELOG.md`。
- 提交 release commit。
- 打与 `modinfo.Version` 对齐的 `v*` tag。
- 推送分支和 tag。
- 等待 GitHub Release 与模组站发布。
- 通知依赖 RecipaediaEX 的内容模组对齐依赖版本。

## 注意

- tag 必须比 `modinfo.Version` 只多一个 `v` 前缀。
- Preview 版本会自动标记为 prerelease。
- 模组站自动发布依赖 `MOD_SITE_TOKEN`。
- Agent 不应仅因为策划案建议发包就自行 tag 或 release；发版需要明确授权。

## 旧入口

为避免链接断裂，实际 SOP 暂不移动。请继续编辑 [../RELEASE.md](../RELEASE.md)。
