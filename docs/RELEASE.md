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
6. 等待 [release.yml](../.github/workflows/release.yml) 完成：
   - GitHub **Releases** 下载 `RecipaediaEX-{Version}.scmod`
   - 模组站资源帖（postId **1739**）自动发布新版本（需已配置 `MOD_SITE_TOKEN`，见下）
7. （手动）在 IE2 主仓更新 `SCIENEW/modinfo.json` 中 `com.recipaediaex` 依赖版本。

## CI 与产物命名

| 场景 | 产物 |
|------|------|
| 本机 `dotnet build` | `ModsFolder/RecipaediaEX.scmod`（短名覆盖） |
| 日常 CI（`build.yml`） | `RecipaediaEX-ci.{sha7}.scmod` |
| GitHub Release | `RecipaediaEX-{Version}.scmod` |
| 模组站（post 1739） | 同上 `.scmod` + `docs/CHANGELOG.md` 对应版本说明 |

Preview 版本 Release 会自动标记为 **Pre-release**。

## 模组站自动发布

Release CI 在 GitHub Release 成功后调用 `tools/publish-mod-site.ps1`，向 [RecipaediaEX 资源帖](https://test.suancaixianyu.cn/#/postDetails/1739) 登记新版本。

### GitHub Secrets

| Secret | 说明 |
|--------|------|
| `MOD_SITE_TOKEN` | 模组站 Bearer Token（须具备 post 1739 的上传与发版权限） |

填写 **JWT 本体**，不要带 `Bearer ` 前缀（脚本会自动加）。可在浏览器登录模组站后，DevTools → Application → Local Storage 中查找 token，或请网站开发提供 CI 专用 Token。

在 RecipaediaEX 仓库 **Settings → Secrets and variables → Actions** 中添加。

### 配置

`tools/mod-site.config.json`（可提交，无密钥）：

| 字段 | 默认 | 说明 |
|------|------|------|
| `ApiBaseUrl` | `https://m.suancaixianyu.cn/api` | 模组站 API |
| `PostId` | `1739` | RecipaediaEX 资源帖 ID |
| `ScmodTypeId` | `5` | `.scmod` 文件类型（必填，否则发版失败） |
| `GameVersionIds` | `[27]` | 插件 API 版本标签（API1.9.x） |

### 本地验证

```powershell
$ver = (Get-Content modinfo.json | ConvertFrom-Json).Version
$out = "bin/Release/net10.0"
./tools/pack.ps1 -BuildOutputDir $out -ArtifactDir $env:TEMP -Version $ver

$env:MOD_SITE_TOKEN = "<你的 Bearer Token>"
./tools/publish-mod-site.ps1 `
  -ScmodPath "$env:TEMP/RecipaediaEX-$ver.scmod" `
  -Version $ver
```

版本说明优先读取 `docs/CHANGELOG.md` 中 `## [{Version}]` 段落；若无则回退到 `release_notes.md`（GitHub Release body，由 `git log` 生成）。

## 本地验证（打包）

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
