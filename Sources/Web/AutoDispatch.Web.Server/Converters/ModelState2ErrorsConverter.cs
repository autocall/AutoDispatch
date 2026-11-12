using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AutoDispatch.Web.Server.Converters;

public class ModelState2ErrorsConverter : ITypeConverter<ModelStateDictionary, Dictionary<string, string>> {
    public Dictionary<string, string> Convert(ModelStateDictionary source, Dictionary<string, string> destination, ResolutionContext context) {
        destination = new Dictionary<string, string>();
        foreach (var key in source.Keys) {
            var errors = source[key].Errors;
            if (errors.Count > 0) {
                destination.Add(key, String.Join("\n", errors.Select(e => e.ErrorMessage)));
            }
        }
        return destination;
    }
}
