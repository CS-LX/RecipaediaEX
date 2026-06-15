# RecipaediaEX 发版指南

版本号以 **`modinfo.json` → `Version`** 为唯一真相源；构建前 `tools/sync-version.ps1` 会同步到 `RecipaediaEX.csproj`。

## 版本格式

| 类型 | modinfo `Version` | git tag |
|------|-------------------|---------|
| Preview | `X.X.X.X-previewN` | `vX.X.X.X-previewN` |
| 正式版 | `X.X.X.X` | `vX.X.X.X` |

## 发版步骤

1. 修改 `modinfo.json` 的 `Version`（例如 `2.0.0.0-preview6`）。
2. （推荐）在 `docs/CHANGELOG.md` 顶部添加对应版本块。
3. 提交：`release: RecipaediaEX 2.0.0.0-preview6`
4. 打标签（**必须与 modinfo 完全一致，仅多 `v` 前缀**）：
   ```bash
   git tag v2.0.0.0-preview6
   ```
5. 推送分支与标签：
   ```bash
   git push origin main
   git push origin v2.0.0.0-preview6
   ```
6. 等待 [release.yml](../.github/workflows/release.yml) 完成；在 GitHub **Releases** 下载 `RecipaediaEX-{Version}.scmod`。
7. （手动）在 IE2 主仓更新 `SCIENEW/modinfo.json` 中 `com.recipaediaex` 依赖版本。

## CI 与产物命名

| 场景 | 产物 |
|------|------|
| 本机 `dotnet build` | `ModsFolder/RecipaediaEX.scmod`（短名覆盖） |
| 日常 CI（`build.yml`） | `RecipaediaEX-ci.{sha7}.scmod` |
| GitHub Release | `RecipaediaEX-{Version}.scmod` |

Preview 版本 Release 会自动标记为 **Pre-release**。

Release 说明第一版由 `git log` 生成；流程稳定后可改为从 `CHANGELOG.md` 提取。

## 本地验证

```powershell
./tools/sync-version.ps1
dotnet build RecipaediaEX.csproj -c Release -p:RecipaediaEXSkipPack=true

# 模拟 Release 打包
$out = "bin/Release/net10.0"
./tools/pack.ps1 -BuildOutputDir $out -ArtifactDir $env:TEMP -Version (Get-Content modinfo.json | ConvertFrom-Json).Version
```

## 参考

- [打包 → 发布 CI 策划](打包发布CI策划.md)
- [更新日志](CHANGELOG.md)
