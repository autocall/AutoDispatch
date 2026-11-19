#!/usr/bin/env bash
# Документы dispatch: E:\Files V1, залитый FileZilla в /home/safer/temp, -> /home/autodispatch/files (volume /files контейнера autodispatch.web).
# AI/wiki/data-migration.md «Файлы». Запускать на хосте под root.
#
# Копируются все каталоги с именем-Guid (остальное в источнике не трогается); имя каталога — в нижнем регистре: сервер строит путь через
# Guid.ToString(), Linux различает регистр, а в V1 на NTFS регистр мог быть любым. rsync: повторный запуск докопирует только новые
# и изменённые файлы — так же делается досинк (дельта FileZilla в тот же источник). Владелец — root, права 755/644.
# --check dirs.txt (блок 5 DataBase/migrate from autocall dispatch.sql) — после копирования показать, сколько нужных App каталогов
# нет в /home/autodispatch/files (у части сущностей файлов нет — сверять с E:\Files, а не считать ошибкой).
#
# С машины разработки (скрипт и dirs.txt в рабочей копии с CRLF — sed/tr убирают \r):
#   sed 's/\r$//' Scripts/copy-dispatch-files.sh | ssh root@67.220.74.190 'cat > /root/copy-dispatch-files.sh'
#   scp DataBase/dirs.txt root@67.220.74.190:/root/dirs.txt
#   ssh root@67.220.74.190 'bash /root/copy-dispatch-files.sh --check /root/dirs.txt'
# Параметры: --src DIR (по умолчанию /home/safer/temp; если там есть подкаталог Files — берётся он), --dst DIR, --check FILE.
set -euo pipefail
export LC_ALL=C

src=/home/safer/temp; dst=/home/autodispatch/files; check=""
while [ $# -gt 0 ]; do
    case "$1" in
        --src) src=$2; shift ;;
        --dst) dst=$2; shift ;;
        --check) check=$2; shift ;;
        *) echo "usage: $0 [--src DIR] [--dst DIR] [--check dirs.txt]"; exit 1 ;;
    esac
    shift
done
[ -d "$src/Files" ] && src="$src/Files"    # FileZilla перетащил саму папку E:\Files
[ -d "$src" ] || { echo "source not found: $src"; exit 1; }
mkdir -p "$dst"

work=$(mktemp -d); trap 'rm -rf "$work"' EXIT
guid='^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$'

# каталоги источника: "имя в нижнем регистре<TAB>имя как есть"
find "$src" -mindepth 1 -maxdepth 1 -type d -printf '%f\n' | grep -E "$guid" | awk '{ print tolower($0) "\t" $0 }' | sort > "$work/src" || true
awk -F'\t' '$1 == $2 { print $1 }' "$work/src" > "$work/same"
awk -F'\t' '$1 != $2' "$work/src" > "$work/upper"
echo "source: $src — Guid dirs: $(wc -l < "$work/src") (lower case: $(wc -l < "$work/same"), with upper case: $(wc -l < "$work/upper"))"

opts=(-a --no-owner --no-group --chmod=D755,F644)
# каталоги уже в нижнем регистре — одним rsync по списку (быстро), остальные — по одному с переименованием
if [ -s "$work/same" ]; then
    rsync "${opts[@]}" -r --info=progress2 --files-from="$work/same" "$src/" "$dst/"
fi
while IFS=$'\t' read -r lower actual; do
    rsync "${opts[@]}" "$src/$actual/" "$dst/$lower/"
done < "$work/upper"
echo "done: $(find "$dst" -mindepth 1 -maxdepth 1 -type d | wc -l) dirs in $dst"

if [ -n "$check" ]; then
    # dirs.txt из SSMS: BOM, заголовок, итоги и CR отбрасываются
    tr -d '\r' < "$check" | tr 'A-F' 'a-f' | grep -oE '[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}' | sort -u > "$work/want"
    find "$dst" -mindepth 1 -maxdepth 1 -type d -printf '%f\n' | sort > "$work/have"
    comm -23 "$work/want" "$work/have" > "$work/missing"
    echo "check: needed $(wc -l < "$work/want"), present $(comm -12 "$work/want" "$work/have" | wc -l), missing $(wc -l < "$work/missing")"
    cp "$work/missing" "$dst/../files-missing.txt" && echo "missing list: $(cd "$dst/.." && pwd)/files-missing.txt"
fi
