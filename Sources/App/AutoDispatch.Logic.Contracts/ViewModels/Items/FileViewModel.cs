using System;
using System.Windows.Media.Imaging;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class FileViewModel : ViewModel, ICloneable {
        private BitmapImage dataImage;

        /// <summary>
        ///     Load/Company ID </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        ///     Name </summary>
        public string Name { get; set; }
        /// <summary>
        ///     Size of file </summary>
        public long Size { get; set; }
        /// <summary>
        ///     Created date </summary>
        public DateTime CreatedDate { get; set; }
        /// <summary>
        ///     Image </summary>
        public BitmapImage DataImage {
            get => dataImage;
            set {
                dataImage = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        ///     Image </summary>
        public BitmapImage IconImage { get; set; }
        /// <summary>
        ///     Binary dataImage </summary>
        public byte[] Data { get; set; }

        public bool Preview { get; set; }

        public bool LazySave { get; set; }

        public object Clone() {
            return new FileViewModel() {
                EntityId = this.EntityId,
                Name = this.Name,
                Size = this.Size,
                CreatedDate = this.CreatedDate,
                IconImage = this.IconImage,
                DataImage = this.DataImage,
                Data = (byte[])this.Data?.Clone(),
                Preview = Preview,
            };
        }
    }
}
