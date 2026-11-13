using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    #region [ Defects ]

    private async Task<List<Defect>> LoadDefectEntitiesAsync(List<Guid> companyIds, List<Guid> ids, bool byTruck, bool allDefects) {
        var dateNow = DateTime.Now.Date;
        IQueryable<Defect> query = Alive<Defect>().Where(e => companyIds.Contains(e.CompanyId));
        query = byTruck
            ? query.Where(e => e.TruckId != null && ids.Contains(e.TruckId.Value))
            : query.Where(e => e.TrailerId != null && ids.Contains(e.TrailerId.Value));
        var defects = await query.ToListAsync();
        var repairIds = defects.Where(e => e.RepairId != null).Select(e => e.RepairId.Value).Distinct().ToList();
        var repairs = repairIds.Any()
            ? (await Table<DefectRepair>().Where(e => repairIds.Contains(e.EntityId)).ToListAsync()).ToDictionary(e => e.EntityId)
            : new Dictionary<Guid, DefectRepair>();
        foreach (var defect in defects) {
            defect.Repair = defect.RepairId != null && repairs.TryGetValue(defect.RepairId.Value, out var repair) ? repair : null;
        }
        if (allDefects == false) {
            // V1: ServiceNeeded == false || Repair == null || CreatedDate > today
            defects = defects.Where(e => e.ServiceNeeded == false || e.Repair == null || e.CreatedDate > dateNow).ToList();
        }
        return defects;
    }

    private static void LoadDefectFiles(List<DefectDto> defects) {
        foreach (DefectDto defect in defects) {
            defect.Files = new List<string>();
            var path = DispatchFileHelper.FileGetFullPath(defect.EntityId);
            if (Directory.Exists(path)) {
                defect.Files.AddRange(Directory.GetFiles(path).Select(Path.GetFileName));
            }
            if (defect.Repair != null) {
                defect.Repair.Files = new List<string>();
                var repairPath = DispatchFileHelper.FileGetFullPath(defect.Repair.EntityId);
                if (Directory.Exists(repairPath)) {
                    defect.Repair.Files.AddRange(Directory.GetFiles(repairPath).Select(Path.GetFileName));
                }
            }
        }
    }

    private async Task loadDefectsAsync(List<TruckDto> dtos, bool allDefects = false) {
        var defectEntities = await LoadDefectEntitiesAsync(dtos.Select(e => e.CompanyId).Distinct().ToList(), dtos.Select(e => e.EntityId).ToList(), true, allDefects);
        foreach (TruckDto dto in dtos) {
            dto.Defects = Mapper.Map<List<DefectDto>>(defectEntities.Where(e => e.TruckId == dto.EntityId));
            LoadDefectFiles(dto.Defects);
        }
    }

    private async Task loadDefectsAsync(List<TrailerDto> dtos, bool allDefects = false) {
        var defectEntities = await LoadDefectEntitiesAsync(dtos.Select(e => e.CompanyId).Distinct().ToList(), dtos.Select(e => e.EntityId).ToList(), false, allDefects);
        foreach (TrailerDto dto in dtos) {
            dto.Defects = Mapper.Map<List<DefectDto>>(defectEntities.Where(e => e.TrailerId == dto.EntityId));
            LoadDefectFiles(dto.Defects);
        }
    }

    // without deleting
    private async Task SaveDefectsAsync(List<DefectDto> dtos, Guid companyId, Guid userId) {
        if (dtos == null) return;

        foreach (DefectDto dto in dtos) {
            if (dto == null) {
                _l.f("dto is null!! #err");
                continue;
            }
            var defectEnitity = await GetByIdAsync<Defect>(dto.EntityId);
            if (defectEnitity != null) {
                var newDefectEntity = Mapper.Map<Defect>(dto);
                if (CompareEntities.Compare(defectEnitity, newDefectEntity) == false) {
                    newDefectEntity.LastModifiedById = userId;
                    newDefectEntity.LastModifiedDate = DateTime.UtcNow;
                    newDefectEntity.CreatedById = defectEnitity.CreatedById;
                    newDefectEntity.CreatedDate = defectEnitity.CreatedDate;
                    newDefectEntity.CompanyId = defectEnitity.CompanyId;
                    await UpdateAsync(newDefectEntity);
                }
            } else {
                defectEnitity = Mapper.Map<Defect>(dto);
                defectEnitity.LastModifiedById = defectEnitity.CreatedById = userId;
                defectEnitity.LastModifiedDate = defectEnitity.CreatedDate = DateTime.UtcNow;
                defectEnitity.CompanyId = companyId;
                await InsertAsync(defectEnitity);
            }

            if (dto.Repair != null) {
                var repairEnitity = await GetByIdAsync<DefectRepair>(dto.Repair.EntityId);
                if (repairEnitity != null) {
                    var newRepairEntity = Mapper.Map<DefectRepair>(dto.Repair);
                    if (CompareEntities.Compare(repairEnitity, newRepairEntity) == false) {
                        newRepairEntity.LastModifiedById = userId;
                        newRepairEntity.LastModifiedDate = DateTime.UtcNow;
                        newRepairEntity.CreatedById = repairEnitity.CreatedById;
                        newRepairEntity.CreatedDate = repairEnitity.CreatedDate;
                        newRepairEntity.CompanyId = repairEnitity.CompanyId;
                        await UpdateAsync(newRepairEntity);
                    }
                } else {
                    repairEnitity = Mapper.Map<DefectRepair>(dto.Repair);
                    repairEnitity.LastModifiedById = repairEnitity.CreatedById = userId;
                    repairEnitity.LastModifiedDate = repairEnitity.CreatedDate = DateTime.UtcNow;
                    repairEnitity.CompanyId = companyId;
                    await InsertAsync(repairEnitity);
                }
            }
        }
    }

    private async Task DeleteDefectsAsync(Guid? truckId, Guid? trailerId, Guid userId) {
        await Alive<Defect>().Where(e => e.TruckId == truckId && e.TrailerId == trailerId)
            .Set(e => e.IsDeleted, true)
            .Set(e => e.LastModifiedById, userId)
            .Set(e => e.LastModifiedDate, DateTime.UtcNow)
            .UpdateAsync();
    }

    public async Task<LoadDefectsResponseData> LoadDefects(LoadDefectsRequestData request) {
        if (request.TruckId != null) {
            var entity = await GetByIdAsync<Truck>(request.TruckId.Value);
            var dto = Mapper.Map<TruckDto>(entity);
            await loadDefectsAsync(new List<TruckDto>() { dto }, true);
            return new LoadDefectsResponseData() { Defects = dto.Defects };
        }
        if (request.TrailerId != null) {
            var entity = await GetByIdAsync<Trailer>(request.TrailerId.Value);
            var dto = Mapper.Map<TrailerDto>(entity);
            await loadDefectsAsync(new List<TrailerDto>() { dto }, true);
            return new LoadDefectsResponseData() { Defects = dto.Defects };
        }
        return new LoadDefectsResponseData();
    }

    public async Task<DeleteDefectResponseData> DeleteDefect(DeleteDefectRequestData request) {
        var defect = await GetByIdAsync<Defect>(request.DefectId);
        if (defect != null) {
            defect.IsDeleted = true;
            await UpdateAsync(defect);
            if (defect.RepairId != null) {
                await Table<DefectRepair>().Where(e => e.EntityId == defect.RepairId).Set(e => e.IsDeleted, true).UpdateAsync();
            }
            if (defect.TruckId != null) {
                var truck = await GetByIdAsync<Truck>(defect.TruckId.Value);
                var dto = Mapper.Map<TruckDto>(truck);
                Truck entity = await BaseSaveAsync<TruckDto, Truck>(request.UserId, dto);
                await this._saveEventHistoryTruckAsync(EventTypeModel.Truck, entity, null, request.UserId);
            }
            if (defect.TrailerId != null) {
                var trailer = await GetByIdAsync<Trailer>(defect.TrailerId.Value);
                var dto = Mapper.Map<TrailerDto>(trailer);
                Trailer entity = await BaseSaveAsync<TrailerDto, Trailer>(request.UserId, dto);
                await this._saveEventHistoryTrailerAsync(EventTypeModel.Trailer, entity, request.UserId);
            }
        }
        return new DeleteDefectResponseData() { Defect = Mapper.Map<DefectDto>(defect) };
    }

    public async Task<DeleteDefectRepairResponseData> DeleteDefectRepair(DeleteDefectRepairRequestData request) {
        var repair = await GetByIdAsync<DefectRepair>(request.RepairId);
        if (repair != null) {
            repair.IsDeleted = true;
            await UpdateAsync(repair);
            var defect = await Alive<Defect>().FirstOrDefaultAsync(e => e.RepairId == request.RepairId);
            if (defect != null) {
                defect.RepairId = null;
                await UpdateAsync(defect);
                if (defect.TruckId != null) {
                    var truck = await GetByIdAsync<Truck>(defect.TruckId.Value);
                    var dto = Mapper.Map<TruckDto>(truck);
                    Truck entity = await BaseSaveAsync<TruckDto, Truck>(request.UserId, dto);
                    await this._saveEventHistoryTruckAsync(EventTypeModel.Truck, entity, null, request.UserId);
                }
                if (defect.TrailerId != null) {
                    var trailer = await GetByIdAsync<Trailer>(defect.TrailerId.Value);
                    var dto = Mapper.Map<TrailerDto>(trailer);
                    Trailer entity = await BaseSaveAsync<TrailerDto, Trailer>(request.UserId, dto);
                    await this._saveEventHistoryTrailerAsync(EventTypeModel.Trailer, entity, request.UserId);
                }
            }
        }
        return new DeleteDefectRepairResponseData() { Repair = Mapper.Map<DefectRepairDto>(repair) };
    }

    #endregion [ Defects ]

    #region [ Files ]

    private static FileDto ToFileDto(FileInfo info, Guid entityId, bool preview, byte[] data) {
        return new FileDto() {
            CreatedDate = info.CreationTime,
            Name = info.Name,
            Size = info.Length,
            Preview = preview,
            EntityId = entityId,
            Data = data
        };
    }

    public async Task<FilesLoadResponseData> FilesLoad(FilesLoadRequestData request) {
        var path = DispatchFileHelper.FileGetFullPath(request.EntityId);
        if (Directory.Exists(path) == false) {
            return new FilesLoadResponseData() { Files = new List<FileDto>() };
        }
        var files = new List<FileDto>();
        foreach (string f in Directory.GetFiles(path)) {
            var info = new FileInfo(f);
            byte[] data = request.LoadData ? await File.ReadAllBytesAsync(f)
                : request.Preview && ImageHelper.IsImage(f) ? ImageHelper.OptimizeImages(await File.ReadAllBytesAsync(f)) : null;
            files.Add(ToFileDto(info, request.EntityId, request.Preview, data));
        }
        return new FilesLoadResponseData() { Files = files };
    }

    public async Task<FileLoadResponseData> FileLoad(FileLoadRequestData request) {
        if (Directory.Exists(DispatchFileHelper.FileGetFullPath(request.File.EntityId)) == false) {
            throw new InfoException("File doesn't exists");
        }
        string fullPath;
        if (request.File.Name.Contains("*")) {
            var files = Directory.GetFiles(DispatchFileHelper.FileGetFullPath(request.File.EntityId), request.File.Name, SearchOption.TopDirectoryOnly);
            if (files.Any() == false) {
                throw new InfoException("File doesn't exists");
            }
            fullPath = files.First();
        } else {
            fullPath = DispatchFileHelper.FileGetFullPath(request.File.EntityId, request.File.Name);
        }
        if (File.Exists(fullPath) == false) {
            throw new InfoException("File doesn't exists");
        }
        var info = new FileInfo(fullPath);
        var data = await File.ReadAllBytesAsync(fullPath);
        if (request.File.Preview && ImageHelper.IsImage(fullPath)) {
            data = ImageHelper.OptimizeImages(data);
        }
        return new FileLoadResponseData() { File = ToFileDto(info, request.File.EntityId, request.File.Preview, data) };
    }

    public async Task<FileSaveResponseData> FileSave(FileSaveRequestData request) {
        var path = DispatchFileHelper.FileGetFullPath(request.File.EntityId);
        Directory.CreateDirectory(path);
        var fullPath = Path.Combine(path, request.File.Name);
        await BinaryHelper.WriteAllBytesAsync(fullPath, request.File.Data);
        var info = new FileInfo(fullPath);
        return new FileSaveResponseData() { File = ToFileDto(info, request.File.EntityId, false, null) };
    }

    /// <summary>
    ///     Файл уходит в RecyclePath/{EntityId}, каталог без имени файла удаляется целиком </summary>
    public FileDeleteResponseData FileDelete(FileDeleteRequestData request) {
        if (request.File.Name != null) {
            var fullPath = DispatchFileHelper.FileGetFullPath(request.File.EntityId, request.File.Name);
            if (File.Exists(fullPath)) {
                var recycleDirectory = Path.Combine(AppSettings.RecyclePath, request.File.EntityId.ToString());
                Directory.CreateDirectory(recycleDirectory);
                var recycleFullPath = Path.Combine(recycleDirectory, request.File.Name);
                File.Move(fullPath, recycleFullPath, true);
            }
        } else {
            var fullPath = DispatchFileHelper.FileGetFullPath(request.File.EntityId);
            if (Directory.Exists(fullPath)) {
                Directory.Delete(fullPath, true);
            }
        }
        return new FileDeleteResponseData() {
            File = new FileDto() { Name = request.File.Name, EntityId = request.File.EntityId }
        };
    }

    public FileRenameResponseData FileRename(FileRenameRequestData request) {
        var fullPathOld = DispatchFileHelper.FileGetFullPath(request.File.EntityId, request.File.Name);
        var fullPathNew = DispatchFileHelper.FileGetFullPath(request.File.EntityId, request.NewName);
        File.Move(fullPathOld, fullPathNew);
        var info = new FileInfo(fullPathNew);
        return new FileRenameResponseData() { File = ToFileDto(info, request.File.EntityId, false, null) };
    }

    #endregion [ Files ]

    #region [ Insurance ]

    public async Task<InsuranceLoadResponseData> InsuranceLoad(InsuranceLoadRequestData request) {
        var company = await GetByIdAsync<DatCompany>(request.EntityId);
        var fullPath = DispatchFileHelper.InsuranceGetFullPath(request.EntityId, company.InsuranceFile);
        if (File.Exists(fullPath)) {
            var dto = this.Mapper.Map<DatCompanyDto>(company);
            dto.InsuranceFileData = await File.ReadAllBytesAsync(fullPath);
            return new InsuranceLoadResponseData() { DatCompany = dto };
        }
        throw new InfoException("File doesn't exists");
    }

    private async Task InsuranceSaveAsync(DatCompanyDto company) {
        var path = DispatchFileHelper.InsuranceGetFullPath(company.EntityId);
        Directory.CreateDirectory(path);
        var fullPath = DispatchFileHelper.UniqueFileFullPath(Path.Combine(path, company.InsuranceFilePath));
        await BinaryHelper.WriteAllBytesAsync(fullPath, company.InsuranceFileData);

        if (company.InsuranceFilePath != Path.GetFileName(fullPath)) {
            // File name had changed
            company.InsuranceFilePath = Path.GetFileName(fullPath);
        }
        // removes old insurance
        if (company.SetupPacketId != null) {
            var setupPaket = await GetByIdAsync<SetupPacket>(company.SetupPacketId.Value);
            if (String.IsNullOrEmpty(setupPaket?.FileCOI) == false) {
                var fullPath2 = DispatchFileHelper.FileGetFullPath(company.EntityId, setupPaket.FileCOI);
                if (File.Exists(fullPath2)) {
                    File.Delete(fullPath2);
                }
            }
        }
    }

    #endregion [ Insurance ]
}
