using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Entities.Configuration
{
    public sealed class ClassificationConfiguration
    {
        
        public int PositiveThreshold { get; init; }

        public int NegativeThreshold { get; init; }

        public bool UseRatingAsMainClassifier { get; init; }

        public HashSet<string> CustomPositiveWords { get; init; } = new();

        public HashSet<string> CustomNegativeWords { get; init; } = new();

        private ClassificationConfiguration() { }

        public static ClassificationConfiguration Default => new()
        {
            PositiveThreshold = 1,
            NegativeThreshold = 1,
            UseRatingAsMainClassifier = true,
            CustomPositiveWords = new HashSet<string>
        {
            "excelente", "bueno", "perfecto", "recomiendo", "encanta",
            "satisfecho", "calidad", "rápido", "funciona", "contento"
        },
            CustomNegativeWords = new HashSet<string>
        {
            "mala", "malo", "pésima", "rompió", "decepcionado",
            "insatisfecho", "tardío", "dañado", "problema", "defecto"
        }
        };

        public ClassificationConfiguration AddPositiveWords(params string[] words)
        {
            var newWords = new HashSet<string>(CustomPositiveWords);
            foreach (var word in words)
            {
                newWords.Add(word.ToLower());
            }

            return new ClassificationConfiguration
            {
                PositiveThreshold = PositiveThreshold,
                NegativeThreshold = NegativeThreshold,
                UseRatingAsMainClassifier = UseRatingAsMainClassifier,
                CustomPositiveWords = newWords,
                CustomNegativeWords = CustomNegativeWords
            };
        }

        public ClassificationConfiguration AddNegativeWords(params string[] words)
        {
            var newWords = new HashSet<string>(CustomNegativeWords);
            foreach (var word in words)
            {
                newWords.Add(word.ToLower());
            }

            return new ClassificationConfiguration
            {
                PositiveThreshold = PositiveThreshold,
                NegativeThreshold = NegativeThreshold,
                UseRatingAsMainClassifier = UseRatingAsMainClassifier,
                CustomPositiveWords = CustomPositiveWords,
                CustomNegativeWords = newWords
            };
        }
    }

}
