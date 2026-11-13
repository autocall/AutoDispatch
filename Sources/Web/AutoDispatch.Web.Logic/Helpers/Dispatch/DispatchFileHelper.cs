using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Документы dispatch: каталог {FilesPath}/{EntityId} груза, карриера, трейлера, дефекта (V1 DispatchService.FileGetFullPath) </summary>
public static class DispatchFileHelper {

    public static string FileGetFullPath(Guid entityId, string name = null) {
        return name != null
            ? Path.Combine(AppSettings.FilesPath, entityId.ToString(), name)
            : Path.Combine(AppSettings.FilesPath, entityId.ToString());
    }

    /// <summary>
    ///     Страховка карриера: старые файлы в подкаталоге Insurance, новые — в каталоге карриера </summary>
    public static string InsuranceGetFullPath(Guid entityId, string name = null) {
        if (name != null) {
            var path1 = Path.Combine(AppSettings.FilesPath, entityId.ToString(), "Insurance", name);
            if (File.Exists(path1)) {
                return path1;
            }
            return Path.Combine(AppSettings.FilesPath, entityId.ToString(), name);
        }
        return Path.Combine(AppSettings.FilesPath, entityId.ToString());
    }

    /// <summary>
    ///     name (1).ext, name (2).ext … пока имя занято </summary>
    public static string UniqueFileFullPath(string fullPath) {
        var originFileName = Path.GetFileNameWithoutExtension(fullPath);
        var nextFileName = originFileName;
        for (int i = 1; ; i++) {
            fullPath = Path.Combine(Path.GetDirectoryName(fullPath), nextFileName + Path.GetExtension(fullPath));
            if (File.Exists(fullPath) == false) {
                return fullPath;
            }
            nextFileName = $"{originFileName} ({i})";
        }
    }

    /// <summary>
    ///     Документ/шаблон setup packet по версии: name_02.ext, если есть (V1 GetFileByVersion) </summary>
    public static string GetFileByVersion(string fullpath, int version) {
        if (version > 1) {
            var path = Path.GetDirectoryName(fullpath);
            var fileName = Path.GetFileNameWithoutExtension(fullpath);
            var extension = Path.GetExtension(fullpath);
            var newFullPath = Path.Combine(path, $"{fileName}_{version:00}" + extension);
            if (File.Exists(newFullPath)) {
                return newFullPath;
            }
        }
        return fullpath;
    }
}
