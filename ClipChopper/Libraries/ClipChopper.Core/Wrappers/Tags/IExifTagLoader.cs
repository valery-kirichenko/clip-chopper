using System.Collections.Generic;
using System.Threading.Tasks;
using ClipChopper.Models.IO;
using ClipChopper.Models.Wrappers.Tags;

namespace ClipChopper.Core.Wrappers.Tags
{
    public interface IExifTagLoader
    {
        Task<IReadOnlyList<ExifTag>> LoadTagsAsync(FilePath path);
    }
}
