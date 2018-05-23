using Cadmus.Core.Config;
using Cadmus.Lexicon.Parts;
using Cadmus.Parts.General;
using Cadmus.Philology.Parts.Layers;
using System.Reflection;

namespace CadmusApi.Services
{
    /// <summary>
    /// Part types provider for statically linked plugins.
    /// </summary>
    public sealed class StaticPartTypeProvider : AttributedPartTypeProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticPartTypeProvider"/> class.
        /// </summary>
        public StaticPartTypeProvider() : base(new[]
            {
                // plugins:
                // Cadmus.Parts
                typeof(NotePart).GetTypeInfo().Assembly,
                // Cadmus.Lexicon.Parts
                typeof(WordFormPart).GetTypeInfo().Assembly,
                // Cadmus.Philology.Parts
                typeof(ApparatusLayerFragment).GetTypeInfo().Assembly
            })
        {
        }
    }
}
