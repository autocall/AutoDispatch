using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.Heplers;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 File/DtoToVm, File/VmToDto </summary>
    public static class FileConverter {

        public static FileViewModel ToVm(FileDto source) {
            if (source == null) return null;
            var result = new FileViewModel();
            result.CreatedDate = source.CreatedDate;
            result.EntityId = source.EntityId;
            result.Data = source.Data;
            result.Name = source.Name;
            result.Size = source.Size;
            result.Preview = source.Preview;
            result.IconImage = FileIconLoader.GetFileIcon(source.Name, false);
            return result;
        }

        public static FileDto ToDto(FileViewModel source) {
            if (source == null) return null;
            var result = new FileDto();
            result.CreatedDate = source.CreatedDate;
            result.EntityId = source.EntityId;
            //result.Data = source.Data; skip
            result.Name = source.Name;
            result.Size = source.Size;
            result.Preview = source.Preview;
            return result;
        }
    }
}
