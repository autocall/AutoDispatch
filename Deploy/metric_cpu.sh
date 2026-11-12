#!/bin/bash
# DB CPU -> AutoDispatch.Web Api/Metrics/Add (type 2 = DbCpu), runs on the DB server (same as AutoCall V3 / Safer metric_cpu.sh).
# cron, 3 times a minute: * * * * * for i in 0 1 2; do /opt/autodispatch/metric_cpu.sh; sleep 20; done
# TOKEN = Secrets:Metric:Token of the web server (appsettings.Production.json on 67.220.74.190).

TOKEN="<Secrets:Metric:Token>"
URL="https://dispatch.myautocall.net/Api/Metrics/Add"

cpu_usage() {
    read cpu user nice system idle iowait irq softirq steal guest < /proc/stat
    total1=$((user + nice + system + idle + iowait + irq + softirq + steal))
    idle1=$idle

    sleep 1

    read cpu user2 nice2 system2 idle2 iowait2 irq2 softirq2 steal2 guest2 < /proc/stat
    total2=$((user2 + nice2 + system2 + idle2 + iowait2 + irq2 + softirq2 + steal2))
    idle2=$idle2

    total_diff=$((total2 - total1))
    idle_diff=$((idle2 - idle1))
    usage=$(( (100 * (total_diff - idle_diff)) / total_diff ))

    echo "$usage"
}

value=$(cpu_usage)

curl -s -X POST "$URL" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer $TOKEN" \
     -d "{\"type\":2,\"value\":$value}"
