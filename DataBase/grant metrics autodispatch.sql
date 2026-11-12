-- Метрика DB CPU (Deploy/metric_cpu.sh шлёт её в Api/Metrics/Add); чтение sys.dm_os_ring_buffers требует VIEW SERVER STATE.
-- Выполнять на db.myautocall.net, как для autocall_v3.
use master;
grant view server state to autodispatch;
