#!/usr/bin/env bash
# 一次性把 Assets 下中文目录/文件名迁移为 ASCII（GUID 不变）。
# 用法：
#   Tools/ci/migrate-ascii.sh dry     # 只输出报表，不改动（默认）
#   Tools/ci/migrate-ascii.sh apply   # 执行迁移
# 前置条件：先退出 Unity Editor（同一项目不能同时被两个 Unity 实例打开）。
set -euo pipefail

UNITY="/Applications/Unity/Hub/Editor/2022.3.62f2/Unity.app/Contents/MacOS/Unity"
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
MODE="${1:-dry}"

if [[ ! -x "$UNITY" ]]; then
  echo "找不到 Unity: $UNITY" >&2
  exit 1
fi

if [[ -f "$PROJECT_ROOT/Temp/UnityLockfile" ]] && pgrep -f "Unity .* -projectpath $PROJECT_ROOT" >/dev/null 2>&1; then
  echo "检测到该项目的 Unity Editor 仍在运行，请先退出 Unity 再执行。" >&2
  exit 2
fi

case "$MODE" in
  dry)   METHOD="Dilao.Tools.AsciiAssetMigration.DryRunBatch" ;;
  apply) METHOD="Dilao.Tools.AsciiAssetMigration.ApplyBatch" ;;
  *) echo "用法: $0 [dry|apply]" >&2; exit 1 ;;
esac

echo "== Unity batch ASCII migration ($MODE) =="
"$UNITY" -batchmode -quit -nographics \
  -projectPath "$PROJECT_ROOT" \
  -executeMethod "$METHOD" \
  -logFile "$PROJECT_ROOT/Logs/ascii-migration-batch.log"
echo "退出码: $? ；报表: $PROJECT_ROOT/Logs/ascii-migration-report.txt"
